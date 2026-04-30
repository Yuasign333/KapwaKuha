// FILE: KapwaDataService.cs  (FULL REVISED)
// Database: KAPWAKUHA_DATABASE
// Pattern: identical to CarDataService — async, parameterized, dual-connection auto-detect.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using KapwaKuha.Models;
using Microsoft.Data.SqlClient;

namespace KapwaKuha.Services
{
    public static class KapwaDataService
    {
        // ── Connection ────────────────────────────────────────────────────────
        private static string? _cachedConn;

        private static readonly string _laptopConn =
            @"Server=DESKTOP-8P1VJSE;Database=KapwaKuha_Database;Trusted_Connection=True;TrustServerCertificate=True;";
        private static readonly string _pcConn =
            @"Server=CCL2-12\MSSQLSERVER01;Database=KapwaKuha_Database;User Id=sa;Password=ccl2;TrustServerCertificate=True;";

        private static string _conn
        {
            get
            {
                if (_cachedConn != null) return _cachedConn;
                try
                {
                    using var t = new SqlConnection(_pcConn + "Connect Timeout=5;");
                    t.Open();
                    return _cachedConn = _pcConn;
                }
                catch { }
                return _cachedConn = _laptopConn;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // AUTH / LOGIN
        // ══════════════════════════════════════════════════════════════════════

        public static async Task<(bool OK, string UserId, string FullName, string Username)>
            LoginDonor(string username, string password)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(@"
                    SELECT d.Donor_ID, d.Donor_FullName, d.Donor_Username
                    FROM Donors d
                    INNER JOIN Users u ON u.UserID = d.Donor_ID
                    WHERE d.Donor_Username = @uname AND u.Password = @pw", conn);
                cmd.Parameters.AddWithValue("@uname", username);
                cmd.Parameters.AddWithValue("@pw", password);
                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                    return (true,
                        r["Donor_ID"].ToString() ?? "",
                        r["Donor_FullName"].ToString() ?? "",
                        r["Donor_Username"].ToString() ?? "");
            }
            catch (Exception ex) { MessageBox.Show("LoginDonor failed: " + ex.Message); }
            return (false, "", "", "");
        }

        public static async Task<(bool OK, string UserId, string FullName, string Username)>
            LoginBeneficiary(string username, string password)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(@"
                    SELECT b.Beneficiary_ID,
                           b.Beneficiary_FullName AS FullName,
                           b.Beneficiary_Username AS Username
                    FROM Beneficiaries b
                    INNER JOIN Users u ON u.UserID = b.Beneficiary_ID
                    WHERE b.Beneficiary_Username = @uname AND u.Password = @pw", conn);
                cmd.Parameters.AddWithValue("@uname", username);
                cmd.Parameters.AddWithValue("@pw", password);
                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                    return (true,
                        r["Beneficiary_ID"].ToString() ?? "",
                        r["FullName"].ToString() ?? "",
                        r["Username"].ToString() ?? "");
            }
            catch (Exception ex) { MessageBox.Show("LoginBeneficiary failed: " + ex.Message); }
            return (false, "", "", "");
        }

        // ══════════════════════════════════════════════════════════════════════
        // REGISTRATION (sp_RegisterDonor / sp_RegisterBeneficiary)
        // ══════════════════════════════════════════════════════════════════════

