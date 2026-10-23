using System.Collections.Generic;
using UnityEngine;
using NguyenVanHai.UI;
using NguyenVanHai.Enemies;
using NguyenVanHai.Combat;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Điều phối viên trung tâm cho Scene Dungeon:
    /// 1. Tự động khởi tạo hệ thống UI (HUD, Hòm đồ balo 10 ô + 4 quick slot, Màn hình Thắng/Thua).
    /// 2. Tự động gắn DungeonDoor vào toàn bộ cánh cửa 'doors-d1-opening-h_0' để người chơi nhấn F mở cửa và disable object.
    /// 3. Tự động thiết lập Collider và AI thông minh cho quái vật (Slime, Nấm, Bọ, Boss) nếu chưa có.
    /// 4. Quản lý trạng thái hoàn thành Dungeon.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class DungeonGameManager : MonoBehaviour
    {
        public static DungeonGameManager Instance { get; private set; }

        private DungeonHUD hud;
        private DungeonInventoryUI inventoryUI;
        private DungeonEndScreens endScreens;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureUISystems();
            SetupDoors();
            SetupEnemies();
        }

        private void EnsureUISystems()
        {
            hud = GetComponentInChildren<DungeonHUD>();
            if (hud == null) hud = gameObject.AddComponent<DungeonHUD>();

            inventoryUI = GetComponentInChildren<DungeonInventoryUI>();
            if (inventoryUI == null) inventoryUI = gameObject.AddComponent<DungeonInventoryUI>();

            endScreens = GetComponentInChildren<DungeonEndScreens>();
            if (endScreens == null) endScreens = gameObject.AddComponent<DungeonEndScreens>();
        }

        /// <summary>
        /// Quét và gắn DungeonDoor vào toàn bộ cửa trong scene để nhấn phím F mở và Disable object.
        /// </summary>
        private void SetupDoors()
        {
            var allObjects = FindObjectsOfType<Transform>(true);
            foreach (var t in allObjects)
            {
                if (t.name.Contains("doors-d1-opening-h") || t.name.ToLower().Contains("door"))
                {
                    if (t.GetComponent<DungeonDoor>() == null)
                    {
                        t.gameObject.AddComponent<DungeonDoor>();
                    }
                }
            }
        }

        /// <summary>
        /// Đảm bảo tất cả quái vật đã đặt trong scene có đầy đủ Rigidbody2D, Collider2D và Script AI tương ứng.
        /// </summary>
        private void SetupEnemies()
        {
            var allSR = FindObjectsOfType<SpriteRenderer>(true);
            foreach (var sr in allSR)
            {
                string objName = sr.gameObject.name.ToLower();
                string spriteName = sr.sprite != null ? sr.sprite.name.ToLower() : "";

                // 1. Quái Slime
                if (objName.Contains("slime") || spriteName.Contains("slime"))
                {
                    EnsureSlimeSetup(sr.gameObject);
                }
                // 2. Quái Nấm (Spore / Mushroom)
                else if (objName.Contains("mushroom") || objName.Contains("spore") || spriteName.Contains("spore") || spriteName.Contains("mushroom"))
                {
                    EnsureMushroomSetup(sr.gameObject);
                }
                // 3. Quái Bọ (Beetle / Bug)
                else if (objName.Contains("beetle") || objName.Contains("bug") || spriteName.Contains("beetle") || spriteName.Contains("bug"))
                {
                    EnsureBeetleSetup(sr.gameObject);
                }
                // 4. Boss King Boar
                else if (objName.Contains("boar") || objName.Contains("boss") || spriteName.Contains("boar"))
                {
                    EnsureBossSetup(sr.gameObject);
                }
            }
        }

        private void EnsureSlimeSetup(GameObject go)
        {
            if (go.GetComponent<SlimeAI>() == null && go.GetComponent<EnemyBase>() == null)
            {
                EnsurePhysics(go, 0.45f);
                go.AddComponent<SlimeAI>();
            }
        }

        private void EnsureMushroomSetup(GameObject go)
        {
            if (go.GetComponent<SporeShooterAI>() == null && go.GetComponent<EnemyBase>() == null)
            {
                EnsurePhysics(go, 0.5f);
                go.AddComponent<SporeShooterAI>();
            }
        }

        private void EnsureBeetleSetup(GameObject go)
        {
            if (go.GetComponent<BeetleMeleeAI>() == null && go.GetComponent<EnemyBase>() == null)
            {
                EnsurePhysics(go, 0.45f);
                go.AddComponent<BeetleMeleeAI>();
            }
        }

        private void EnsureBossSetup(GameObject go)
        {
            if (go.GetComponent<BossKingBoarAI>() == null && go.GetComponent<EnemyBase>() == null)
            {
                EnsurePhysics(go, 1.2f);
                go.AddComponent<BossKingBoarAI>();
            }
        }

        private void EnsurePhysics(GameObject go, float colliderRadius)
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            var col = go.GetComponent<Collider2D>();
            if (col == null)
            {
                var cc = go.AddComponent<CircleCollider2D>();
                cc.radius = colliderRadius;
                cc.isTrigger = false;
            }

            if (go.GetComponent<DamageFlash>() == null)
            {
                go.AddComponent<DamageFlash>();
            }
        }
    }
}
