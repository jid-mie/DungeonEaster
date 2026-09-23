using UnityEngine;
using System;
using System.Collections.Generic;
using DungeonEater.Core;

namespace DungeonEater.Restaurant
{
    [Serializable]
    public class Recipe
    {
        public string id;
        public string name;
        public string description;
        public int goldPrice;
        public int repBonus;
        public List<IngredientRequirement> ingredients;
    }

    [Serializable]
    public class IngredientRequirement
    {
        public IngredientType type;
        public int count;
    }

    public class CookingSystem : MonoBehaviour
    {
        public static CookingSystem Instance { get; private set; }

        [Header("Recipe Catalog")]
        [SerializeField] private List<Recipe> recipes = new List<Recipe>();
        public List<Recipe> Recipes => recipes;

        // Lưu các món đã nấu chín sẵn sàng bưng phục vụ
        private Dictionary<string, int> preparedDishes = new Dictionary<string, int>();

        public event Action OnCooked;

        private void Awake()
        {
            Instance = this;
            InitializeDefaultRecipes();
        }

        private void InitializeDefaultRecipes()
        {
            recipes.Clear();

            // Món 1: Thịt xiên nướng quái thú
            recipes.Add(new Recipe
            {
                id = "skewer",
                name = "Thịt xiên nướng dã thú",
                description = "Thịt quái nướng than hồng thơm lừng béo ngậy.",
                goldPrice = 25,
                repBonus = 5,
                ingredients = new List<IngredientRequirement>
                {
                    new IngredientRequirement { type = IngredientType.MonsterMeat, count = 1 }
                }
            });

            // Món 2: Súp nấm hang sâu
            recipes.Add(new Recipe
            {
                id = "soup",
                name = "Súp nấm hang sâu",
                description = "Nước súp thanh mát từ nấm hầm ngục giúp hồi phục sinh lực.",
                goldPrice = 35,
                repBonus = 8,
                ingredients = new List<IngredientRequirement>
                {
                    new IngredientRequirement { type = IngredientType.CaveMushroom, count = 2 }
                }
            });

            // Món 3: Bít tết bùng nổ
            recipes.Add(new Recipe
            {
                id = "steak",
                name = "Bít tết bùng nổ",
                description = "Thịt quái áp chảo bằng nhiệt hạch của Bọ phát nổ, vị cay tê rần!",
                goldPrice = 65,
                repBonus = 15,
                ingredients = new List<IngredientRequirement>
                {
                    new IngredientRequirement { type = IngredientType.MonsterMeat, count = 1 },
                    new IngredientRequirement { type = IngredientType.BombCore, count = 1 }
                }
            });

            // Món 4: Đại tiệc Vua Thú
            recipes.Add(new Recipe
            {
                id = "feast",
                name = "Đại tiệc Vua Thú",
                description = "Sườn heo rừng hoàng gia hảo hạng cực hiếm, món ăn đỉnh cao làng Dun.",
                goldPrice = 160,
                repBonus = 35,
                ingredients = new List<IngredientRequirement>
                {
                    new IngredientRequirement { type = IngredientType.BossRib, count = 1 },
                    new IngredientRequirement { type = IngredientType.MonsterMeat, count = 2 },
                    new IngredientRequirement { type = IngredientType.CaveMushroom, count = 1 }
                }
            });

            foreach (var r in recipes)
            {
                preparedDishes[r.id] = 0;
            }
        }

        public bool CanCook(Recipe recipe)
        {
            if (InventoryManager.Instance == null) return false;
            foreach (var req in recipe.ingredients)
            {
                if (InventoryManager.Instance.GetItemCount(req.type) < req.count)
                {
                    return false;
                }
            }
            return true;
        }

        public bool Cook(string recipeId)
        {
            var recipe = recipes.Find(r => r.id == recipeId);
            if (recipe == null || !CanCook(recipe))
            {
                Debug.LogWarning($"[CookingSystem] Không đủ nguyên liệu để nấu món: {recipe?.name}");
                return false;
            }

            // Trừ nguyên liệu trong kho
            foreach (var req in recipe.ingredients)
            {
                InventoryManager.Instance.ConsumeItem(req.type, req.count);
            }

            preparedDishes[recipeId] = GetPreparedCount(recipeId) + 1;
            SoundManager.Instance?.PlayCook();
            Debug.Log($"🍳 [CookingSystem] Nấu thành công {recipe.name}! Hiện có {preparedDishes[recipeId]} đĩa sẵn sàng phục vụ.");
            OnCooked?.Invoke();
            return true;
        }

        public int GetPreparedCount(string recipeId)
        {
            return preparedDishes.ContainsKey(recipeId) ? preparedDishes[recipeId] : 0;
        }

        public bool ConsumePreparedDish(string recipeId)
        {
            if (GetPreparedCount(recipeId) <= 0) return false;
            preparedDishes[recipeId]--;
            OnCooked?.Invoke();
            return true;
        }
    }
}

