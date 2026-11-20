using System;
using System.Collections;
using UnityEngine;
using NguyenVanHai.Combat;
using NguyenVanHai.Data;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// Boss Tầng 4 (Dual Boss - Hiệp Sĩ Kiếm Đỏ): Kế thừa EnemyBase.
    /// - Đọc toàn bộ thông số từ dungeon_combat_config.json (id: "boss_sword_knight").
    /// </summary>
    [ExecuteAlways]
    public class BossSwordKnightAI : EnemyBase
    {
        [Header("--- Tham Chiếu Vũ Khí Rời & Đồng Đội ---")]
        [Tooltip("Kéo GameObject con chứa Sprite Thanh Kiếm rời (Boss_SwordKnight1_0) vào đây. Nếu để trống sẽ tự tìm object con.")]
        [SerializeField] private Transform weaponTransform;
        [Tooltip("Kéo GameObject Hiệp Sĩ Thương (BossSpearKnightAI) vào đây để phối hợp nhịp chiêu")]
        [SerializeField] private BossSpearKnightAI partnerKnight;
        [Tooltip("Tùy chọn: Kéo 4 Sprite VFX_Slash_Arc_0..3 để tạo thêm hiệu ứng lốc kiếm khi xoay 360 độ")]
        [SerializeField] private Sprite[] slashArcSprites;

        [Header("--- Đòn 1: Chém 360 Độ (Kiếm Bay Vòng Tròn) ---")]
        [SerializeField] private float spinRadius = 2.8f;
        [SerializeField] private float spinDamage = 35f;
        [SerializeField] private float spinTelegraphTime = 1.05f;
        [SerializeField] private float spinActiveDuration = 0.65f;
        [SerializeField] private int spinRevolutions = 2;
        [SerializeField] private float spinRecoveryTime = 1.80f;
        [SerializeField] private float spinCooldown = 4.8f;

        [Header("--- Đòn 2: Kiếm Từ Trên Cao Bay Xuống Player ---")]
        [SerializeField] private float skySwordRadius = 2.2f;
        [SerializeField] private float skySwordDamage = 40f;
        [SerializeField] private float skySwordTelegraphTime = 1.15f;
        [SerializeField] private float skyDropHeight = 9.0f;
        [SerializeField] private float skyDropDuration = 0.22f;
        [SerializeField] private float skySwordRecoveryTime = 2.00f;
        [SerializeField] private float skySwordCooldown = 6.5f;

        [Header("--- Giãn Cách Giữa 2 Chiêu ---")]
        [SerializeField] private float minMoveBetweenSkills = 1.6f;

        private Vector3 weaponRestLocalPos = new Vector3(-0.78f, 0.25f, 0f);
        private Quaternion weaponRestLocalRot = Quaternion.identity;
        private SpriteRenderer weaponSr;
        private float lastSpinTime = -99f;
        private float lastSkySwordTime = -99f;
        private bool isEnraged = false;
        private bool hasEngaged = false;

        public bool IsPerformingMajorSkill => isAttacking;

        public static event Action<BossSwordKnightAI> OnBossSpawned;
        public static event Action OnDualBossDefeated;

        protected override void Awake()
        {
            isBoss = true;
            sr = GetComponent<SpriteRenderer>();
            if (!Application.isPlaying)
            {
                SyncWeaponSortingAndHandOffset();
                return;
            }

            var cfg = DungeonCombatDatabase.GetBoss("boss_sword_knight");
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
                    spinDamage = cfg.skill1.damage;
                    spinRadius = cfg.skill1.rangeOrRadius;
                    spinRevolutions = cfg.skill1.projectileCount > 0 ? cfg.skill1.projectileCount : 2;
                    spinTelegraphTime = cfg.skill1.telegraphTime;
                    spinActiveDuration = cfg.skill1.activeDuration;
                    spinRecoveryTime = cfg.skill1.recoveryTime;
                    spinCooldown = cfg.skill1.cooldown;
                }

                if (cfg.skill2 != null)
                {
                    skySwordDamage = cfg.skill2.damage;
                    skySwordRadius = cfg.skill2.rangeOrRadius;
                    skyDropHeight = cfg.skill2.arcOrDropHeight;
                    skySwordTelegraphTime = cfg.skill2.telegraphTime;
                    skyDropDuration = cfg.skill2.activeDuration;
                    skySwordRecoveryTime = cfg.skill2.recoveryTime;
                    skySwordCooldown = cfg.skill2.cooldown;
                }
            }
            else
            {
                maxHealth = 600f;
                moveSpeed = 2.7f;
                attackDamage = 35f;
                detectRange = 15f;
                attackRange = 2.8f;
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
                partnerKnight = FindObjectOfType<BossSpearKnightAI>();
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
                    weaponTransform.localPosition = new Vector3(-0.78f, 0.25f, 0f);
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
                bool shouldFlip = toPlayer.x > 0f;
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
                // 1. Nếu ở gần Player (<= spinRadius): Chém 360 độ
                if (distanceToPlayer <= spinRadius && Time.time >= lastSpinTime + (spinCooldown * cdMult))
                {
                    StartCoroutine(SpinSword360Routine());
                    return;
                }

                // 2. Kiếm từ trên cao bay xuống Player (Nhường nhịp nếu Hiệp sĩ Thương đang ra chiêu)
                bool partnerBusy = partnerKnight != null && partnerKnight.IsAlive && partnerKnight.IsPerformingJumpSlam;
                if (!partnerBusy && Time.time >= lastSkySwordTime + (skySwordCooldown * cdMult) && distanceToPlayer <= 11f)
                {
                    StartCoroutine(SkySwordStrikeRoutine());
                    return;
                }
            }

            // Di chuyển áp sát người chơi
            currentState = AIState.Chase;
            rb.velocity = toPlayer.normalized * (isEnraged ? moveSpeed * 1.15f : moveSpeed);
        }

        private void ActivateEnrage()
        {
            isEnraged = true;
            baseTintColor = new Color(1f, 0.78f, 0.78f, 1f);
            if (sr != null) sr.color = baseTintColor;
        }

        private IEnumerator SpinSword360Routine()
        {
            isAttacking = true;
            currentState = AIState.Attack;

            float warnTime = spinTelegraphTime;

            // 1. Hiện vòng tròn cảnh báo đỏ dưới đất + Thân Hiệp sĩ hóa ĐỎ RỰC trong 1.05s
            BossTelegraphVFX.SpawnCircle(transform.position, spinRadius, warnTime);

            SetPhysicsLocked(true);
            if (flash != null)
            {
                flash.StopFlashImmediate();
                flash.IsWarningActive = true;
            }

            Color redBright = new Color(1f, 0.15f, 0.15f, 1f);
            Color redDeep = new Color(1f, 0.42f, 0.12f, 1f);
            Vector3 handPos = GetCurrentHandLocalPos();

            float elapsed = 0f;
            while (elapsed < warnTime && IsAlive)
            {
                elapsed += Time.deltaTime;
                float pulse = (Mathf.Sin(elapsed * 20f) + 1f) * 0.5f;
                if (sr != null) sr.color = Color.Lerp(redBright, redDeep, pulse);

                if (weaponTransform != null)
                {
                    float offset = Mathf.Sin(elapsed * 45f) * 0.06f;
                    weaponTransform.localPosition = handPos + new Vector3(offset, Mathf.Abs(offset), 0f);
                }
                yield return null;
            }

            if (sr != null) sr.color = baseTintColor;
            if (flash != null) flash.IsWarningActive = false;

            // 2. Cho thanh kiếm rời tay bay vòng tròn 360 độ xung quanh Hiệp sĩ
            if (slashArcSprites != null && slashArcSprites.Length > 0)
            {
                StartCoroutine(SpawnSpinSlashVfxRoutine());
            }

            float activeDur = Mathf.Max(0.1f, spinActiveDuration);
            float spinElapsed = 0f;
            bool hasHitPlayer = false;
            float orbitR = spinRadius * 0.82f;

            while (spinElapsed < activeDur && IsAlive)
            {
                spinElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(spinElapsed / activeDur);
                float angleDeg = t * 360f * spinRevolutions;
                float rad = angleDeg * Mathf.Deg2Rad;

                if (weaponTransform != null)
                {
                    Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitR;
                    weaponTransform.position = transform.position + offset;
                    weaponTransform.rotation = Quaternion.Euler(0f, 0f, angleDeg - 90f);
                }

                if (!hasHitPlayer && targetPlayer != null)
                {
                    float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
                    if (dist <= spinRadius)
                    {
                        hasHitPlayer = true;
                        Vector2 kbDir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;
                        targetPlayer.TakeDamage(spinDamage, kbDir);
                    }
                }

                yield return null;
            }

            // 3. Thu kiếm bay mượt về tay
            yield return StartCoroutine(ReturnWeaponToHandRoutine(0.25f));

            lastSpinTime = Time.time;

            // 4. Quãng nghỉ đứng yên 1.8s
            yield return StartCoroutine(EnterPostAttackRestRoutine(spinRecoveryTime));
        }

        private IEnumerator SpawnSpinSlashVfxRoutine()
        {
            float activeDur = Mathf.Max(0.1f, spinActiveDuration);
            for (int dirIdx = 0; dirIdx < 4; dirIdx++)
            {
                float angle = dirIdx * 90f;
                var vfxGo = new GameObject("SpinSlash_VFX");
                vfxGo.transform.position = transform.position;
                vfxGo.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                vfxGo.transform.localScale = Vector3.one * 1.8f;

                var vfxSr = vfxGo.AddComponent<SpriteRenderer>();
                vfxSr.color = new Color(1f, 0.3f, 0.3f, 0.85f);
                vfxSr.sortingLayerName = "Projectiles_VFX";
                vfxSr.sortingOrder = 5850;
                vfxSr.sprite = slashArcSprites[dirIdx % slashArcSprites.Length];

                Destroy(vfxGo, activeDur);
            }
            yield return null;
        }

        private IEnumerator SkySwordStrikeRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;

            Vector2 strikePos = targetPlayer != null ? (Vector2)targetPlayer.transform.position : (Vector2)transform.position;
            float warnTime = skySwordTelegraphTime;

            // 1. Hiện vòng tròn cảnh báo đỏ tại vị trí Player + Thân Hiệp sĩ hóa ĐỎ RỰC trong 1.15s
            BossTelegraphVFX.SpawnCircle(strikePos, skySwordRadius, warnTime);

            SetPhysicsLocked(true);
            if (flash != null)
            {
                flash.StopFlashImmediate();
                flash.IsWarningActive = true;
            }

            Color redBright = new Color(1f, 0.15f, 0.15f, 1f);
            Color redDeep = new Color(1f, 0.42f, 0.12f, 1f);
            Vector3 launchStart = weaponTransform != null ? weaponTransform.position : transform.position;
            Vector3 skyApex = new Vector3(strikePos.x, strikePos.y + skyDropHeight, 0f);

            float elapsed = 0f;
            while (elapsed < warnTime && IsAlive)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / warnTime);

                float pulse = (Mathf.Sin(elapsed * 20f) + 1f) * 0.5f;
                if (sr != null) sr.color = Color.Lerp(redBright, redDeep, pulse);

                if (weaponTransform != null)
                {
                    if (t < 0.45f)
                    {
                        float upT = t / 0.45f;
                        weaponTransform.position = Vector3.Lerp(launchStart, launchStart + Vector3.up * skyDropHeight, upT);
                        weaponTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                    else
                    {
                        weaponTransform.position = skyApex;
                        weaponTransform.rotation = Quaternion.Euler(0f, 0f, 180f);
                    }
                }
                yield return null;
            }

            if (sr != null) sr.color = baseTintColor;
            if (flash != null) flash.IsWarningActive = false;

            // 2. Phóng thanh kiếm từ đỉnh trời cắm thẳng xuống tâm vòng tròn cảnh báo
            float dropDur = Mathf.Max(0.05f, skyDropDuration);
            float dropElapsed = 0f;
            while (dropElapsed < dropDur && IsAlive)
            {
                dropElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(dropElapsed / dropDur);
                if (weaponTransform != null)
                {
                    weaponTransform.position = Vector3.Lerp(skyApex, new Vector3(strikePos.x, strikePos.y, 0f), t);
                    weaponTransform.rotation = Quaternion.Euler(0f, 0f, 180f);
                }
                yield return null;
            }

            if (weaponTransform != null)
            {
                weaponTransform.position = new Vector3(strikePos.x, strikePos.y, 0f);
            }

            if (IsAlive && targetPlayer != null)
            {
                float dist = Vector2.Distance(strikePos, targetPlayer.transform.position);
                if (dist <= skySwordRadius)
                {
                    targetPlayer.TakeDamage(skySwordDamage);
                }
            }

            // 3. Kiếm cắm dưới mặt đất 0.65s rồi bay thu hồi về tay Hiệp sĩ
            yield return new WaitForSeconds(0.35f);
            yield return StartCoroutine(ReturnWeaponToHandRoutine(0.22f));

            lastSkySwordTime = Time.time;

            // 4. Quãng nghỉ đứng yên 1.4s còn lại (tổng nghỉ ~2.0s)
            yield return StartCoroutine(EnterPostAttackRestRoutine(Mathf.Max(0f, skySwordRecoveryTime - 0.35f)));
        }

        private Vector3 GetCurrentHandLocalPos()
        {
            float absX = Mathf.Abs(weaponRestLocalPos.x);
            float signedX = (sr != null && sr.flipX) ? absX : -absX;
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
            ResetWeaponToHandImmediate();
            base.Die();

            if (partnerKnight == null || !partnerKnight.IsAlive)
            {
                OnDualBossDefeated?.Invoke();
            }
        }
    }
}
