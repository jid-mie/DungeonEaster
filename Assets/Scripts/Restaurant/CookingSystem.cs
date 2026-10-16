using UnityEngine;
using System;
using System.Collections.Generic;
using DungeonEater.Core;

namespace DungeonEater.Restaurant
{
    /// <summary>
    /// Chất lượng món sau khi nấu. Nấu nhanh luôn ra "Ngon"; minigame có thể ra "Hoàn hảo" hoặc "Cháy khét".
    /// </summary>
    public enum DishQuality
    {
        Burnt = 0,   // ★    Cháy khét  — bán 60% giá
        Good = 1,    // ★★   Ngon       — giá gốc
        Perfect = 2  // ★★★  Hoàn hảo   — bán 150% giá
    }

    /// <summary>Loại thử thách của một bước nấu trong minigame bếp.</summary>
    public enum CookStepType
    {
        Chop,    // Thái / chặt: bấm liên tục để đầy thanh trước khi hết giờ
        Season,  // Nêm / ướp: gõ đúng chuỗi phím mũi tên
        Heat,    // Áp chảo / nướng / hầm: dừng con trỏ trong vùng HOÀN HẢO
        Plate    // Bày đĩa: giống Heat nhưng con trỏ nhanh và vùng hẹp hơn
    }

    [Serializable]
    public class CookStep
    {
        public CookStepType type;
        public string label;

        public CookStep(CookStepType type, string label)
        {
            this.type = type;
            this.label = label;
        }
    }

    [Serializable]
    public class Recipe
    {
        public string id;
        public string name;
        public string description;
        public int goldPrice;
        public int repBonus;
        [Range(1, 4)] public int difficulty = 1;
        public List<IngredientRequirement> ingredients;
        public List<CookStep> steps;
    }

    [Serializable]
    public class IngredientRequirement
    {
        public IngredientType type;
        public int count;
    }

    public class CookingSystem : MonoBehaviour
    {
        private static CookingSystem instance;
        private static bool isQuitting;

        /// <summary>
        /// Singleton bền qua các scene. Core_GameManagers của RestaurantLevel bị GameManager hủy khi
        /// đi từ màn khác vào (bản trùng lặp), nên nếu chưa có thì tự tạo một CookingSystem riêng.
        /// Nhờ vậy các đĩa đã nấu cũng được giữ lại khi ra làng / vào hầm ngục rồi quay lại.
        /// </summary>
        public static CookingSystem Instance
        {
            get
            {
                if (instance == null && Application.isPlaying && !isQuitting)
                {
                    var go = new GameObject("CookingSystem");
                    instance = go.AddComponent<CookingSystem>();
                }
                return instance;
            }
        }

        [Header("Recipe Catalog")]
        [SerializeField] private List<Recipe> recipes = new List<Recipe>();
        public List<Recipe> Recipes => recipes;

        // Các đĩa đã nấu chín sẵn sàng bưng ra, mỗi đĩa giữ chất lượng riêng
        private readonly Dictionary<string, List<DishQuality>> preparedDishes = new Dictionary<string, List<DishQuality>>();

        public event Action OnCooked;

        private void Awake()
        {
            // Object này là Core trùng lặp sắp bị GameManager hủy -> không đăng ký
            var gm = GameManager.Instance;
            bool doomed = gm != null && gm.gameObject != gameObject && GetComponent<GameManager>() != null;
            if ((instance != null && instance != this) || doomed)
            {
                Destroy(this);
                return;
            }

            instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
            Application.quitting += () => isQuitting = true;
            InitializeDefaultRecipes();
        }

        private void InitializeDefaultRecipes()
        {
            recipes.Clear();

            // Bảng công thức theo tài liệu thiết kế (README mục 5)
            recipes.Add(new Recipe
            {
                id = "skewer",
                name = "Thịt xiên nướng quái thú",
                description = "Thịt quái thái miếng, ướp gia vị rồi nướng trên than hồng. Món nhập môn của quán.",
                goldPrice = 35,
                repBonus = 5,
                difficulty = 1,
                ingredients = new List<IngredientRequirement>
                {
                    new IngredientRequirement { type = IngredientType.MonsterMeat, count = 2 }
                },
                steps = new List<CookStep>
                {
                    new CookStep(CookStepType.Chop, "Thái thịt"),
                    new CookStep(CookStepType.Season, "Ướp gia vị"),
                    new CookStep(CookStepType.Heat, "Nướng than hồng"),
                    new CookStep(CookStepType.Plate, "Bày đĩa")
                }
            });

            recipes.Add(new Recipe
            {
                id = "soup",
                name = "Bát súp nấm hang sâu",
                description = "Nấm phát quang hầm lửa nhỏ, nước súp ngọt thanh giúp hồi phục sinh lực.",
                goldPrice = 45,
                repBonus = 8,
                difficulty = 2,
                ingredients = new List<IngredientRequirement>
                {
                    new IngredientRequirement { type = IngredientType.CaveMushroom, count = 2 }
                },
                steps = new List<CookStep>
                {
                    new CookStep(CookStepType.Chop, "Thái nấm"),
                    new CookStep(CookStepType.Heat, "Hầm lửa nhỏ"),
                    new CookStep(CookStepType.Season, "Nêm nếm"),
                    new CookStep(CookStepType.Plate, "Múc ra bát")
                }
            });

            recipes.Add(new Recipe
            {
                id = "steak",
                name = "Bít tết bùng nổ",
                description = "Thịt quái áp chảo bằng nhiệt của Lõi bọ nổ, vị cay tê rần đầu lưỡi!",
                goldPrice = 85,
                repBonus = 15,
                difficulty = 3,
                ingredients = new List<IngredientRequirement>
                {
                    new IngredientRequirement { type = IngredientType.MonsterMeat, count = 2 },
                    new IngredientRequirement { type = IngredientType.BombCore, count = 1 }
                },
                steps = new List<CookStep>
                {
                    new CookStep(CookStepType.Chop, "Lọc thịt"),
                    new CookStep(CookStepType.Season, "Rắc bột lõi bọ"),
                    new CookStep(CookStepType.Heat, "Áp chảo bùng nổ"),
                    new CookStep(CookStepType.Plate, "Bày đĩa")
                }
            });

            recipes.Add(new Recipe
            {
                id = "feast",
                name = "Đại tiệc Vua Thú",
                description = "Sườn Vua Lợn Rừng quay lửa lớn ăn kèm nấm hầm — món đỉnh cao của làng Dun.",
                goldPrice = 220,
                repBonus = 35,
                difficulty = 4,
                ingredients = new List<IngredientRequirement>
                {
                    new IngredientRequirement { type = IngredientType.BossRib, count = 1 },
                    new IngredientRequirement { type = IngredientType.CaveMushroom, count = 2 },
                    new IngredientRequirement { type = IngredientType.MonsterMeat, count = 2 }
                },
                steps = new List<CookStep>
                {
                    new CookStep(CookStepType.Chop, "Chặt sườn"),
                    new CookStep(CookStepType.Season, "Tẩm mật & thảo mộc"),
                    new CookStep(CookStepType.Heat, "Quay lửa lớn"),
                    new CookStep(CookStepType.Heat, "Hầm nấm"),
                    new CookStep(CookStepType.Plate, "Bày tiệc")
                }
            });

            foreach (var r in recipes)
            {
                preparedDishes[r.id] = new List<DishQuality>();
            }
        }

