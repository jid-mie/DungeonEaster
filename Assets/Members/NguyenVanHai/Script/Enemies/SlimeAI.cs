using System.Collections;
using UnityEngine;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// AI Slime Nhầy (Đột kích / Nhảy vồ):
    /// - Máu 40, Tốc độ 2.5, Sát thương 8.
    /// - Trước khi nhảy vồ: Đứng yên hóa Đỏ Rực Cảnh Báo trong 0.80s.
    /// - Sau khi nhảy vồ: Đứng nghỉ tại chỗ 1.25s rồi mới truy đuổi tiếp.
    /// </summary>
    public class SlimeAI : EnemyBase
    {
        [Header("--- Cơ chế Nhảy vồ (Lunge) ---")]
        [SerializeField] private float lungeSpeed = 7.0f;
        [SerializeField] private float lungeDuration = 0.28f;
        [SerializeField] private float telegraphTime = 0.80f;
        [SerializeField] private float restAfterAttack = 1.25f;

        protected override void Awake()
        {
            isBoss = false;
            var cfg = NguyenVanHai.Data.DungeonCombatDatabase.GetEnemy("slime");
            if (cfg != null)
            {
                maxHealth = cfg.maxHealth;
                moveSpeed = cfg.moveSpeed;
                attackDamage = cfg.attackDamage;
                detectRange = cfg.detectRange;
                attackRange = cfg.attackRange;
                attackCooldown = cfg.attackCooldown;
                telegraphTime = cfg.telegraphTime;
                restAfterAttack = cfg.restAfterAttack;
                lungeSpeed = cfg.lungeSpeed;
                lungeDuration = cfg.lungeDuration;
                dropItemId = cfg.dropItemId;
                dropItemName = cfg.dropItemName;
                dropCount = cfg.dropCount;
            }

            base.Awake();
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking || Time.time < postAttackRestUntil) return;

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

            TrySetAnimBool("IsAttack", true);
            if (anim != null) anim.Play("slimeattack", 0, 0f);

            // 1. Cảnh báo Đỏ Rực theo thời gian telegraphTime từ JSON
            yield return StartCoroutine(PlayRedWarningRoutine(telegraphTime, 1.18f));

            // 2. Phóng chồm tới (Lunge)
            float timer = 0f;
            bool hitPlayer = false;

            while (timer < lungeDuration && IsAlive)
            {
                timer += Time.deltaTime;
                rb.velocity = lungeDir * lungeSpeed;

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

            // 3. Quãng nghỉ bắt buộc theo restAfterAttack từ JSON
            yield return StartCoroutine(EnterPostAttackRestRoutine(restAfterAttack));
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isAttacking && Time.time >= postAttackRestUntil && collision.gameObject.CompareTag("Player"))
            {
                var p = collision.gameObject.GetComponent<HunterPlayer>();
                if (p != null) p.TakeDamage(attackDamage);
            }
        }
    }
}
