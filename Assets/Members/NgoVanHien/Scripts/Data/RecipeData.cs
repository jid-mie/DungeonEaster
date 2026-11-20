using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonEater.Tavern
{
    [Serializable]
    public struct ItemAmount
    {
        public ItemData item;
        [Min(1)] public int amount;

        public ItemAmount(ItemData item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }
    }

    /// <summary>
    /// Công thức nấu: nguyên liệu tiêu hao -> 1 món ăn, kèm giá bán và danh tiếng (README mục 5).
    /// </summary>
    [CreateAssetMenu(menuName = "Dungeon Eater/Tavern/Recipe", fileName = "Recipe_")]
    public class RecipeData : ScriptableObject
    {
        [Tooltip("Món ăn tạo ra (ItemData loại Dish)")]
        public ItemData dish;
        public List<ItemAmount> ingredients = new List<ItemAmount>();
        [Tooltip("Số món tạo ra cho mỗi lần nấu")]
        [Min(1)] public int outputAmount = 1;
        // Giá bán + danh tiếng: ghi đè từ restaurant_config.json lúc chạy (RestaurantDatabase.ApplyRecipe)
        [Min(0)] public int goldPrice;
        [Min(0)] public int reputation;
        [Tooltip("Recipe đã mở khóa chưa (mock: mở hết để debug)")]
        public bool unlocked = true;

        public string DisplayName => dish != null ? dish.displayName : name;
        public Sprite Icon => dish != null ? dish.icon : null;
    }
}
