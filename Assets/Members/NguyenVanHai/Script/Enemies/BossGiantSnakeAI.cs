using System;
using System.Collections;
using UnityEngine;
using NguyenVanHai.Combat;
using NguyenVanHai.Data;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// Boss Tầng 1: Rắn Khổng Lồ (Chúa tể hang tối) - Kế thừa EnemyBase.
    /// - Đọc toàn bộ thông số từ dungeon_combat_config.json (id: "boss_giant_snake").
    /// </summary>
    public class BossGiantSnakeAI : EnemyBase
    {
        [Header("--- Đòn 1: Đứng Yên Bắn Đạn Từ Xa (VFX Cây Nấm) ---")]
        [Tooltip("Kéo 4 Sprite từ Assets/Sprites/Detailed/VFX_Poison_Spore_0..3 vào đây")]
        [SerializeField] private Sprite[] sporeAnimSprites;
        [SerializeField] private float rangedDamage = 18f;
        [SerializeField] private float projectileSpeed = 5.5f;
        [SerializeField] private int projectileCount = 3;
        [SerializeField] private float spreadAngle = 32f;
        [SerializeField] private float shootTelegraphTime = 0.95f;
        [SerializeField] private float shootRecoveryTime = 1.50f;
        [SerializeField] private float shootCooldown = 3.5f;

        [Header("--- Đòn 2: Lao Đi (Có Đường Cảnh Báo Đỏ Dưới Đất) ---")]
        [SerializeField] private float dashDamage = 25f;
        [SerializeField] private float dashSpeed = 9.5f;
        [SerializeField] private float dashDistance = 7.0f;
        [SerializeField] private float dashWidth = 1.4f;
        [SerializeField] private float dashTelegraphTime = 1.05f;
        [SerializeField] private float dashRecoveryTime = 1.80f;
        [SerializeField] private float dashCooldown = 5.5f;

        [Header("--- Khoảng Cách & Giãn Cách Chiêu ---")]
        [SerializeField] private float preferredMinRange = 3.2f;
        [SerializeField] private float preferredMaxRange = 8.5f;
        [SerializeField] private float minMoveBetweenSkills = 1.4f;

        private float lastShootTime = -99f;
        private float lastDashTime = -99f;
        private bool hasEngaged = false;

        public static event Action<BossGiantSnakeAI> OnBossSpawned;
        public static event Action OnBossDefeated;

        protected override void Awake()
        {
            isBoss = true;
            var cfg = DungeonCombatDatabase.GetBoss("boss_giant_snake");
            if (cfg != null)
            {
                maxHealth = cfg.maxHealth;
                moveSpeed = cfg.moveSpeed;
                attackDamage = cfg.attackDamage;
                detectRange = cfg.detectRange;
                attackRange = cfg.attackRange;
                attackCooldown = cfg.attackCooldown;
                minMoveBetweenSkills = cfg.minMoveBetweenSkills;
                preferredMinRange = cfg.preferredMinRange;
                preferredMaxRange = cfg.preferredMaxRange;
                dropItemId = cfg.dropItemId;
                dropItemName = cfg.dropItemName;
                dropCount = cfg.dropCount;

                if (cfg.skill1 != null)
                {
                    rangedDamage = cfg.skill1.damage;
                    projectileSpeed = cfg.skill1.speed;
                    projectileCount = cfg.skill1.projectileCount;
                    spreadAngle = cfg.skill1.widthOrAngle;
                    shootTelegraphTime = cfg.skill1.telegraphTime;
                    shootRecoveryTime = cfg.skill1.recoveryTime;
                    shootCooldown = cfg.skill1.cooldown;
                }

                if (cfg.skill2 != null)
                {
                    dashDamage = cfg.skill2.damage;
                    dashSpeed = cfg.skill2.speed;
                    dashDistance = cfg.skill2.rangeOrRadius;
                    dashWidth = cfg.skill2.widthOrAngle;
                    dashTelegraphTime = cfg.skill2.telegraphTime;
                    dashRecoveryTime = cfg.skill2.recoveryTime;
                    dashCooldown = cfg.skill2.cooldown;
                }
            }
            else
            {
                maxHealth = 450f;
                moveSpeed = 2.8f;
                attackDamage = 20f;
                detectRange = 14f;
                attackRange = 8.5f;
                attackCooldown = 2.5f;
                dropItemId = "snake_meat";
                dropItemName = "Thịt nạc Rắn khổng lồ";
                dropCount = 1;
            }

            base.Awake();
            // Đảm bảo maxHealth từ JSON được áp dụng (override Inspector serialized value từ Scene)
            currentHealth = maxHealth;
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking || Time.time < postAttackRestUntil || targetPlayer == null) return;

            if (distanceToPlayer > detectRange)
            {
                currentState = AIState.Idle;
                rb.velocity = Vector2.zero;
                return;
            }

            if (!hasEngaged)
            {
                hasEngaged = true;
                OnBossSpawned?.Invoke(this);
            }

            Vector2 toPlayer = (Vector2)targetPlayer.transform.position - (Vector2)transform.position;
            if (sr != null && Mathf.Abs(toPlayer.x) > 0.1f)
            {
                sr.flipX = toPlayer.x < 0f;
            }

            bool canCastAnySkill = Time.time >= lastAttackTime + minMoveBetweenSkills;

            if (canCastAnySkill)
            {
                // Ưu tiên 1: Đòn Lao Đi
                if (Time.time >= lastDashTime + dashCooldown && distanceToPlayer >= 2.2f && distanceToPlayer <= dashDistance + 0.5f)
                {
                    StartCoroutine(DashAttackRoutine());
                    return;
                }

                // Ưu tiên 2: Đứng yên bắn đạn tầm xa
                if (Time.time >= lastShootTime + shootCooldown && distanceToPlayer <= preferredMaxRange)
                {
                    StartCoroutine(RangedSporeBurstRoutine());
                    return;
                }
            }

            // Di chuyển giữ vị trí hợp lý
            if (distanceToPlayer > preferredMaxRange * 0.85f)
            {
                currentState = AIState.Chase;
                rb.velocity = toPlayer.normalized * moveSpeed;
            }
            else if (distanceToPlayer < preferredMinRange)
            {
                currentState = AIState.Chase;
                rb.velocity = -toPlayer.normalized * (moveSpeed * 0.65f);
            }
            else
            {
                currentState = AIState.Idle;
                rb.velocity = Vector2.zero;
            }
        }

        private IEnumerator RangedSporeBurstRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;

            // 1. Cảnh báo: Vòng tròn đỏ dưới đất + Thân Rắn chuyển sang MÀU ĐỎ RỰC
            BossTelegraphVFX.SpawnCircle(transform.position, 1.35f, shootTelegraphTime);
            yield return StartCoroutine(PlayRedWarningRoutine(shootTelegraphTime, 1.15f));

            // 2. Bắn chùm đạn bào tử
            if (IsAlive && targetPlayer != null)
            {
                Vector2 aimDir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;
                if (aimDir.sqrMagnitude < 0.001f) aimDir = Vector2.down;

                float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
                int count = Mathf.Max(1, projectileCount);

                for (int i = 0; i < count; i++)
                {
                    float offsetAngle = 0f;
                    if (count > 1)
                    {
                        float t = (float)i / (count - 1) - 0.5f;
                        offsetAngle = t * spreadAngle;
                    }

                    float rad = (baseAngle + offsetAngle) * Mathf.Deg2Rad;
                    Vector2 shotDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                    SpawnSnakeSpore(shotDir);
                }
            }

            lastShootTime = Time.time;

            // 3. Quãng nghỉ đứng yên sau khi bắn
            yield return StartCoroutine(EnterPostAttackRestRoutine(shootRecoveryTime));
        }

        private void SpawnSnakeSpore(Vector2 dir)
        {
            var go = new GameObject("SnakeSporeBullet");
            go.transform.position = transform.position + (Vector3)(dir * 0.55f);
            go.transform.localScale = Vector3.one * 1.25f;

            var bulletSr = go.AddComponent<SpriteRenderer>();
            if (sporeAnimSprites != null && sporeAnimSprites.Length > 0)
            {
                bulletSr.sprite = sporeAnimSprites[0];
            }
            bulletSr.sortingLayerName = "Projectiles_VFX";
            bulletSr.sortingOrder = 5800;

            var bulletCol = go.AddComponent<CircleCollider2D>();
            bulletCol.radius = 0.35f;
            bulletCol.isTrigger = true;

            var proj = go.AddComponent<SporeProjectile>();
            proj.Initialize(dir, rangedDamage, projectileSpeed, sporeAnimSprites);
        }

        private IEnumerator DashAttackRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;

            Vector2 dashDir = targetPlayer != null
                ? ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized
                : Vector2.down;
            if (dashDir.sqrMagnitude < 0.001f) dashDir = Vector2.down;

            float actualDist = GetUnobstructedDashDistance(dashDir, dashDistance);

            // 1. Hiện đường cảnh báo đỏ dưới đất + Thân Rắn hóa ĐỎ RỰC
            BossTelegraphVFX.SpawnLine(transform.position, dashDir, actualDist, dashWidth, dashTelegraphTime);
            yield return StartCoroutine(PlayRedWarningRoutine(dashTelegraphTime, 1.12f));

            // 2. Thực thi lao đi
            float dashDuration = Mathf.Max(0.15f, actualDist / Mathf.Max(1f, dashSpeed));
            float elapsed = 0f;
            bool hasDealtDamage = false;

            while (elapsed < dashDuration && IsAlive)
            {
                elapsed += Time.deltaTime;
                rb.velocity = dashDir * dashSpeed;

                if (!hasDealtDamage && targetPlayer != null)
                {
                    float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
                    if (dist <= dashWidth * 0.85f)
                    {
                        hasDealtDamage = true;
                        targetPlayer.TakeDamage(dashDamage);
                    }
                }
                yield return null;
            }

            rb.velocity = Vector2.zero;
            lastDashTime = Time.time;

            // 3. Quãng nghỉ đứng thở dốc cho Player phản công
            yield return StartCoroutine(EnterPostAttackRestRoutine(dashRecoveryTime));
        }

        private float GetUnobstructedDashDistance(Vector2 dir, float maxDist)
        {
            int wallMask = LayerMask.GetMask("Obstacle", "Default");
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, maxDist, wallMask);
            foreach (var h in hits)
            {
                if (h.collider != null && !h.collider.isTrigger && h.collider.gameObject != gameObject && h.collider.GetComponent<HunterPlayer>() == null)
                {
                    return Mathf.Max(1.5f, h.distance - 0.6f);
                }
            }
            return maxDist;
        }

        protected override void Die()
        {
            base.Die();
            OnBossDefeated?.Invoke();
        }
    }
}
