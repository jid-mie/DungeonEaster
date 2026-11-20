using System;
using System.Collections.Generic;
using UnityEngine;
using NguyenVanHai.Enemies;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Vùng chiến đấu đặt bên trong mỗi phòng Quái hoặc phòng Boss.
    /// - Gắn lên 1 Empty GameObject có BoxCollider2D (Is Trigger = true) phủ phần lòng bên trong phòng
    ///   (nên thụt vào khỏi cánh cửa ~0.8 unit để người chơi bước hẳn vào phòng mới đóng cửa, không bị kẹt).
    /// - Khi HunterPlayer bước vào vùng này và bên trong vẫn còn Quái / Boss còn sống:
    ///   Tự động phong ấn toàn bộ cửa trong danh sách `roomDoors`.
    /// - Khi toàn bộ Quái / Boss trong phòng bị tiêu diệt:
    ///   Gỡ phong ấn và mở lại cửa cho người chơi đi tiếp.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class CombatRoomZone : MonoBehaviour
    {
        [Header("--- Cửa cần chặn khi vào phòng ---")]
        [Tooltip("Kéo tất cả các DungeonDoor nối vào phòng này (cửa vào, cửa ra)")]
        [SerializeField] private List<DungeonDoor> roomDoors = new List<DungeonDoor>();

        [Header("--- Quái / Boss trong phòng ---")]
        [Tooltip("Tự động quét toàn bộ Quái & Boss (EnemyBase) đang nằm trong khung BoxCollider2D này khi người chơi bước vào")]
        [SerializeField] private bool detectEnemiesInsideBox = true;
        [Tooltip("Tùy chọn: Kéo trực tiếp Quái hoặc Boss (VD: Dual Boss Tầng 4) vào đây nếu muốn quản lý tường minh")]
        [SerializeField] private List<EnemyBase> explicitEnemies = new List<EnemyBase>();

        [Header("--- Hành vi & Thông báo ---")]
        [Tooltip("Tự động mở rộng cửa khi dọn xong phòng. Nếu bỏ tick, cửa chuyển về trạng thái Đóng bình thường (nhấn F để mở).")]
        [SerializeField] private bool openDoorsOnClear = true;
        [SerializeField] private string roomDisplayName = "";
        [SerializeField] private bool showStatusToast = true;

        private BoxCollider2D zoneCollider;
        private readonly List<EnemyBase> trackedEnemies = new List<EnemyBase>();
        private bool isCombatActive = false;
        private bool isCleared = false;
        private float nextPollTime = 0f;

        public bool IsCombatActive => isCombatActive;
        public bool IsCleared => isCleared;

        public event Action<CombatRoomZone> OnRoomSealed;
        public event Action<CombatRoomZone> OnRoomCleared;

        private void Awake()
        {
            zoneCollider = GetComponent<BoxCollider2D>();
            if (zoneCollider != null)
            {
                zoneCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryStartCombatEncounter(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // Bảo đảm kích hoạt cả trong trường hợp Player dịch chuyển (Teleport) thẳng vào giữa vùng Trigger
            if (!isCombatActive && !isCleared)
            {
                TryStartCombatEncounter(other);
            }
        }

        private void Update()
        {
            if (!isCombatActive) return;

            // Kiểm tra định kỳ mỗi 0.25s phòng trường hợp enemy bị Destroy mà không kịp bắn OnDeath
            if (Time.time >= nextPollTime)
            {
                nextPollTime = Time.time + 0.25f;
                CheckRoomCleared();
            }
        }

        private void TryStartCombatEncounter(Collider2D other)
        {
            if (isCombatActive || isCleared) return;

            var player = other.GetComponent<HunterPlayer>() ?? other.GetComponentInParent<HunterPlayer>();
            if (player == null || !player.isActiveAndEnabled) return;

            CollectAliveEnemiesInRoom();

            if (trackedEnemies.Count == 0)
            {
                // Phòng không có quái sống -> coi như đã sạch, không cần chặn cửa
                isCleared = true;
                return;
            }

            isCombatActive = true;

            for (int i = 0; i < trackedEnemies.Count; i++)
            {
                trackedEnemies[i].OnDeath += HandleTrackedEnemyDeath;
            }

            string sealMsg = string.IsNullOrEmpty(roomDisplayName)
                ? "Diệt hết quái để mở cửa!"
                : $"Diệt hết kẻ địch trong {roomDisplayName}!";

            for (int i = 0; i < roomDoors.Count; i++)
            {
                if (roomDoors[i] != null)
                {
                    roomDoors[i].SealForCombat(sealMsg);
                }
            }

            if (showStatusToast)
            {
                string title = string.IsNullOrEmpty(roomDisplayName)
                    ? "Cửa phòng đã bị phong ấn! Tiêu diệt toàn bộ kẻ địch!"
                    : $"[{roomDisplayName}] Cửa đã đóng chặt! Tiêu diệt toàn bộ kẻ địch!";
                DungeonBannerToast.Show(title, new Color(1f, 0.45f, 0.35f, 1f), 2.5f);
            }

            OnRoomSealed?.Invoke(this);
        }

        private void CollectAliveEnemiesInRoom()
        {
            trackedEnemies.Clear();

            // 1. Lấy các kẻ địch được kéo thả tường minh
            if (explicitEnemies != null)
            {
                for (int i = 0; i < explicitEnemies.Count; i++)
                {
                    AddEnemyIfAlive(explicitEnemies[i]);
                }
            }

            // 2. Quét tất cả EnemyBase đang nằm trong BoxCollider2D của phòng
            if (detectEnemiesInsideBox && zoneCollider != null)
            {
                Bounds b = zoneCollider.bounds;
                Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, b.size, 0f);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i] == null || hits[i] == zoneCollider) continue;
                    var enemy = hits[i].GetComponent<EnemyBase>() ?? hits[i].GetComponentInParent<EnemyBase>();
                    AddEnemyIfAlive(enemy);
                }
            }
        }

        private void AddEnemyIfAlive(EnemyBase enemy)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy || !enemy.IsAlive) return;
            if (!trackedEnemies.Contains(enemy))
            {
                trackedEnemies.Add(enemy);
            }
        }

        private void HandleTrackedEnemyDeath(EnemyBase deadEnemy)
        {
            if (deadEnemy != null)
            {
                deadEnemy.OnDeath -= HandleTrackedEnemyDeath;
            }
            CheckRoomCleared();
        }

        private void CheckRoomCleared()
        {
            if (!isCombatActive) return;

            for (int i = trackedEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = trackedEnemies[i];
                if (enemy == null || !enemy.IsAlive || !enemy.gameObject.activeInHierarchy)
                {
                    if (enemy != null) enemy.OnDeath -= HandleTrackedEnemyDeath;
                    trackedEnemies.RemoveAt(i);
                }
            }

            if (trackedEnemies.Count == 0)
            {
                CompleteRoomEncounter();
            }
        }

        private void CompleteRoomEncounter()
        {
            isCombatActive = false;
            isCleared = true;

            for (int i = 0; i < roomDoors.Count; i++)
            {
                if (roomDoors[i] != null)
                {
                    roomDoors[i].UnsealAfterCombat(openDoorsOnClear);
                }
            }

            if (showStatusToast)
            {
                string clearMsg = string.IsNullOrEmpty(roomDisplayName)
                    ? "Đã dọn sạch phòng! Cửa đã mở trở lại."
                    : $"[{roomDisplayName}] Đã chiến thắng! Cửa đã mở trở lại.";
                DungeonBannerToast.Show(clearMsg, new Color(0.45f, 1f, 0.55f, 1f), 2.6f);
            }

            OnRoomCleared?.Invoke(this);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < trackedEnemies.Count; i++)
            {
                if (trackedEnemies[i] != null)
                {
                    trackedEnemies[i].OnDeath -= HandleTrackedEnemyDeath;
                }
            }
        }

        private void OnDrawGizmos()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col == null) return;

            Color fillColor = isCombatActive
                ? new Color(1f, 0.2f, 0.2f, 0.22f)
                : (isCleared ? new Color(0.2f, 1f, 0.4f, 0.14f) : new Color(0.2f, 0.75f, 1f, 0.16f));

            Color wireColor = isCombatActive
                ? new Color(1f, 0.25f, 0.25f, 0.9f)
                : (isCleared ? new Color(0.25f, 1f, 0.45f, 0.8f) : new Color(0.25f, 0.8f, 1f, 0.85f));

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = fillColor;
            Gizmos.DrawCube(col.offset, col.size);
            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(col.offset, col.size);
            Gizmos.matrix = Matrix4x4.identity;

            // Vẽ đường nối từ tâm phòng đến từng cánh cửa sẽ bị khóa
            if (roomDoors != null)
            {
                Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.85f);
                Vector3 center = col.bounds.center;
                for (int i = 0; i < roomDoors.Count; i++)
                {
                    if (roomDoors[i] != null)
                    {
                        Gizmos.DrawLine(center, roomDoors[i].transform.position);
                    }
                }
            }
        }
    }
}
