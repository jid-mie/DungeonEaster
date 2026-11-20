using System;
using System.Collections;
using UnityEngine;
using NguyenVanHai.Combat;
using NguyenVanHai.Data;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// Boss Cuối Tầng 5: Gwyn - Thần Sáng Tạo (Kế thừa EnemyBase).
    /// - Đọc toàn bộ thông số từ dungeon_combat_config.json (id: "boss_gwyn").
    ///
    /// SKILLS (4 Skills + 1 Background Loop):
    ///   Skill 1 → Phi Kiếm (Sword Throw)   : Giống SpearThrow — kiếm bay theo đường thẳng có cảnh báo đỏ dưới đất
    ///   Skill 2 → Quét 360° (Spin Slash)   : Giống SpinSword360 — kiếm xoay quanh boss, damage ai trong spinRadius
    ///   Skill 3 → Bắn Đạn Chùm (Fan Shot) : Chùm đạn tỏa rộng, cảnh báo đỏ dưới chân
    ///   Skill 4 → Lao Người (Godspeed Dash): Đường thẳng lao nhanh với cảnh báo đỏ
    ///   Background → Sky Barrage Loop      : Coroutine nền — rơi 1 thiên thạch mỗi 2s độc lập với skills
    /// </summary>
    [ExecuteAlways]
    public class BossGwynAI : EnemyBase
    {
        // ===== Weapon Transform (giống SwordKnight / SpearKnight) =====
        [Header("--- Vũ Khí & Sprites ---")]
        [Tooltip("Kéo GameObject con chứa Sprite Thanh Kiếm của Gwyn vào đây. Nếu để trống sẽ tự tìm object con.")]
        [SerializeField] private Transform weaponTransform;
        [Tooltip("Sprite chùm đạn (VFX_Poison_Spore hoặc tương tự)")]
        [SerializeField] private Sprite[] projectileSprites;
        [Tooltip("Sprite vụ nổ thiên thạch (VFX_Explosion_0..3)")]
        [SerializeField] private Sprite[] explosionSprites;
        [Tooltip("Sprite hiệu ứng Slash khi Quét 360° (tùy chọn)")]
        [SerializeField] private Sprite[] slashArcSprites;
        [SerializeField] private Color projectileTint = new Color(1f, 0.78f, 0.25f, 1f);

        // ===== SKILL 1: Phi Kiếm (Sword Throw) =====
        [Header("--- Đòn 1: Phi Kiếm (Sword Throw) ---")]
        [SerializeField] private float throwDamage = 55f;
        [SerializeField] private float throwFlySpeed = 14.0f;
        [SerializeField] private float throwMaxDistance = 9.5f;
        [SerializeField] private float throwLineWidth = 1.1f;
        [SerializeField] private float throwTelegraphTime = 1.05f;
        [SerializeField] private float throwRecoveryTime = 1.3f;
        [SerializeField] private float throwCooldown = 5.0f;

        // ===== SKILL 2: Quét 360° (Spin Slash) =====
        [Header("--- Đòn 2: Quét Kiếm 360° (Spin Slash) ---")]
        [SerializeField] private float spinDamage = 50f;
        [SerializeField] private float spinRadius = 2.8f;
        [SerializeField] private float spinTelegraphTime = 1.0f;
        [SerializeField] private float spinActiveDuration = 0.65f;
        [SerializeField] private int spinRevolutions = 2;
        [SerializeField] private float spinRecoveryTime = 1.3f;
        [SerializeField] private float spinCooldown = 5.0f;

        // ===== SKILL 3: Bắn Đạn Chùm (Fan Shot) =====
        [Header("--- Đòn 3: Bắn Chùm Đạn Từ Xa (Fan Shot) ---")]
        [SerializeField] private float rangedDamage = 50f;
        [SerializeField] private float projectileSpeed = 6.0f;
        [SerializeField] private int phase1ProjectileCount = 5;
        [SerializeField] private int phase2ProjectileCount = 7;
        [SerializeField] private float spreadAngle = 65f;
        [SerializeField] private float shootTelegraphTime = 0.90f;
        [SerializeField] private float shootRecoveryTime = 1.20f;
        [SerializeField] private float shootCooldown = 3.5f;

        // ===== SKILL 4: Lao Người (Godspeed Dash) =====
        [Header("--- Đòn 4: Lao Người Thần Tốc (Godspeed Dash) ---")]
        [SerializeField] private float dashDamage = 60f;
        [SerializeField] private float dashSpeed = 11.0f;
        [SerializeField] private float dashDistance = 8.5f;
        [SerializeField] private float dashWidth = 1.6f;
        [SerializeField] private float dashTelegraphTime = 1.05f;
        [SerializeField] private float dashRecoveryTime = 1.5f;
        [SerializeField] private float dashCooldown = 6.0f;

        // ===== BACKGROUND: Sky Barrage Loop =====
        [Header("--- Background: Mưa Thiên Thạch Liên Tục (Mỗi 2s/viên) ---")]
        [SerializeField] private float skyBarrageInterval = 2.0f;    // Mỗi N giây rơi 1 viên
        [SerializeField] private float skyBarrageDamage = 65f;
        [SerializeField] private float skyBarrageRadius = 2.2f;
        [SerializeField] private float skyBarrageDropHeight = 8.0f;
        [SerializeField] private float skyBarrageTelegraphTime = 1.1f;
        [SerializeField] private float skyBarrageWarmup = 3.0f;      // Chờ 3s đầu trận trước khi bắt đầu

        // ===== Khoảng Cách & Giãn Cách =====
        [Header("--- Khoảng Cách & Giãn Cách Chiêu ---")]
        [SerializeField] private float preferredMinRange = 3.8f;
        [SerializeField] private float preferredMaxRange = 9.0f;
        [SerializeField] private float minMoveBetweenSkills = 1.0f;

        // ===== State =====
        private Vector3 weaponRestLocalPos = new Vector3(0.78f, 0.05f, 0f);
        private Quaternion weaponRestLocalRot = Quaternion.identity;
        private SpriteRenderer weaponSr;

        private float lastThrowTime  = -99f;
        private float lastSpinTime   = -99f;
        private float lastShootTime  = -99f;
        private float lastDashTime   = -99f;
        private bool isPhase2 = false;
        private bool hasEngaged = false;
        private bool skyBarrageStarted = false;

        private static Sprite fallbackOrbSprite;

        public static event Action<BossGwynAI> OnBossSpawned;
        public static event Action OnBossDefeated;

        // ============================================================
        // AWAKE — load JSON
        // ============================================================
        protected override void Awake()
        {
            isBoss = true;
            sr = GetComponent<SpriteRenderer>();

            if (!Application.isPlaying)
            {
                SyncWeaponSortingAndHandOffset();
                return;
            }

            var cfg = DungeonCombatDatabase.GetBoss("boss_gwyn");
            if (cfg != null)
            {
                maxHealth    = cfg.maxHealth;
                moveSpeed    = cfg.moveSpeed;
                attackDamage = cfg.attackDamage;
                detectRange  = cfg.detectRange;
                attackRange  = cfg.attackRange;
                attackCooldown = cfg.attackCooldown;
                minMoveBetweenSkills = cfg.minMoveBetweenSkills;
                preferredMinRange    = cfg.preferredMinRange;
                preferredMaxRange    = cfg.preferredMaxRange;
                dropItemId   = cfg.dropItemId;
                dropItemName = cfg.dropItemName;
                dropCount    = cfg.dropCount;

                // Skill 1: Phi Kiếm
                if (cfg.skill1 != null && cfg.skill1.damage > 0)
                {
                    throwDamage        = cfg.skill1.damage;
                    throwFlySpeed      = cfg.skill1.speed;
                    throwMaxDistance   = cfg.skill1.rangeOrRadius;
                    throwLineWidth     = cfg.skill1.widthOrAngle;
                    throwTelegraphTime = cfg.skill1.telegraphTime;
                    throwRecoveryTime  = cfg.skill1.recoveryTime;
                    throwCooldown      = cfg.skill1.cooldown;
                }

                // Skill 2: Quét 360°
                if (cfg.skill2 != null && cfg.skill2.damage > 0)
                {
                    spinDamage         = cfg.skill2.damage;
                    spinRadius         = cfg.skill2.rangeOrRadius;
                    spinRevolutions    = cfg.skill2.projectileCount > 0 ? cfg.skill2.projectileCount : 2;
                    spinTelegraphTime  = cfg.skill2.telegraphTime;
                    spinActiveDuration = cfg.skill2.activeDuration;
                    spinRecoveryTime   = cfg.skill2.recoveryTime;
                    spinCooldown       = cfg.skill2.cooldown;
                }

                // Skill 3: Bắn Đạn Chùm
                if (cfg.skill3 != null && cfg.skill3.damage > 0)
                {
                    rangedDamage           = cfg.skill3.damage;
                    projectileSpeed        = cfg.skill3.speed;
                    phase1ProjectileCount  = cfg.skill3.projectileCount;
                    phase2ProjectileCount  = cfg.skill3.phase2Count > 0 ? cfg.skill3.phase2Count : cfg.skill3.projectileCount;
                    spreadAngle            = cfg.skill3.widthOrAngle;
                    shootTelegraphTime     = cfg.skill3.telegraphTime;
                    shootRecoveryTime      = cfg.skill3.recoveryTime;
                    shootCooldown          = cfg.skill3.cooldown;
                }

                // Skill 4: Lao Người
                if (cfg.skill4 != null && cfg.skill4.damage > 0)
                {
                    dashDamage        = cfg.skill4.damage;
                    dashSpeed         = cfg.skill4.speed;
                    dashDistance      = cfg.skill4.rangeOrRadius;
                    dashWidth         = cfg.skill4.widthOrAngle;
                    dashTelegraphTime = cfg.skill4.telegraphTime;
                    dashRecoveryTime  = cfg.skill4.recoveryTime;
                    dashCooldown      = cfg.skill4.cooldown;
                }

                // Sky Barrage background (dùng spawnInterval & arcOrDropHeight của skill2 cũ nếu có)
                // Hoặc dùng giá trị riêng từ skyBarrage fields nếu JSON có thêm field sau này
                // Hiện tại dùng Inspector defaults + có thể override bằng JSON extension
            }
            else
            {
                // Fallback cứng nếu JSON không load được
                maxHealth    = 2500f;
                moveSpeed    = 2.8f;
                attackDamage = 55f;
                detectRange  = 16f;
                attackRange  = 10f;
                attackCooldown = 2.5f;
                dropItemId   = "life_core";
                dropItemName = "Hạt nhân Sinh Mệnh";
                dropCount    = 1;
            }

            base.Awake();
            InitializeWeaponReference();
            // Đảm bảo maxHealth từ JSON được áp dụng (override Inspector serialized value)
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
            if (!Application.isPlaying) return;
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (weaponSr == null && weaponTransform != null)
                weaponSr = weaponTransform.GetComponent<SpriteRenderer>();

            if (weaponSr != null && sr != null)
            {
                weaponSr.sortingLayerID = sr.sortingLayerID;
                weaponSr.sortingOrder   = sr.sortingOrder + 10;
            }
        }

        // ============================================================
        // UPDATE AI
        // ============================================================
        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking || Time.time < postAttackRestUntil || targetPlayer == null) return;

            if (distanceToPlayer > detectRange)
            {
                currentState = AIState.Idle;
                rb.velocity  = Vector2.zero;
                return;
            }

            // Lần đầu engage: fire event + bắt đầu Sky Barrage background
            if (!hasEngaged)
            {
                hasEngaged = true;
                OnBossSpawned?.Invoke(this);
            }

            // Bắt đầu background loop một lần duy nhất
            if (!skyBarrageStarted)
            {
                skyBarrageStarted = true;
                StartCoroutine(SkyBarrageBackgroundLoop());
            }

            // Phase 2 khi HP <= 50%
            if (!isPhase2 && HealthPercent <= 0.5f)
            {
                isPhase2 = true;
                baseTintColor = new Color(1f, 0.88f, 0.65f, 1f);
                if (sr != null) sr.color = baseTintColor;
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

            float cdMult = isPhase2 ? 0.85f : 1.0f;
            bool canCastAnySkill = Time.time >= lastAttackTime + minMoveBetweenSkills;

            if (canCastAnySkill)
            {
                // 1. Quét 360° — ưu tiên khi player ở gần (spin radius)
                if (distanceToPlayer <= spinRadius * 1.1f && Time.time >= lastSpinTime + (spinCooldown * cdMult))
                {
                    StartCoroutine(SpinSlash360Routine());
                    return;
                }

                // 2. Phi Kiếm — tầm trung đến xa
                if (Time.time >= lastThrowTime + (throwCooldown * cdMult) && distanceToPlayer <= throwMaxDistance && distanceToPlayer >= 2.0f)
                {
                    StartCoroutine(SwordThrowRoutine());
                    return;
                }

                // 3. Lao Người — tầm phù hợp
                if (Time.time >= lastDashTime + (dashCooldown * cdMult) && distanceToPlayer >= 2.5f && distanceToPlayer <= dashDistance + 0.5f)
                {
                    StartCoroutine(GodspeedDashRoutine());
                    return;
                }

                // 4. Bắn Đạn Chùm — cooldown ngắn, fill gap
                if (Time.time >= lastShootTime + (shootCooldown * cdMult) && distanceToPlayer <= 11f)
                {
                    StartCoroutine(RangedFanShotRoutine());
                    return;
                }
            }

            // Di chuyển giữ vị trí chiến thuật
            float spd = isPhase2 ? moveSpeed * 1.12f : moveSpeed;
            if (distanceToPlayer > preferredMaxRange)
            {
                currentState = AIState.Chase;
                rb.velocity  = toPlayer.normalized * spd;
            }
            else if (distanceToPlayer < preferredMinRange)
            {
                currentState = AIState.Chase;
                rb.velocity  = -toPlayer.normalized * (spd * 0.65f);
            }
            else
            {
                currentState = AIState.Chase;
                Vector2 perp = new Vector2(-toPlayer.y, toPlayer.x).normalized;
                rb.velocity  = perp * (spd * 0.45f);
            }
        }

        // ============================================================
        // SKILL 1: Phi Kiếm (Sword Throw) — tham khảo SpearThrowRoutine
        // ============================================================
        private IEnumerator SwordThrowRoutine()
        {
            isAttacking  = true;
            currentState = AIState.Attack;

            Vector2 throwDir = targetPlayer != null
                ? ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized
                : Vector2.down;
            if (throwDir.sqrMagnitude < 0.001f) throwDir = Vector2.down;

            float actualDist = GetUnobstructedDistance(throwDir, throwMaxDistance);
            float aimAngle   = Mathf.Atan2(throwDir.y, throwDir.x) * Mathf.Rad2Deg;

            // 1. Hiện đường cảnh báo đỏ dưới đất + thân Gwyn hóa ĐỎ RỰC
            BossTelegraphVFX.SpawnLine(transform.position, throwDir, actualDist, throwLineWidth, throwTelegraphTime);

            SetPhysicsLocked(true);
            if (flash != null) { flash.StopFlashImmediate(); flash.IsWarningActive = true; }

            Color redBright = new Color(1f, 0.15f, 0.15f, 1f);
            Color redDeep   = new Color(1f, 0.42f, 0.12f, 1f);
            Vector3 startHandWorld   = weaponTransform != null ? weaponTransform.position : transform.position;
            Quaternion throwRotation = Quaternion.Euler(0f, 0f, aimAngle - 90f);

            float elapsed = 0f;
            while (elapsed < throwTelegraphTime && IsAlive)
            {
                elapsed += Time.deltaTime;
                float t     = Mathf.Clamp01(elapsed / throwTelegraphTime);
                float pulse = (Mathf.Sin(elapsed * 20f) + 1f) * 0.5f;
                if (sr != null) sr.color = Color.Lerp(redBright, redDeep, pulse);

                if (weaponTransform != null)
                {
                    weaponTransform.rotation = Quaternion.Slerp(transform.rotation * weaponRestLocalRot, throwRotation, Mathf.Min(1f, t * 2f));
                    weaponTransform.position = startHandWorld - (Vector3)(throwDir * (0.35f * t));
                }
                yield return null;
            }

            if (sr    != null) sr.color = baseTintColor;
            if (flash != null) flash.IsWarningActive = false;

            // 2. Phóng kiếm bay dọc theo đường cảnh báo đỏ
            Vector2 spearStart = transform.position;
            Vector2 spearEnd   = spearStart + throwDir * actualDist;
            float flyDuration  = Mathf.Max(0.18f, actualDist / Mathf.Max(1f, throwFlySpeed));
            float flyElapsed   = 0f;
            bool hasHit        = false;

            while (flyElapsed < flyDuration && IsAlive)
            {
                flyElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(flyElapsed / flyDuration);
                Vector2 curPos = Vector2.Lerp(spearStart, spearEnd, t);

                if (weaponTransform != null)
                {
                    weaponTransform.position = new Vector3(curPos.x, curPos.y, 0f);
                    weaponTransform.rotation = throwRotation;
                }

                if (!hasHit && targetPlayer != null)
                {
                    float distToWeapon = Vector2.Distance(curPos, targetPlayer.transform.position);
                    if (distToWeapon <= throwLineWidth * 0.75f)
                    {
                        hasHit = true;
                        targetPlayer.TakeDamage(throwDamage, throwDir);
                    }
                }
                yield return null;
            }

            // 3. Kiếm cắm ở cuối đường 0.3s rồi thu về tay
            yield return new WaitForSeconds(0.30f);
            yield return StartCoroutine(ReturnWeaponToHandRoutine(0.22f));

            lastThrowTime = Time.time;
            yield return StartCoroutine(EnterPostAttackRestRoutine(Mathf.Max(0f, throwRecoveryTime - 0.30f)));
        }

        // ============================================================
        // SKILL 2: Quét Kiếm 360° (Spin Slash) — tham khảo SpinSword360Routine
        // ============================================================
        private IEnumerator SpinSlash360Routine()
        {
            isAttacking  = true;
            currentState = AIState.Attack;

            // 1. Vòng tròn cảnh báo đỏ + Gwyn hóa ĐỎ RỰC
            BossTelegraphVFX.SpawnCircle(transform.position, spinRadius, spinTelegraphTime);

            SetPhysicsLocked(true);
            if (flash != null) { flash.StopFlashImmediate(); flash.IsWarningActive = true; }

            Color redBright = new Color(1f, 0.15f, 0.15f, 1f);
            Color redDeep   = new Color(1f, 0.42f, 0.12f, 1f);
            Vector3 handPos = GetCurrentHandLocalPos();

            float elapsed = 0f;
            while (elapsed < spinTelegraphTime && IsAlive)
            {
                elapsed += Time.deltaTime;
                float pulse = (Mathf.Sin(elapsed * 20f) + 1f) * 0.5f;
                if (sr != null) sr.color = Color.Lerp(redBright, redDeep, pulse);

                if (weaponTransform != null)
                {
                    float vibrate = Mathf.Sin(elapsed * 45f) * 0.06f;
                    weaponTransform.localPosition = handPos + new Vector3(vibrate, Mathf.Abs(vibrate), 0f);
                }
                yield return null;
            }

            if (sr    != null) sr.color = baseTintColor;
            if (flash != null) flash.IsWarningActive = false;

            // VFX slash arc tùy chọn
            if (slashArcSprites != null && slashArcSprites.Length > 0)
                StartCoroutine(SpawnSpinSlashVfxRoutine());

            // 2. Kiếm xoay vòng tròn 360° * spinRevolutions
            float activeDur  = Mathf.Max(0.1f, spinActiveDuration);
            float spinElapsed = 0f;
            bool hasHitPlayer = false;
            float orbitR = spinRadius * 0.82f;

            while (spinElapsed < activeDur && IsAlive)
            {
                spinElapsed += Time.deltaTime;
                float t        = Mathf.Clamp01(spinElapsed / activeDur);
                float angleDeg = t * 360f * spinRevolutions;
                float rad      = angleDeg * Mathf.Deg2Rad;

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

            // 3. Thu kiếm mượt về tay
            yield return StartCoroutine(ReturnWeaponToHandRoutine(0.25f));

            lastSpinTime = Time.time;
            yield return StartCoroutine(EnterPostAttackRestRoutine(spinRecoveryTime));
        }

        private IEnumerator SpawnSpinSlashVfxRoutine()
        {
            float activeDur = Mathf.Max(0.1f, spinActiveDuration);
            for (int dirIdx = 0; dirIdx < 4; dirIdx++)
            {
                float angle  = dirIdx * 90f;
                var vfxGo    = new GameObject("GwynSpinSlash_VFX");
                vfxGo.transform.position    = transform.position;
                vfxGo.transform.rotation    = Quaternion.Euler(0f, 0f, angle);
                vfxGo.transform.localScale  = Vector3.one * 1.8f;

                var vfxSr = vfxGo.AddComponent<SpriteRenderer>();
                vfxSr.color             = new Color(1f, 0.3f, 0.3f, 0.85f);
                vfxSr.sortingLayerName  = "Projectiles_VFX";
                vfxSr.sortingOrder      = 5850;
                vfxSr.sprite            = slashArcSprites[dirIdx % slashArcSprites.Length];

                Destroy(vfxGo, activeDur);
            }
            yield return null;
        }

        // ============================================================
        // SKILL 3: Bắn Chùm Đạn Từ Xa (Fan Shot)
        // ============================================================
        private IEnumerator RangedFanShotRoutine()
        {
            isAttacking  = true;
            currentState = AIState.Attack;

            // 1. Vòng tròn cảnh báo nhỏ dưới chân + Gwyn hóa ĐỎ RỰC
            BossTelegraphVFX.SpawnCircle(transform.position, 1.4f, shootTelegraphTime);
            yield return StartCoroutine(PlayRedWarningRoutine(shootTelegraphTime, 1.14f));

            // 2. Bắn chùm đạn tỏa rộng về phía Player
            if (IsAlive && targetPlayer != null)
            {
                Vector2 aimDir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;
                if (aimDir.sqrMagnitude < 0.001f) aimDir = Vector2.down;

                float baseAngle   = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
                int   count       = isPhase2 ? phase2ProjectileCount : phase1ProjectileCount;
                float totalSpread = isPhase2 ? spreadAngle * 1.2f : spreadAngle;

                for (int i = 0; i < count; i++)
                {
                    float offsetAngle = 0f;
                    if (count > 1)
                    {
                        float t = (float)i / (count - 1) - 0.5f;
                        offsetAngle = t * totalSpread;
                    }
                    float rad      = (baseAngle + offsetAngle) * Mathf.Deg2Rad;
                    Vector2 shotDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                    SpawnGwynOrb(shotDir);
                }
            }

            lastShootTime = Time.time;
            yield return StartCoroutine(EnterPostAttackRestRoutine(shootRecoveryTime));
        }

        private void SpawnGwynOrb(Vector2 dir)
        {
            var go = new GameObject("GwynArcaneOrb");
            go.transform.position   = transform.position + (Vector3)(dir * 0.65f);
            go.transform.localScale = Vector3.one * 1.35f;

            var orbSr = go.AddComponent<SpriteRenderer>();
            orbSr.sprite           = (projectileSprites != null && projectileSprites.Length > 0)
                ? projectileSprites[0] : GetOrCreateFallbackOrbSprite();
            orbSr.color            = projectileTint;
            orbSr.sortingLayerName = "Projectiles_VFX";
            orbSr.sortingOrder     = 5900;

            var orbCol    = go.AddComponent<CircleCollider2D>();
            orbCol.radius = 0.32f;
            orbCol.isTrigger = true;

            var proj = go.AddComponent<SporeProjectile>();
            proj.Initialize(dir, rangedDamage, projectileSpeed, projectileSprites);
        }

        // ============================================================
        // SKILL 4: Lao Người Thần Tốc (Godspeed Dash)
        // ============================================================
        private IEnumerator GodspeedDashRoutine()
        {
            isAttacking  = true;
            currentState = AIState.Attack;

            Vector2 dashDir = targetPlayer != null
                ? ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized
                : Vector2.down;
            if (dashDir.sqrMagnitude < 0.001f) dashDir = Vector2.down;

            float actualDist = GetUnobstructedDistance(dashDir, dashDistance);

            // 1. Đường cảnh báo đỏ + Gwyn hóa ĐỎ RỰC
            BossTelegraphVFX.SpawnLine(transform.position, dashDir, actualDist, dashWidth, dashTelegraphTime);
            yield return StartCoroutine(PlayRedWarningRoutine(dashTelegraphTime, 1.12f));

            // 2. Lao vút qua đường cảnh báo
            float dashDuration = Mathf.Max(0.15f, actualDist / Mathf.Max(1f, dashSpeed));
            float elapsed      = 0f;
            bool hasHit        = false;

            while (elapsed < dashDuration && IsAlive)
            {
                elapsed    += Time.deltaTime;
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

            rb.velocity  = Vector2.zero;
            lastDashTime = Time.time;
            yield return StartCoroutine(EnterPostAttackRestRoutine(dashRecoveryTime));
        }

        // ============================================================
        // BACKGROUND: Sky Barrage Loop — rơi 1 thiên thạch mỗi skyBarrageInterval giây
        //   • KHÔNG block isAttacking
        //   • Chạy song song với tất cả Skills
        //   • Dừng khi boss chết
        // ============================================================
        private IEnumerator SkyBarrageBackgroundLoop()
        {
            // Chờ warmup đầu trận (default 3s)
            yield return new WaitForSeconds(skyBarrageWarmup);

            while (IsAlive)
            {
                // Chờ interval rồi thả 1 thiên thạch
                yield return new WaitForSeconds(skyBarrageInterval);
                if (!IsAlive) yield break;

                Vector2 targetPos = targetPlayer != null
                    ? (Vector2)targetPlayer.transform.position + UnityEngine.Random.insideUnitCircle * 1.5f
                    : (Vector2)transform.position;

                // Spawn thiên thạch song song (không block)
                StartCoroutine(SingleSkyMeteorRoutine(targetPos));
            }
        }

        private IEnumerator SingleSkyMeteorRoutine(Vector2 impactPos)
        {
            // Vòng tròn cảnh báo đỏ dưới đất
            BossTelegraphVFX.SpawnCircle(impactPos, skyBarrageRadius, skyBarrageTelegraphTime);

            float waitBeforeDrop = skyBarrageTelegraphTime * 0.65f;
            float dropFlyTime    = skyBarrageTelegraphTime * 0.35f;

            yield return new WaitForSeconds(waitBeforeDrop);

            // Spawn meteor rơi từ trên trời xuống
            var meteorGo = new GameObject("GwynSkyMeteor_BG_VFX");
            Vector3 skyStart  = new Vector3(impactPos.x, impactPos.y + skyBarrageDropHeight, 0f);
            Vector3 groundEnd = new Vector3(impactPos.x, impactPos.y, 0f);
            meteorGo.transform.position   = skyStart;
            meteorGo.transform.localScale = Vector3.one * 1.8f;

            var meteorSr = meteorGo.AddComponent<SpriteRenderer>();
            meteorSr.sprite = (projectileSprites != null && projectileSprites.Length > 0)
                ? projectileSprites[0] : GetOrCreateFallbackOrbSprite();
            meteorSr.color             = new Color(1f, 0.35f, 0.15f, 1f);
            meteorSr.sortingLayerName  = "Projectiles_VFX";
            meteorSr.sortingOrder      = 6100;

            float elapsed = 0f;
            while (elapsed < dropFlyTime)
            {
                elapsed += Time.deltaTime;
                if (meteorGo != null)
                    meteorGo.transform.position = Vector3.Lerp(skyStart, groundEnd, Mathf.Clamp01(elapsed / dropFlyTime));
                yield return null;
            }

            if (meteorGo != null) Destroy(meteorGo);

            // Gây sát thương
            if (IsAlive && targetPlayer != null)
            {
                float dist = Vector2.Distance(impactPos, targetPlayer.transform.position);
                if (dist <= skyBarrageRadius)
                    targetPlayer.TakeDamage(skyBarrageDamage);
            }

            // Hiệu ứng vụ nổ
            if (explosionSprites != null && explosionSprites.Length > 0)
                StartCoroutine(PlayExplosionVfxRoutine(impactPos, skyBarrageRadius));
        }

        private IEnumerator PlayExplosionVfxRoutine(Vector2 pos, float radius)
        {
            var expGo = new GameObject("GwynExplosion_VFX");
            expGo.transform.position   = new Vector3(pos.x, pos.y, 0f);
            expGo.transform.localScale = Vector3.one * (radius * 1.1f);

            var expSr = expGo.AddComponent<SpriteRenderer>();
            expSr.sortingLayerName = "Projectiles_VFX";
            expSr.sortingOrder     = 6200;

            for (int i = 0; i < explosionSprites.Length; i++)
            {
                if (expGo == null) yield break;
                expSr.sprite = explosionSprites[i];
                yield return new WaitForSeconds(0.055f);
            }
            if (expGo != null) Destroy(expGo);
        }

        // ============================================================
        // Weapon helpers (giống SwordKnight / SpearKnight)
        // ============================================================
        private void InitializeWeaponReference()
        {
            SyncWeaponSortingAndHandOffset();
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
                    weaponTransform.localPosition = new Vector3(0.78f, 0.05f, 0f);

                weaponRestLocalPos = weaponTransform.localPosition;
                weaponRestLocalRot = weaponTransform.localRotation;

                weaponSr = weaponTransform.GetComponent<SpriteRenderer>();
                if (weaponSr != null)
                {
                    if (sr != null)
                    {
                        weaponSr.sortingLayerID = sr.sortingLayerID;
                        weaponSr.sortingOrder   = sr.sortingOrder + 10;
                    }
                    else
                    {
                        weaponSr.sortingLayerName = "Entities";
                        weaponSr.sortingOrder     = 5500;
                    }
                }
            }
        }

        private Vector3 GetCurrentHandLocalPos()
        {
            float absX    = Mathf.Abs(weaponRestLocalPos.x);
            float signedX = (sr != null && sr.flipX) ? -absX : absX;
            return new Vector3(signedX, weaponRestLocalPos.y, weaponRestLocalPos.z);
        }

        private IEnumerator ReturnWeaponToHandRoutine(float duration)
        {
            if (weaponTransform == null) yield break;

            Vector3    startWorldPos = weaponTransform.position;
            Quaternion startWorldRot = weaponTransform.rotation;
            float elapsed = 0f;

            while (elapsed < duration && IsAlive)
            {
                elapsed += Time.deltaTime;
                float t  = Mathf.Clamp01(elapsed / duration);
                Vector3    targetWorldPos = transform.TransformPoint(GetCurrentHandLocalPos());
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

        private float GetUnobstructedDistance(Vector2 dir, float maxDist)
        {
            int wallMask = LayerMask.GetMask("Obstacle", "Default");
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, maxDist, wallMask);
            foreach (var h in hits)
            {
                if (h.collider != null && !h.collider.isTrigger
                    && h.collider.gameObject != gameObject
                    && h.collider.GetComponent<HunterPlayer>() == null)
                    return Mathf.Max(1.8f, h.distance - 0.7f);
            }
            return maxDist;
        }

        // ============================================================
        // Fallback Orb Sprite
        // ============================================================
        private static Sprite GetOrCreateFallbackOrbSprite()
        {
            if (fallbackOrbSprite != null) return fallbackOrbSprite;
            const int size   = 32;
            var tex          = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center     = (size - 1) * 0.5f;
            var pixels       = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                    pixels[y * size + x] = dist > 1f ? Color.clear
                        : new Color(1f, 1f, 1f, Mathf.Clamp01(1.2f - dist));
                }
            tex.SetPixels(pixels);
            tex.Apply();
            fallbackOrbSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return fallbackOrbSprite;
        }

        // ============================================================
        // DIE
        // ============================================================
        protected override void Die()
        {
            ResetWeaponToHandImmediate();
            base.Die();
            OnBossDefeated?.Invoke();
        }
    }
}
