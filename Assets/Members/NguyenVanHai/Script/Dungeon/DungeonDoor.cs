using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Điều khiển cửa ngục trong Dungeon.
    /// Khi người chơi lại gần trong phạm vi tương tác và nhấn phím F:
    /// Chạy hiệu ứng mở cửa rồi Disable toàn bộ GameObject cửa để giải phóng lối đi 100%.
    /// </summary>
    public class DungeonDoor : MonoBehaviour
    {
        [Header("Khoảng cách tương tác")]
        [SerializeField] private float interactRadius = 2.0f;
        [SerializeField] private KeyCode interactKey = KeyCode.F;

        [Header("Sprites mở cửa (nếu có)")]
        [SerializeField] private Sprite[] openFrames;
        [SerializeField] private float frameRate = 0.08f;

        private Transform playerTransform;
        private SpriteRenderer sr;
        private bool isOpening = false;
        private bool isPlayerNearby = false;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (isOpening) return;

            if (playerTransform == null)
            {
                var p = FindObjectOfType<HunterPlayer>();
                if (p != null) playerTransform = p.transform;
                if (playerTransform == null) return;
            }

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            isPlayerNearby = (dist <= interactRadius);

            if (isPlayerNearby && Input.GetKeyDown(interactKey))
            {
                StartCoroutine(OpenAndDisableRoutine());
            }
        }

        private IEnumerator OpenAndDisableRoutine()
        {
            isOpening = true;

            // Chạy chuỗi frame mở cửa nếu có
            if (openFrames != null && openFrames.Length > 0 && sr != null)
            {
                for (int i = 0; i < openFrames.Length; i++)
                {
                    sr.sprite = openFrames[i];
                    yield return new WaitForSeconds(frameRate);
                }
            }
            else
            {
                yield return new WaitForSeconds(0.15f);
            }

            // Vô hiệu hóa hoàn toàn GameObject cửa theo đề xuất của bạn
            gameObject.SetActive(false);
        }

        private void OnGUI()
        {
            if (isPlayerNearby && !isOpening)
            {
                Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.0f) : Vector3.zero;
                if (screenPos.z > 0)
                {
                    GUI.backgroundColor = new Color(0.15f, 0.1f, 0.05f, 0.9f);
                    GUI.contentColor = new Color(1f, 0.85f, 0.4f, 1f);
                    GUI.Box(new Rect(screenPos.x - 70, Screen.height - screenPos.y - 15, 140, 30), "Nhấn [F] Mở Cửa");
                }
            }
        }
    }
}
