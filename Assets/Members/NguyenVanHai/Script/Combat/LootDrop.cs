using System.Collections;
using UnityEngine;

namespace NguyenVanHai.Combat
{
    /// <summary>
    /// Vật phẩm rơi ra trên sàn khi tiêu diệt quái theo bảng Ma trận Nguyên liệu 7.1.
    /// Có hiệu ứng nảy lên khi xuất hiện, bay vào túi đồ khi người chơi lại gần hoặc bấm F.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
    public class LootDrop : MonoBehaviour
    {
        [Header("Thông tin vật phẩm")]
        [SerializeField] private string itemId = "slime_gel";
        [SerializeField] private string itemName = "Chất nhầy dẻo";
        [SerializeField] private int amount = 1;
        [SerializeField] private Sprite itemIcon;

        [Header("Khoảng cách nhặt")]
        [SerializeField] private float magnetDistance = 2.0f;
        [SerializeField] private float pickupDistance = 0.8f;
        [SerializeField] private float flySpeed = 7f;

        private Transform playerTarget;
        private SpriteRenderer sr;
        private bool isCollected = false;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = pickupDistance;
        }

        private void Start()
        {
            StartCoroutine(SpawnBounceRoutine());
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

        private IEnumerator SpawnBounceRoutine()
        {
            Vector3 startPos = transform.position;
            Vector3 targetOffset = (Vector3)(Random.insideUnitCircle * 0.7f);
            Vector3 peakPos = startPos + targetOffset * 0.5f + Vector3.up * 0.6f;
            Vector3 landPos = startPos + targetOffset;

            float elapsed = 0f;
            float duration = 0.35f;

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

        private void Update()
        {
            if (isCollected) return;

            if (playerTarget == null)
            {
                var p = FindObjectOfType<HunterPlayer>();
                if (p != null) playerTarget = p.transform;
            }

            if (playerTarget != null)
            {
                float dist = Vector2.Distance(transform.position, playerTarget.position);
                if (dist <= magnetDistance)
                {
                    // Tự động bay về phía Player
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

            // Thêm vào hòm đồ Dungeon (hoặc PlayerInventory)
            var invUI = FindObjectOfType<NguyenVanHai.UI.DungeonInventoryUI>();
            if (invUI != null)
            {
                invUI.AddItem(itemId, itemName, amount, itemIcon);
            }

            Destroy(gameObject);
        }
    }
}
