using UnityEngine;
using DungeonEater.Core;

namespace DungeonEater.Dungeon.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] protected float maxHealth = 50f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float contactDamage = 10f;

        [Header("Loot Drop Settings")]
        [SerializeField] protected IngredientType dropItem = IngredientType.MonsterMeat;
        [SerializeField] protected GameObject lootPrefab;

        [Header("Hit Feedback")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        protected Color originalColor = Color.white;
        protected float hitFlashTimer = 0f;

        protected Transform playerTransform;
        protected Rigidbody2D rb;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
            currentHealth = maxHealth;
        }

        protected virtual void Start()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null) playerTransform = player.transform;
        }

        protected virtual void Update()
        {
            if (hitFlashTimer > 0f)
            {
                hitFlashTimer -= Time.deltaTime;
                if (hitFlashTimer <= 0f && spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }

            // Tự động lật hướng nhìn theo vận tốc di chuyển ngang (2 hướng Trái / Phải)
            if (spriteRenderer != null && rb != null && Mathf.Abs(rb.velocity.x) > 0.05f)
            {
                spriteRenderer.flipX = (rb.velocity.x < 0f);
            }
        }

        public virtual void TakeDamage(float damage, Vector2 knockback)
        {
            currentHealth -= damage;
            SoundManager.Instance?.PlayHit();

            // Hit feedback: chớp trắng/đỏ và knockback
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
                hitFlashTimer = 0.12f;
            }

            if (rb != null)
            {
                rb.AddForce(knockback, ForceMode2D.Impulse);
            }

            Debug.Log($"[Enemy {gameObject.name}] Nhận {damage} sát thương. Còn {currentHealth}/{maxHealth} HP.");

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            SpawnLoot();
            DungeonManager.Instance?.OnEnemyKilled(this);
            Destroy(gameObject);
        }

        protected virtual void SpawnLoot()
        {
            GameObject lootObj = null;
            if (lootPrefab != null)
            {
                lootObj = Instantiate(lootPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                // Fallback runtime primitive
                lootObj = new GameObject("Loot_" + dropItem);
                lootObj.transform.position = transform.position;
                var sr = lootObj.AddComponent<SpriteRenderer>();
                sr.color = Color.magenta;
                var col = lootObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                var loot = lootObj.AddComponent<LootItem>();
                loot.SetItemType(dropItem);
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
            }
        }
    }
}

