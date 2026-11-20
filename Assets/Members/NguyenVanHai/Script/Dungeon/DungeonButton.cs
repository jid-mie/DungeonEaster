using System;
using UnityEngine;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Công tắc tương tác ở Tầng 4 (gắn lên button_blue_up, button_blue_up (1), button_blue_up (2) trong Dungeon4/Key).
    /// - Khi người chơi đứng gần và nhấn phím [F]:
    ///   + Đổi trạng thái hình ảnh sang đã nhấn.
    ///   + Bật (SetActive true) các GameObject trong `objectsToEnable` (VD: Water1, Water2, Water3).
    ///   + Tắt (SetActive false) các GameObject trong `objectsToDisable` (VD: lavademo).
    ///   + Phát sự kiện OnPressed để Floor4LavaPuzzleController kiểm tra điều kiện xóa lavaboss.
    /// </summary>
    public class DungeonButton : MonoBehaviour
    {
        [Header("--- Tương tác ---")]
        [SerializeField] private float interactRadius = 1.4f;
        [SerializeField] private KeyCode interactKey = KeyCode.F;
        [SerializeField] private string promptText = "Nhấn [F] Kích hoạt Công Tắc";

        [Header("--- Hình ảnh sau khi nhấn ---")]
        [Tooltip("Tùy chọn: Kéo sprite nút ở trạng thái đã ấn xuống (VD: button_blue_down)")]
        [SerializeField] private Sprite pressedSprite;
        [SerializeField] private Color pressedTint = new Color(0.55f, 0.85f, 1f, 1f);

        [Header("--- Đối tượng Bật / Tắt khi nhấn nút này ---")]
        [Tooltip("Các object sẽ được BẬT lên khi nhấn nút này (VD: Water1, Water2, Water3)")]
        [SerializeField] private GameObject[] objectsToEnable;
        [Tooltip("Các object sẽ bị TẮT/XÓA đi khi nhấn nút này (VD: lavademo)")]
        [SerializeField] private GameObject[] objectsToDisable;

        [Header("--- Thông báo ---")]
        [SerializeField] private string activatedToastMessage = "Đã kích hoạt công tắc nước!";

        private HunterPlayer hunter;
        private SpriteRenderer sr;
        private bool isPlayerNearby = false;

        public bool IsPressed { get; private set; } = false;
        public event Action<DungeonButton> OnPressed;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (IsPressed)
            {
                isPlayerNearby = false;
                return;
            }

            if (hunter == null)
            {
                hunter = FindObjectOfType<HunterPlayer>();
                if (hunter == null) return;
            }

            isPlayerNearby = CheckPlayerNearby();

            if (isPlayerNearby && Input.GetKeyDown(interactKey))
            {
                Press();
            }
        }

        private bool CheckPlayerNearby()
        {
            float dist = Vector2.Distance(transform.position, hunter.transform.position);
            if (dist > interactRadius) return false;

            int wallMask = LayerMask.GetMask("Obstacle", "Default");
            RaycastHit2D hit = Physics2D.Linecast(hunter.transform.position, transform.position, wallMask);
            if (hit.collider != null
                && !hit.collider.isTrigger
                && hit.collider.gameObject != gameObject
                && hit.collider.GetComponent<HunterPlayer>() == null
                && hit.collider.GetComponentInParent<HunterPlayer>() == null)
            {
                return false;
            }

            return true;
        }

        public void Press()
        {
            if (IsPressed) return;
            IsPressed = true;
            isPlayerNearby = false;

            if (sr != null)
            {
                if (pressedSprite != null) sr.sprite = pressedSprite;
                sr.color = pressedTint;
            }

            if (objectsToEnable != null)
            {
                for (int i = 0; i < objectsToEnable.Length; i++)
                {
                    if (objectsToEnable[i] != null)
                    {
                        objectsToEnable[i].SetActive(true);
                    }
                }
            }

            if (objectsToDisable != null)
            {
                for (int i = 0; i < objectsToDisable.Length; i++)
                {
                    if (objectsToDisable[i] != null)
                    {
                        objectsToDisable[i].SetActive(false);
                    }
                }
            }

            if (!string.IsNullOrEmpty(activatedToastMessage))
            {
                DungeonBannerToast.Show(activatedToastMessage, new Color(0.45f, 0.88f, 1f, 1f), 2.5f);
            }

            OnPressed?.Invoke(this);
        }

        private void OnGUI()
        {
            if (IsPressed || !isPlayerNearby || Camera.main == null) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.85f);
            if (screenPos.z <= 0f) return;

            float width = Mathf.Max(195f, promptText.Length * 8.5f);
            GUI.backgroundColor = new Color(0.06f, 0.12f, 0.2f, 0.92f);
            GUI.contentColor = new Color(0.55f, 0.9f, 1f, 1f);
            GUI.Box(new Rect(screenPos.x - width * 0.5f, Screen.height - screenPos.y - 16f, width, 30f), promptText);
        }
    }
}

