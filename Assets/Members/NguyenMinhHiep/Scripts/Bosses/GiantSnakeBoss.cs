using System.Collections;
using UnityEngine;
using NguyenMinhHiep.Combat;

namespace NguyenMinhHiep.Bosses
{
    /// <summary>
    /// Boss Tầng 1: RẮN KHỔNG LỒ (Chúa tể hang tối) theo TaiLieu.md mục 7.4:
    /// - Máu (HP): 450.
    /// - Sát thương (ATK): 20 - 25.
    /// - Tốc độ chạy: 3.2 u/s.
    /// - Kỹ năng 1: Quét đuôi 180° phạm vi 3 units (20 DMG, telegraph 0.4s).
    /// - Kỹ năng 2: Cắn bổ nhào về phía trước (25 DMG, telegraph 0.5s).
    /// - Kỹ năng tầm xa: Phun cầu độc ma quái khi người chơi kiting ở xa.
    /// - Rơi nguyên liệu: 100% rơi 1x Thịt nạc Rắn khổng lồ.
    /// </summary>
    public class GiantSnakeBoss : BossBase
    {
        [Header("--- Kỹ năng 1: Quét Đuôi (Tail Sweep 180°) ---")]
        [SerializeField] private float sweepRadius = 3.2f;
        [SerializeField] private float sweepDamage = 20f;
        [SerializeField] private float sweepTelegraph = 0.4f;

        [Header("--- Kỹ năng 2: Cắn Bổ Nhào (Poison Lunge) ---")]
        [SerializeField] private float lungeSpeed = 9.0f;
        [SerializeField] private float lungeDamage = 25f;
        [SerializeField] private float lungeTelegraph = 0.5f;
        [SerializeField] private float lungeDuration = 0.45f;

        [Header("--- Kỹ năng 3: Phun Độc Tầm Xa (Poison Spit) ---")]
        [SerializeField] private float spitDamage = 18f;
        [SerializeField] private float spitSpeed = 6.5f;

        protected override void Awake()
        {
            bossName = "Rắn Khổng Lồ";
            bossTitle = "Chúa tể hang tối";
            dungeonFloor = 1;

            maxHealth = 450f;
            moveSpeed = 3.2f;
            attackDamage = 20f;
            detectRange = 15f;
            attackRange = 3.0f;
            attackCooldown = 2.4f;

            dropItemId = "snake_meat";
            dropItemName = "Thịt nạc Rắn khổng lồ";
            dropCount = 1;

            base.Awake();
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking) return;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                // Chọn đòn đánh phù hợp theo khoảng cách
                if (distanceToPlayer <= sweepRadius + 0.5f)
                {
                    if (Random.value < 0.55f)
                        StartCoroutine(TailSweepRoutine());
                    else
                        StartCoroutine(PoisonLungeRoutine());
                }
                else if (distanceToPlayer <= 7.0f)
                {
                    StartCoroutine(PoisonLungeRoutine());
                }
                else
                {
                    StartCoroutine(PoisonSpitRoutine());
                }
            }
            else
            {
                // Áp sát người chơi
                currentState = BossState.Chase;
                Vector2 dir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;
                rb.velocity = dir * moveSpeed;
            }
        }

        /// <summary>
        /// Kỹ năng 1: Quét đuôi hình quạt 180 độ
        /// </summary>
        private IEnumerator TailSweepRoutine()
        {
            isAttacking = true;
            currentState = BossState.Attack;
            rb.velocity = Vector2.zero;

            Vector2 dir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;

            // Telegraph hình nón 180 độ
            BossAoEIndicator.CreateCone(transform.position, dir, sweepRadius, 180f, sweepTelegraph, new Color(0.3f, 0.9f, 0.3f, 0.7f));
            if (sr != null) sr.color = new Color(0.6f, 1f, 0.6f);

            yield return new WaitForSeconds(sweepTelegraph);
            if (sr != null) sr.color = Color.white;

            // Kiểm tra va chạm góc 180 độ
            if (targetPlayer != null)
            {
                Vector2 toPlayer = (Vector2)targetPlayer.position - (Vector2)transform.position;
                if (toPlayer.magnitude <= sweepRadius && Vector2.Dot(dir, toPlayer.normalized) >= 0f)
                {
                    DealDamageToPlayer(sweepDamage);
                }
            }

            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = BossState.Idle;
        }

        /// <summary>
        /// Kỹ năng 2: Cắn bổ nhào về phía trước
        /// </summary>
        private IEnumerator PoisonLungeRoutine()
        {
            isAttacking = true;
            currentState = BossState.Attack;
            rb.velocity = Vector2.zero;

            Vector2 lungeDir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;

            // Telegraph đường thẳng
            BossAoEIndicator.CreateLine(transform.position, lungeDir, 4.5f, 1.2f, lungeTelegraph, new Color(0.8f, 0.2f, 0.8f, 0.7f));
            if (sr != null) sr.color = new Color(0.9f, 0.4f, 1f);

            yield return new WaitForSeconds(lungeTelegraph);
            if (sr != null) sr.color = Color.white;

            // Phóng tới với tốc độ cao
            rb.velocity = lungeDir * lungeSpeed;
            float timer = 0f;
            bool hit = false;

            while (timer < lungeDuration)
            {
                timer += Time.deltaTime;
                if (!hit && targetPlayer != null)
                {
                    if (Vector2.Distance(transform.position, targetPlayer.position) <= 1.4f)
                    {
                        hit = true;
                        DealDamageToPlayer(lungeDamage);
                    }
                }
                yield return null;
            }

            rb.velocity = Vector2.zero;
            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = BossState.Idle;
        }

        /// <summary>
        /// Kỹ năng 3: Bắn cầu độc tím tầm xa
        /// </summary>
        private IEnumerator PoisonSpitRoutine()
        {
            isAttacking = true;
            currentState = BossState.Attack;
            rb.velocity = Vector2.zero;

            if (sr != null) sr.color = new Color(0.8f, 0.3f, 0.9f);
            yield return new WaitForSeconds(0.4f);
            if (sr != null) sr.color = Color.white;

            if (targetPlayer != null)
            {
                Vector2 dir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;
                BossProjectile.Spawn(transform.position, dir, spitSpeed, spitDamage, 4f, null, new Color(0.7f, 0.2f, 0.9f, 1f));
            }

            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = BossState.Idle;
        }
    }
}

