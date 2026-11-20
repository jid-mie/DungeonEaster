using System;
using UnityEngine;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Chìa khóa thu thập cho Tầng 1 & Tầng 2 (gắn lên từng object Key trong Dungeon1/Key và Dungeon2/Key).
    /// Khi người chơi đứng gần trong phạm vi tương tác và nhấn phím [F]:
    /// Thu thập chìa khóa, báo về FloorKeyGateController và ẩn object chìa khóa.
    /// </summary>
    public class DungeonCollectibleKey : MonoBehaviour
    {
        [Header("Cấu hình tương tác")]
        [SerializeField] private float interactRadius = 1.4f;
        [SerializeField] private KeyCode interactKey = KeyCode.F;
        [SerializeField] private string promptText = "Nhấn [F] Nhặt Chìa Khóa";

        [Header("Hiệu ứng nổi nhẹ (Tùy chọn)")]
        [SerializeField] private bool enableFloatAnimation = true;
        [SerializeField] private float floatAmplitude = 0.08f;
        [SerializeField] private float floatSpeed = 3.2f;

        private HunterPlayer hunter;
        private Vector3 baseLocalPos;
        private bool isPlayerNearby = false;

        public bool IsCollected { get; private set; } = false;
        public event Action<DungeonCollectibleKey> OnCollected;

        private void Awake()
        {
            baseLocalPos = transform.localPosition;
        }

        private void Update()
        {
            if (IsCollected) return;

            if (enableFloatAnimation)
            {
                float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
                transform.localPosition = baseLocalPos + new Vector3(0f, yOffset, 0f);
            }

            if (hunter == null)
            {
                hunter = FindObjectOfType<HunterPlayer>();
                if (hunter == null) return;
            }

            isPlayerNearby = CheckPlayerNearby();

            if (isPlayerNearby && Input.GetKeyDown(interactKey))
            {
                Collect();
            }
        }

        private bool CheckPlayerNearby()
        {
            float dist = Vector2.Distance(transform.position, hunter.transform.position);
            if (dist > interactRadius) return false;

            // Kiểm tra tường chắn giữa Player và Key
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

        public void Collect()
        {
            if (IsCollected) return;
            IsCollected = true;
            isPlayerNearby = false;

            OnCollected?.Invoke(this);
            gameObject.SetActive(false);
        }

        private void OnGUI()
        {
            if (IsCollected || !isPlayerNearby || Camera.main == null) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.85f);
            if (screenPos.z <= 0f) return;

            float width = Mathf.Max(175f, promptText.Length * 8.5f);
            GUI.backgroundColor = new Color(0.12f, 0.1f, 0.04f, 0.9f);
            GUI.contentColor = new Color(1f, 0.92f, 0.35f, 1f);
            GUI.Box(new Rect(screenPos.x - width * 0.5f, Screen.height - screenPos.y - 16f, width, 30f), promptText);
        }
    }
}

