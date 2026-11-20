using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DungeonEater.Tavern
{
    [Serializable]
    public class RestaurantWalletConfigDto
    {
        public int startingGold = 10000;
        public int startingReputation = 1000;
    }

    [Serializable]
    public class RestaurantItemAmountDto
    {
        public string itemId = "";
        public int amount = 1;
    }

    [Serializable]
    public class RestaurantInventoryConfigDto
    {
        public int bagSlotCount = 10;
        public int quickSlotCount = 5;
        public int defaultMaxStack = 5;
        public List<RestaurantItemAmountDto> startingItems = new List<RestaurantItemAmountDto>();
    }

    [Serializable]
    public class RestaurantShiftConfigDto
    {
        public bool startOnPlay = true;
        public float durationSeconds = 600f;
        public int maxCustomers = 20;
        public float spawnIntervalMin = 6f;
        public float spawnIntervalMax = 12f;
        public float firstCustomerDelay = 2f;
        public float retrySpawnDelay = 1f;
    }

    [Serializable]
    public class RestaurantCustomerConfigDto
    {
        public float moveSpeed = 2.6f;
        public float eatDuration = 4f;
        /// <summary>Còn >= tỉ lệ kiên nhẫn này khi nhận món thì khách boa thêm.</summary>
        public float tipPatienceThreshold = 0.5f;
        /// <summary>Tiền boa = % giá món.</summary>
        public float tipPercent = 0.2f;
        public int angryReputationPenalty = 3;
        public float defaultPatience = 45f;
    }

    [Serializable]
    public class RestaurantCustomerProfileDto
    {
        public string id = "";
        public string displayName = "Khách";
        /// <summary>Số giây khách chờ món trước khi bỏ về.</summary>
        public float patience = 45f;
    }

    [Serializable]
    public class RestaurantItemConfigDto
    {
        public string id = "";
        public string displayName = "";
        /// <summary>CommonIngredient | EliteMaterial | BossMaterial | Dish (khớp enum ItemCategory).</summary>
        public string category = "CommonIngredient";
        public int maxStack = 5;
        public int sellPrice = 0;
        public string description = "";
    }

    [Serializable]
    public class RestaurantRecipeConfigDto
    {
        public string id = "";
        public string dishId = "";
        public int goldPrice = 35;
        public int reputation = 5;
        public int outputAmount = 1;
        public bool unlocked = true;
        public List<RestaurantItemAmountDto> ingredients = new List<RestaurantItemAmountDto>();
    }

    [Serializable]
    public class RestaurantCatalogRoot
    {
        public string version = "1.0.0";
        public string description = "";
        public RestaurantWalletConfigDto wallet = new RestaurantWalletConfigDto();
        public RestaurantInventoryConfigDto inventory = new RestaurantInventoryConfigDto();
        public RestaurantShiftConfigDto shift = new RestaurantShiftConfigDto();
        public RestaurantCustomerConfigDto customer = new RestaurantCustomerConfigDto();
        public List<RestaurantCustomerProfileDto> customerProfiles = new List<RestaurantCustomerProfileDto>();
        public List<RestaurantItemConfigDto> items = new List<RestaurantItemConfigDto>();
        public List<RestaurantRecipeConfigDto> recipes = new List<RestaurantRecipeConfigDto>();
    }

    /// <summary>
    /// Bộ nạp dữ liệu JSON trung tâm cho scene Quán ăn (cùng kiến trúc với DungeonCombatDatabase của Hải):
    /// - Đọc từ: Assets/Members/NgoVanHien/Data/restaurant_config.json
    ///   (fallback Resources/Data/restaurant_config.json khi đóng gói Build).
    /// - Cung cấp API tĩnh cho các script của Hiến và Backend của Hiệp gọi trực tiếp.
    /// Ngoài số liệu, class còn giữ sổ đăng ký ItemData (asset giữ icon) theo id để các script tra chéo.
    /// </summary>
    public static class RestaurantDatabase
    {
        private const string DIRECT_JSON_RELATIVE_PATH = "Members/NgoVanHien/Data/restaurant_config.json";
        private const string RESOURCES_JSON_PATH = "Data/restaurant_config";

        private static bool isLoaded = false;
        private static RestaurantCatalogRoot catalog = new RestaurantCatalogRoot();
        private static readonly Dictionary<string, RestaurantItemConfigDto> itemById = new Dictionary<string, RestaurantItemConfigDto>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, RestaurantRecipeConfigDto> recipeByKey = new Dictionary<string, RestaurantRecipeConfigDto>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, RestaurantCustomerProfileDto> profileByKey = new Dictionary<string, RestaurantCustomerProfileDto>(StringComparer.OrdinalIgnoreCase);
        // ItemData asset (giữ icon) đã được ghi đè số liệu từ JSON, tra theo id
        private static readonly Dictionary<string, ItemData> itemAssets = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);

        public static RestaurantCatalogRoot Catalog
        {
            get
            {
                EnsureLoaded();
                return catalog;
            }
        }

        public static RestaurantWalletConfigDto Wallet => Catalog.wallet ?? new RestaurantWalletConfigDto();
        public static RestaurantInventoryConfigDto Inventory => Catalog.inventory ?? new RestaurantInventoryConfigDto();
        public static RestaurantShiftConfigDto Shift => Catalog.shift ?? new RestaurantShiftConfigDto();
        public static RestaurantCustomerConfigDto Customer => Catalog.customer ?? new RestaurantCustomerConfigDto();

        public static void EnsureLoaded(bool forceReload = false)
        {
            if (isLoaded && !forceReload) return;

            string jsonText = null;
            string fullFilePath = Path.Combine(Application.dataPath, DIRECT_JSON_RELATIVE_PATH);

            if (File.Exists(fullFilePath))
            {
                try
                {
                    jsonText = File.ReadAllText(fullFilePath);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[RestaurantDatabase] Không thể đọc file trực tiếp {fullFilePath}: {ex.Message}");
                }
            }

            if (string.IsNullOrEmpty(jsonText))
            {
                TextAsset ta = Resources.Load<TextAsset>(RESOURCES_JSON_PATH);
                if (ta != null)
                {
                    jsonText = ta.text;
                }
            }

            if (!string.IsNullOrEmpty(jsonText))
            {
                try
                {
                    var parsed = JsonUtility.FromJson<RestaurantCatalogRoot>(jsonText);
                    if (parsed != null)
                    {
                        catalog = parsed;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[RestaurantDatabase] Lỗi phân tích JSON restaurant_config.json: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[RestaurantDatabase] Không tìm thấy restaurant_config.json, dùng giá trị mặc định.");
            }

            RebuildLookupTables();
            isLoaded = true;
        }

        private static void RebuildLookupTables()
        {
            itemById.Clear();
            recipeByKey.Clear();
            profileByKey.Clear();
            itemAssets.Clear();

            if (catalog.items != null)
            {
                foreach (var it in catalog.items)
                {
                    if (it == null || string.IsNullOrEmpty(it.id)) continue;
                    itemById[it.id.Trim()] = it;
                }
            }

            if (catalog.recipes != null)
            {
                foreach (var r in catalog.recipes)
                {
                    if (r == null) continue;
                    // tra được bằng cả id công thức lẫn id món tạo ra
                    if (!string.IsNullOrEmpty(r.id)) recipeByKey[r.id.Trim()] = r;
                    if (!string.IsNullOrEmpty(r.dishId)) recipeByKey[r.dishId.Trim()] = r;
                }
            }

            if (catalog.customerProfiles != null)
            {
                foreach (var p in catalog.customerProfiles)
                {
                    if (p == null) continue;
                    // tra được bằng cả id lẫn tên hiển thị (profile trong scene chỉ có tên)
                    if (!string.IsNullOrEmpty(p.id)) profileByKey[p.id.Trim()] = p;
                    if (!string.IsNullOrEmpty(p.displayName)) profileByKey[p.displayName.Trim()] = p;
                }
            }
        }

        public static RestaurantItemConfigDto GetItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureLoaded();
            return itemById.TryGetValue(id.Trim(), out var dto) ? dto : null;
        }

        /// <summary>Công thức theo id công thức (recipe_skewer) hoặc id món tạo ra (dish_skewer).</summary>
        public static RestaurantRecipeConfigDto GetRecipe(string recipeOrDishId)
        {
            if (string.IsNullOrEmpty(recipeOrDishId)) return null;
            EnsureLoaded();
            return recipeByKey.TryGetValue(recipeOrDishId.Trim(), out var dto) ? dto : null;
        }

        /// <summary>Loại thực khách theo id (farmer) hoặc tên hiển thị (Nông dân).</summary>
        public static RestaurantCustomerProfileDto GetCustomerProfile(string idOrDisplayName)
        {
            if (string.IsNullOrEmpty(idOrDisplayName)) return null;
            EnsureLoaded();
            return profileByKey.TryGetValue(idOrDisplayName.Trim(), out var dto) ? dto : null;
        }

        // ------------------------------------------------------------------ Ghi đè lên asset trong scene

        /// <summary>
        /// Ghi đè số liệu của 1 ItemData asset bằng dữ liệu JSON (tra theo id) rồi đăng ký để tra chéo.
        /// Asset chỉ còn giữ id + icon; tên, mô tả, loại, stack, giá đều theo restaurant_config.json.
        /// </summary>
        public static void RegisterItemAsset(ItemData item)
        {
            if (item == null || string.IsNullOrEmpty(item.id)) return;
            EnsureLoaded();
            if (itemAssets.TryGetValue(item.id, out var known) && known == item) return;

            var dto = GetItem(item.id);
            if (dto != null)
            {
                item.displayName = dto.displayName;
                item.description = dto.description;
                item.maxStack = Mathf.Max(1, dto.maxStack);
                item.sellPrice = Mathf.Max(0, dto.sellPrice);
                if (Enum.TryParse(dto.category, true, out ItemCategory category)) item.category = category;
                else Debug.LogWarning($"[RestaurantDatabase] Vật phẩm '{dto.id}' có category không hợp lệ: '{dto.category}'.");
            }
            else
            {
                Debug.LogWarning($"[RestaurantDatabase] Vật phẩm '{item.id}' chưa có trong restaurant_config.json, giữ số liệu trong asset.");
            }
            itemAssets[item.id] = item;
        }

        /// <summary>ItemData asset đã đăng ký, tra theo id trong JSON.</summary>
        public static ItemData GetItemAsset(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return itemAssets.TryGetValue(id.Trim(), out var item) ? item : null;
        }

        /// <summary>
        /// Ghi đè giá bán, danh tiếng, số lượng tạo ra, trạng thái mở khóa và nguyên liệu của 1 RecipeData asset
        /// bằng dữ liệu JSON (tra theo id món). Gọi sau khi mọi ItemData liên quan đã RegisterItemAsset.
        /// </summary>
        public static void ApplyRecipe(RecipeData recipe)
        {
            if (recipe == null || recipe.dish == null) return;
            var dto = GetRecipe(recipe.dish.id);
            if (dto == null)
            {
                Debug.LogWarning($"[RestaurantDatabase] Công thức của món '{recipe.dish.id}' chưa có trong restaurant_config.json, giữ số liệu trong asset.");
                return;
            }

            recipe.goldPrice = Mathf.Max(0, dto.goldPrice);
            recipe.reputation = Mathf.Max(0, dto.reputation);
            recipe.outputAmount = Mathf.Max(1, dto.outputAmount);
            recipe.unlocked = dto.unlocked;

            if (dto.ingredients == null || dto.ingredients.Count == 0) return;
            var ingredients = new List<ItemAmount>();
            foreach (var ing in dto.ingredients)
            {
                var asset = ing != null ? GetItemAsset(ing.itemId) : null;
                if (asset == null || ing.amount <= 0)
                {
                    Debug.LogWarning($"[RestaurantDatabase] Công thức '{dto.id}': không tìm thấy ItemData cho nguyên liệu '{(ing != null ? ing.itemId : "null")}', giữ nguyên liệu trong asset.");
                    return;
                }
                ingredients.Add(new ItemAmount(asset, ing.amount));
            }
            recipe.ingredients = ingredients;
        }
    }
}
