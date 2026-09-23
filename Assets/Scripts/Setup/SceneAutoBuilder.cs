using UnityEngine;
using DungeonEater.Core;
using DungeonEater.Dungeon;
using DungeonEater.Restaurant;
using DungeonEater.Upgrades;
using DungeonEater.UI;

namespace DungeonEater.Setup
{
    /// <summary>
    /// Script hỗ trợ tự động khởi tạo toàn bộ GameObject, Canvas UI, Player và Managers
    /// khi mở Scene trong Unity Editor mà chưa kịp kéo thả thủ công!
    /// Giúp người chơi / giảng viên bấm PLAY là trải nghiệm được ngay lập tức.
    /// </summary>
    public class SceneAutoBuilder : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoSetupSceneIfEmpty()
        {
            if (FindObjectOfType<GameManager>() != null) return; // Đã setup trước đó

            Debug.Log("🚀 [SceneAutoBuilder] Đang tự động kiến tạo Scene Dungeon Eater hoàn chỉnh...");

            // 1. Tạo Core Managers
            GameObject coreObj = new GameObject("Core_GameManager");
            coreObj.AddComponent<GameManager>();
            coreObj.AddComponent<InventoryManager>();
            coreObj.AddComponent<SoundManager>();
            coreObj.AddComponent<CookingSystem>();
            coreObj.AddComponent<UpgradeManager>();

            // 2. Thiết lập Main Camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                mainCam.orthographic = true;
                mainCam.orthographicSize = 5f;
                mainCam.backgroundColor = new Color(0.08f, 0.06f, 0.15f);
                camObj.tag = "MainCamera";
            }
            if (mainCam.GetComponent<CameraFollow>() == null)
            {
                mainCam.gameObject.AddComponent<CameraFollow>();
            }

            // 3. Tạo Player (Hunter-Chef)
            GameObject playerObj = new GameObject("Player_HunterChef");
            playerObj.transform.position = Vector3.zero;

            var rb = playerObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = playerObj.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;

            var sr = playerObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.2f, 0.5f, 0.9f); // Đại diện nhân vật thợ săn

            playerObj.AddComponent<PlayerController>();
            playerObj.AddComponent<WeaponSystem>();

            // 4. Tạo Dungeon Manager & Boundaries
            GameObject dungeonObj = new GameObject("Dungeon_Manager");
            dungeonObj.AddComponent<DungeonManager>();

            // 5. Tạo Restaurant Manager & Dining Tables
            GameObject restObj = new GameObject("Restaurant_Manager");
            var restMgr = restObj.AddComponent<RestaurantManager>();

            for (int i = 1; i <= 4; i++)
            {
                GameObject tableObj = new GameObject($"DiningTable_{i}");
                tableObj.transform.position = new Vector3((i % 2 == 0 ? 3f : -3f), (i > 2 ? -2f : 2f), 0);
                var tableSr = tableObj.AddComponent<SpriteRenderer>();
                tableSr.color = new Color(0.55f, 0.27f, 0.07f); // Màu gỗ sồi
                var tableCol = tableObj.AddComponent<BoxCollider2D>();
                tableCol.size = new Vector2(1.2f, 0.8f);

                var diningTable = tableObj.AddComponent<DiningTable>();
                restMgr.AddTable(diningTable);
            }

            Debug.Log("✅ [SceneAutoBuilder] Scene đã sẵn sàng! Bạn có thể điều khiển bằng WASD, LMB chém, RMB gồng đòn, Space dash i-frame, Q đổi kiếm/cung!");
        }
    }
}

