using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NguyenMinhHiep.Data
{
    [Serializable]
    public class InventoryConfigDto
    {
        public int defaultBagSlots = 10;
        public int bonusSlotsPerBoss = 5;
        public int quickSlotCount = 4;
        public float defaultMaxHP = 100f;
        public float defaultMaxStamina = 100f;
        public int defaultStartingGold = 500;
        public int defaultStartingReputation = 20;
    }

    [Serializable]
    public class RecipeIngredientDto
    {
        public string itemId;
        public int count;
    }

    [Serializable]
    public class ItemDefinitionDto
    {
        public string id;
        public string[] aliases;
        public string displayName;
        public string category;       // "Ingredient", "Dish", "Consumable"
        public string subCategory;    // "Nguyên liệu", "Nguyên liệu Boss", "Món ăn Thường", "Món Boss", "Hồi phục", ...
        public string rarity;         // "Thường", "Hiếm", "Hiếm (Boss)", "Huyền thoại"
        public string floor;          // "Tầng 1", ...
        public string dropSource;
        public int maxStack = 10;
        public int buyPrice;
        public int sellPrice;
        public int healAmount;
        public int damageAmount;
        public float effectRadius;
        public float effectDuration;
        public string unlockCondition;
        public string iconPath;
        public string fallbackColorHex = "#FFD24A";
        public string description;
        public RecipeIngredientDto[] recipeIngredients;

        [NonSerialized] private Sprite cachedIcon;

        public bool IsConsumable => string.Equals(category, "Consumable", StringComparison.OrdinalIgnoreCase);
        public bool IsDish => string.Equals(category, "Dish", StringComparison.OrdinalIgnoreCase);
        public bool IsIngredient => string.Equals(category, "Ingredient", StringComparison.OrdinalIgnoreCase);

        public Sprite GetIcon()
        {
            if (cachedIcon != null) return cachedIcon;
            cachedIcon = GameItemDatabase.ResolveSprite(id, iconPath, fallbackColorHex, category);
            return cachedIcon;
        }
    }

    [Serializable]
    public class WeaponLevelDto
    {
        public int level = 1;
        public int lightDamage = 15;
        public int lightComboFinisherDamage = 25;
        public int heavyMinDamage = 20;
        public int heavyDamage = 40;
        public int skillDamage = 60;
        public int upgradeCostGold = 0;
        public int requiredBossFloor = 0;
        public string upgradeRequirement = "Mặc định đầu game";
        public RecipeIngredientDto[] upgradeMaterials;
    }

    [Serializable]
    public class WeaponDefinitionDto
    {
        public string id;
        public string displayName;
        public string weaponType;     // "Sword" | "Bow"
        public string hotKey = "Q";
        public string iconPath;
        public string fallbackColorHex = "#F59E0B";
        public string lightAttackName;
        public string heavyAttackName;
        public string skillAttackName;
        public string description;
        public WeaponLevelDto[] levels;

        [NonSerialized] private Sprite cachedIcon;

        public WeaponLevelDto GetLevelStats(int level)
        {
            if (levels == null || levels.Length == 0)
            {
                return new WeaponLevelDto { level = 1, lightDamage = 15, heavyDamage = 30, skillDamage = 40 };
            }
            int idx = Mathf.Clamp(level - 1, 0, levels.Length - 1);
            return levels[idx];
        }

        public Sprite GetIcon()
        {
            if (cachedIcon != null) return cachedIcon;
            cachedIcon = GameItemDatabase.ResolveSprite(id, iconPath, fallbackColorHex, "Weapon");
            return cachedIcon;
        }
    }

    [Serializable]
    public class ItemCatalogRoot
    {
        public string version;
        public string sourceReport;
        public InventoryConfigDto inventoryConfig = new InventoryConfigDto();
        public List<ItemDefinitionDto> items = new List<ItemDefinitionDto>();
        public List<WeaponDefinitionDto> weapons = new List<WeaponDefinitionDto>();
    }

    /// <summary>
    /// Cơ sở dữ liệu vật phẩm & vũ khí toàn game đọc từ file JSON:
    /// Assets/Members/NguyenMinhHiep/Data/items_catalog.json (và Resources/Data/items_catalog.json).
    /// Tuân thủ chính xác các bảng trong báo cáo TaiLieu.md:
    /// - Bảng 7.1: 13 Nguyên liệu (Tầng 1 -> Tầng 5)
    /// - Bảng 7.2: 10 Món ăn (Tầng 1 -> Tầng 5)
    /// - Bảng 7.5: 2 Vũ khí (Kiếm & Cung, Lv.1 -> Lv.4)
    /// - Bảng 7.7: 4 Vật phẩm chiến đấu & tiêu hao (Bình máu, Lựu đạn nổ, Lựu đạn băng, Lựu đạn độc)
    /// </summary>
    public static class GameItemDatabase
    {
        private static ItemCatalogRoot catalog;
        private static readonly Dictionary<string, ItemDefinitionDto> itemByIdOrAlias = new Dictionary<string, ItemDefinitionDto>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, WeaponDefinitionDto> weaponByIdOrType = new Dictionary<string, WeaponDefinitionDto>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Sprite> fallbackIcons = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static bool isInitialized;

        public static ItemCatalogRoot Catalog
        {
            get
            {
                EnsureLoaded();
                return catalog;
            }
        }

        public static IReadOnlyList<ItemDefinitionDto> AllItems
        {
            get
            {
                EnsureLoaded();
                return catalog.items;
            }
        }

        public static IReadOnlyList<WeaponDefinitionDto> AllWeapons
        {
            get
            {
                EnsureLoaded();
                return catalog.weapons;
            }
        }

        public static void EnsureLoaded(bool forceReload = false)
        {
            if (isInitialized && !forceReload && catalog != null) return;

            string jsonText = LoadJsonText();
            if (!string.IsNullOrEmpty(jsonText))
            {
                try
                {
                    catalog = JsonUtility.FromJson<ItemCatalogRoot>(jsonText);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GameItemDatabase] Lỗi phân tích JSON items_catalog.json: {ex.Message}");
                }
            }

            if (catalog == null)
            {
                catalog = new ItemCatalogRoot();
            }
            if (catalog.inventoryConfig == null)
            {
                catalog.inventoryConfig = new InventoryConfigDto();
            }
            if (catalog.items == null)
            {
                catalog.items = new List<ItemDefinitionDto>();
            }
            if (catalog.weapons == null)
            {
                catalog.weapons = new List<WeaponDefinitionDto>();
            }

            RebuildLookupTables();
            isInitialized = true;
        }

        private static string LoadJsonText()
        {
            // 1. Ưu tiên đọc trực tiếp từ thư mục Data của NguyenMinhHiep để dễ chỉnh sửa JSON
            string directPath = Path.Combine(Application.dataPath, "Members/NguyenMinhHiep/Data/items_catalog.json");
            if (File.Exists(directPath))
            {
                return File.ReadAllText(directPath);
            }

            // 2. Đọc từ Resources/Data/items_catalog
            var textAsset = Resources.Load<TextAsset>("Data/items_catalog");
            if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
            {
                return textAsset.text;
            }

            // 3. Đọc từ đường dẫn Resources trong thư mục NguyenMinhHiep
            string resPath = Path.Combine(Application.dataPath, "Members/NguyenMinhHiep/Resources/Data/items_catalog.json");
            if (File.Exists(resPath))
            {
                return File.ReadAllText(resPath);
            }

            Debug.LogWarning("[GameItemDatabase] Không tìm thấy file items_catalog.json trên ổ đĩa!");
            return null;
        }

        private static void RebuildLookupTables()
        {
            itemByIdOrAlias.Clear();
            weaponByIdOrType.Clear();

            foreach (var item in catalog.items)
            {
                if (item == null || string.IsNullOrEmpty(item.id)) continue;
                itemByIdOrAlias[item.id] = item;

                if (item.aliases != null)
                {
                    foreach (var alias in item.aliases)
                    {
                        if (!string.IsNullOrEmpty(alias))
                        {
                            itemByIdOrAlias[alias] = item;
                        }
                    }
                }
            }

            foreach (var wp in catalog.weapons)
            {
                if (wp == null || string.IsNullOrEmpty(wp.id)) continue;
                weaponByIdOrType[wp.id] = wp;
                if (!string.IsNullOrEmpty(wp.weaponType))
                {
                    weaponByIdOrType[wp.weaponType] = wp;
                }
            }
        }

        /// <summary>
        /// Tra cứu định nghĩa vật phẩm theo ID chuẩn hoặc ID bí danh (alias) từ các scene khác.
        /// </summary>
        public static ItemDefinitionDto GetItem(string idOrAlias)
        {
            if (string.IsNullOrEmpty(idOrAlias)) return null;
            EnsureLoaded();
            return itemByIdOrAlias.TryGetValue(idOrAlias.Trim(), out var def) ? def : null;
        }

        /// <summary>
        /// Chuyển đổi bất kỳ ID vật phẩm nào (ví dụ "cave_mushroom", "mat_mushroom") về ID chuẩn trong JSON ("ing_glow_mushroom").
        /// </summary>
        public static string NormalizeItemId(string idOrAlias)
        {
            var def = GetItem(idOrAlias);
            return def != null ? def.id : idOrAlias;
        }

        /// <summary>
        /// Tra cứu định nghĩa Vũ khí theo ID ("weapon_sword", "weapon_bow") hoặc loại ("Sword", "Bow").
        /// </summary>
        public static WeaponDefinitionDto GetWeapon(string idOrType)
        {
            if (string.IsNullOrEmpty(idOrType)) return null;
            EnsureLoaded();
            return weaponByIdOrType.TryGetValue(idOrType.Trim(), out var wp) ? wp : null;
        }

        public static List<ItemDefinitionDto> GetItemsByCategory(string category)
        {
            EnsureLoaded();
            var list = new List<ItemDefinitionDto>();
            foreach (var item in catalog.items)
            {
                if (string.Equals(item.category, category, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public static Sprite ResolveSprite(string id, string iconPath, string fallbackColorHex, string category)
        {
            Sprite sp = null;

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(iconPath))
            {
                sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            }
#endif
            if (sp == null)
            {
                sp = CreateFallbackIcon(id, fallbackColorHex, category);
            }

            return sp;
        }

        private static Sprite CreateFallbackIcon(string id, string hexColor, string category)
        {
            string cacheKey = $"{id}_{hexColor}_{category}";
            if (fallbackIcons.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            if (!ColorUtility.TryParseHtmlString(hexColor, out var baseColor))
            {
                baseColor = new Color(0.9f, 0.75f, 0.3f, 1f);
            }

            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = $"ItemIcon_{id}"
            };
            var pixels = new Color32[size * size];
            var border = new Color(0.12f, 0.08f, 0.06f, 1f);
            var highlight = Color.Lerp(baseColor, Color.white, 0.45f);
            var shadow = Color.Lerp(baseColor, Color.black, 0.35f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - 7.5f;
                    float dy = y - 7.5f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    Color color = distance > 6.8f ? Color.clear : distance > 5.6f ? border :
                        x + y > 16 && distance < 4.5f ? highlight : x + y < 11 ? shadow : baseColor;
                    pixels[y * size + x] = color;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            sprite.name = $"ItemIcon_{id}";
            fallbackIcons[cacheKey] = sprite;
            return sprite;
        }
    }
}
