#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NguyenMinhHiep.Bosses;
using NguyenMinhHiep.UI;

namespace NguyenMinhHiep.Editor
{
    /// <summary>
    /// Công cụ hỗ trợ Editor cho Nguyễn Minh Hiệp:
    /// - 1 click spawn ngay lập tức bất kỳ Boss nào vào Scene hiện hành.
    /// - Tự động thiết lập Sprite, Collider2D, Rigidbody2D, Sorting Layer và thanh máu BossHealthBarUI.
    /// - 1 click khởi tạo Scene test riêng biệt "BossArena_Hiep.unity" để thử nghiệm toàn diện.
    /// </summary>
    public static class BossSpawnerTool
    {
        private const string SpriteDir = "Assets/Members/NguyenMinhHiep/Sprites";

        [MenuItem("DungeonEater/NguyenMinhHiep/Spawn Boss Tang 1 (Ran Khong Lo)")]
        public static void SpawnGiantSnake()
        {
            EnsureBossHealthBar();
            var bossGo = new GameObject("Boss_GiantSnake");
            bossGo.tag = "Enemy";
            bossGo.transform.position = GetSpawnPosition();

            var sr = bossGo.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/Boss_GiantSnake.png");
            sr.sortingOrder = 3000;

            var col = bossGo.AddComponent<CircleCollider2D>();
            col.radius = 0.9f;

            var rb = bossGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            bossGo.AddComponent<GiantSnakeBoss>();

            Selection.activeGameObject = bossGo;
            Undo.RegisterCreatedObjectUndo(bossGo, "Spawn Giant Snake Boss");
            Debug.Log("[BossSpawner] ✅ Đã spawn Boss Tầng 1: Rắn Khổng Lồ (450 HP)!");
        }

        [MenuItem("DungeonEater/NguyenMinhHiep/Spawn Boss Tang 3 (Nguoi Da Khong Lo)")]
        public static void SpawnStoneGolem()
        {
            EnsureBossHealthBar();
            var bossGo = new GameObject("Boss_StoneGolem");
            bossGo.tag = "Enemy";
            bossGo.transform.position = GetSpawnPosition();

            var sr = bossGo.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/Boss_StoneGolem.png");
            sr.sortingOrder = 3000;

            var col = bossGo.AddComponent<CircleCollider2D>();
            col.radius = 1.1f;

            var rb = bossGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            bossGo.AddComponent<StoneGolemBoss>();

            Selection.activeGameObject = bossGo;
            Undo.RegisterCreatedObjectUndo(bossGo, "Spawn Stone Golem Boss");
            Debug.Log("[BossSpawner] ✅ Đã spawn Boss Tầng 3: Người Đá Khổng Lồ (1200 HP)!");
        }

        [MenuItem("DungeonEater/NguyenMinhHiep/Spawn Boss Tang 4 (Doi Hiep Si)")]
        public static void SpawnDualKnights()
        {
            EnsureBossHealthBar();
            Vector3 center = GetSpawnPosition();

            var rootGo = new GameObject("Boss_DualKnights");
            rootGo.transform.position = center;
            var dualBoss = rootGo.AddComponent<DualKnightsBoss>();

            // 1. Hiệp sĩ Kiếm
            var k1Go = new GameObject("Knight_Sword");
            k1Go.transform.SetParent(rootGo.transform, false);
            k1Go.transform.localPosition = new Vector3(-1.8f, 0, 0);
            k1Go.tag = "Enemy";

            var sr1 = k1Go.AddComponent<SpriteRenderer>();
            sr1.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/Boss_SwordKnight.png");
            sr1.sortingOrder = 3000;

            var col1 = k1Go.AddComponent<CircleCollider2D>();
            col1.radius = 0.65f;

            var rb1 = k1Go.AddComponent<Rigidbody2D>();
            rb1.gravityScale = 0f;
            rb1.constraints = RigidbodyConstraints2D.FreezeRotation;

            k1Go.AddComponent<SwordKnightMember>();

            // 2. Hiệp sĩ Giáo
            var k2Go = new GameObject("Knight_Spear");
            k2Go.transform.SetParent(rootGo.transform, false);
            k2Go.transform.localPosition = new Vector3(1.8f, 0, 0);
            k2Go.tag = "Enemy";

            var sr2 = k2Go.AddComponent<SpriteRenderer>();
            sr2.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/Boss_SpearKnight.png");
            sr2.sortingOrder = 3000;

            var col2 = k2Go.AddComponent<CircleCollider2D>();
            col2.radius = 0.65f;

            var rb2 = k2Go.AddComponent<Rigidbody2D>();
            rb2.gravityScale = 0f;
            rb2.constraints = RigidbodyConstraints2D.FreezeRotation;

            k2Go.AddComponent<SpearKnightMember>();

            Selection.activeGameObject = rootGo;
            Undo.RegisterCreatedObjectUndo(rootGo, "Spawn Dual Knights Boss");
            Debug.Log("[BossSpawner] ✅ Đã spawn Boss Tầng 4: Đôi Hiệp Sĩ Cấm Vệ (600 HP x2)!");
        }

        [MenuItem("DungeonEater/NguyenMinhHiep/Spawn Boss Tang 5 (Gwyn Than Sang Tao)")]
        public static void SpawnGwyn()
        {
            EnsureBossHealthBar();
            var bossGo = new GameObject("Boss_Gwyn_Final");
            bossGo.tag = "Enemy";
            bossGo.transform.position = GetSpawnPosition();

            var sr = bossGo.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/Boss_Gwyn.png");
            sr.sortingOrder = 3000;

            var col = bossGo.AddComponent<CircleCollider2D>();
            col.radius = 1.0f;

            var rb = bossGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            bossGo.AddComponent<GwynFinalBoss>();

            Selection.activeGameObject = bossGo;
            Undo.RegisterCreatedObjectUndo(bossGo, "Spawn Gwyn Final Boss");
            Debug.Log("[BossSpawner] ✅ Đã spawn Boss Tầng 5: Gwyn Thần Sáng Tạo (2500 HP)!");
        }

        [MenuItem("DungeonEater/NguyenMinhHiep/Create Boss Testing Arena Scene")]
        public static void CreateBossArenaScene()
        {
            string scenePath = "Assets/Members/NguyenMinhHiep/Scenes/BossArena_Hiep.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 1. Tạo sàn đấu Arena
            var arenaRoot = new GameObject("Arena_Environment");
            var floorGo = new GameObject("Floor_Tile");
            floorGo.transform.SetParent(arenaRoot.transform, false);

            // 2. Tạo Camera chuẩn 2D
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.orthographic = true;
                mainCam.orthographicSize = 8.5f;
                mainCam.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
            }

            // 3. Tạo Boss Health Bar UI
            EnsureBossHealthBar();

            // 4. Lưu Scene
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[BossSpawner] ✅ Đã tạo Scene Đấu Trường Thử Nghiệm Boss tại: {scenePath}");
        }

        private static void EnsureBossHealthBar()
        {
            if (Object.FindObjectOfType<BossHealthBarUI>() == null)
            {
                var uiGo = new GameObject("BossHealthBarUI");
                uiGo.AddComponent<BossHealthBarUI>();
                Undo.RegisterCreatedObjectUndo(uiGo, "Create BossHealthBarUI");
            }
        }

        private static Vector3 GetSpawnPosition()
        {
            if (SceneView.lastActiveSceneView != null)
            {
                Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
                return new Vector3(camPos.x, camPos.y, 0f);
            }
            return Vector3.zero;
        }
    }
}
#endif

