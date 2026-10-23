using System.Collections;
using UnityEngine;

namespace NguyenVanHai.Combat
{
    /// <summary>
    /// Mũi tên do Hunter bắn ra từ Cung (Bow), bay theo hướng trỏ chuột.
    /// Gây sát thương và hiệu ứng đẩy lùi nhẹ khi chạm trúng quái vật.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class ArrowProjectile : MonoBehaviour
    {
        [Header("Thông số đường đạn")]
        [SerializeField] private float speed = 14f;
        [SerializeField] private float damage = 15f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private float knockbackForce = 2.5f;
        [SerializeField] private bool isPiercing = false;

        private Vector2 moveDirection;
        private Rigidbody2D rb;
        private bool hasHit = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        public void Initialize(Vector2 direction, float arrowDamage, float arrowSpeed = 14f, bool piercing = false)
        {
            moveDirection = direction.normalized;
            damage = arrowDamage;
            speed = arrowSpeed;
            isPiercing = piercing;

            // Xoay mũi tên hướng theo đường bay
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            rb.velocity = moveDirection * speed;
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Bỏ qua va chạm với Player hoặc đạn khác
            if (other.CompareTag("Player") || other.GetComponent<ArrowProjectile>() != null)
            {
                return;
            }

            // Gây sát thương cho quái vật
            var enemy = other.GetComponentInParent<NguyenVanHai.Enemies.EnemyBase>();
            if (enemy != null && enemy.IsAlive)
            {
                enemy.TakeDamage(damage, moveDirection * knockbackForce);

                if (!isPiercing)
                {
                    hasHit = true;
                    Destroy(gameObject);
                }
                return;
            }

            // Va chạm với vật cản / tường
            if (!other.isTrigger && !hasHit)
            {
                hasHit = true;
                rb.velocity = Vector2.zero;
                Destroy(gameObject, 0.05f);
            }
        }
    }
}
