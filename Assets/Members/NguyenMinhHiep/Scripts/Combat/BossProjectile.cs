using System.Collections;
using UnityEngine;

namespace NguyenMinhHiep.Combat
{
    /// <summary>
    /// Đạn ma pháp / Sóng kiếm / Bào tử độc bắn ra từ các Boss.
    /// Tự động bay theo hướng, kiểm tra va chạm với Người chơi và tường, tự hủy sau thời gian sống.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class BossProjectile : MonoBehaviour
    {
        [Header("Thông số đạn")]
        [SerializeField] private float speed = 7.0f;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float lifeTime = 5.0f;
        [SerializeField] private bool isPiercing = false;
        [SerializeField] private bool rotateTowardVelocity = true;

        private Vector2 moveDirection = Vector2.right;
        private Rigidbody2D rb;
        private CircleCollider2D col;
        private SpriteRenderer sr;
        private bool hasHit = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CircleCollider2D>();
            sr = GetComponent<SpriteRenderer>();

            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            col.isTrigger = true;
            col.radius = 0.35f;

            if (sr != null)
            {
                sr.sortingOrder = 6000; // Projectiles_VFX
            }
        }

        public void Initialize(Vector2 direction, float projSpeed, float projDamage, float duration = 5f, bool piercing = false, Sprite customSprite = null, Color? customColor = null)
        {
            moveDirection = direction.normalized;
            speed = projSpeed;
            damage = projDamage;
            lifeTime = duration;
            isPiercing = piercing;

            if (customSprite != null && sr != null)
            {
                sr.sprite = customSprite;
            }

            if (customColor.HasValue && sr != null)
            {
                sr.color = customColor.Value;
            }

            if (rotateTowardVelocity)
            {
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            rb.velocity = moveDirection * speed;
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit && !isPiercing) return;

            // Bỏ qua va chạm với quái / Boss
            if (other.CompareTag("Enemy") || other.GetComponent<NguyenMinhHiep.Bosses.BossBase>() != null)
            {
                return;
            }

            // Va chạm với Player
            bool isPlayer = other.CompareTag("Player") 
                || other.name.Contains("Player") 
                || other.name.Contains("Hunter")
                || other.GetComponent("HunterPlayer") != null
                || other.GetComponent("PlayerController") != null;

            if (isPlayer)
            {
                hasHit = true;
                // Gửi sát thương qua SendMessage hoặc gọi trực tiếp
                other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

                // Hiệu ứng va chạm nhỏ (Scale pop)
                if (!isPiercing)
                {
                    StartCoroutine(HitAndDestroyRoutine());
                }
            }
            // Va chạm tường / chướng ngại vật
            else if (other.CompareTag("Obstacle") || other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                if (!isPiercing)
                {
                    Destroy(gameObject);
                }
            }
        }

        private IEnumerator HitAndDestroyRoutine()
        {
            rb.velocity = Vector2.zero;
            col.enabled = false;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < 0.1f)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, startScale * 1.4f, elapsed / 0.1f);
                yield return null;
            }

            Destroy(gameObject);
        }

        public static BossProjectile Spawn(Vector3 pos, Vector2 dir, float speed, float dmg, float lifeTime = 4f, Sprite sprite = null, Color? color = null, bool piercing = false)
        {
            var go = new GameObject("BossProjectile");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();

            // Tạo sprite tròn phát sáng mặc định nếu chưa truyền sprite
            if (sprite == null)
            {
                sr.sprite = CreateDefaultProjectileSprite();
            }
            else
            {
                sr.sprite = sprite;
            }

            var proj = go.AddComponent<BossProjectile>();
            proj.Initialize(dir, speed, dmg, lifeTime, piercing, sprite, color);
            return proj;
        }

        private static Sprite CreateDefaultProjectileSprite()
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    if (dist < 14f)
                    {
                        float alpha = Mathf.Clamp01((14f - dist) / 14f);
                        tex.SetPixel(x, y, new Color(1f, 0.9f, 0.3f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }
    }
}

