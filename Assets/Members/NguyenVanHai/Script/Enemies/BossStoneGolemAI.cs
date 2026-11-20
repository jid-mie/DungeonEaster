using System;
using System.Collections;
using UnityEngine;
using NguyenVanHai.Combat;
using NguyenVanHai.Data;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// Boss Tầng 3: Người Đá Khổng Lồ (Hộ vệ băng nguyên) - Kế thừa EnemyBase.
    /// - Đọc toàn bộ thông số từ dungeon_combat_config.json (id: "boss_stone_golem").
    /// </summary>
    public class BossStoneGolemAI : EnemyBase
    {
        [Header("--- Đòn 1: Cận Chiến Bán Nguyệt (VFX Con Bọ) ---")]
        [Tooltip("Kéo 4 Sprite từ Assets/Sprites/Detailed/VFX_Slash_Arc_0..3 vào đây")]
        [SerializeField] private Sprite[] clawSlashSprites;
        [SerializeField] private Color slashColor = new Color(0.55f, 0.85f, 1f, 1f);
        [SerializeField] private float meleeRange = 2.4f;
        [SerializeField] private float meleeArcAngle = 180f;
        [SerializeField] private float meleeDamage = 40f;
        [SerializeField] private float meleeTelegraphTime = 0.95f;
        [SerializeField] private float meleeRecoveryTime = 1.50f;
        [SerializeField] private float meleeCooldown = 3.2f;

        [Header("--- Đòn 2: Lao Đi (Có Đường Cảnh Báo Đỏ Dưới Đất) ---")]
        [SerializeField] private float dashDamage = 42f;
        [SerializeField] private float dashSpeed = 8.5f;
        [SerializeField] private float dashDistance = 7.5f;
        [SerializeField] private float dashWidth = 1.8f;
        [SerializeField] private float dashTelegraphTime = 1.15f;
        [SerializeField] private float dashRecoveryTime = 2.00f;
        [SerializeField] private float dashCooldown = 6.5f;

        [Header("--- Đòn 3: Nhảy Lên Cao & Dậm Xuống Player ---")]
        [SerializeField] private float jumpSlamDamage = 48f;
        [SerializeField] private float jumpSlamRadius = 3.2f;
        [SerializeField] private float jumpAirTime = 1.15f;
        [SerializeField] private float jumpArcHeight = 3.8f;
        [SerializeField] private float jumpRecoveryTime = 2.20f;
        [SerializeField] private float jumpCooldown = 8.0f;

        [Header("--- Giãn Cách Giữa 2 Chiêu ---")]
        [SerializeField] private float minMoveBetweenSkills = 1.6f;

        private float lastMeleeTime = -99f;
        private float lastDashTime = -99f;
        private float lastJumpTime = -99f;
        private bool hasEngaged = false;

        public static event Action<BossStoneGolemAI> OnBossSpawned;
        public static event Action OnBossDefeated;

        protected override void Awake()
        {
            isBoss = true;
            var cfg = DungeonCombatDatabase.GetBoss("boss_stone_golem");
            if (cfg != null)
            {
                maxHealth = cfg.maxHealth;
                moveSpeed = cfg.moveSpeed;
                attackDamage = cfg.attackDamage;
                detectRange = cfg.detectRange;
                attackRange = cfg.attackRange;
                attackCooldown = cfg.attackCooldown;
                minMoveBetweenSkills = cfg.minMoveBetweenSkills;
                dropItemId = cfg.dropItemId;
                dropItemName = cfg.dropItemName;
                dropCount = cfg.dropCount;

                if (cfg.skill1 != null)
                {
                    meleeDamage = cfg.skill1.damage;
                    meleeRange = cfg.skill1.rangeOrRadius;
                    meleeArcAngle = cfg.skill1.widthOrAngle;
                    meleeTelegraphTime = cfg.skill1.telegraphTime;
                    meleeRecoveryTime = cfg.skill1.recoveryTime;
                    meleeCooldown = cfg.skill1.cooldown;
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

                if (cfg.skill3 != null)
                {
                    jumpSlamDamage = cfg.skill3.damage;
                    jumpSlamRadius = cfg.skill3.rangeOrRadius;
                    jumpArcHeight = cfg.skill3.arcOrDropHeight;
                    jumpAirTime = cfg.skill3.telegraphTime;
                    jumpRecoveryTime = cfg.skill3.recoveryTime;
                    jumpCooldown = cfg.skill3.cooldown;
                }
            }
            else
            {
                maxHealth = 1200f;
                moveSpeed = 1.6f;
                attackDamage = 40f;
                detectRange = 15f;
                attackRange = 2.4f;
                attackCooldown = 2.5f;
                dropItemId = "golem_core";
                dropItemName = "Tủy đá nguyên khối";
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
                // 1. Nếu Player đứng gần (<= meleeRange): Đòn cận chiến bán nguyệt
                if (distanceToPlayer <= meleeRange && Time.time >= lastMeleeTime + meleeCooldown)
                {
                    StartCoroutine(MeleeClawSweepRoutine());
                    return;
                }

                // 2. Nhảy lên cao dậm xuống Player
                if (Time.time >= lastJumpTime + jumpCooldown && distanceToPlayer >= 2.6f && distanceToPlayer <= 10.5f)
                {
                    StartCoroutine(JumpSlamRoutine());
                    return;
                }

                // 3. Lao đi càn quét
                if (Time.time >= lastDashTime + dashCooldown && distanceToPlayer >= 2.8f && distanceToPlayer <= dashDistance + 0.5f)
                {
                    StartCoroutine(DashChargeRoutine());
                    return;
                }
            }

            // Di chuyển bước nặng nề tiến về phía Player
            currentState = AIState.Chase;
            rb.velocity = toPlayer.normalized * moveSpeed;
        }

        private IEnumerator MeleeClawSweepRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;

            Vector2 attackDir = targetPlayer != null
                ? ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized
                : Vector2.down;
            if (attackDir.sqrMagnitude < 0.001f) attackDir = Vector2.down;

            // 1. Cảnh báo bán nguyệt đỏ dưới đất + Thân Golem hóa ĐỎ RỰC
            BossTelegraphVFX.SpawnSemiCircle(transform.position, attackDir, meleeRange, meleeArcAngle, meleeTelegraphTime);
            yield return StartCoroutine(PlayRedWarningRoutine(meleeTelegraphTime, 1.12f));

            // 2. Phát VFX vệt cào & Gây sát thương
            SpawnGolemClawVFX(attackDir);

            if (IsAlive && targetPlayer != null)
            {
                Vector2 toPlayer = (Vector2)targetPlayer.transform.position - (Vector2)transform.position;
                if (toPlayer.magnitude <= meleeRange)
                {
                    float angle = Vector2.Angle(attackDir, toPlayer.normalized);
                    if (angle <= meleeArcAngle * 0.5f)
                    {
                        targetPlayer.TakeDamage(meleeDamage, attackDir);
                    }
                }
            }

            lastMeleeTime = Time.time;

            // 3. Quãng nghỉ đứng yên
            yield return StartCoroutine(EnterPostAttackRestRoutine(meleeRecoveryTime));
        }

        private void SpawnGolemClawVFX(Vector2 dir)
        {
            if (clawSlashSprites == null || clawSlashSprites.Length == 0) return;

            var vfxGo = new GameObject("GolemClaw_VFX");
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            vfxGo.transform.position = transform.position + (Vector3)(dir * (meleeRange * 0.45f));
            vfxGo.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            vfxGo.transform.localScale = Vector3.one * 2.2f;

            var vfxSr = vfxGo.AddComponent<SpriteRenderer>();
            vfxSr.color = slashColor;
            vfxSr.sortingLayerName = "Projectiles_VFX";
            vfxSr.sortingOrder = 6000;

            StartCoroutine(PlaySlashFramesRoutine(vfxGo, vfxSr));
        }

        private IEnumerator PlaySlashFramesRoutine(GameObject vfxGo, SpriteRenderer vfxSr)
        {
            const float frameDur = 0.04f;
            for (int i = 0; i < clawSlashSprites.Length; i++)
            {
                if (vfxGo == null) yield break;
                vfxSr.sprite = clawSlashSprites[i];
                yield return new WaitForSeconds(frameDur);
            }
            if (vfxGo != null) Destroy(vfxGo);
        }

        private IEnumerator DashChargeRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;

            Vector2 dashDir = targetPlayer != null
                ? ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized
                : Vector2.down;
            if (dashDir.sqrMagnitude < 0.001f) dashDir = Vector2.down;

            float actualDist = GetUnobstructedDashDistance(dashDir, dashDistance);

            // 1. Hiện đường cảnh báo đỏ dưới đất + Thân Golem hóa ĐỎ RỰC
            BossTelegraphVFX.SpawnLine(transform.position, dashDir, actualDist, dashWidth, dashTelegraphTime);
            yield return StartCoroutine(PlayRedWarningRoutine(dashTelegraphTime, 1.10f));

            // 2. Lao húc
            float dashDuration = Mathf.Max(0.15f, actualDist / Mathf.Max(1f, dashSpeed));
            float elapsed = 0f;
            bool hasHit = false;

            while (elapsed < dashDuration && IsAlive)
            {
                elapsed += Time.deltaTime;
                rb.velocity = dashDir * dashSpeed;

                if (!hasHit && targetPlayer != null)
                {
                    float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
                    if (dist <= dashWidth * 0.85f)
                    {
                        hasHit = true;
                        targetPlayer.TakeDamage(dashDamage, dashDir);
                    }
                }
                yield return null;
            }

            rb.velocity = Vector2.zero;
            lastDashTime = Time.time;

            // 3. Quãng nghỉ sau khi húc
            yield return StartCoroutine(EnterPostAttackRestRoutine(dashRecoveryTime));
        }

        private IEnumerator JumpSlamRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;

            Vector2 startPos = transform.position;
            Vector2 landingPos = targetPlayer != null ? (Vector2)targetPlayer.transform.position : startPos;

            float preJumpWarn = 0.40f;
            float flightTime = jumpAirTime;

            // 1. Hiện vòng tròn cảnh báo đỏ mờ tại điểm rơi
            BossTelegraphVFX.SpawnCircle(landingPos, jumpSlamRadius, preJumpWarn + flightTime);

            // Golem hóa ĐỎ RỰC nhún chân lấy đà 0.40s trước khi rời mặt đất
            yield return StartCoroutine(PlayRedWarningRoutine(preJumpWarn, 1.15f));

            // 2. Bật nhảy vút lên không trung theo quỹ đạo parabol
            SetPhysicsLocked(true);
            if (col != null) col.enabled = false;
            if (sr != null) sr.color = new Color(1f, 0.4f, 0.4f, 1f);

            float elapsed = 0f;
            while (elapsed < flightTime && IsAlive)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / flightTime);

                Vector2 groundPos = Vector2.Lerp(startPos, landingPos, t);
                float arcY = Mathf.Sin(t * Mathf.PI) * jumpArcHeight;
                transform.position = new Vector3(groundPos.x, groundPos.y + arcY, 0f);

                yield return null;
            }

            // 3. Đáp thẳng xuống tâm vòng tròn cảnh báo
            transform.position = new Vector3(landingPos.x, landingPos.y, 0f);
            if (sr != null) sr.color = baseTintColor;
            if (col != null && IsAlive) col.enabled = true;

            Vector3 origScale = transform.localScale;
            transform.localScale = new Vector3(origScale.x * 1.22f, origScale.y * 0.78f, origScale.z);

            if (IsAlive && targetPlayer != null)
            {
                float dist = Vector2.Distance(landingPos, targetPlayer.transform.position);
                if (dist <= jumpSlamRadius)
                {
                    Vector2 kbDir = ((Vector2)targetPlayer.transform.position - landingPos).normalized;
                    targetPlayer.TakeDamage(jumpSlamDamage, kbDir);
                }
            }

            yield return new WaitForSeconds(0.20f);
            transform.localScale = origScale;

            lastJumpTime = Time.time;

            // 4. Quãng nghỉ đứng yên sau khi dậm
            yield return StartCoroutine(EnterPostAttackRestRoutine(jumpRecoveryTime));
        }

        private float GetUnobstructedDashDistance(Vector2 dir, float maxDist)
        {
            int wallMask = LayerMask.GetMask("Obstacle", "Default");
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, maxDist, wallMask);
            foreach (var h in hits)
            {
                if (h.collider != null && !h.collider.isTrigger && h.collider.gameObject != gameObject && h.collider.GetComponent<HunterPlayer>() == null)
                {
                    return Mathf.Max(1.5f, h.distance - 0.8f);
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
