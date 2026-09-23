using UnityEngine;
using DungeonEater.Core;

namespace DungeonEater.Dungeon.Enemies
{
    public class KamikazeBeetle : EnemyBase
    {
        [Header("Kamikaze Settings")]
        [SerializeField] private float triggerDistance = 1.6f;
        [SerializeField] private float blastRadius = 3f;
        [SerializeField] private float blastDamage = 35f;
        [SerializeField] private float primeDuration = 0.9f;
        [SerializeField] private GameObject explosionPrefab;

        private bool isPrimed = false;
        private float primeTimer = 0f;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 28f;
            currentHealth = maxHealth;
            moveSpeed = 3.6f; // Chạy nhanh áp sát người chơi
            dropItem = Core.IngredientType.BombCore;
        }

        protected override void Update()
        {
            base.Update();
            if (playerTransform == null) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (!isPrimed)
            {
                // Chạy nhanh về phía người chơi
                Vector2 dir = (playerTransform.position - transform.position).normalized;
                rb.velocity = dir * moveSpeed;

                if (distance <= triggerDistance)
                {
                    // Kích hoạt ngòi nổ
                    isPrimed = true;
                    primeTimer = primeDuration;
                    rb.velocity = Vector2.zero;
                }
            }
            else
            {
                // Đang đếm ngược ngòi nổ: chớp giật đỏ trắng liên tục
                primeTimer -= Time.deltaTime;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = (Mathf.FloorToInt(Time.time * 20) % 2 == 0) ? Color.white : Color.red;
                }

                if (primeTimer <= 0f)
                {
                    Explode();
                }
            }
        }

        private void Explode()
        {
            SoundManager.Instance?.PlayExplosion();
            CameraFollow.Instance?.TriggerScreenShake(0.25f, 0.4f);

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // Gây sát thương nổ diện rộng (AOE)
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blastRadius);
            foreach (var hit in hits)
            {
                var player = hit.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(blastDamage);
                }
            }

            Die();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, blastRadius);
        }
    }
}

