using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DungeonEater.Tavern;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonEater.Tavern.EditorTools
{
    /// <summary>
    /// Tool dựng hệ thống quán ăn của Hiến:
    /// 1. Sinh ItemData / RecipeData (số liệu theo README mục 5).
    /// 2. Copy map Restaurant của Hải sang scene riêng rồi gắn: bàn (TavernTable), bếp (CookingStation),
    ///    cửa + đường đi của khách, PlayerWallet/PlayerInventory (mock 10000 vàng / 1000 danh tiếng), UI.
    /// Chạy lại nhiều lần không bị nhân đôi.
    /// </summary>
    public static class TavernSceneSetup
    {
        private const string Root = "Assets/Members/NgoVanHien";
        private const string IconDir = Root + "/Sprites/Icons";
        private const string DataDir = Root + "/Data";
        private const string SceneDir = Root + "/Scenes";
        private const string SourceScene = "Assets/Members/NguyenVanHai/Scenes/Restaurant.unity";
        public const string TargetScene = SceneDir + "/Restaurant_Hien.unity";
        private const string CustomerSpriteDir = "Assets/Sprites/Detailed";

        // Map của Hải: cửa là khe tường góc dưới phải; lối đi ngang nằm giữa 2 hàng bàn
        private static readonly Vector3 DoorPosition = new Vector3(7f, -6.7f, 0f);
        private static readonly Vector3 InsideDoorPosition = new Vector3(7f, -5.6f, 0f);
        private const float AisleY = -2.75f;
        private const float ApproachOffsetX = 1.05f;

        [MenuItem("Dungeon Eater/Hiến - Quán ăn/Tạo scene Restaurant_Hien (từ map của Hải)")]
        public static void CreateSceneFromMap()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildEverything();
            Debug.Log($"[TavernSetup] Xong. Mở scene {TargetScene} và bấm Play.");
        }

        [MenuItem("Dungeon Eater/Hiến - Quán ăn/Setup scene đang mở")]
        public static void SetupOpenScene()
        {
            var data = CreateData();
            SetupScene(SceneManager.GetActiveScene(), data);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        [MenuItem("Dungeon Eater/Hiến - Quán ăn/Chỉ tạo lại dữ liệu Item + Recipe")]
        public static void CreateDataMenu()
        {
            CreateData();
        }

        /// <summary>Dùng cho Unity -batchmode -executeMethod.</summary>
        public static void BatchBuild()
        {
            BuildEverything();
        }

        private static void BuildEverything()
        {
            var data = CreateData();

            Directory.CreateDirectory(SceneDir);
            if (!File.Exists(TargetScene))
            {
                if (!AssetDatabase.CopyAsset(SourceScene, TargetScene))
                {
                    Debug.LogError($"[TavernSetup] Không copy được {SourceScene}");
                    return;
                }
                AssetDatabase.Refresh();
            }

            var scene = EditorSceneManager.OpenScene(TargetScene, OpenSceneMode.Single);
            SetupScene(scene, data);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        // ------------------------------------------------------------------ DATA

        private class TavernData
        {
            public readonly Dictionary<string, ItemData> items = new Dictionary<string, ItemData>();
            public readonly List<RecipeData> recipes = new List<RecipeData>();
            public Sprite goldIcon;
            public Sprite reputationIcon;
        }

        private static TavernData CreateData()
        {
            ConfigureIconImport();
            Directory.CreateDirectory(DataDir + "/Items");
            Directory.CreateDirectory(DataDir + "/Recipes");

            var data = new TavernData
            {
                goldIcon = LoadIcon("Icon_Gold"),
                reputationIcon = LoadIcon("Icon_Reputation")
            };

            // Nguyên liệu (README mục 4: loot từ quái)
            data.items["meat"] = Item("Item_MonsterMeat", "monster_meat", "Thịt quái dã thú", ItemCategory.CommonIngredient, 10, "Icon_MonsterMeat",
                "Thịt rơi từ Slime xanh. Nguyên liệu cơ bản của mọi món nướng.");
            data.items["mushroom"] = Item("Item_CaveMushroom", "cave_mushroom", "Nấm hang sâu", ItemCategory.CommonIngredient, 10, "Icon_CaveMushroom",
                "Nấm bào tử phát sáng trong hầm ngục, nấu súp rất thơm.");
            data.items["bomb"] = Item("Item_BombCore", "bomb_core", "Lõi bọ nổ", ItemCategory.CommonIngredient, 10, "Icon_BombCore",
                "Lõi nhiệt hạch của Bọ cảm tử. Tạo vị cay nổ tung trong miệng.");
            data.items["rib"] = Item("Item_BossRib", "boss_rib", "Sườn Boss Hoàng Kim", ItemCategory.BossMaterial, 5, "Icon_BossRib",
                "Sườn Vua Lợn Rừng cực hiếm, chỉ rơi khi hạ Boss.");

            // Món ăn
            data.items["skewer"] = Item("Dish_Skewer", "dish_skewer", "Thịt xiên nướng quái thú", ItemCategory.Dish, 5, "Icon_Dish_Skewer",
                "Thịt quái nướng than hồng thơm lừng béo ngậy.");
            data.items["soup"] = Item("Dish_Soup", "dish_soup", "Bát súp nấm hang sâu", ItemCategory.Dish, 5, "Icon_Dish_Soup",
                "Nước súp thanh mát từ nấm hầm ngục, ấm bụng dân làng.");
            data.items["steak"] = Item("Dish_Steak", "dish_steak", "Bít tết bùng nổ", ItemCategory.Dish, 5, "Icon_Dish_Steak",
                "Thịt quái áp chảo bằng nhiệt của lõi bọ nổ, cay tê đầu lưỡi!");
            data.items["feast"] = Item("Dish_Feast", "dish_feast", "Đại tiệc Vua Thú", ItemCategory.Dish, 5, "Icon_Dish_Feast",
                "Sườn Boss hoàng kim cùng nấm và thịt quái — món đỉnh cao của làng Dun.");

            // Công thức + giá theo README mục 5
            data.recipes.Add(Recipe("Recipe_Skewer", data.items["skewer"], 35, 5, (data.items["meat"], 2)));
            data.recipes.Add(Recipe("Recipe_Soup", data.items["soup"], 45, 8, (data.items["mushroom"], 2)));
            data.recipes.Add(Recipe("Recipe_Steak", data.items["steak"], 85, 15, (data.items["meat"], 2), (data.items["bomb"], 1)));
            data.recipes.Add(Recipe("Recipe_Feast", data.items["feast"], 220, 35, (data.items["rib"], 1), (data.items["mushroom"], 2), (data.items["meat"], 2)));

            AssetDatabase.SaveAssets();
            return data;
        }

        private static void ConfigureIconImport()
        {
            AssetDatabase.Refresh();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { IconDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;
                if (importer.textureType == TextureImporterType.Sprite && importer.filterMode == FilterMode.Point) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static Sprite LoadIcon(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{IconDir}/{fileName}.png");
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static ItemData Item(string assetName, string id, string displayName, ItemCategory category, int maxStack, string icon, string description)
        {
            var item = LoadOrCreate<ItemData>($"{DataDir}/Items/{assetName}.asset");
            item.id = id;
            item.displayName = displayName;
            item.category = category;
            item.maxStack = maxStack;
            item.icon = LoadIcon(icon);
            item.description = description;
            EditorUtility.SetDirty(item);
            return item;
        }

        private static RecipeData Recipe(string assetName, ItemData dish, int gold, int reputation, params (ItemData item, int amount)[] ingredients)
        {
            var recipe = LoadOrCreate<RecipeData>($"{DataDir}/Recipes/{assetName}.asset");
            recipe.dish = dish;
            recipe.goldPrice = gold;
            recipe.reputation = reputation;
            recipe.outputAmount = 1;
            recipe.unlocked = true;
            recipe.ingredients = ingredients.Select(i => new ItemAmount(i.item, i.amount)).ToList();
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        // ------------------------------------------------------------------ SCENE

        private static void SetupScene(Scene scene, TavernData data)
        {
            var all = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToList();

            SetupTables(all);
            SetupStove(all);
            SetupPlayer(all);

            // SceneAutoBuilder (prototype cũ trong Assets/Scripts/Setup) tự dựng player thợ săn + DungeonManager
            // + CameraFollow trong mọi scene thiếu GameManager -> quái đánh player ẩn, camera rung liên tục.
            // Đặt sẵn 1 GameManager để nó bỏ qua scene này.
            var legacy = FindOrCreate(scene, "LegacyGameManager (chặn SceneAutoBuilder)", Vector3.zero);
            GetOrAdd<DungeonEater.Core.GameManager>(legacy);

            var door = FindOrCreate(scene, "Tavern_Door", DoorPosition);
            var inside = FindOrCreate(scene, "Tavern_InsideDoor", InsideDoorPosition);
            var aisle = FindOrCreate(scene, "Tavern_Aisle", new Vector3(InsideDoorPosition.x, AisleY, 0f));
            door.transform.position = DoorPosition;
            inside.transform.position = InsideDoorPosition;
            aisle.transform.position = new Vector3(InsideDoorPosition.x, AisleY, 0f);

            // Dữ liệu người chơi: sống xuyên scene
            var playerData = FindOrCreate(scene, "TavernPlayerData", Vector3.zero);
            var wallet = GetOrAdd<PlayerWallet>(playerData);
            var inventory = GetOrAdd<PlayerInventory>(playerData);
            var invSo = new SerializedObject(inventory);
            var starting = invSo.FindProperty("startingItems");
            starting.ClearArray();
            AddStarting(starting, data.items["meat"], 10);
            AddStarting(starting, data.items["meat"], 10);
            AddStarting(starting, data.items["mushroom"], 10);
            AddStarting(starting, data.items["mushroom"], 10);
            AddStarting(starting, data.items["bomb"], 5);
            AddStarting(starting, data.items["rib"], 3);
            AddStarting(starting, data.items["soup"], 2);
            AddStarting(starting, data.items["skewer"], 2);
            invSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wallet);

            var systems = FindOrCreate(scene, "TavernSystems", Vector3.zero);
            var kitchen = GetOrAdd<Kitchen>(systems);
            var kitchenSo = new SerializedObject(kitchen);
            var recipes = kitchenSo.FindProperty("recipes");
            recipes.ClearArray();
            foreach (var r in data.recipes)
            {
                recipes.InsertArrayElementAtIndex(recipes.arraySize);
                recipes.GetArrayElementAtIndex(recipes.arraySize - 1).objectReferenceValue = r;
            }
            kitchenSo.ApplyModifiedPropertiesWithoutUndo();

            var shift = GetOrAdd<TavernShift>(systems);
            var shiftSo = new SerializedObject(shift);
            shiftSo.FindProperty("spawnPoint").objectReferenceValue = door.transform;
            var waypoints = shiftSo.FindProperty("entryWaypoints");
            waypoints.ClearArray();
            waypoints.InsertArrayElementAtIndex(0);
            waypoints.GetArrayElementAtIndex(0).objectReferenceValue = inside.transform;
            waypoints.InsertArrayElementAtIndex(1);
            waypoints.GetArrayElementAtIndex(1).objectReferenceValue = aisle.transform;
            var profiles = shiftSo.FindProperty("profiles");
            profiles.ClearArray();
            AddProfile(profiles, "Nông dân", "Farmer", 50f);
            AddProfile(profiles, "Thương nhân", "Merchant", 35f);
            AddProfile(profiles, "Mạo hiểm gia", "Adventurer", 40f);
            AddProfile(profiles, "Cô gái làng", "Villager", 45f);
            AddProfile(profiles, "Bà lão", "OldLady", 60f);
            AddProfile(profiles, "Lữ khách bí ẩn", "Traveler", 30f);
            shiftSo.ApplyModifiedPropertiesWithoutUndo();

            var ui = GetOrAdd<TavernUI>(systems);
            var uiSo = new SerializedObject(ui);
            uiSo.FindProperty("goldIcon").objectReferenceValue = data.goldIcon;
            uiSo.FindProperty("reputationIcon").objectReferenceValue = data.reputationIcon;
            uiSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetupTables(List<GameObject> all)
        {
            var tableRegex = new Regex(@"^Table\d*$");
            var tables = all.Where(g => tableRegex.IsMatch(g.name) && g.GetComponent<Collider2D>() != null)
                // đánh số từ hàng trên xuống, trái sang phải
                .OrderByDescending(g => Mathf.Round(g.transform.position.y * 2f))
                .ThenBy(g => g.transform.position.x)
                .ToList();

            int number = 1;
            foreach (var go in tables)
            {
                var table = GetOrAdd<TavernTable>(go);
                var sr = go.GetComponent<SpriteRenderer>();
                Bounds b = sr != null ? sr.bounds : go.GetComponent<Collider2D>().bounds;

                // Khách đứng phía sau bàn (bàn che phần chân) -> trông như đang ngồi bàn, đĩa đặt giữa mặt bàn
                float seatY = b.center.y + 0.05f;
                var seat = FindOrCreateChild(go.transform, "SeatPoint", new Vector3(b.center.x, seatY, 0f));
                var dish = FindOrCreateChild(go.transform, "DishPoint", new Vector3(b.center.x, b.center.y - 0.3f, 0f));
                // từ lối đi giữa 2 hàng bàn -> khe trống bên phải bàn -> vào chỗ ngồi
                float sideX = b.center.x + ApproachOffsetX;
                var approachAisle = FindOrCreateChild(go.transform, "ApproachAisle", new Vector3(sideX, AisleY, 0f));
                var approachSide = FindOrCreateChild(go.transform, "ApproachSide", new Vector3(sideX, seatY, 0f));

                var so = new SerializedObject(table);
                so.FindProperty("tableNumber").intValue = number++;
                so.FindProperty("seatPoint").objectReferenceValue = seat;
                so.FindProperty("dishPoint").objectReferenceValue = dish;
                var approach = so.FindProperty("approachPath");
                approach.ClearArray();
                approach.InsertArrayElementAtIndex(0);
                approach.GetArrayElementAtIndex(0).objectReferenceValue = approachAisle;
                approach.InsertArrayElementAtIndex(1);
                approach.GetArrayElementAtIndex(1).objectReferenceValue = approachSide;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            Debug.Log($"[TavernSetup] Gắn TavernTable cho {tables.Count} bàn.");
        }

        private static void SetupStove(List<GameObject> all)
        {
            var stove = all.FirstOrDefault(g => g.name.ToLowerInvariant() == "stove");
            if (stove == null)
            {
                Debug.LogWarning("[TavernSetup] Không tìm thấy object 'stove' để làm quầy nấu.");
                return;
            }
            GetOrAdd<CookingStation>(stove);
        }

        private static void SetupPlayer(List<GameObject> all)
        {
            int layer = LayerMask.NameToLayer("Interactable");
            foreach (var go in all)
            {
                var player = go.GetComponent<ChiefPlayer>();
                if (player == null) continue;
                // Prefab ChiefPlayer đang để Interactable Layer = Nothing -> F không bắt được gì.
                // Override trong scene này (không sửa prefab của Hải).
                var so = new SerializedObject(player);
                so.FindProperty("interactableLayer").intValue = 1 << layer;
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.RecordPrefabInstancePropertyModifications(player);
            }
        }

        private static void AddStarting(SerializedProperty list, ItemData item, int amount)
        {
            list.InsertArrayElementAtIndex(list.arraySize);
            var el = list.GetArrayElementAtIndex(list.arraySize - 1);
            el.FindPropertyRelative("item").objectReferenceValue = item;
            el.FindPropertyRelative("amount").intValue = amount;
        }

        private static void AddProfile(SerializedProperty list, string displayName, string spriteKey, float patience)
        {
            list.InsertArrayElementAtIndex(list.arraySize);
            var el = list.GetArrayElementAtIndex(list.arraySize - 1);
            el.FindPropertyRelative("displayName").stringValue = displayName;
            el.FindPropertyRelative("idleSprite").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>($"{CustomerSpriteDir}/Customer_{spriteKey}_idle.png");
            el.FindPropertyRelative("eatingSprite").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>($"{CustomerSpriteDir}/Customer_{spriteKey}_eating.png");
            el.FindPropertyRelative("patience").floatValue = patience;
        }

        private static GameObject FindOrCreate(Scene scene, string name, Vector3 position)
        {
            var existing = scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
            if (existing != null) return existing;
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.position = position;
            return go;
        }

        private static Transform FindOrCreateChild(Transform parent, string name, Vector3 worldPosition)
        {
            var child = parent.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                child.SetParent(parent, false);
            }
            child.position = worldPosition;
            return child;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }
    }
}
