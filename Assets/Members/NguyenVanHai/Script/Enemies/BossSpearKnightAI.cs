using System;
using System.Collections;
using UnityEngine;
using NguyenVanHai.Combat;
using NguyenVanHai.Data;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// Boss Tầng 4 (Dual Boss - Hiệp Sĩ Thương Xanh): Kế thừa EnemyBase.
    /// - Đọc toàn bộ thông số từ dungeon_combat_config.json (id: "boss_spear_knight").
    /// </summary>
    [ExecuteAlways]
    public class BossSpearKnightAI : EnemyBase
    {
        [Header("--- Tham Chiếu Vũ Khí Rời & Đồng Đội ---")]
        [Tooltip("Kéo GameObject con chứa Sprite Cây Thương rời (Boss_SpearKnight1_1) vào đây. Nếu để trống sẽ tự tìm object con.")]
        [SerializeField] private Transform weaponTransform;
        [Tooltip("Kéo GameObject Hiệp Sĩ Kiếm (BossSwordKnightAI) vào đây để phối hợp nhịp chiêu")]
        [SerializeField] private BossSwordKnightAI partnerKnight;

        [Header("--- Đòn 1: Phi Giáo (Có Đường Cảnh Báo Đỏ Dưới Đất) ---")]
        [SerializeField] private float throwDamage = 38f;
        [SerializeField] private float spearFlySpeed = 12.5f;
        [SerializeField] private float throwMaxDistance = 9.5f;
        [SerializeField] private float throwLineWidth = 1.15f;
        [SerializeField] private float throwTelegraphTime = 1.10f;
        [SerializeField] private float throwRecoveryTime = 1.80f;
        [SerializeField] private float throwCooldown = 5.0f;

        [Header("--- Đòn 2: Nhảy Lên Cao & Dậm Xuống Player ---")]
        [SerializeField] private float jumpSlamDamage = 45f;
        [SerializeField] private float jumpSlamRadius = 2.8f;
        [SerializeField] private float jumpAirTime = 1.05f;
        [SerializeField] private float jumpArcHeight = 3.6f;
        [SerializeField] private float jumpRecoveryTime = 2.10f;
        [SerializeField] private float jumpCooldown = 7.5f;

        [Header("--- Khoảng Cách & Giãn Cách Chiêu ---")]
        [SerializeField] private float preferredMinDistance = 3.5f;
        [SerializeField] private float preferredMaxDistance = 8.0f;
        [SerializeField] private float minMoveBetweenSkills = 1.6f;

        private Vector3 weaponRestLocalPos = new Vector3(0.78f, 0.05f, 0f);
        private Quaternion weaponRestLocalRot = Quaternion.identity;
        private SpriteRenderer weaponSr;
        private float lastThrowTime = -99f;
        private float lastJumpTime = -99f;
        private bool isEnraged = false;
        private bool isPerformingJumpSlam = false;
        private bool hasEngaged = false;

        public bool IsPerformingJumpSlam => isPerformingJumpSlam;

        public static event Action<BossSpearKnightAI> OnBossSpawned;

        protected override void Awake()
        {
            isBoss = true;
            sr = GetComponent<SpriteRenderer>();
            if (!Application.isPlaying)
            {
                SyncWeaponSortingAndHandOffset();
                return;
            }

            var cfg = DungeonCombatDatabase.GetBoss("boss_spear_knight");
            if (cfg != null)
            {
                maxHealth = cfg.maxHealth;
                moveSpeed = cfg.moveSpeed;
                attackDamage = cfg.attackDamage;
                detectRange = cfg.detectRange;
                attackRange = cfg.attackRange;
                attackCooldown = cfg.attackCooldown;
                minMoveBetweenSkills = cfg.minMoveBetweenSkills;
                preferredMinDistance = cfg.preferredMinRange;
                preferredMaxDistance = cfg.preferredMaxRange;
                dropItemId = cfg.dropItemId;
                dropItemName = cfg.dropItemName;
                dropCount = cfg.dropCount;

                if (cfg.skill1 != null)
                {
                    throwDamage = cfg.skill1.damage;
                    spearFlySpeed = cfg.skill1.speed;
                    throwMaxDistance = cfg.skill1.rangeOrRadius;
                    throwLineWidth = cfg.skill1.widthOrAngle;
                    throwTelegraphTime = cfg.skill1.telegraphTime;
                    throwRecoveryTime = cfg.skill1.recoveryTime;
                    throwCooldown = cfg.skill1.cooldown;
                }

                if (cfg.skill2 != null)
                {
                    jumpSlamDamage = cfg.skill2.damage;
                    jumpSlamRadius = cfg.skill2.rangeOrRadius;
                    jumpArcHeight = cfg.skill2.arcOrDropHeight;
                    jumpAirTime = cfg.skill2.telegraphTime;
                    jumpRecoveryTime = cfg.skill2.recoveryTime;
                    jumpCooldown = cfg.skill2.cooldown;
                }
            }
            else
            {
                maxHealth = 600f;
                moveSpeed = 2.4f;
                attackDamage = 38f;
                detectRange = 15f;
                attackRange = 8.5f;
                attackCooldown = 2.5f;
                dropItemId = "knight_egg";
                dropItemName = "Trứng thiết giáp quái";
                dropCount = 1;
            }

            base.Awake();
            InitializeWeaponReference();
            // Đảm bảo maxHealth từ JSON được áp dụng (override Inspector serialized value từ Scene)
            currentHealth = maxHealth;
        }

        protected override void Start()
        {
            if (!Application.isPlaying) return;
            base.Start();
        }

        protected override void Update()
        {
            if (!Application.isPlaying)
            {
                SyncWeaponSortingAndHandOffset();
                return;
            }
            base.Update();
        }

        private void OnValidate()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            SyncWeaponSortingAndHandOffset();
        }

        private void LateUpdate()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (weaponSr == null && weaponTransform != null) weaponSr = weaponTransform.GetComponent<SpriteRenderer>();

            if (weaponSr != null && sr != null)
            {
                weaponSr.sortingLayerID = sr.sortingLayerID;
                weaponSr.sortingOrder = sr.sortingOrder + 10;
            }
        }

        private void InitializeWeaponReference()
        {
            SyncWeaponSortingAndHandOffset();

            if (partnerKnight == null && Application.isPlaying)
            {
                partnerKnight = FindObjectOfType<BossSwordKnightAI>();
            }
        }

        private void SyncWeaponSortingAndHandOffset()
        {
            if (weaponTransform == null)
            {
                foreach (Transform child in GetComponentsInChildren<Transform>())
                {
                    if (child != transform && child.GetComponent<SpriteRenderer>() != null)
                    {
                        weaponTransform = child;
                        break;
                    }
                }
            }

            if (weaponTransform != null)
            {
                if (Mathf.Abs(weaponTransform.localPosition.x) < 0.35f)
                {
                    weaponTransform.localPosition = new Vector3(0.78f, 0.05f, 0f);
                }

                weaponRestLocalPos = weaponTransform.localPosition;
                weaponRestLocalRot = weaponTransform.localRotation;

                weaponSr = weaponTransform.GetComponent<SpriteRenderer>();
                if (weaponSr != null)
                {
                    if (sr != null)
                    {
                        weaponSr.sortingLayerID = sr.sortingLayerID;
                        weaponSr.sortingOrder = sr.sortingOrder + 10;
                    }
                    else
                    {
                        weaponSr.sortingLayerName = "Entities";
                        weaponSr.sortingOrder = 5500;
                    }
                }
            }
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

            if (!isEnraged && partnerKnight != null && !partnerKnight.IsAlive)
            {
                ActivateEnrage();
            }

            Vector2 toPlayer = (Vector2)targetPlayer.transform.position - (Vector2)transform.position;
            if (sr != null && Mathf.Abs(toPlayer.x) > 0.1f)
            {
                bool shouldFlip = toPlayer.x < 0f;
                if (sr.flipX != shouldFlip)
                {
                    sr.flipX = shouldFlip;
                    ResetWeaponToHandImmediate();
                }
            }

            float cdMult = isEnraged ? 0.85f : 1.0f;
            bool canCastAnySkill = Time.time >= lastAttackTime + minMoveBetweenSkills;

            if (canCastAnySkill)
            {
                // 1. Nhảy lên cao dậm xuống Player (nhường nhịp nếu Hiệp sĩ Kiếm đang ra chiêu)
                bool partnerBusy = partnerKnight != null && partnerKnight.IsAlive && partnerKnight.IsPerformingMajorSkill;
                if (!partnerBusy && Time.time >= lastJumpTime + (jumpCooldown * cdMult) && distanceToPlayer >= 2.2f && distanceToPlayer <= 10.5f)
                {
                    StartCoroutine(JumpSlamRoutine());
                    return;
                }

                // 2. Phi Giáo tầm xa
                if (Time.time >= lastThrowTime + (throwCooldown * cdMult) && distanceToPlayer <= throwMaxDistance)
                {
                    StartCoroutine(SpearThrowRoutine());
                    return;
                }
            }

            // Di chuyển giữ cự ly trung bình
            float spd = isEnraged ? moveSpeed * 1.15f : moveSpeed;
            if (distanceToPlayer > preferredMaxDistance)
            {
                currentState = AIState.Chase;
                rb.velocity = toPlayer.normalized * spd;
            }
            else if (distanceToPlayer < preferredMinDistance && !isEnraged)
            {
                currentState = AIState.Chase;
                rb.velocity = -toPlayer.normalized * (spd * 0.65f);
            }
            else
            {
                currentState = AIState.Chase;
                Vector2 perp = new Vector2(-toPlayer.y, toPlayer.x).normalized;
                rb.velocity = perp * (spd * 0.50f);
            }
        }

        private void ActivateEnrage()
        {
            isEnraged = true;
            baseTintColor = new Color(0.75f, 0.88f, 1f, 1f);
            if (sr != null) sr.color = baseTintColor;
        }

        private IEnumerator SpearThrowRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;

            Vector2 throwDir = targetPlayer != null
                ? ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized
                : Vector2.down;
            if (throwDir.sqrMagnitude < 0.001f) throwDir = Vector2.down;

            float actualDist = GetUnobstructedThrowDistance(throwDir, throwMaxDistance);
            float aimAngle = Mathf.Atan2(throwDir.y, throwDir.x) * Mathf.Rad2Deg;
            float warnTime = throwTelegraphTime;

            // 1. Hiện đường cảnh báo đỏ dưới đất + Thân Hiệp sĩ hóa ĐỎ RỰC trong 1.10s
            BossTelegraphVFX.SpawnLine(transform.position, throwDir, actualDist, throwLineWidth, warnTime);

            SetPhysicsLocked(true);
            if (flash != null)
            {
                flash.StopFlashImmediate();
                flash.IsWarningActive = true;
            }

            Color redBright = new Color(1f, 0.15f, 0.15f, 1f);
            Color redDeep = new Color(1f, 0.42f, 0.12f, 1f);
            Vector3 startHandWorld = weaponTransform != null ? weaponTransform.position : transform.position;
            Quaternion spearThrowRot = Quaternion.Euler(0f, 0f, aimAngle - 90f);

            float elapsed = 0f;
            while (elapsed < warnTime && IsAlive)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / warnTime);

                float pulse = (Mathf.Sin(elapsed * 20f) + 1f) * 0.5f;
                if (sr != null) sr.color = Color.Lerp(redBright, redDeep, pulse);

                if (weaponTransform != null)
                {
                    weaponTransform.rotation = Quaternion.Slerp(transform.rotation * weaponRestLocalRot, spearThrowRot, Mathf.Min(1f, t * 2f));
                    weaponTransform.position = startHandWorld - (Vector3)(throwDir * (0.35f * t));
                }
                yield return null;
            }

            if (sr != null) sr.color = baseTintColor;
            if (flash != null) flash.IsWarningActive = false;

            // 2. Phóng cây thương rời tay bay dọc theo đường cảnh báo đỏ
            Vector2 spearStartPos = transform.position;
            Vector2 spearEndPos = spearStartPos + throwDir * actualDist;
            float safeFlySpeed = spearFlySpeed;
            float flyDuration = Mathf.Max(0.18f, actualDist / Mathf.Max(1f, safeFlySpeed));
            float flyElapsed = 0f;
            bool hasHitPlayer = false;

            while (flyElapsed < flyDuration && IsAlive)
            {
                flyElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(flyElapsed / flyDuration);
                Vector2 curSpearPos = Vector2.Lerp(spearStartPos, spearEndPos, t);

                if (weaponTransform != null)
                {
                    weaponTransform.position = new Vector3(curSpearPos.x, curSpearPos.y, 0f);
                    weaponTransform.rotation = spearThrowRot;
                }

                if (!hasHitPlayer && targetPlayer != null)
                {
                    float distToSpear = Vector2.Distance(curSpearPos, targetPlayer.transform.position);
                    if (distToSpear <= throwLineWidth * 0.75f)
                    {
                        hasHitPlayer = true;
                        targetPlayer.TakeDamage(throwDamage, throwDir);
                    }
                }

                yield return null;
            }

            // 3. Thương cắm ở cuối đường bay 0.55s rồi bay ngược về tay Hiệp sĩ
            yield return new WaitForSeconds(0.30f);
            yield return StartCoroutine(ReturnWeaponToHandRoutine(0.22f));

            lastThrowTime = Time.time;

            // 4. Quãng nghỉ đứng yên 1.4s còn lại (tổng nghỉ ~1.95s)
            yield return StartCoroutine(EnterPostAttackRestRoutine(Mathf.Max(0f, throwRecoveryTime - 0.30f)));
        }

        private IEnumerator JumpSlamRoutine()
        {
            isAttacking = true;
            isPerformingJumpSlam = true;
            currentState = AIState.Attack;

            Vector2 startPos = transform.position;
            Vector2 landingPos = targetPlayer != null ? (Vector2)targetPlayer.transform.position : startPos;

            float preJumpWarn = 0.35f;
            float flightTime = Mathf.Max(0.2f, jumpAirTime);

            // 1. Hiện vòng tròn cảnh báo đỏ mờ tại điểm rơi trong tổng thời gian 1.40s
            BossTelegraphVFX.SpawnCircle(landingPos, jumpSlamRadius, preJumpWarn + flightTime);

            // Hóa ĐỎ RỰC nhún chân lấy đà 0.35s trước khi bật nhảy
            yield return StartCoroutine(PlayRedWarningRoutine(preJumpWarn, 1.15f));

            SetPhysicsLocked(true);
            if (col != null) col.enabled = false;
            if (sr != null) sr.color = new Color(1f, 0.4f, 0.4f, 1f);

            if (weaponTransform != null)
            {
                weaponTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            }

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

            // 2. Đáp xuống tâm vòng tròn cảnh báo đỏ
            transform.position = new Vector3(landingPos.x, landingPos.y, 0f);
            if (sr != null) sr.color = baseTintColor;
            if (col != null && IsAlive) col.enabled = true;

            if (IsAlive && targetPlayer != null)
            {
                float dist = Vector2.Distance(landingPos, targetPlayer.transform.position);
                if (dist <= jumpSlamRadius)
                {
                    Vector2 kbDir = ((Vector2)targetPlayer.transform.position - landingPos).normalized;
                    targetPlayer.TakeDamage(jumpSlamDamage, kbDir);
                }
            }

            lastJumpTime = Time.time;

            // 3. Quãng nghỉ 2.1s rút thương khỏi mặt đất rồi dựng thương lại
            yield return new WaitForSeconds(0.35f);
            ResetWeaponToHandImmediate();
            isPerformingJumpSlam = false;

            yield return StartCoroutine(EnterPostAttackRestRoutine(Mathf.Max(0f, jumpRecoveryTime - 0.35f)));
        }

        private Vector3 GetCurrentHandLocalPos()
        {
            float absX = Mathf.Abs(weaponRestLocalPos.x);
            float signedX = (sr != null && sr.flipX) ? -absX : absX;
            return new Vector3(signedX, weaponRestLocalPos.y, weaponRestLocalPos.z);
        }

        private IEnumerator ReturnWeaponToHandRoutine(float duration)
        {
            if (weaponTransform == null) yield break;

            Vector3 startWorldPos = weaponTransform.position;
            Quaternion startWorldRot = weaponTransform.rotation;
            float elapsed = 0f;

            while (elapsed < duration && IsAlive)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector3 targetWorldPos = transform.TransformPoint(GetCurrentHandLocalPos());
                Quaternion targetWorldRot = transform.rotation * weaponRestLocalRot;

                weaponTransform.position = Vector3.Lerp(startWorldPos, targetWorldPos, t);
                weaponTransform.rotation = Quaternion.Slerp(startWorldRot, targetWorldRot, t);
                yield return null;
            }

            ResetWeaponToHandImmediate();
        }

        private void ResetWeaponToHandImmediate()
        {
            if (weaponTransform == null) return;
            weaponTransform.localPosition = GetCurrentHandLocalPos();
            weaponTransform.localRotation = weaponRestLocalRot;
        }

        private float GetUnobstructedThrowDistance(Vector2 dir, float maxDist)
        {
            int wallMask = LayerMask.GetMask("Obstacle", "Default");
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, maxDist, wallMask);
            foreach (var h in hits)
            {
                if (h.collider != null && !h.collider.isTrigger && h.collider.gameObject != gameObject && h.collider.GetComponent<HunterPlayer>() == null)
                {
                    return Mathf.Max(2.0f, h.distance - 0.4f);
                }
            }
            return maxDist;
        }

        protected override void SpawnLoot()
        {
            if (partnerKnight != null && partnerKnight.IsAlive)
            {
                return;
            }
            base.SpawnLoot();
        }

        protected override void Die()
        {
            isPerformingJumpSlam = false;
            ResetWeaponToHandImmediate();
            base.Die();
        }
    }
}
