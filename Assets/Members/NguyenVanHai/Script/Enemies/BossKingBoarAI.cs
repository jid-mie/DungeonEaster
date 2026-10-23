using System;
using System.Collections;
using UnityEngine;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// Boss King Boar (Lợn Rừng Khổng Lồ - Bá chủ đầm lầy) theo bảng ma trận Boss 7.4 TaiLieu.md:
    /// - Máu 750 HP, Tốc độ 4.5 units/s.
    /// - Kỹ năng 1: Lao húc thẳng đường dài (35 DMG), báo hiệu 0.6s.
    /// - Kỹ năng 2: Dậm chân chấn động mặt đất AoE (28 DMG) bán kính 3.5 units.
    /// - 100% rơi: 1x Nạc vai heo rừng.
    /// </summary>
    public class BossKingBoarAI : EnemyBase
    {
        [Header("--- Kỹ năng 1: Lao Húc (Charge) ---")]
        [SerializeField] private float chargeSpeed = 9.0f;
        [SerializeField] private float chargeDamage = 35f;
        [SerializeField] private float chargeTelegraphTime = 0.6f;
        [SerializeField] private float chargeDuration = 1.0f;

        [Header("--- Kỹ năng 2: Dậm Đất (Ground Slam AoE) ---")]
        [SerializeField] private float slamRadius = 3.5f;
        [SerializeField] private float slamDamage = 28f;
        [SerializeField] private float slamTelegraphTime = 0.5f;

        public static event Action<BossKingBoarAI> OnBossSpawned;
        public static event Action OnBossDefeated;

        protected override void Awake()
        {
            maxHealth = 750f;
            moveSpeed = 3.5f;
            attackDamage = 20f;
            detectRange = 15f;
            attackRange = 4.0f;
            attackCooldown = 3.0f;

            dropItemId = "boar_shoulder";
            dropItemName = "Nạc vai heo rừng";
            dropCount = 1;

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            OnBossSpawned?.Invoke(this);
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking) return;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                // Luân phiên giữa Lao húc và Dậm đất
                if (distanceToPlayer <= slamRadius + 0.5f && UnityEngine.Random.value > 0.4f)
                {
                    StartCoroutine(GroundSlamRoutine());
                }
                else
                {
                    StartCoroutine(ChargeRoutine());
                }
            }
            else
            {
                // Áp sát cơ bản
                currentState = AIState.Chase;
                Vector2 dir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;
                rb.velocity = dir * moveSpeed;
            }
        }

        private IEnumerator ChargeRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;
            rb.velocity = Vector2.zero;

            Vector2 chargeDir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;

            // Telegraph 0.6s: Báo hiệu chuẩn bị lao
            if (sr != null) sr.color = new Color(1f, 0.4f, 0.4f, 1f);
            yield return new WaitForSeconds(chargeTelegraphTime);
            if (sr != null) sr.color = Color.white;

            // Lao húc với tốc độ cao
            rb.velocity = chargeDir * chargeSpeed;

            float timer = 0f;
            bool hit = false;

            while (timer < chargeDuration)
            {
                timer += Time.deltaTime;
                if (!hit && targetPlayer != null)
                {
                    float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
                    if (dist <= 1.5f)
                    {
                        hit = true;
                        targetPlayer.TakeDamage(chargeDamage);
                    }
                }
                yield return null;
            }

            rb.velocity = Vector2.zero;
            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = AIState.Idle;
        }

        private IEnumerator GroundSlamRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;
            rb.velocity = Vector2.zero;

            // Telegraph 0.5s: Nhún chân chuẩn bị dậm
            Vector3 origScale = transform.localScale;
            transform.localScale = new Vector3(origScale.x * 1.2f, origScale.y * 0.8f, origScale.z);
            yield return new WaitForSeconds(slamTelegraphTime);
            transform.localScale = origScale;

            // Dậm chân gây chấn động AoE
            if (targetPlayer != null)
            {
                float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
                if (dist <= slamRadius)
                {
                    targetPlayer.TakeDamage(slamDamage);
                }
            }

            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = AIState.Idle;
        }

        protected override void Die()
        {
            base.Die();
            OnBossDefeated?.Invoke();
        }
    }
}
