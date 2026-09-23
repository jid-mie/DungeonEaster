using UnityEngine;

namespace DungeonEater.Dungeon.Enemies
{
    public class RangedSpore : EnemyBase
    {
        [Header("Ranged Spore Settings")]
        [SerializeField] private float preferredDistance = 4.5f;
        [SerializeField] private float telegraphDuration = 0.5f; // 0.5s telegraph theo GDD
        [SerializeField] private float shootCooldown = 2.2f;
        [SerializeField] private GameObject sporeProjectilePrefab;
        [SerializeField] private LineRenderer aimLineRenderer;

        private float currentTelegraph = 0f;
        private float currentCooldown = 1f;
        private Vector2 aimDirection;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 35f;
            currentHealth = maxHealth;
            moveSpeed = 1.6f;
            dropItem = Core.IngredientType.CaveMushroom;
        }

        protected override void Update()
        {
            base.Update();
            if (playerTransform == null) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            aimDirection = (playerTransform.position - transform.position).normalized;

            if (currentCooldown > 0f) currentCooldown -= Time.deltaTime;

            if (currentTelegraph > 0f)
            {
                currentTelegraph -= Time.deltaTime;
                rb.velocity = Vector2.zero;

                // Telegraph visual (đổi màu cảnh báo ngắm bắn)
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.magenta;
                }

                if (currentTelegraph <= 0f)
                {
                    ShootSpore();
                    if (spriteRenderer != null) spriteRenderer.color = originalColor;
                    currentCooldown = shootCooldown;
                }
                return;
            }

            // Giữ cự ly với người chơi
            if (distance < preferredDistance - 0.5f)
            {
                // Lùi ra xa nếu người chơi đến quá gần
                rb.velocity = -aimDirection * moveSpeed;
            }
            else if (distance > preferredDistance + 1.2f)
            {
                // Tiến lại gần nếu quá xa
                rb.velocity = aimDirection * moveSpeed;
            }
            else
            {
                rb.velocity = Vector2.zero;
                if (currentCooldown <= 0f)
                {
                    // Bắt đầu nhắm bắn telegraph
                    currentTelegraph = telegraphDuration;
                }
            }
        }

        private void ShootSpore()
        {
            GameObject projObj = null;
            if (sporeProjectilePrefab != null)
            {
                projObj = Instantiate(sporeProjectilePrefab, transform.position, Quaternion.identity);
            }
            else
            {
                projObj = new GameObject("SporeBullet");
                projObj.transform.position = transform.position;
                var col = projObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.25f;
                var sr = projObj.AddComponent<SpriteRenderer>();
                sr.color = Color.magenta;
                projObj.AddComponent<Projectile>();
            }

            var proj = projObj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(aimDirection, 8f, 12f, 1, false);
            }
        }
    }
}

