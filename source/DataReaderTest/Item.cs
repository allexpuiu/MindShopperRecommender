using System;
using System.Collections.Generic;
using System.Text;

namespace DataReaderTest
{
    public class Item
    {
        public Item(string itemId, string itemName, string categoryCode, string category, decimal salesValue, int itemRank, decimal recommendationScore = 0)
        {
            ItemId = itemId;
            ItemName = itemName;
            CategoryCode = categoryCode;
            Category = category;
            SalesValue = salesValue;
            ItemRank = itemRank;
            RecommendationScore = recommendationScore;
        }

        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string CategoryCode { get; set; }
        public string Category { get; set; }
        public decimal SalesValue { get; set; }
        public int ItemRank { get; set; }
        public decimal RecommendationScore { get; set; }

        public override string ToString()
        {
            return $"ItemId: {ItemId}\nItemName: {ItemName}\nCategoryCode: {CategoryCode}\nCategory: {Category}\nSalesValue: {SalesValue}\nItemRank: {ItemRank}\nRecommendationScore: {RecommendationScore}";
        }
    }
}
