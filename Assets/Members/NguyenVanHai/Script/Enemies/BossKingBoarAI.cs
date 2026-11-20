using System;
using System.Collections;
using UnityEngine;
using NguyenVanHai.Combat;
using NguyenVanHai.Data;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// Boss Tầng 2: King Boar (Lợn Rừng Khổng Lồ - Bá chủ đầm lầy):
    /// - Đọc toàn bộ thông số từ dungeon_combat_config.json (id: "boss_king_boar").
    /// </summary>
    public class BossKingBoarAI : EnemyBase
    {
        [Header("--- Hoạt Ảnh Sprite (Kéo từ Boss_KingBoar_Animations, bỏ qua _8 và _29) ---")]
        [Tooltip("Kéo 4 frame đi bộ hướng Phải: Boss_KingBoar_Animations_0, _1, _2, _3")]
        [SerializeField] private Sprite[] walkSprites;
        [Tooltip("Kéo 4 frame dậm chân hướng Phải: _4, _5, _6 (chồm lên) và _7 (dậm nổ tóe đất)")]
        [SerializeField] private Sprite[] stompSprites;
        [Tooltip("Kéo 4 frame lao húc hướng Phải: _9 (lấy đà) và _10, _11, _12 (lao tốc độ cao)")]
        [SerializeField] private Sprite[] chargeSprites;
        [SerializeField] private float walkFrameRate = 0.12f;

        [Header("--- Đòn 1: Dậm Chân Phạm Vi (Có Vòng Tròn Cảnh Báo Đỏ Dưới Đất) ---")]
        [SerializeField] private float slamRadius = 3.5f;
        [SerializeField] private float slamDamage = 28f;
        [SerializeField] private float slamTelegraphTime = 1.05f;
        [SerializeField] private float slamRecoveryTime = 1.75f;
        [SerializeField] private float slamCooldown = 4.5f;

        [Header("--- Đòn 2: Lao Tới (Có Đường Cảnh Báo Đỏ Dưới Đất) ---")]
        [SerializeField] private float chargeSpeed = 9.5f;
        [SerializeField] private float chargeDamage = 35f;
        [SerializeField] private float chargeDistance = 8.0f;
        [SerializeField] private float chargeWidth = 1.6f;
        [SerializeField] private float chargeTelegraphTime = 1.10f;
        [SerializeField] private float chargeRecoveryTime = 2.00f;
        [SerializeField] private float chargeCooldown = 5.5f;

        [Header("--- Giãn Cách Giữa 2 Chiêu ---")]
        [SerializeField] private float minMoveBetweenSkills = 1.5f;

        private float lastSlamTime = -99f;
        private float lastChargeTime = -99f;
        private float walkAnimTimer = 0f;
        private int walkFrameIndex = 0;
        private Sprite overrideActionSprite = null;
        private bool hasEngaged = false;

        public static event Action<BossKingBoarAI> OnBossSpawned;
        public static event Action OnBossDefeated;

        protected override void Awake()
        {
            isBoss = true;
            var cfg = DungeonCombatDatabase.GetBoss("boss_king_boar");
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
                    slamDamage = cfg.skill1.damage;
                    slamRadius = cfg.skill1.rangeOrRadius;
                    slamTelegraphTime = cfg.skill1.telegraphTime;
                    slamRecoveryTime = cfg.skill1.recoveryTime;
                    slamCooldown = cfg.skill1.cooldown;
                }

                if (cfg.skill2 != null)
                {
                    chargeDamage = cfg.skill2.damage;
                    chargeSpeed = cfg.skill2.speed;
                    chargeDistance = cfg.skill2.rangeOrRadius;
                    chargeWidth = cfg.skill2.widthOrAngle;
                    chargeTelegraphTime = cfg.skill2.telegraphTime;
                    chargeRecoveryTime = cfg.skill2.recoveryTime;
                    chargeCooldown = cfg.skill2.cooldown;
                }
            }
            else
            {
                maxHealth = 750f;
                moveSpeed = 3.0f;
                attackDamage = 28f;
                detectRange = 15f;
                attackRange = 3.6f;
                attackCooldown = 2.5f;
                dropItemId = "boar_shoulder";
                dropItemName = "Nạc vai heo rừng";
                dropCount = 1;
            }

            base.Awake();
            // Đảm bảo maxHealth từ JSON được áp dụng (override Inspector serialized value từ Scene)
            currentHealth = maxHealth;
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking || Time.time < postAttackRestUntil || targetPlayer == null)
            {
                UpdateWalkAnimation(false);
                return;
            }

            if (distanceToPlayer > detectRange)
            {
                currentState = AIState.Idle;
                rb.velocity = Vector2.zero;
                UpdateWalkAnimation(false);
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
                // 1. Dậm Chân AoE khi Player áp sát <= slamRadius + 0.15m
                if (distanceToPlayer <= slamRadius + 0.15f && Time.time >= lastSlamTime + slamCooldown)
                {
                    StartCoroutine(GroundSlamRoutine());
                    return;
                }

                // 2. Lao Tới khi Player ở cự ly 2.6m - chargeDistance + 0.5m
                if (distanceToPlayer >= 2.6f && distanceToPlayer <= chargeDistance + 0.5f && Time.time >= lastChargeTime + chargeCooldown)
                {
                    StartCoroutine(ChargeRoutine());
                    return;
                }
            }

            // 3. Di chuyển truy đuổi cơ bản
            currentState = AIState.Chase;
            rb.velocity = toPlayer.normalized * moveSpeed;
            UpdateWalkAnimation(true);
        }

        private void LateUpdate()
        {
            if (overrideActionSprite != null && sr != null)
            {
                sr.sprite = overrideActionSprite;
            }
        }

        private void UpdateWalkAnimation(bool isMoving)
        {
            overrideActionSprite = null;
            if (walkSprites == null || walkSprites.Length == 0 || sr == null) return;

            if (!isMoving)
            {
                sr.sprite = walkSprites[0];
                return;
            }

            walkAnimTimer += Time.deltaTime;
            if (walkAnimTimer >= walkFrameRate)
            {
                walkAnimTimer = 0f;
                walkFrameIndex = (walkFrameIndex + 1) % walkSprites.Length;
            }
            sr.sprite = walkSprites[walkFrameIndex];
        }

        private IEnumerator GroundSlamRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;
            SetPhysicsLocked(true);

            // 1. Hiện vòng tròn cảnh báo đỏ mờ dưới đất + Thân Lợn nhấp nháy ĐỎ RỰC
            BossTelegraphVFX.SpawnCircle(transform.position, slamRadius, slamTelegraphTime);

            if (flash != null)
            {
                flash.StopFlashImmediate();
                flash.IsWarningActive = true;
            }

            Vector3 origScale = transform.localScale;
            Color redBright = new Color(1f, 0.15f, 0.15f, 1f);
            Color redDeep = new Color(1f, 0.42f, 0.12f, 1f);
            float elapsed = 0f;

            while (elapsed < slamTelegraphTime && IsAlive)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slamTelegraphTime);

                // Nháy đỏ rực cảnh báo trên thân Lợn
                float pulse = (Mathf.Sin(elapsed * 20f) + 1f) * 0.5f;
                if (sr != null) sr.color = Color.Lerp(redBright, redDeep, pulse);

                if (stompSprites != null && stompSprites.Length > 1)
                {
                    int windupCount = stompSprites.Length - 1;
                    int idx = Mathf.Clamp(Mathf.FloorToInt(t * windupCount), 0, windupCount - 1);
                    overrideActionSprite = stompSprites[idx];
                }
                else
                {
                    transform.localScale = new Vector3(origScale.x * (1f + 0.12f * t), origScale.y * (1f - 0.12f * t), origScale.z);
                }

                yield return null;
            }

            transform.localScale = origScale;
            if (sr != null) sr.color = baseTintColor;
            if (flash != null) flash.IsWarningActive = false;

            // 2. Dậm mạnh xuống đất (Frame cuối _7 có hiệu ứng nổ đất vàng)
            if (stompSprites != null && stompSprites.Length > 0)
            {
                overrideActionSprite = stompSprites[stompSprites.Length - 1];
            }

            transform.localScale = new Vector3(origScale.x * 1.18f, origScale.y * 0.84f, origScale.z);

            if (IsAlive && targetPlayer != null)
            {
                float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
                if (dist <= slamRadius)
                {
                    Vector2 kbDir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;
                    targetPlayer.TakeDamage(slamDamage, kbDir);
                }
            }

            yield return new WaitForSeconds(0.28f);
            transform.localScale = origScale;

            lastSlamTime = Time.time;

            // 3. Quãng nghỉ đứng yên sau khi dậm chân
            if (walkSprites != null && walkSprites.Length > 0)
            {
                overrideActionSprite = walkSprites[0];
            }

            yield return StartCoroutine(EnterPostAttackRestRoutine(slamRecoveryTime));
            overrideActionSprite = null;
        }

        private IEnumerator ChargeRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;

            Vector2 chargeDir = targetPlayer != null
                ? ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized
                : Vector2.down;
            if (chargeDir.sqrMagnitude < 0.001f) chargeDir = Vector2.down;

            if (sr != null && Mathf.Abs(chargeDir.x) > 0.05f)
            {
                sr.flipX = chargeDir.x < 0f;
            }

            float actualDist = GetUnobstructedChargeDistance(chargeDir, chargeDistance);

            // 1. Hiện đường cảnh báo đỏ dưới đất + Frame lấy đà (_9) + Thân Lợn hóa ĐỎ RỰC
            BossTelegraphVFX.SpawnLine(transform.position, chargeDir, actualDist, chargeWidth, chargeTelegraphTime);

            if (chargeSprites != null && chargeSprites.Length > 0)
            {
                overrideActionSprite = chargeSprites[0];
            }

            yield return StartCoroutine(PlayRedWarningRoutine(chargeTelegraphTime, 1.10f));

            // 2. Lao vọt tới dọc đường cảnh báo
            float chargeDuration = Mathf.Max(0.15f, actualDist / Mathf.Max(1f, chargeSpeed));
            float timer = 0f;
            float animTimer = 0f;
            int chargeFrameIdx = (chargeSprites != null && chargeSprites.Length > 1) ? 1 : 0;
            bool hasHitPlayer = false;

            while (timer < chargeDuration && IsAlive)
            {
                timer += Time.deltaTime;
                rb.velocity = chargeDir * chargeSpeed;

                if (chargeSprites != null && chargeSprites.Length > 0)
                {
                    animTimer += Time.deltaTime;
                    if (animTimer >= 0.07f)
                    {
                        animTimer = 0f;
                        if (chargeSprites.Length > 1)
                        {
                            chargeFrameIdx++;
                            if (chargeFrameIdx >= chargeSprites.Length) chargeFrameIdx = 1;
                        }
                    }
                    overrideActionSprite = chargeSprites[chargeFrameIdx];
                }

                if (!hasHitPlayer && targetPlayer != null)
                {
                    float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
                    if (dist <= chargeWidth * 0.85f)
                    {
                        hasHitPlayer = true;
                        targetPlayer.TakeDamage(chargeDamage, chargeDir);
                    }
                }

                yield return null;
            }

            rb.velocity = Vector2.zero;
            lastChargeTime = Time.time;

            // 3. Quãng nghỉ đứng thở dốc cho Player phản công
            if (walkSprites != null && walkSprites.Length > 0)
            {
                overrideActionSprite = walkSprites[0];
            }

            yield return StartCoroutine(EnterPostAttackRestRoutine(chargeRecoveryTime));
            overrideActionSprite = null;
        }

        private float GetUnobstructedChargeDistance(Vector2 dir, float maxDist)
        {
            int wallMask = LayerMask.GetMask("Obstacle", "Default");
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, maxDist, wallMask);
            foreach (var h in hits)
            {
                if (h.collider != null && !h.collider.isTrigger && h.collider.gameObject != gameObject && h.collider.GetComponent<HunterPlayer>() == null)
                {
                    return Mathf.Max(1.8f, h.distance - 0.7f);
                }
            }
            return maxDist;
        }

        protected override void Die()
        {
            overrideActionSprite = null;
            base.Die();
            OnBossDefeated?.Invoke();
        }
    }
}
