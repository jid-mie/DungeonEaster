using System.Collections.Generic;
using UnityEngine;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Quản lý điều kiện mở cửa Phòng Boss bằng Chìa Khóa cho Tầng 1 và Tầng 2.
    /// - Gắn lên GameObject Dungeon1 (và gắn thêm 1 bản lên Dungeon2).
    /// - Kéo toàn bộ DungeonCollectibleKey của tầng đó vào `floorKeys` và kéo cửa Boss vào `bossDoor`.
    /// - Khi mới vào tầng: tự khóa cửa Boss.
    /// - Khi nhặt đủ toàn bộ Key: mở khóa cửa Boss để người chơi chạy tới nhấn [F] mở.
    /// </summary>
    public class FloorKeyGateController : MonoBehaviour
    {
        [Header("--- Cấu hình Tầng ---")]
        [SerializeField] private string floorLabel = "Tầng 1";

        [Header("--- Danh sách Chìa Khóa & Cửa Phòng Boss ---")]
        [Tooltip("Kéo tất cả các DungeonCollectibleKey của tầng này vào đây (VD: 3 Key của Tầng 1)")]
        [SerializeField] private List<DungeonCollectibleKey> floorKeys = new List<DungeonCollectibleKey>();
        [Tooltip("Kéo cánh cửa dẫn vào Phòng Boss của tầng này vào đây")]
        [SerializeField] private DungeonDoor bossDoor;

        [Header("--- Hành vi khi đủ Key ---")]
        [Tooltip("Để FALSE: mở khóa cửa để người chơi chạy tới nhấn [F] mở. Tick TRUE nếu muốn cửa tự mở luôn.")]
        [SerializeField] private bool autoOpenBossDoorOnComplete = false;

        private int collectedCount = 0;
        private int totalKeys = 0;
        private bool isUnlocked = false;

        public int CollectedCount => collectedCount;
        public int TotalKeys => totalKeys;
        public bool IsBossDoorUnlocked => isUnlocked;

        private void Start()
        {
            totalKeys = 0;
            collectedCount = 0;

            for (int i = 0; i < floorKeys.Count; i++)
            {
                var key = floorKeys[i];
                if (key == null) continue;

                totalKeys++;
                if (key.IsCollected)
                {
                    collectedCount++;
                }
                else
                {
                    key.OnCollected += HandleKeyCollected;
                }
            }

            if (totalKeys == 0)
            {
                // Không cấu hình Key nào -> mở khóa luôn để tránh soft-lock
                UnlockBossDoor(false);
                return;
            }

            if (collectedCount >= totalKeys)
            {
                UnlockBossDoor(false);
            }
            else if (bossDoor != null)
            {
                bossDoor.SetConditionLocked(true, BuildLockedPrompt());
            }
        }

        private void HandleKeyCollected(DungeonCollectibleKey key)
        {
            if (key != null)
            {
                key.OnCollected -= HandleKeyCollected;
            }

            collectedCount++;

            if (collectedCount >= totalKeys)
            {
                UnlockBossDoor(true);
            }
            else
            {
                if (bossDoor != null)
                {
                    bossDoor.SetConditionLocked(true, BuildLockedPrompt());
                }

                DungeonBannerToast.Show(
                    $"[{floorLabel}] Đã nhặt Chìa Khóa ({collectedCount}/{totalKeys})",
                    new Color(1f, 0.9f, 0.4f, 1f),
                    2.4f
                );
            }
        }

        private void UnlockBossDoor(bool showToast)
        {
            if (isUnlocked) return;
            isUnlocked = true;

            if (bossDoor != null)
            {
                bossDoor.UnlockCondition(autoOpenBossDoorOnComplete);
            }

            if (showToast)
            {
                string msg = autoOpenBossDoorOnComplete
                    ? $"[{floorLabel}] Đã đủ Chìa Khóa ({collectedCount}/{totalKeys})! Cửa phòng Boss đã mở!"
                    : $"[{floorLabel}] Đã đủ Chìa Khóa ({collectedCount}/{totalKeys})! Hãy tới cửa phòng Boss và nhấn [F] để mở!";
                DungeonBannerToast.Show(msg, new Color(0.45f, 1f, 0.6f, 1f), 3.4f);
            }
        }

        private string BuildLockedPrompt()
        {
            return $"Cần đủ Chìa Khóa ({collectedCount}/{totalKeys})";
        }

        private void OnDestroy()
        {
            for (int i = 0; i < floorKeys.Count; i++)
            {
                if (floorKeys[i] != null)
                {
                    floorKeys[i].OnCollected -= HandleKeyCollected;
                }
            }
        }
    }
}

