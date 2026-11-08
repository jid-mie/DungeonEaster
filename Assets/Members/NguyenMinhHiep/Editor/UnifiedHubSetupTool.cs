#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NguyenMinhHiep.Core;
using NguyenMinhHiep.Data;
using NguyenMinhHiep.UI;

namespace NguyenMinhHiep.Editor
{
    /// <summary>
    /// Công cụ Editor hỗ trợ Nguyễn Minh Hiệp tích hợp Unified Canvas Hub & SharedGameDataManager
    /// vào Scene hiện hành hoặc tự động gắn vào cả 3 Scene (Dungeon, Village, Restaurant).
    /// </summary>
    public static class UnifiedHubSetupTool
    {
        private static readonly string[] TargetScenePaths =
        {
            "Assets/Members/NguyenVanHai/Scenes/Dungeon.unity",
            "Assets/Members/NguyenVanHai/Scenes/Village.unity",
            "Assets/Members/NguyenVanHai/Scenes/Restaurant.unity",
            "Assets/Members/TranHuuHau/Scenes/Village_Hau.unity",
            "Assets/Members/NgoVanHien/Scenes/Restaurant_Hien.unity"
        };

        [MenuItem("DungeonEater/NguyenMinhHiep/1. Spawn Unified Canvas Hub (Scene Hien Tai)")]
        public static void SpawnUnifiedHubInActiveScene()
        {
            EnsureHubObjectsInCurrentScene(true);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[UnifiedHubSetup] ✅ Đã tạo SharedGameDataManager + UnifiedGameHubCanvas trong Scene hiện tại!");
        }

        [MenuItem("DungeonEater/NguyenMinhHiep/2. Tich Hop Unified Hub Vao Ca 3 Scene (Dungeon, Village, Restaurant)")]
        public static void IntegrateIntoAllThreeScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[UnifiedHubSetup] Đã hủy thao tác tích hợp hàng loạt do người dùng hủy lưu Scene hiện tại.");
                return;
            }

            string originalScene = EditorSceneManager.GetActiveScene().path;
            int count = 0;

            try
            {
                for (int i = 0; i < TargetScenePaths.Length; i++)
                {
                    string path = TargetScenePaths[i];
                    EditorUtility.DisplayProgressBar("Integrating Unified Hub...", path, (float)i / TargetScenePaths.Length);
                    if (!System.IO.File.Exists(path)) continue;

                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    EnsureHubObjectsInCurrentScene(false);
                    EditorSceneManager.SaveScene(scene);
                    count++;
                    Debug.Log($"[UnifiedHubSetup] ✅ Đã tích hợp Unified Canvas Hub vào Scene: {path}");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (!string.IsNullOrEmpty(originalScene) && System.IO.File.Exists(originalScene))
            {
                EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
            }

            Debug.Log($"[UnifiedHubSetup] 🎉 Hoàn tất tích hợp Unified Canvas Hub cho {count} Scene!");
        }

        [MenuItem("DungeonEater/NguyenMinhHiep/3. Kiem Tra Danh Muc JSON (items_catalog.json)")]
        public static void ValidateJsonItemCatalog()
        {
            GameItemDatabase.EnsureLoaded(true);
            var cat = GameItemDatabase.Catalog;
            int ingCount = GameItemDatabase.GetItemsByCategory("Ingredient").Count;
            int dishCount = GameItemDatabase.GetItemsByCategory("Dish").Count;
            int consCount = GameItemDatabase.GetItemsByCategory("Consumable").Count;
            int wpCount = cat.weapons != null ? cat.weapons.Count : 0;

            Debug.Log(
                $"[GameItemDatabase] ✅ Đọc thành công items_catalog.json (v{cat.version}):\n" +
                $"• Nguyên liệu (Bảng 7.1): {ingCount} vật phẩm\n" +
                $"• Món ăn (Bảng 7.2): {dishCount} món ăn\n" +
                $"• Tiêu hao chiến đấu (Bảng 7.7): {consCount} vật phẩm\n" +
                $"• Vũ khí (Bảng 7.5): {wpCount} vũ khí (Kiếm & Cung Lv.1-4)\n" +
                $"• Tổng cộng: {cat.items.Count} vật phẩm + {wpCount} vũ khí.");
        }

        private static void EnsureHubObjectsInCurrentScene(bool registerUndo = true)
        {
            var dataMgr = Object.FindObjectOfType<SharedGameDataManager>();
            if (dataMgr == null)
            {
                var dataGo = new GameObject("SharedGameDataManager_Hiep");
                dataMgr = dataGo.AddComponent<SharedGameDataManager>();
                if (registerUndo) Undo.RegisterCreatedObjectUndo(dataGo, "Create SharedGameDataManager");
            }

            var hub = Object.FindObjectOfType<UnifiedGameHubCanvas>();
            if (hub == null)
            {
                var hubGo = new GameObject("UnifiedGameHubCanvas_Hiep");
                hub = hubGo.AddComponent<UnifiedGameHubCanvas>();
                if (registerUndo) Undo.RegisterCreatedObjectUndo(hubGo, "Create UnifiedGameHubCanvas");
            }

            Selection.activeGameObject = hub.gameObject;
        }
    }
}
#endif
