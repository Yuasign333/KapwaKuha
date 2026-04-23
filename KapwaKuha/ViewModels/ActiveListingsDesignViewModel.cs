using KapwaKuha.Models;
using System.Collections.ObjectModel;

public class ActiveListingsDesignViewModel
{
    public ObservableCollection<ItemModel> Items { get; } = new()
        {
            new ItemModel { Item_ID="ITEM001", Item_Name="School Bag",      Item_Status="Available", Category_Name="School Supplies", Item_Condition="Good", PostType="GeneralPost" },
            new ItemModel { Item_ID="ITEM002", Item_Name="Winter Blanket",  Item_Status="Claimed",   Category_Name="Clothing",        Item_Condition="New",  PostType="DirectTarget" },
            new ItemModel { Item_ID="ITEM003", Item_Name="Canned Goods Box",Item_Status="Reserved",  Category_Name="Food",            Item_Condition="Good", PostType="GeneralPost" },
        };
    public string StatusMessage { get; } = "3 item(s) posted.";
    public bool IsItemSelected { get; } = false;
    public bool IsBusy { get; } = false;
}