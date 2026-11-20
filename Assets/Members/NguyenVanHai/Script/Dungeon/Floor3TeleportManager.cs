using System.Collections.Generic;
using UnityEngine;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Quản lý hệ thống 12 Key dịch chuyển ở Tầng 3 (gắn lên GameObject Dungeon3).
    /// - Đầu mỗi phiên chơi (Start): Xáo trộn ngẫu nhiên 12 Key bằng Fisher-Yates Shuffle:
    ///   + Đảm bảo CHẮC CHẮN đúng 3 Key (bossPortalCount) nhảy thẳng đến Phòng Boss (bossRoomSpawnPoint).
    ///   + 9 Key còn lại mỗi Key nhảy sang vị trí của 1 Key ở phòng khác (không tự trỏ vào chính nó,
    ///     ưu tiên khác RoomId và tránh tạo cặp nhảy qua lại A <-> B nếu có lựa chọn khác).
    /// </summary>
    public class Floor3TeleportManager : MonoBehaviour
    {
        [Header("--- Danh sách 12 Key & Điểm đáp Phòng Boss ---")]
        [Tooltip("Kéo đủ cả 12 GameObject Key (đã gắn Floor3TeleportKey) của Tầng 3 vào đây")]
        [SerializeField] private List<Floor3TeleportKey> allKeys = new List<Floor3TeleportKey>();

        [Tooltip("Kéo 1 Empty GameObject đánh dấu vị trí đáp bên trong Phòng Boss Tầng 3 (nên nằm trong CombatRoomZone của phòng Boss)")]
        [SerializeField] private Transform bossRoomSpawnPoint;

        [Header("--- Luật phân bổ mỗi phiên chơi ---")]
        [Tooltip("Số lượng Key chắc chắn dẫn tới phòng Boss trong mỗi phiên chơi (Mặc định: 3)")]
        [SerializeField] private int bossPortalCount = 3;

        [Tooltip("Thời gian hồi ngắn sau mỗi lần dịch chuyển để tránh bấm nhầm liên tiếp")]
        [SerializeField] private float teleportCooldown = 0.65f;

        [Header("--- Debug (Tùy chọn) ---")]
        [Tooltip("Tick để in bảng phân bổ 3 Key Boss + 9 Key phòng khác ra Console khi chạy game")]
        [SerializeField] private bool logMappingToConsole = false;

        private readonly Dictionary<Floor3TeleportKey, Floor3TeleportKey> decoyTargets = new Dictionary<Floor3TeleportKey, Floor3TeleportKey>();
        private float nextAllowedTeleportTime = -99f;

        public bool IsOnTeleportCooldown => Time.time < nextAllowedTeleportTime;

        private void Start()
        {
            InitializeSessionPortals();
        }

        /// <summary>
        /// Xáo trộn và gán đích dịch chuyển cho toàn bộ Key trong phiên chơi hiện tại.
        /// </summary>
        public void InitializeSessionPortals()
        {
            decoyTargets.Clear();

            var validKeys = new List<Floor3TeleportKey>();
            for (int i = 0; i < allKeys.Count; i++)
            {
                var k = allKeys[i];
                if (k != null && !validKeys.Contains(k))
                {
                    k.BindManager(this);
                    k.IsBossPortal = false;
                    validKeys.Add(k);
                }
            }

            int total = validKeys.Count;
            if (total == 0)
            {
                Debug.LogWarning("[Floor3TeleportManager] Danh sách allKeys đang trống! Hãy kéo 12 Key Tầng 3 vào Inspector.");
                return;
            }

            // 1. Xáo trộn ngẫu nhiên bằng Fisher-Yates Shuffle
            var shuffled = new List<Floor3TeleportKey>(validKeys);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var tmp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = tmp;
            }

            // 2. Chọn đúng bossPortalCount (3) Key đầu tiên làm Key dẫn vào Phòng Boss
            int actualBossCount = Mathf.Clamp(bossPortalCount, 0, shuffled.Count);
            for (int i = 0; i < actualBossCount; i++)
            {
                shuffled[i].IsBossPortal = true;
            }

            // 3. 9 Key còn lại: gán mỗi Key nhảy tới 1 Key ở phòng khác
            for (int i = actualBossCount; i < shuffled.Count; i++)
            {
                Floor3TeleportKey source = shuffled[i];
                Floor3TeleportKey destination = PickDecoyDestination(source, validKeys);
                if (destination != null)
                {
                    decoyTargets[source] = destination;
                }
            }

            if (logMappingToConsole)
            {
                Debug.Log($"[Floor3TeleportManager] Đã khởi tạo {total} Key: {actualBossCount} Key Phòng Boss, {total - actualBossCount} Key phòng khác.");
                for (int i = 0; i < shuffled.Count; i++)
                {
                    var k = shuffled[i];
                    if (k.IsBossPortal)
                    {
                        Debug.Log($"  - [BOSS PORTAL] {k.name} (Room {k.RoomId}) -> Phòng Boss");
                    }
                    else if (decoyTargets.TryGetValue(k, out var dest))
                    {
                        Debug.Log($"  - [DECOY] {k.name} (Room {k.RoomId}) -> {dest.name} (Room {dest.RoomId})");
                    }
                }
            }
        }

        private Floor3TeleportKey PickDecoyDestination(Floor3TeleportKey source, List<Floor3TeleportKey> allValid)
        {
            if (allValid.Count <= 1) return null;

            // Ưu tiên 1: Key khác phòng (RoomId != source.RoomId) và chưa trỏ ngược lại source
            var preferredDifferentRoom = new List<Floor3TeleportKey>();
            var fallbackDifferentRoom = new List<Floor3TeleportKey>();
            var preferredAnyOther = new List<Floor3TeleportKey>();
            var fallbackAnyOther = new List<Floor3TeleportKey>();

            for (int i = 0; i < allValid.Count; i++)
            {
                var candidate = allValid[i];
                if (candidate == source) continue;

                bool wouldPingPong = decoyTargets.TryGetValue(candidate, out var candTarget) && candTarget == source;
                bool isDifferentRoom = (source.RoomId != 0 && candidate.RoomId != 0)
                    ? (candidate.RoomId != source.RoomId)
                    : true;

                if (isDifferentRoom)
                {
                    fallbackDifferentRoom.Add(candidate);
                    if (!wouldPingPong) preferredDifferentRoom.Add(candidate);
                }

                fallbackAnyOther.Add(candidate);
                if (!wouldPingPong) preferredAnyOther.Add(candidate);
            }

            if (preferredDifferentRoom.Count > 0)
                return preferredDifferentRoom[Random.Range(0, preferredDifferentRoom.Count)];

            if (fallbackDifferentRoom.Count > 0)
                return fallbackDifferentRoom[Random.Range(0, fallbackDifferentRoom.Count)];

            if (preferredAnyOther.Count > 0)
                return preferredAnyOther[Random.Range(0, preferredAnyOther.Count)];

            return fallbackAnyOther[Random.Range(0, fallbackAnyOther.Count)];
        }

        internal void TriggerTeleportFrom(Floor3TeleportKey sourceKey, HunterPlayer player)
        {
            if (sourceKey == null || player == null || IsOnTeleportCooldown) return;

            nextAllowedTeleportTime = Time.time + teleportCooldown;
            sourceKey.MarkVisited();

            if (sourceKey.IsBossPortal)
            {
                if (bossRoomSpawnPoint == null)
                {
                    Debug.LogWarning("[Floor3TeleportManager] Chưa gán bossRoomSpawnPoint trong Inspector!");
                    return;
                }

                TeleportPlayerTo(player, bossRoomSpawnPoint.position);
                DungeonBannerToast.FlashTeleport(new Color(1f, 0.55f, 0.35f, 0.78f), 0.32f);
                DungeonBannerToast.Show("Ấn ký chính xác! Bạn đã tiến vào Phòng Boss Tầng 3!", new Color(1f, 0.65f, 0.35f, 1f), 3.0f);
            }
            else if (decoyTargets.TryGetValue(sourceKey, out var targetKey) && targetKey != null)
            {
                TeleportPlayerTo(player, targetKey.GetArrivalPosition());
                DungeonBannerToast.FlashTeleport(new Color(0.55f, 0.85f, 1f, 0.72f), 0.26f);
                DungeonBannerToast.Show("Ấn ký dịch chuyển bạn sang một căn phòng khác!", new Color(0.65f, 0.92f, 1f, 1f), 2.4f);
            }
        }

        private void TeleportPlayerTo(HunterPlayer player, Vector3 worldPos)
        {
            Vector3 finalPos = new Vector3(worldPos.x, worldPos.y, player.transform.position.z);

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.position = new Vector2(finalPos.x, finalPos.y);
            }

            player.transform.position = finalPos;
            Physics2D.SyncTransforms();
        }

        private void OnDrawGizmosSelected()
        {
            if (bossRoomSpawnPoint != null)
            {
                Gizmos.color = new Color(1f, 0.45f, 0.2f, 0.9f);
                Gizmos.DrawWireSphere(bossRoomSpawnPoint.position, 0.55f);
            }
        }
    }
}

