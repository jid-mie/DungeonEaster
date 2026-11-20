using UnityEngine;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Key dịch chuyển ở Tầng 3 (gắn lên CẢ 12 object Key nằm trong Dungeon3/Key).
    /// - Khi người chơi lại gần và nhấn phím [F], Key báo về Floor3TeleportManager để dịch chuyển:
    ///   + 3 Key thật: nhảy thẳng vào Phòng Boss Tầng 3.
    ///   + 9 Key còn lại: nhảy sang vị trí 1 Key ở phòng khác.
    /// </summary>
    public class Floor3TeleportKey : MonoBehaviour
    {
        [Header("--- Phân nhóm Phòng (Tùy chọn) ---")]
        [Tooltip("Nếu nhiều Key nằm trong cùng 1 phòng, điền cùng số Room ID (VD: 1, 2, 3...) để không bao giờ dịch chuyển loanh quanh trong cùng 1 phòng. Để 0 nếu mỗi Key nằm riêng phòng.")]
        [SerializeField] private int roomId = 0;

        [Header("--- Tương tác ---")]
        [SerializeField] private float interactRadius = 1.4f;
        [SerializeField] private KeyCode interactKey = KeyCode.F;
        [SerializeField] private string promptText = "Nhấn [F] Kích hoạt Ấn Ký";

        [Header("--- Vị trí đáp an toàn & Hiển thị ---")]
        [Tooltip("Độ lệch so với tâm Key khi Player dịch chuyển tới Key này (tránh kẹt vào collider)")]
        [SerializeField] private Vector2 arrivalOffset = new Vector2(0f, -0.75f);
        [Tooltip("Màu nhuộm nhẹ sau khi người chơi đã thử kích hoạt Key này")]
        [SerializeField] private Color visitedTint = new Color(0.55f, 0.8f, 1f, 0.88f);

        private Floor3TeleportManager manager;
        private HunterPlayer hunter;
        private SpriteRenderer sr;
        private bool isPlayerNearby = false;
        private bool hasBeenVisited = false;

        public int RoomId => roomId;
        public bool IsBossPortal { get; internal set; } = false;
        public bool HasBeenVisited => hasBeenVisited;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        internal void BindManager(Floor3TeleportManager ownerManager)
        {
            manager = ownerManager;
        }

        public Vector3 GetArrivalPosition()
        {
            return transform.position + new Vector3(arrivalOffset.x, arrivalOffset.y, 0f);
        }

        public void MarkVisited()
        {
            if (hasBeenVisited) return;
            hasBeenVisited = true;
            if (sr != null)
            {
                sr.color = visitedTint;
            }
        }

        private void Update()
        {
            if (manager == null) return;

            if (hunter == null)
            {
                hunter = FindObjectOfType<HunterPlayer>();
                if (hunter == null) return;
            }

            isPlayerNearby = CheckPlayerNearby();

            if (isPlayerNearby && Input.GetKeyDown(interactKey))
            {
                manager.TriggerTeleportFrom(this, hunter);
            }
        }

        private bool CheckPlayerNearby()
        {
            if (manager.IsOnTeleportCooldown) return false;

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

        private void OnGUI()
        {
            if (!isPlayerNearby || manager == null || manager.IsOnTeleportCooldown || Camera.main == null) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.85f);
            if (screenPos.z <= 0f) return;

            string displayPrompt = hasBeenVisited ? $"{promptText} (Đã thử)" : promptText;
            float width = Mathf.Max(195f, displayPrompt.Length * 8.2f);
            GUI.backgroundColor = new Color(0.08f, 0.1f, 0.18f, 0.92f);
            GUI.contentColor = new Color(0.65f, 0.92f, 1f, 1f);
            GUI.Box(new Rect(screenPos.x - width * 0.5f, Screen.height - screenPos.y - 16f, width, 30f), displayPrompt);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(GetArrivalPosition(), 0.22f);
        }
    }
}

