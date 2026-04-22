// FILE: KapwaDataService.cs
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

        /// <summary>
        /// Validates admin credentials against the Users table.
        /// Returns (true, userId) on success.
        /// </summary>
        public static async Task<(bool OK, string UserId, string FullName)>
            LoginAdmin(string userId, string password)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "SELECT UserID FROM Users WHERE UserID=@id AND Password=@pw AND Role='Admin'", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@pw", password);
                using var r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                    return (true, r["UserID"].ToString() ?? "", "Admin");
            }
            catch (Exception ex) { MessageBox.Show("LoginAdmin failed: " + ex.Message); }
            return (false, "", "");
        }

        /// <summary>
        /// Validates donor credentials. Login by Username (same as CarRentals Customer).
        /// Returns (true, userId, fullName, username) on success.
        /// </summary>
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

        // ══════════════════════════════════════════════════════════════════════
        // ITEMS  (Strong Entity — parallel to Cars)
        // ══════════════════════════════════════════════════════════════════════

        public static async Task<List<ItemModel>> GetAllItems()
        {
            var list = new List<ItemModel>();
            const string sql = @"
                SELECT i.Item_ID, i.Item_Name, i.Item_Condition, i.Item_Status, i.Date_Found,
                       i.Donor_ID, i.Category_ID,
                       d.Donor_FullName  AS Donor_Name,
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
                while (await r.ReadAsync())
                    list.Add(MapItem(r));
            }
            catch (Exception ex) { MessageBox.Show("GetAllItems failed: " + ex.Message); }
            return list;
        }

        public static async Task<List<ItemModel>> GetFoundItems()
        {
            var all = await GetAllItems();
            var found = new List<ItemModel>();
            foreach (var i in all)
                if (i.Item_Status == "Found") found.Add(i);
            return found;
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

        public static async Task AddItem(ItemModel item)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(@"
                    INSERT INTO Items(Item_ID,Item_Name,Item_Condition,Item_Status,Date_Found,Donor_ID,Category_ID)
                    VALUES(@id,@name,@cond,@status,@date,@did,@catid)", conn);
                cmd.Parameters.AddWithValue("@id", item.Item_ID);
                cmd.Parameters.AddWithValue("@name", item.Item_Name);
                cmd.Parameters.AddWithValue("@cond", item.Item_Condition);
                cmd.Parameters.AddWithValue("@status", item.Item_Status);
                cmd.Parameters.AddWithValue("@date", item.Date_Found);
                cmd.Parameters.AddWithValue("@did", item.Donor_ID);
                cmd.Parameters.AddWithValue("@catid", item.Category_ID);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { MessageBox.Show("AddItem failed: " + ex.Message); throw; }
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

        private static ItemModel MapItem(SqlDataReader r) => new()
        {
            Item_ID = r["Item_ID"].ToString() ?? "",
            Item_Name = r["Item_Name"].ToString() ?? "",
            Item_Condition = r["Item_Condition"].ToString() ?? "Unknown",
            Item_Status = r["Item_Status"].ToString() ?? "Lost",
            Date_Found = Convert.ToDateTime(r["Date_Found"]),
            Donor_ID = r["Donor_ID"].ToString() ?? "",
            Donor_Name = r["Donor_Name"].ToString() ?? "",
            Category_ID = r["Category_ID"].ToString() ?? "",
            Category_Name = r["Category_Name"].ToString() ?? ""
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
                       i.Item_Name,
                       b.Beneficiary_FName + ' ' + b.Beneficiary_LName AS Beneficiary_Name
                FROM Claims cl
                LEFT JOIN Items         i ON i.Item_ID          = cl.Item_ID
                LEFT JOIN Beneficiaries b ON b.Beneficiary_ID   = cl.Beneficiary_ID";
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(sql, conn);
                using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    list.Add(MapClaim(r));
            }
            catch (Exception ex) { MessageBox.Show("GetAllClaims failed: " + ex.Message); }
            return list;
        }

        public static async Task SaveClaim(ClaimModel claim)
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(@"
                    INSERT INTO Claims(Claim_ID,Item_ID,Beneficiary_ID,Claim_Date,Claim_Status,Verification_Notes)
                    VALUES(@cid,@iid,@bid,@date,@status,@notes)", conn);
                cmd.Parameters.AddWithValue("@cid", claim.Claim_ID);
                cmd.Parameters.AddWithValue("@iid", claim.Item_ID);
                cmd.Parameters.AddWithValue("@bid", claim.Beneficiary_ID);
                cmd.Parameters.AddWithValue("@date", claim.Claim_Date);
                cmd.Parameters.AddWithValue("@status", claim.Claim_Status);
                cmd.Parameters.AddWithValue("@notes", claim.Verification_Notes);
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
            Beneficiary_ID = r["Beneficiary_ID"].ToString() ?? "",
            Beneficiary_Name = r["Beneficiary_Name"].ToString() ?? "",
            Claim_Date = Convert.ToDateTime(r["Claim_Date"]),
            Claim_Status = r["Claim_Status"].ToString() ?? "Pending",
            Verification_Notes = r["Verification_Notes"].ToString() ?? ""
        };

        // ══════════════════════════════════════════════════════════════════════
        // BENEFICIARIES
        // ══════════════════════════════════════════════════════════════════════

        public static async Task<List<(string Id, string DisplayName)>> GetActiveBeneficiaries()
        {
            var list = new List<(string, string)>();
            const string sql = @"
                SELECT b.Beneficiary_ID,
                       b.Beneficiary_FName + ' ' + b.Beneficiary_LName + ' — ' + o.Organization_Name AS DisplayName
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
                    list.Add((r["Beneficiary_ID"].ToString() ?? "", r["DisplayName"].ToString() ?? ""));
            }
            catch (Exception ex) { MessageBox.Show("GetActiveBeneficiaries failed: " + ex.Message); }
            return list;
        }

        // ══════════════════════════════════════════════════════════════════════
        // IMPACT METRICS  (parallel to Revenue analytics)
        // ══════════════════════════════════════════════════════════════════════

        public static async Task<(int TotalReturned, int ActiveLost, double AvgStorageDays)>
            GetImpactMetrics()
        {
            try
            {
                using var conn = new SqlConnection(_conn);
                await conn.OpenAsync();

                using var c1 = new SqlCommand(
                    "SELECT COUNT(*) FROM Claims WHERE Claim_Status='Released'", conn);
                int returned = Convert.ToInt32(await c1.ExecuteScalarAsync());

                using var c2 = new SqlCommand(
                    "SELECT COUNT(*) FROM Items WHERE Item_Status='Lost'", conn);
                int lost = Convert.ToInt32(await c2.ExecuteScalarAsync());

                using var c3 = new SqlCommand(@"
                    SELECT ISNULL(AVG(CAST(DATEDIFF(day,i.Date_Found,cl.Claim_Date) AS FLOAT)),0)
                    FROM Claims cl JOIN Items i ON i.Item_ID=cl.Item_ID
                    WHERE cl.Claim_Status='Released'", conn);
                double avg = Convert.ToDouble(await c3.ExecuteScalarAsync());

                return (returned, lost, avg);
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetImpactMetrics failed: " + ex.Message);
                return (0, 0, 0);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // FILE REPORTS  (parallel to GenerateReturnReport / GenerateReceipt)
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
                w.WriteLine($"Status          : {claim.Claim_Status}");
                w.WriteLine($"Verification    : {claim.Verification_Notes}");
                w.WriteLine("================================================");
                w.WriteLine("  Kapwa — Together We Return What Was Lost      ");
                w.WriteLine("================================================");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Claim report error: {ex.Message}",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}