        public static async Task<string> GetNextDonorId()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Donors", conn);
                int n = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return $"D{n + 1:D3}";
            }
            catch { return $"D{DateTime.Now.Ticks % 900 + 100:D3}"; }
        }

        public static async Task<string> GetNextBeneficiaryId()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Beneficiaries", conn);
                int n = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return $"B{n + 1:D3}";
            }
            catch { return $"B{DateTime.Now.Ticks % 900 + 100:D3}"; }
        }

        public static async Task RegisterDonor(DonorModel donor, string password,
            string securityQuestion, string securityAnswer)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_RegisterDonor", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DonorId", donor.Donor_ID);
                cmd.Parameters.AddWithValue("@FullName", donor.Donor_FullName);
                cmd.Parameters.AddWithValue("@Username", donor.Donor_Username);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@Contact", donor.Donor_ContactNumber);
                cmd.Parameters.AddWithValue("@SecurityQ", securityQuestion);
                cmd.Parameters.AddWithValue("@SecurityA", securityAnswer);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("RegisterDonor failed: " + ex.Message); throw; }
        }

        public static async Task RegisterBeneficiary(BeneficiaryModel bene, string password,
     string securityQuestion, string securityAnswer)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_RegisterBeneficiary", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BeneficiaryId", bene.Beneficiary_ID);

                // DB has a single Beneficiary_FullName column — combine FName+LName if needed
                var fullName = string.IsNullOrWhiteSpace(bene.Beneficiary_FullName)
                    ? $"{bene.Beneficiary_FName} {bene.Beneficiary_LName}".Trim()
                    : bene.Beneficiary_FullName;
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Username", bene.Beneficiary_Username);
                cmd.Parameters.AddWithValue("@Sex", bene.Beneficiary_Sex);
                cmd.Parameters.AddWithValue("@Contact", bene.Beneficiary_Contact);
                cmd.Parameters.AddWithValue("@OrgId", bene.Organization_ID);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@SecurityQ", securityQuestion);
                cmd.Parameters.AddWithValue("@SecurityA", securityAnswer);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("RegisterBeneficiary failed: " + ex.Message); throw; }
        }

        // ══════════════════════════════════════════════════════════════════════
        // FORGOT PASSWORD (sp_GetSecurityQuestion / sp_ResetPassword)
        // ══════════════════════════════════════════════════════════════════════

        public static async Task<(bool Found, string Question, string UserId)>
            GetSecurityQuestion(string username)
        {
            try
            {

                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                // Try Donors first, then Beneficiaries
                using var cmd = new SqlCommand(@"
                    SELECT Donor_ID AS UserId, SecurityQuestion FROM Donors WHERE Donor_Username = @u
                    UNION ALL
                    SELECT Beneficiary_ID AS UserId, SecurityQuestion FROM Beneficiaries WHERE Beneficiary_ID = @u", conn);
                cmd.Parameters.AddWithValue("@u", username);
                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                    return (true, r["SecurityQuestion"].ToString() ?? "", r["UserId"].ToString() ?? "");
            }
            catch (Exception ex) { MessageBox.Show("GetSecurityQuestion failed: " + ex.Message); }
            return (false, "", "");
        }

        public static async Task<(bool Success, string Message)>
            ResetPassword(string username, string securityAnswer, string newPassword)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_ResetPassword", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@SecurityAnswer", securityAnswer);
                cmd.Parameters.AddWithValue("@NewPassword", newPassword);
                await cmd.ExecuteNonQueryAsync();
                return (true, "Password reset successfully.");
            }
            catch (SqlException ex) when (ex.Number == 50002)
            {
                return (false, "Security answer is incorrect.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ITEMS  (Strong Entity — parallel to Cars)
        // ══════════════════════════════════════════════════════════════════════

        public static async Task<List<ItemModel>> GetAllItems()
        {
            var list = new List<ItemModel>();
            const string sql = @"
                SELECT i.Item_ID, i.Item_Name, i.Item_Condition, i.Item_Status,
                       i.Date_Found, i.Donor_ID, i.Category_ID,
                       ISNULL(i.PostType,'GeneralPost') AS PostType,
                       ISNULL(i.TargetBeneficiary_ID,'') AS TargetBeneficiary_ID,
                       ISNULL(i.Item_Description,'') AS Item_Description,
                       ISNULL(i.Item_ImagePath,'') AS Item_ImagePath,
                       d.Donor_FullName AS Donor_Name,
                       c.Category_Name
                FROM Items i
                LEFT JOIN Donors   d ON d.Donor_ID    = i.Donor_ID
                LEFT JOIN Category c ON c.Category_ID = i.Category_ID";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(sql, conn);
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync()) list.Add(MapItem(r));
            }
            catch (Exception ex) { MessageBox.Show("GetAllItems failed: " + ex.Message); }
            return list;
        }

        public static async Task<List<ItemModel>> GetAvailableItems()
        {
            var all = await GetAllItems();
            return all.FindAll(i => i.Item_Status == "Available" && i.PostType == "GeneralPost");
        }

        public static async Task<List<ItemModel>> GetItemsByDonor(string donorId)
        {
            var all = await GetAllItems();
            return all.FindAll(i => i.Donor_ID == donorId);
        }

        public static async Task UpdateItemStatus(string itemId, string status)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "UPDATE Items SET Item_Status=@s WHERE Item_ID=@id", conn);
                cmd.Parameters.AddWithValue("@s", status);
                cmd.Parameters.AddWithValue("@id", itemId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("UpdateItemStatus failed: " + ex.Message); }
        }

        /// <summary>
        /// Alias used by PostItemViewModel — maps PostType/TargetBeneficiary_ID
        /// from ItemModel's field names to the SQL INSERT.
        /// </summary>
        public static async Task AddItem(ItemModel item)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(@"
            INSERT INTO Items(Item_ID, Item_Name, Item_Condition, Item_Status,
                             Date_Found, Donor_ID, Category_ID, PostType, TargetBeneficiary_ID,
                             Item_Description, Item_ImagePath)
            VALUES(@id, @name, @cond, @status, @date, @did, @catid, @ptype, @tbid, @desc, @img)", conn);
                cmd.Parameters.AddWithValue("@id", item.Item_ID);
                cmd.Parameters.AddWithValue("@name", item.Item_Name);
                cmd.Parameters.AddWithValue("@cond", item.Item_Condition);
                cmd.Parameters.AddWithValue("@status", item.Item_Status);
                cmd.Parameters.AddWithValue("@date", item.Date_Found);
                cmd.Parameters.AddWithValue("@did", item.Donor_ID);
                cmd.Parameters.AddWithValue("@catid", item.Category_ID);
                cmd.Parameters.AddWithValue("@ptype", item.PostType);
                cmd.Parameters.AddWithValue("@tbid",
                    string.IsNullOrEmpty(item.TargetBeneficiary_ID)
                        ? "" : item.TargetBeneficiary_ID);
                cmd.Parameters.AddWithValue("@desc", item.Item_Description ?? "");
                cmd.Parameters.AddWithValue("@img", item.Item_ImagePath ?? "");
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("AddItem failed: " + ex.Message); throw; }
        }


        public static async Task DeleteItem(string itemId)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "DELETE FROM Items WHERE Item_ID=@id AND Item_Status='Available'", conn);
                cmd.Parameters.AddWithValue("@id", itemId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("DeleteItem failed: " + ex.Message); throw; }
        }

        public static async Task<string> GetNextItemId()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Items", conn);
                int n = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return $"ITEM{n + 1:D3}";
            }
            catch { return $"ITEM{DateTime.Now.Ticks % 900 + 100:D3}"; }
        }

        public static async Task<List<string>> GetAllCategories()
        {
            var list = new List<string>();
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT Category_ID, Category_Name FROM Category ORDER BY Category_Name", conn);
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    list.Add(r["Category_Name"].ToString() ?? "");
            }
            catch (Exception ex) { MessageBox.Show("GetAllCategories failed: " + ex.Message); }
            return list;
        }

        public static async Task<string> GetCategoryIdByName(string name)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT Category_ID FROM Category WHERE Category_Name=@n", conn);
                cmd.Parameters.AddWithValue("@n", name);
                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "";
            }
            catch { return ""; }
        }

        /// <summary>Alias for GetCategoryIdByName — used by PostItemViewModel.</summary>
        public static Task<string> GetCategoryId(string name)
            => GetCategoryIdByName(name);

        /// <summary>
        /// Items with status "Available" — used by ClaimProcessViewModel.
        /// Parallel to GetActiveRentals() in CarRentals.
        /// </summary>
        public static async Task<List<ItemModel>> GetFoundItems()
        {
            var all = await GetAllItems();
            return all.FindAll(i => i.Item_Status == "Available");
        }
        private static ItemModel MapItem(SqlDataReader r) => new()
        {
            Item_ID = r["Item_ID"].ToString() ?? "",
            Item_Name = r["Item_Name"].ToString() ?? "",
            Item_Condition = r["Item_Condition"].ToString() ?? "Good",
            Item_Status = r["Item_Status"].ToString() ?? "Available",
            Date_Found = Convert.ToDateTime(r["Date_Found"]),
            Donor_ID = r["Donor_ID"].ToString() ?? "",
            Donor_Name = r["Donor_Name"].ToString() ?? "",
            Category_ID = r["Category_ID"].ToString() ?? "",
            Category_Name = r["Category_Name"].ToString() ?? "",
            PostType = r["PostType"].ToString() ?? "GeneralPost",
            TargetBeneficiary_ID = r["TargetBeneficiary_ID"].ToString() ?? "",
            Item_Description = r["Item_Description"].ToString() ?? "",
            Item_ImagePath = r["Item_ImagePath"].ToString() ?? ""
        };

        // ══════════════════════════════════════════════════════════════════════
        // CLAIMS  (Weak Entity — parallel to Rentals)
        // ══════════════════════════════════════════════════════════════════════

        
       public static async Task<List<ClaimModel>> GetAllClaims()
        {
            var list = new List<ClaimModel>();
            const string sql = @"
        SELECT cl.Claim_ID, cl.Item_ID, cl.Beneficiary_ID,
               cl.Claim_Date, cl.Claim_Status, cl.Verification_Notes,
               ISNULL(cl.Handoff_Type,'Pickup') AS Handoff_Type,
               i.Item_Name,
               ISNULL(i.Item_ImagePath,'') AS Item_ImagePath,
               b.Beneficiary_FullName AS Beneficiary_Name
        FROM Claims cl
        LEFT JOIN Items         i ON i.Item_ID        = cl.Item_ID
        LEFT JOIN Beneficiaries b ON b.Beneficiary_ID = cl.Beneficiary_ID";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(sql, conn);
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync()) list.Add(MapClaim(r));
            }
            catch (Exception ex) { MessageBox.Show("GetAllClaims failed: " + ex.Message); }
            return list;
        }

        public static async Task<List<ClaimModel>> GetClaimsByBeneficiary(string beneficiaryId)
        {
            var all = await GetAllClaims();
            return all.FindAll(c => c.Beneficiary_ID == beneficiaryId);
        }

        public static async Task SaveClaim(ClaimModel claim)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_ProcessClaim", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ClaimId", claim.Claim_ID);
                cmd.Parameters.AddWithValue("@ItemId", claim.Item_ID);
                cmd.Parameters.AddWithValue("@BeneficiaryId", claim.Beneficiary_ID);
                cmd.Parameters.AddWithValue("@ClaimStatus", claim.Claim_Status);
                cmd.Parameters.AddWithValue("@HandoffType", claim.Handoff_Type);
                cmd.Parameters.AddWithValue("@Notes", claim.Verification_Notes);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("SaveClaim failed: " + ex.Message); throw; }
        }

        public static async Task<string> GetNextClaimId()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM Claims", conn);
                int n = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return $"CL{n + 1:D3}";
            }
            catch { return $"CL{DateTime.Now.Ticks % 900 + 100:D3}"; }
        }

        private static ClaimModel MapClaim(SqlDataReader r) => new()
        {
            Claim_ID = r["Claim_ID"].ToString() ?? "",
            Item_ID = r["Item_ID"].ToString() ?? "",
            Item_Name = r["Item_Name"].ToString() ?? "",
            Item_ImagePath = r["Item_ImagePath"].ToString() ?? "",
            Beneficiary_ID = r["Beneficiary_ID"].ToString() ?? "",
            Beneficiary_Name = r["Beneficiary_Name"].ToString() ?? "",
            Claim_Date = Convert.ToDateTime(r["Claim_Date"]),
            Claim_Status = r["Claim_Status"].ToString() ?? "Pending",
            Verification_Notes = r["Verification_Notes"].ToString() ?? "",
            Handoff_Type = r["Handoff_Type"].ToString() ?? "Pickup"
        };

        // ══════════════════════════════════════════════════════════════════════
        // BENEFICIARIES
        // ══════════════════════════════════════════════════════════════════════

        public static async Task<List<BeneficiaryModel>> GetActiveBeneficiariesFull()
        {
            var list = new List<BeneficiaryModel>();
            const string sql = @"
        SELECT b.Beneficiary_ID, b.Beneficiary_FullName,
               b.Beneficiary_Username,
               b.Beneficiary_Sex, b.Beneficiary_Contact,
               b.Beneficiaries_Status, b.Organization_ID,
               o.Organization_Name
        FROM Beneficiaries b
        LEFT JOIN Organization o ON o.Organization_ID = b.Organization_ID
        WHERE b.Beneficiaries_Status = 'Active'";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(sql, conn);
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    list.Add(new BeneficiaryModel
                    {
                        Beneficiary_ID = r["Beneficiary_ID"].ToString() ?? "",
                        Beneficiary_FullName = r["Beneficiary_FullName"].ToString() ?? "",
                        Beneficiary_Username = r["Beneficiary_Username"].ToString() ?? "",
                        Beneficiary_Sex = r["Beneficiary_Sex"].ToString() ?? "",
                        Beneficiary_Contact = r["Beneficiary_Contact"].ToString() ?? "",
                        Beneficiaries_Status = r["Beneficiaries_Status"].ToString() ?? "Active",
                        Organization_ID = r["Organization_ID"].ToString() ?? "",
                        Organization_Name = r["Organization_Name"].ToString() ?? ""
                    });
            }
            catch (Exception ex) { MessageBox.Show("GetActiveBeneficiariesFull failed: " + ex.Message); }
            return list;
        }
        /// <summary>Returns ALL active beneficiaries for donor chat search.</summary>
        public static Task<List<BeneficiaryModel>> GetAllBeneficiariesForChat()
            => GetActiveBeneficiariesFull();

        // Legacy tuple version used by ClaimProcessViewModel
        public static async Task<List<(string Id, string DisplayName)>> GetActiveBeneficiaries()
        {
            var full = await GetActiveBeneficiariesFull();
            var result = new List<(string, string)>();
            foreach (var b in full)
                result.Add((b.Beneficiary_ID, b.DisplayName));
            return result;
        }

        public static async Task<List<(string Id, string Name)>> GetAllOrganizations()
        {
            var list = new List<(string, string)>();
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT Organization_ID, Organization_Name FROM Organization ORDER BY Organization_Name", conn);
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    list.Add((r["Organization_ID"].ToString() ?? "", r["Organization_Name"].ToString() ?? ""));
            }
            catch (Exception ex) { MessageBox.Show("GetAllOrganizations failed: " + ex.Message); }
            return list;
        }

        // ══════════════════════════════════════════════════════════════════════
        // IMPACT METRICS  (parallel to Revenue analytics)
        // ══════════════════════════════════════════════════════════════════════

        public static async Task<(int TotalDonated, int TotalClaimed, int ActiveItems, double AvgStorageDays)>
            GetImpactMetrics(string donorId)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                using var c1 = new SqlCommand(
                    "SELECT COUNT(*) FROM Items WHERE Donor_ID=@did", conn);
                c1.Parameters.AddWithValue("@did", donorId);
                int total = Convert.ToInt32(await c1.ExecuteScalarAsync());

                using var c2 = new SqlCommand(
                    "SELECT COUNT(*) FROM Items WHERE Donor_ID=@did AND Item_Status='Claimed'", conn);
                c2.Parameters.AddWithValue("@did", donorId);
                int claimed = Convert.ToInt32(await c2.ExecuteScalarAsync());

                using var c3 = new SqlCommand(
                    "SELECT COUNT(*) FROM Items WHERE Donor_ID=@did AND Item_Status='Available'", conn);
                c3.Parameters.AddWithValue("@did", donorId);
                int active = Convert.ToInt32(await c3.ExecuteScalarAsync());

                using var c4 = new SqlCommand(@"
                    SELECT ISNULL(AVG(CAST(DATEDIFF(day,i.Date_Found,cl.Claim_Date) AS FLOAT)),0)
                    FROM Claims cl JOIN Items i ON i.Item_ID=cl.Item_ID
                    WHERE i.Donor_ID=@did AND cl.Claim_Status='Released'", conn);
                c4.Parameters.AddWithValue("@did", donorId);
                double avg = Convert.ToDouble(await c4.ExecuteScalarAsync());

                return (total, claimed, active, avg);
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetImpactMetrics failed: " + ex.Message);
                return (0, 0, 0, 0);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // NEEDS POSTS  (organization wishlists)
        // ══════════════════════════════════════════════════════════════════════

        public static async Task<List<NeedsPostModel>> GetOpenNeedsPosts()
        {
            var list = new List<NeedsPostModel>();
            const string sql = @"
                SELECT n.NeedsPost_ID, n.Org_ID, n.Title, n.Description,
                       n.Urgency, n.Status, n.Post_Date,
                       o.Organization_Name AS Org_Name
                FROM NeedsPosts n
                LEFT JOIN Organization o ON o.Organization_ID = n.Org_ID
                WHERE n.Status='Open'
                ORDER BY CASE n.Urgency WHEN 'High' THEN 1 WHEN 'Medium' THEN 2 ELSE 3 END";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(sql, conn);
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    list.Add(MapNeedsPost(r));
            }
            catch (Exception ex) { MessageBox.Show("GetOpenNeedsPosts failed: " + ex.Message); }
            return list;
        }

        public static async Task PostNeedsRequest(NeedsPostModel post)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_PostNeedsRequest", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PostId", post.NeedsPost_ID);
                cmd.Parameters.AddWithValue("@OrgId", post.Org_ID);
                cmd.Parameters.AddWithValue("@Title", post.Title);
                cmd.Parameters.AddWithValue("@Description", post.Description);
                cmd.Parameters.AddWithValue("@Urgency", post.Urgency);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("PostNeedsRequest failed: " + ex.Message); throw; }
        }

        public static async Task<string> GetNextNeedsPostId()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM NeedsPosts", conn);
                int n = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return $"NP{n + 1:D3}";
            }
            catch { return $"NP{DateTime.Now.Ticks % 900 + 100:D3}"; }
        }

        private static NeedsPostModel MapNeedsPost(SqlDataReader r) => new()
        {
            NeedsPost_ID = r["NeedsPost_ID"].ToString() ?? "",
            Org_ID = r["Org_ID"].ToString() ?? "",
            Org_Name = r["Org_Name"].ToString() ?? "",
            Title = r["Title"].ToString() ?? "",
            Description = r["Description"].ToString() ?? "",
            Urgency = r["Urgency"].ToString() ?? "Medium",
            Status = r["Status"].ToString() ?? "Open",
            Post_Date = Convert.ToDateTime(r["Post_Date"])
        };

        // ══════════════════════════════════════════════════════════════════════
        // CHAT  (parallel to CarRentals ChatMessages)
        // ══════════════════════════════════════════════════════════════════════

        public static async Task SaveChatMessage(string senderId, string receiverId, string message)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_SaveChatMessage", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SenderId", senderId);
                cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
                cmd.Parameters.AddWithValue("@Message", message);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("SaveChatMessage failed: " + ex.Message); }
        }

        public static async Task<List<ChatMessage>> GetChatMessages(string userId1, string userId2)
        {
            var list = new List<ChatMessage>();
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetChatMessages", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId1", userId1);
                cmd.Parameters.AddWithValue("@UserId2", userId2);
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    string senderId = r["SenderId"].ToString() ?? "";
                    list.Add(new ChatMessage
                    {
                        Id = Convert.ToInt32(r["Id"]),
                        SenderId = senderId,
                        ReceiverId = r["ReceiverId"].ToString() ?? "",
                        Text = r["Message"].ToString() ?? "",
                        Time = Convert.ToDateTime(r["SentAt"]).ToString("HH:mm"),
                        IsFromUser = senderId == userId1
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("GetChatMessages failed: " + ex.Message); }
            return list;
        }

        public static async Task<List<(string UserId, string FullName, string LastMessage, int UnreadCount)>>
            GetChatDonors()
        {
            var list = new List<(string, string, string, int)>();
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetChatDonors", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    list.Add((
                        r["Donor_ID"].ToString() ?? "",
                        r["Donor_FullName"].ToString() ?? "",
                        r["LastMessage"].ToString() ?? "",
                        Convert.ToInt32(r["UnreadCount"])
                    ));
            }
            catch (Exception ex) { MessageBox.Show("GetChatDonors failed: " + ex.Message); }
            return list;
        }

        // ══════════════════════════════════════════════════════════════════════
        // PROFILE UPDATE
        // ══════════════════════════════════════════════════════════════════════

        public static async Task UpdateDonorProfile(string donorId, string newUsername, string profilePic)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_UpdateDonorProfile", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DonorId", donorId);
                cmd.Parameters.AddWithValue("@NewUsername", newUsername);
                cmd.Parameters.AddWithValue("@ProfilePic", profilePic);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("UpdateDonorProfile failed: " + ex.Message); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // FILE REPORTS
        // ══════════════════════════════════════════════════════════════════════

        public static void GenerateClaimReport(ClaimModel claim)
        {
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                           "KapwaKuhaData", "ClaimReports");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, $"Claim_{claim.Claim_ID}.txt");
                using var w = new StreamWriter(path, append: false);
                w.WriteLine("================================================");
                w.WriteLine("       KAPWAKUHA — ITEM CLAIM REPORT            ");
                w.WriteLine("================================================");
                w.WriteLine($"Claim ID        : {claim.Claim_ID}");
                w.WriteLine($"Claim Date      : {claim.Claim_Date:yyyy-MM-dd HH:mm:ss}");
                w.WriteLine("------------------------------------------------");
                w.WriteLine($"Item ID         : {claim.Item_ID}");
                w.WriteLine($"Item Name       : {claim.Item_Name}");
                w.WriteLine("------------------------------------------------");
                w.WriteLine($"Beneficiary ID  : {claim.Beneficiary_ID}");
                w.WriteLine($"Beneficiary     : {claim.Beneficiary_Name}");
                w.WriteLine("------------------------------------------------");
                w.WriteLine($"Handoff Type    : {claim.Handoff_Type}");
                w.WriteLine($"Status          : {claim.Claim_Status}");
                w.WriteLine($"Notes           : {claim.Verification_Notes}");
                w.WriteLine("================================================");
                w.WriteLine("  Kapwa — Together We Give What Others Need     ");
                w.WriteLine("================================================");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Claim report error: {ex.Message}",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void GenerateDonationReceipt(ClaimModel claim, string donorName)
        {
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                           "KapwaKuhaData", "DonorReceipts");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, $"Receipt_{claim.Claim_ID}.txt");
                using var w = new StreamWriter(path, append: false);
                w.WriteLine("================================================");
                w.WriteLine("       KAPWAKUHA — DONATION RECEIPT             ");
                w.WriteLine("================================================");
                w.WriteLine($"Receipt for     : {donorName}");
                w.WriteLine($"Claim ID        : {claim.Claim_ID}");
                w.WriteLine($"Date            : {claim.Claim_Date:yyyy-MM-dd HH:mm:ss}");
                w.WriteLine("------------------------------------------------");
                w.WriteLine($"Item Donated    : {claim.Item_Name}");
                w.WriteLine($"Received By     : {claim.Beneficiary_Name}");
                w.WriteLine($"Handoff Method  : {claim.Handoff_Type}");
                w.WriteLine("================================================");
                w.WriteLine("  Thank you for your generosity, Kapwa!         ");
                w.WriteLine("================================================");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Receipt error: {ex.Message}",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}