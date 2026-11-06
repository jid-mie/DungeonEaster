using System.Collections;
using UnityEngine;
using NguyenMinhHiep.Combat;

namespace NguyenMinhHiep.Bosses
{
    /// <summary>
    /// Boss Tầng 3: NGƯỜI ĐÁ KHỔNG LỒ (Hộ vệ băng nguyên) theo TaiLieu.md mục 7.4:
    /// - Máu (HP): 1200 (Rất trâu, có nội tại Giáp Đá).
    /// - Sát thương (ATK): 40 - 50.
    /// - Tốc độ chạy: 1.6 u/s (Chậm rãi, nặng nề nhưng cực kỳ uy lực).
    /// - Kỹ năng 1: Nện 2 tay xuống đất tạo sóng lan tỏa (45 DMG, telegraph 0.8s, bán kính 4.5m).
    /// - Kỹ năng 2: Quét tay hình nón phía trước (40 DMG, telegraph 0.5s, góc 120 độ).
    /// - Kỹ năng tầm xa: Ném tảng đá khổng lồ khi mục tiêu ở xa.
    /// - Rơi nguyên liệu: 100% rơi 1x Tủy đá nguyên khối.
    /// </summary>
    public class StoneGolemBoss : BossBase
    {
        [Header("--- Kỹ năng 1: Nện Đất Chấn Động (Ground Slam AoE) ---")]
        [SerializeField] private float slamRadius = 4.5f;
        [SerializeField] private float slamDamage = 45f;
        [SerializeField] private float slamTelegraph = 0.8f;

        [Header("--- Kỹ năng 2: Quét Tay Hình Nón (Cone Arm Sweep) ---")]
        [SerializeField] private float sweepRadius = 2.8f;
        [SerializeField] private float sweepDamage = 40f;
        [SerializeField] private float sweepTelegraph = 0.5f;

        [Header("--- Kỹ năng 3: Ném Tảng Đá (Rock Boulder Throw) ---")]
        [SerializeField] private float boulderDamage = 35f;
        [SerializeField] private float boulderSpeed = 6.0f;

        [Header("--- Nội tại: Giáp Đá (Stone Armor) ---")]
        [SerializeField] private float damageReductionPercent = 0.20f;

        protected override void Awake()
        {
            bossName = "Người Đá Khổng Lồ";
            bossTitle = "Hộ vệ băng nguyên";
            dungeonFloor = 3;

            maxHealth = 1200f;
            moveSpeed = 1.6f;
            attackDamage = 40f;
            detectRange = 16f;
            attackRange = 3.5f;
            attackCooldown = 3.2f;

            dropItemId = "golem_core";
            dropItemName = "Tủy đá nguyên khối";
            dropCount = 1;

            base.Awake();
        }

        public override void TakeDamage(float dmg, Vector2 knockback = default)
        {
            // Nội tại Giáp Đá: Giảm 20% sát thương khi máu trên 50%
            if (HealthPercent > 0.5f)
            {
                dmg *= (1f - damageReductionPercent);
            }
            // Kháng hất văng 80% do cơ thể đá đồ sộ
            base.TakeDamage(dmg, knockback * 0.2f);
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking) return;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                if (distanceToPlayer <= sweepRadius + 0.5f)
                {
                    if (Random.value < 0.6f)
                        StartCoroutine(ConeSweepRoutine());
                    else
                        StartCoroutine(DoubleSlamRoutine());
                }
                else if (distanceToPlayer <= slamRadius + 0.5f)
                {
                    StartCoroutine(DoubleSlamRoutine());
                }
                else
                {
                    StartCoroutine(RockThrowRoutine());
                }
            }
            else
            {
                // Bước đi nặng nhọc đuổi theo người chơi
                currentState = BossState.Chase;
                Vector2 dir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;
                rb.velocity = dir * moveSpeed;
            }
        }

        /// <summary>
        /// Kỹ năng 1: Nện 2 tay xuống đất tạo sóng lan tỏa AoE bán kính 4.5m
        /// </summary>
        private IEnumerator DoubleSlamRoutine()
        {
            isAttacking = true;
            currentState = BossState.Attack;
            rb.velocity = Vector2.zero;

            // Telegraph vòng tròn
            BossAoEIndicator.CreateCircle(transform.position, slamRadius, slamTelegraph, new Color(0.2f, 0.7f, 1f, 0.75f));

            // Nhún tay lên cao báo hiệu
            Vector3 origScale = transform.localScale;
            transform.localScale = new Vector3(origScale.x * 0.9f, origScale.y * 1.25f, origScale.z);
            yield return new WaitForSeconds(slamTelegraph);
            transform.localScale = origScale;

            // Nện xuống đất
            if (targetPlayer != null)
            {
                float dist = Vector2.Distance(transform.position, targetPlayer.position);
                if (dist <= slamRadius)
                {
                    DealDamageToPlayer(slamDamage);
                }
            }

            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = BossState.Idle;
        }

        /// <summary>
        /// Kỹ năng 2: Quét tay hình nón góc 120 độ cự ly 2.8m
        /// </summary>
        private IEnumerator ConeSweepRoutine()
        {
            isAttacking = true;
            currentState = BossState.Attack;
            rb.velocity = Vector2.zero;

            Vector2 dir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;

            // Telegraph hình nón 120 độ
            BossAoEIndicator.CreateCone(transform.position, dir, sweepRadius, 120f, sweepTelegraph, new Color(1f, 0.6f, 0.2f, 0.75f));
            yield return new WaitForSeconds(sweepTelegraph);

            if (targetPlayer != null)
            {
                Vector2 toPlayer = (Vector2)targetPlayer.position - (Vector2)transform.position;
                if (toPlayer.magnitude <= sweepRadius && Vector2.Dot(dir, toPlayer.normalized) >= Mathf.Cos(60f * Mathf.Deg2Rad))
                {
                    DealDamageToPlayer(sweepDamage);
                }
            }

            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = BossState.Idle;
        }

        /// <summary>
        /// Kỹ năng 3: Ném tảng đá lớn tầm xa
        /// </summary>
        private IEnumerator RockThrowRoutine()
        {
            isAttacking = true;
            currentState = BossState.Attack;
            rb.velocity = Vector2.zero;

            if (sr != null) sr.color = new Color(0.8f, 0.85f, 1f);
            yield return new WaitForSeconds(0.6f);
            if (sr != null) sr.color = Color.white;

            if (targetPlayer != null)
            {
                Vector2 dir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;
                BossProjectile.Spawn(transform.position, dir, boulderSpeed, boulderDamage, 4.5f, null, new Color(0.45f, 0.5f, 0.6f, 1f));
            }

            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = BossState.Idle;
        }
    }
}