        public Recipe GetRecipe(string recipeId) => recipes.Find(r => r.id == recipeId);

        public bool CanCook(Recipe recipe) => GetMaxCookable(recipe) > 0;

        /// <summary>Số đĩa tối đa có thể nấu với tồn kho hiện tại.</summary>
        public int GetMaxCookable(Recipe recipe)
        {
            if (recipe == null || InventoryManager.Instance == null) return 0;
            int max = int.MaxValue;
            foreach (var req in recipe.ingredients)
            {
                if (req.count <= 0) continue;
                max = Mathf.Min(max, InventoryManager.Instance.GetItemCount(req.type) / req.count);
            }
            return max == int.MaxValue ? 0 : max;
        }

        /// <summary>Trừ nguyên liệu cho một mẻ nấu. Trả về false nếu không đủ.</summary>
        public bool ConsumeIngredients(Recipe recipe, int portions)
        {
            if (recipe == null || portions <= 0 || GetMaxCookable(recipe) < portions) return false;
            foreach (var req in recipe.ingredients)
            {
                InventoryManager.Instance.ConsumeItem(req.type, req.count * portions);
            }
            return true;
        }

        /// <summary>Thêm một đĩa đã nấu xong (nguyên liệu phải được trừ trước đó).</summary>
        public void AddPreparedDish(string recipeId, DishQuality quality)
        {
            if (!preparedDishes.ContainsKey(recipeId)) preparedDishes[recipeId] = new List<DishQuality>();
            preparedDishes[recipeId].Add(quality);
            OnCooked?.Invoke();
        }

        /// <summary>Nấu nhanh (không minigame): trừ nguyên liệu và ra món chất lượng "Ngon".</summary>
        public bool Cook(string recipeId)
        {
            var recipe = GetRecipe(recipeId);
            if (!ConsumeIngredients(recipe, 1))
            {
                Debug.LogWarning($"[CookingSystem] Không đủ nguyên liệu để nấu món: {recipe?.name}");
                return false;
            }

            SoundManager.Instance?.PlayCook();
            AddPreparedDish(recipeId, DishQuality.Good);
            Debug.Log($"🍳 [CookingSystem] Nấu thành công {recipe.name}! Hiện có {GetPreparedCount(recipeId)} đĩa sẵn sàng phục vụ.");
            return true;
        }

        public int GetPreparedCount(string recipeId)
        {
            return preparedDishes.TryGetValue(recipeId, out var list) ? list.Count : 0;
        }

        public int GetTotalPreparedCount()
        {
            int total = 0;
            foreach (var kvp in preparedDishes) total += kvp.Value.Count;
            return total;
        }

        /// <summary>Lấy ra đĩa ngon nhất của món này để bưng cho khách.</summary>
        public bool ConsumePreparedDish(string recipeId, out DishQuality quality)
        {
            quality = DishQuality.Good;
            if (!preparedDishes.TryGetValue(recipeId, out var list) || list.Count == 0) return false;

            int best = 0;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i] > list[best]) best = i;
            }
            quality = list[best];
            list.RemoveAt(best);
            OnCooked?.Invoke();
            return true;
        }

        public bool ConsumePreparedDish(string recipeId) => ConsumePreparedDish(recipeId, out _);

        public static float GoldMultiplier(DishQuality q)
        {
            switch (q)
            {
                case DishQuality.Perfect: return 1.5f;
                case DishQuality.Burnt: return 0.6f;
                default: return 1f;
            }
        }

        public static float RepMultiplier(DishQuality q)
        {
            switch (q)
            {
                case DishQuality.Perfect: return 1.5f;
                case DishQuality.Burnt: return 0.4f;
                default: return 1f;
            }
        }

        public static string QualityName(DishQuality q)
        {
            switch (q)
            {
                case DishQuality.Perfect: return "Hoàn hảo";
                case DishQuality.Burnt: return "Cháy khét";
                default: return "Ngon";
            }
        }
    }
}
