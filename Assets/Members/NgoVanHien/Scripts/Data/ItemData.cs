using UnityEngine;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Phân loại vật phẩm trong hòm đồ theo GDD (mục 3.2.2 Inventory / Economy):
    /// Common Ingredient, Elite Material, Boss Material thu thập trong dungeon + món ăn đã nấu.
    /// </summary>
    public enum ItemCategory
    {
        CommonIngredient,
        EliteMaterial,
        BossMaterial,
        Dish
    }

    /// <summary>
    /// Dữ liệu 1 loại vật phẩm (nguyên liệu hoặc món ăn). Tạo bằng menu
    /// Create > Dungeon Eater > Tavern > Item, hoặc chạy tool Setup để sinh tự động.
    /// </summary>
    [CreateAssetMenu(menuName = "Dungeon Eater/Tavern/Item", fileName = "Item_")]
    public class ItemData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public ItemCategory category;
        public Sprite icon;
        [Tooltip("Số lượng tối đa xếp chồng trong 1 ô hòm đồ")]
        [Min(1)] public int maxStack = 5;

        public bool IsDish => category == ItemCategory.Dish;

        public string CategoryLabel
        {
            get
            {
                switch (category)
                {
                    case ItemCategory.CommonIngredient: return "Nguyên liệu thường";
                    case ItemCategory.EliteMaterial: return "Nguyên liệu tinh anh";
                    case ItemCategory.BossMaterial: return "Nguyên liệu Boss";
                    case ItemCategory.Dish: return "Món ăn";
                    default: return category.ToString();
                }
            }
        }
    }
}
