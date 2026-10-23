using System.Collections;
using UnityEngine;

namespace NguyenVanHai.Combat
{
    /// <summary>
    /// Đạn bào tử độc do quái Nấm (Spore) bắn ra về phía Player theo bảng cân bằng TaiLieu.md mục 7.3.
    /// Sát thương thuần, không kèm hiệu ứng độc kéo dài.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class SporeProjectile : MonoBehaviour
    {
        [Header("Thông số đạn")]
        [SerializeField] private float speed = 5.5f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float lifeTime = 4f;

        private Vector2 moveDirection;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        public void Initialize(Vector2 direction, float dmg, float spd = 5.5f)
        {
            moveDirection = direction.normalized;
            damage = dmg;
            speed = spd;

            rb.velocity = moveDirection * speed;
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Tránh va chạm với quái khác
            if (other.GetComponentInParent<NguyenVanHai.Enemies.EnemyBase>() != null)
            {
                return;
            }

            // Gây sát thương cho Player
            var player = other.GetComponent<HunterPlayer>();
            if (player == null)
            {
                player = other.GetComponentInParent<HunterPlayer>();
            }

            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            // Chạm tường / vật cản
            if (!other.isTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}
