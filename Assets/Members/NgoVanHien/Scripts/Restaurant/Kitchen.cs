using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonEater.Tavern
{
    public enum CookCheck
    {
        Ok,
        Locked,
        InvalidQuantity,
        NotEnoughIngredients,
        InventoryFull
    }

    /// <summary>
    /// Sổ công thức + logic nấu. GDD 3.2.2: chọn recipe đã mở và đủ nguyên liệu, chọn số lượng,
    /// xem trước nguyên liệu tiêu hao và doanh thu dự kiến, tạo món ngay (minigame mở rộng sau).
    /// Món nấu xong được cho thẳng vào hòm đồ.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Kitchen : MonoBehaviour
    {
        public static Kitchen Instance { get; private set; }

        [SerializeField] private List<RecipeData> recipes = new List<RecipeData>();

        public IReadOnlyList<RecipeData> Recipes => recipes;

        /// <summary>(recipe, số lần nấu)</summary>
        public event Action<RecipeData, int> OnCooked;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public List<RecipeData> GetUnlockedRecipes()
        {
            return recipes.FindAll(r => r != null && r.unlocked && r.dish != null);
        }

        public CookCheck Check(RecipeData recipe, int quantity)
        {
            if (recipe == null || recipe.dish == null) return CookCheck.InvalidQuantity;
            if (!recipe.unlocked) return CookCheck.Locked;
            if (quantity <= 0) return CookCheck.InvalidQuantity;

            var inventory = PlayerInventory.Instance;
            if (inventory == null) return CookCheck.NotEnoughIngredients;

            foreach (var ing in recipe.ingredients)
            {
                if (ing.item == null) continue;
                if (inventory.Count(ing.item) < ing.amount * quantity) return CookCheck.NotEnoughIngredients;
            }

            if (!inventory.CanExchange(ScaledIngredients(recipe, quantity), new ItemAmount(recipe.dish, recipe.outputAmount * quantity)))
            {
                return CookCheck.InventoryFull;
            }
            return CookCheck.Ok;
        }

        /// <summary>Số lần nấu tối đa với nguyên liệu và chỗ trống hiện có.</summary>
        public int MaxCookable(RecipeData recipe)
        {
            if (recipe == null || !recipe.unlocked || PlayerInventory.Instance == null) return 0;

            int byIngredients = int.MaxValue;
            foreach (var ing in recipe.ingredients)
            {
                if (ing.item == null || ing.amount <= 0) continue;
                byIngredients = Mathf.Min(byIngredients, PlayerInventory.Instance.Count(ing.item) / ing.amount);
            }
            if (byIngredients == int.MaxValue) byIngredients = 99;

            for (int q = byIngredients; q > 0; q--)
            {
                if (Check(recipe, q) == CookCheck.Ok) return q;
            }
            return 0;
        }

        public bool Cook(RecipeData recipe, int quantity)
        {
            if (Check(recipe, quantity) != CookCheck.Ok) return false;

            bool ok = PlayerInventory.Instance.TryExchange(
                ScaledIngredients(recipe, quantity),
                new ItemAmount(recipe.dish, recipe.outputAmount * quantity));
            if (!ok) return false;

            if (DungeonEater.Core.SoundManager.Instance != null) DungeonEater.Core.SoundManager.Instance.PlayCook();
            OnCooked?.Invoke(recipe, quantity);
            return true;
        }

        public static List<ItemAmount> ScaledIngredients(RecipeData recipe, int quantity)
        {
            var list = new List<ItemAmount>();
            foreach (var ing in recipe.ingredients)
            {
                if (ing.item == null) continue;
                list.Add(new ItemAmount(ing.item, ing.amount * quantity));
            }
            return list;
        }

        public static string Describe(CookCheck check)
        {
            switch (check)
            {
                case CookCheck.Ok: return "Có thể nấu";
                case CookCheck.Locked: return "Chưa mở khóa";
                case CookCheck.NotEnoughIngredients: return "Thiếu nguyên liệu";
                case CookCheck.InventoryFull: return "Hòm đồ không đủ chỗ";
                default: return "Số lượng không hợp lệ";
            }
        }
    }
}
