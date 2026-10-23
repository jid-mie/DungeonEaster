using System.Collections;
using UnityEngine;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// AI Slime Nhầy (Đột kích / Nhảy vồ) theo bảng cân bằng 7.3 TaiLieu.md:
    /// - Máu 40, Tốc độ 2.5, Sát thương 8.
    /// - Nhảy chồm về phía người chơi khi vào cự ly 2 units, có báo hiệu telegraph 0.4s.
    /// - 100% rơi: Chất nhầy dẻo.
    /// </summary>
    public class SlimeAI : EnemyBase
    {
        [Header("--- Cơ chế Nhảy vồ (Lunge) ---")]
        [SerializeField] private float lungeSpeed = 7.5f;
        [SerializeField] private float lungeDuration = 0.25f;
        [SerializeField] private float telegraphTime = 0.4f;

        protected override void Awake()
        {
            maxHealth = 40f;
            moveSpeed = 2.5f;
            attackDamage = 8f;
            attackRange = 2.0f;
            attackCooldown = 1.0f;

            dropItemId = "slime_gel";
            dropItemName = "Chất nhầy dẻo";
            dropCount = 1;

            base.Awake();
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking) return;

            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(LungeAttackRoutine());
            }
            else if (distanceToPlayer <= detectRange)
            {
                currentState = AIState.Chase;
                Vector2 dir = (targetPlayer.transform.position - transform.position).normalized;
                rb.velocity = dir * moveSpeed;
            }
            else
            {
                currentState = AIState.Idle;
                rb.velocity = Vector2.zero;
            }
        }

        private IEnumerator LungeAttackRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;
            rb.velocity = Vector2.zero;

            Vector2 targetPos = targetPlayer.transform.position;
            Vector2 lungeDir = (targetPos - (Vector2)transform.position).normalized;

            // Telegraph 0.4s: Xẹp người xuống nén lực (Squash) + đổi màu báo hiệu
            Vector3 originalScale = transform.localScale;
            transform.localScale = new Vector3(originalScale.x * 1.25f, originalScale.y * 0.7f, originalScale.z);

            // Bật anim attack nếu có
            TrySetAnimBool("IsAttack", true);
            if (anim != null) anim.Play("slimeattack", 0, 0f);

            yield return new WaitForSeconds(telegraphTime);

            // Phóng chồm tới (Lunge)
            transform.localScale = originalScale;
            rb.velocity = lungeDir * lungeSpeed;

            float timer = 0f;
            bool hitPlayer = false;

            while (timer < lungeDuration)
            {
                timer += Time.deltaTime;
                if (!hitPlayer && targetPlayer != null)
                {
                    float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
                    if (dist <= 0.9f)
                    {
                        hitPlayer = true;
                        targetPlayer.TakeDamage(attackDamage);
                    }
                }
                yield return null;
            }

            rb.velocity = Vector2.zero;
            TrySetAnimBool("IsAttack", false);
            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = AIState.Idle;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Sát thương va chạm phụ khi đang phóng tới
            if (isAttacking && collision.gameObject.CompareTag("Player"))
            {
                var p = collision.gameObject.GetComponent<HunterPlayer>();
                if (p != null) p.TakeDamage(attackDamage);
            }
        }
    }
}
