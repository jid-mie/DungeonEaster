using System.Collections;
using UnityEngine;
using NguyenMinhHiep.Combat;

namespace NguyenMinhHiep.Bosses
{
    /// <summary>
    /// Boss Tầng 5 (Boss Cuối): GWYN - THẦN SÁNG TẠO theo TaiLieu.md mục 7.4:
    /// - Máu (HP): 2500 (Lượng máu khổng lồ nhất trò chơi).
    /// - Sát thương (ATK): 55 - 75.
    /// - Tốc độ chạy: 3.5 u/s.
    /// - Kỹ năng 1: Bắn chùm đạn ma pháp toàn sàn (55 DMG, 12 viên ma pháp hình sao 360 độ).
    /// - Kỹ năng 2: Triệu hồi cột năng lượng dội từ trần nhà (75 DMG, 3-4 cột năng lượng thiên phạt).
    /// - Kỹ năng 3: Chém sóng kiếm càn quét diện rộng (65 DMG, sóng trảm hình trăng khuyết bay xuyên sàn).
    /// - Phase 2 (HP < 50%): Hóa Cuồng Thần Thánh (Divine Ascendance), hồi chiêu giảm 35%, dịch chuyển tức thời.
    /// - Rơi nguyên liệu: 100% rơi 1x Hạt nhân Sinh Mệnh (Vật phẩm Huyền thoại).
    /// </summary>
    public class GwynFinalBoss : BossBase
    {
        [Header("--- Kỹ năng 1: Mưa Đạn Ma Pháp (Bullet Hell) ---")]
        [SerializeField] private int bulletCount = 12;
        [SerializeField] private float bulletSpeed = 6.5f;
        [SerializeField] private float bulletDamage = 55f;

        [Header("--- Kỹ năng 2: Cột Năng Lượng Thiên Phạt (Celestial Pillar) ---")]
        [SerializeField] private int pillarCount = 4;
        [SerializeField] private float pillarRadius = 2.2f;
        [SerializeField] private float pillarDamage = 75f;
        [SerializeField] private float pillarDelay = 0.75f;

        [Header("--- Kỹ năng 3: Sóng Kiếm Càn Quét (Crescent Sword Wave) ---")]
        [SerializeField] private float waveSpeed = 8.0f;
        [SerializeField] private float waveDamage = 65f;

        protected override void Awake()
        {
            bossName = "Gwyn";
            bossTitle = "Thần sáng tạo (Boss Cuối)";
            dungeonFloor = 5;

            maxHealth = 2500f;
            moveSpeed = 3.5f;
            attackDamage = 60f;
            detectRange = 25f;
            attackRange = 6.0f;
            attackCooldown = 3.0f;

            dropItemId = "life_core";
            dropItemName = "Hạt nhân Sinh Mệnh";
            dropCount = 1;

            base.Awake();
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking) return;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                // Chọn ngẫu nhiên chuỗi kỹ năng của Boss Cuối
                float roll = Random.value;
                if (roll < 0.35f)
                {
                    StartCoroutine(BulletHellRoutine());
                }
                else if (roll < 0.70f)
                {
                    StartCoroutine(CelestialPillarsRoutine());
                }
                else
                {
                    StartCoroutine(SwordWaveRoutine());
                }
            }
            else
            {
                // Bay lượn áp sát người chơi
                currentState = BossState.Chase;
                Vector2 dir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;
                rb.velocity = dir * moveSpeed;
            }
        }

        /// <summary>
        /// Kỹ năng 1: Phóng chùm đạn ma pháp 360 độ tỏa khắp sàn đấu
        /// </summary>
        private IEnumerator BulletHellRoutine()
        {
            isAttacking = true;
            currentState = BossState.Attack;
            rb.velocity = Vector2.zero;

            // Telegraph tích tụ ma pháp
            if (sr != null) sr.color = new Color(1f, 0.9f, 0.3f);
            yield return new WaitForSeconds(0.6f);
            if (sr != null) sr.color = Color.white;

            int count = isEnraged ? (bulletCount + 4) : bulletCount;
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                float rad = angle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                BossProjectile.Spawn(transform.position, dir, bulletSpeed, bulletDamage, 5f, null, new Color(1f, 0.85f, 0.2f, 1f));
            }

            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = BossState.Idle;
        }

        /// <summary>
        /// Kỹ năng 2: Gọi các cột năng lượng giáng từ trần nhà xuống
        /// </summary>
        private IEnumerator CelestialPillarsRoutine()
        {
            isAttacking = true;
            currentState = BossState.Attack;
            rb.velocity = Vector2.zero;

            int count = isEnraged ? (pillarCount + 2) : pillarCount;

            for (int i = 0; i < count; i++)
            {
                if (targetPlayer == null) break;

                // Vị trí dưới chân người chơi hoặc xung quanh
                Vector3 spawnPos = targetPlayer.position + (Vector3)(Random.insideUnitCircle * 1.5f);
                StartCoroutine(SinglePillarStrikeRoutine(spawnPos));

                yield return new WaitForSeconds(0.35f);
            }

            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = BossState.Idle;
        }

        private IEnumerator SinglePillarStrikeRoutine(Vector3 pos)
        {
            // Telegraph vòng đỏ cảnh báo
            BossAoEIndicator.CreateCircle(pos, pillarRadius, pillarDelay, new Color(1f, 0.2f, 0.2f, 0.85f));
            yield return new WaitForSeconds(pillarDelay);

            // Sét giáng xuống gây sát thương diện rộng
            if (targetPlayer != null)
            {
                if (Vector2.Distance(pos, targetPlayer.position) <= pillarRadius)
                {
                    DealDamageToPlayer(pillarDamage);
                }
            }
        }

        /// <summary>
        /// Kỹ năng 3: Chém sóng kiếm càn quét diện rộng (xuyên sàn)
        /// </summary>
        private IEnumerator SwordWaveRoutine()
        {
            isAttacking = true;
            currentState = BossState.Attack;
            rb.velocity = Vector2.zero;

            Vector2 toPlayer = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;

            // Telegraph đường thẳng bản rộng
            BossAoEIndicator.CreateLine(transform.position, toPlayer, 12f, 2.5f, 0.6f, new Color(1f, 0.6f, 0.1f, 0.8f));
            yield return new WaitForSeconds(0.6f);

            // Bắn 2 lưỡi sóng kiếm khổng lồ xuyên sàn
            Vector2 perp = new Vector2(-toPlayer.y, toPlayer.x);
            Vector3 offset1 = (Vector3)(perp * 0.7f);
            Vector3 offset2 = (Vector3)(-perp * 0.7f);

            BossProjectile.Spawn(transform.position + offset1, toPlayer, waveSpeed, waveDamage, 4.0f, null, new Color(1f, 0.7f, 0.1f, 1f), true);
            BossProjectile.Spawn(transform.position + offset2, toPlayer, waveSpeed, waveDamage, 4.0f, null, new Color(1f, 0.7f, 0.1f, 1f), true);

            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = BossState.Idle;
        }

        protected override void TriggerEnrage()
        {
            base.TriggerEnrage();
            // Dịch chuyển về tâm nếu cần
            transform.position = Vector3.zero;
            Debug.Log("[Gwyn] ⚡ Gwyn đã giải phóng Thần Uy! Toàn bộ chiêu thức được cường hóa tối đa!");
        }
    }
}

