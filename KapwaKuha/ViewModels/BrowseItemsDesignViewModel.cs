using KapwaKuha.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KapwaKuha.ViewModels
{
    public class BrowseItemsDesignViewModel
    {
        public ObservableCollection<ItemModel> Items { get; } = new()
        {
            new ItemModel { Item_ID="ITEM001", Item_Name="School Bag",      Item_Condition="Good",  Item_Status="Available", Category_Name="School Supplies", Donor_Name="Juan DC", PostType="GeneralPost" },
            new ItemModel { Item_ID="ITEM002", Item_Name="Winter Blanket",  Item_Condition="New",   Item_Status="Available", Category_Name="Clothing",        Donor_Name="Maria S",  PostType="GeneralPost" },
            new ItemModel { Item_ID="ITEM003", Item_Name="Canned Goods Box",Item_Condition="Good",  Item_Status="Available", Category_Name="Food",            Donor_Name="Pedro R",  PostType="GeneralPost" },
            new ItemModel { Item_ID="ITEM004", Item_Name="Vitamins Pack",   Item_Condition="New",   Item_Status="Reserved",  Category_Name="Medicine",        Donor_Name="Liza M",   PostType="DirectTarget" },
        };
        public string StatusMessage { get; } = "4 item(s) available.";
        public string FilterCategory { get; } = "All";
        public bool IsBusy { get; } = false;
    }
}
