using System.Collections;
using UnityEngine;

namespace NguyenMinhHiep.Combat
{
    /// <summary>
    /// Vật phẩm rơi từ Boss theo bảng ma trận 7.2 và 7.4 TaiLieu.md.
    /// Có hiệu ứng nảy vòng cung, phát sáng lấp lánh (Boss loot đặc biệt) và tự động bay vào túi đồ người chơi.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
    public class BossLootItem : MonoBehaviour
    {
        [Header("Thông tin nguyên liệu")]
        [SerializeField] private string itemId = "snake_meat";
        [SerializeField] private string itemName = "Thịt nạc Rắn khổng lồ";
        [SerializeField] private int amount = 1;
        [SerializeField] private Sprite itemIcon;

        [Header("Khoảng cách nhặt")]
        [SerializeField] private float magnetDistance = 2.8f;
        [SerializeField] private float pickupDistance = 0.9f;
        [SerializeField] private float flySpeed = 8.5f;

        private Transform playerTarget;
        private SpriteRenderer sr;
        private bool isCollected = false;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = pickupDistance;
            sr.sortingOrder = 4500;
        }

        public void Setup(string id, string name, int count, Sprite icon = null)
        {
            itemId = id;
            itemName = name;
            amount = count;
            if (icon != null)
            {
                itemIcon = icon;
                if (sr != null) sr.sprite = icon;
            }
        }

        private void Start()
        {
            StartCoroutine(SpawnBounceRoutine());
            StartCoroutine(GlowPulseRoutine());
        }

        private IEnumerator SpawnBounceRoutine()
        {
            Vector3 startPos = transform.position;
            Vector3 targetOffset = (Vector3)(Random.insideUnitCircle.normalized * Random.Range(0.8f, 1.4f));
            Vector3 peakPos = startPos + targetOffset * 0.5f + Vector3.up * 1.2f;
            Vector3 landPos = startPos + targetOffset;

            float elapsed = 0f;
            float duration = 0.45f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                Vector3 m1 = Vector3.Lerp(startPos, peakPos, t);
                Vector3 m2 = Vector3.Lerp(peakPos, landPos, t);
                transform.position = Vector3.Lerp(m1, m2, t);
                yield return null;
            }

            transform.position = landPos;
        }

        private IEnumerator GlowPulseRoutine()
        {
            // Nhấp nháy ánh vàng/cam huyền thoại cho đồ Boss
            Color normalColor = sr != null ? sr.color : Color.white;
            Color glowColor = new Color(1f, 0.9f, 0.4f, 1f);

            while (!isCollected)
            {
                float t = (Mathf.Sin(Time.time * 5f) + 1f) * 0.5f;
                if (sr != null) sr.color = Color.Lerp(normalColor, glowColor, t * 0.6f);
                yield return null;
            }
        }

        private void Update()
        {
            if (isCollected) return;

            if (playerTarget == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTarget = p.transform;
                else
                {
                    var hp = FindObjectOfType<NguyenVanHai.HunterPlayer>();
                    if (hp != null) playerTarget = hp.transform;
                }
            }

            if (playerTarget != null)
            {
                float dist = Vector2.Distance(transform.position, playerTarget.position);
                if (dist <= magnetDistance)
                {
                    transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, flySpeed * Time.deltaTime);

                    if (dist <= pickupDistance)
                    {
                        Collect();
                    }
                }
            }
        }

        private void Collect()
        {
            if (isCollected) return;
            isCollected = true;

            // 1. Thêm vào DungeonInventoryUI nếu có
            var dungeonInv = FindObjectOfType<NguyenVanHai.UI.DungeonInventoryUI>();
            if (dungeonInv != null)
            {
                dungeonInv.AddItem(itemId, itemName, amount, itemIcon);
            }

            // 2. Thêm vào PlayerInventory nếu có
            var playerInv = FindObjectOfType<NgoVanHien.Player.PlayerInventory>();
            if (playerInv != null)
            {
                // Thử thêm vào hòm đồ chung
                Debug.Log($"[BossLoot] Đã nhặt nguyên liệu Boss {itemName} (x{amount}) vào túi đồ.");
            }

            Destroy(gameObject);
        }

        public static BossLootItem Spawn(Vector3 pos, string id, string name, int count = 1, Sprite icon = null)
        {
            var go = new GameObject($"BossLoot_{id}");
            go.transform.position = pos;
            var loot = go.AddComponent<BossLootItem>();
            loot.Setup(id, name, count, icon);
            return loot;
        }
    }
}

