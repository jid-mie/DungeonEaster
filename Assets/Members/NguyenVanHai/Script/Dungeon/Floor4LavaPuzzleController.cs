using System.Collections.Generic;
using UnityEngine;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Quản lý cơ chế giải đố Công tắc Nước – Dung nham ở Tầng 4 (gắn lên GameObject Dungeon4).
    /// - Khi toàn bộ các nút trong `requiredButtonsForBossLava` (button_blue_up (1) và button_blue_up (2))
    ///   đều đã được kích hoạt, tự động tắt `lavaBossObject` (lavaboss) trong Phòng Boss.
    /// </summary>
    public class Floor4LavaPuzzleController : MonoBehaviour
    {
        [Header("--- Điều kiện xóa Dung Nham Phòng Boss ---")]
        [Tooltip("Kéo 2 công tắc điều kiện vào đây: button_blue_up (1) và button_blue_up (2)")]
        [SerializeField] private List<DungeonButton> requiredButtonsForBossLava = new List<DungeonButton>();

        [Tooltip("Kéo object lavaboss trong Dungeon4/Grid vào đây")]
        [SerializeField] private GameObject lavaBossObject;

        [Header("--- Trạng thái khởi đầu khi vào màn (Tùy chọn) ---")]
        [Tooltip("Các object nên TẮT ban đầu (VD: Water1, Water2, Water3)")]
        [SerializeField] private GameObject[] initialInactiveObjects;
        [Tooltip("Các object nên BẬT ban đầu (VD: lavademo, lavaboss)")]
        [SerializeField] private GameObject[] initialActiveObjects;

        private bool isBossLavaCleared = false;

        public bool IsBossLavaCleared => isBossLavaCleared;

        private void Start()
        {
            if (initialInactiveObjects != null)
            {
                for (int i = 0; i < initialInactiveObjects.Length; i++)
                {
                    if (initialInactiveObjects[i] != null)
                    {
                        initialInactiveObjects[i].SetActive(false);
                    }
                }
            }

            if (initialActiveObjects != null)
            {
                for (int i = 0; i < initialActiveObjects.Length; i++)
                {
                    if (initialActiveObjects[i] != null)
                    {
                        initialActiveObjects[i].SetActive(true);
                    }
                }
            }

            for (int i = 0; i < requiredButtonsForBossLava.Count; i++)
            {
                var btn = requiredButtonsForBossLava[i];
                if (btn != null)
                {
                    btn.OnPressed += HandleRequiredButtonPressed;
                }
            }

            EvaluateBossLavaCondition(false);
        }

        private void HandleRequiredButtonPressed(DungeonButton pressedButton)
        {
            EvaluateBossLavaCondition(true);
        }

        private void EvaluateBossLavaCondition(bool showProgressToast)
        {
            if (isBossLavaCleared) return;

            int validCount = 0;
            int pressedCount = 0;

            for (int i = 0; i < requiredButtonsForBossLava.Count; i++)
            {
                var btn = requiredButtonsForBossLava[i];
                if (btn == null) continue;
                validCount++;
                if (btn.IsPressed) pressedCount++;
            }

            if (validCount == 0) return;

            if (pressedCount >= validCount)
            {
                isBossLavaCleared = true;
                if (lavaBossObject != null)
                {
                    lavaBossObject.SetActive(false);
                }

                DungeonBannerToast.Show(
                    "Hai dòng nước đã dập tắt toàn bộ Dung Nham trong Phòng Boss!",
                    new Color(0.45f, 0.95f, 1f, 1f),
                    3.5f
                );
            }
            else if (showProgressToast)
            {
                DungeonBannerToast.Show(
                    $"Đã kích hoạt công tắc nước phòng Boss ({pressedCount}/{validCount})!",
                    new Color(0.55f, 0.88f, 1f, 1f),
                    2.6f
                );
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < requiredButtonsForBossLava.Count; i++)
            {
                if (requiredButtonsForBossLava[i] != null)
                {
                    requiredButtonsForBossLava[i].OnPressed -= HandleRequiredButtonPressed;
                }
            }
        }
    }
}

