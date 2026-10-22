using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HunterPlayer - Bộ điều khiển nhân vật Thợ săn trong Pha Dungeon Roguelite (Combat Phase).
/// Hiện thực hóa toàn bộ đặc tả trong Dungeon Eater.md (Mục 3.2.1):
/// 1. Điều khiển di chuyển 8 hướng (WASD) chuẩn hóa vector vật lý Rigidbody2D.
/// 2. Điều khiển Animator Blend Tree 4 hướng (MoveX, MoveY, IsMoving).
/// 3. Hệ thống Máu (HP = 100), Thể lực (Stamina = 100, hồi 10/s sau 0.5s nghỉ).
/// 4. Cơ chế Dash né tránh: Phím Space, tiêu hao 20 thể lực, có i-frame bất tử.
/// 5. Hệ thống 2 Vũ khí hoán đổi (Phím Q: Kiếm <-> Cung).
/// 6. Combo Kiếm (Light LMB/J 3-hit, Heavy RMB/K Cleave, Skill E Whirlwind).
/// 7. Bắn Cung (Light Quick Shot, Heavy Charged Shot, Skill E Piercing Arrow).
/// 8. Dùng bình máu (Phím R hồi 20 HP), Phím F Tương tác/Nhặt loot.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class HunterPlayer : MonoBehaviour
{
    /// <summary>
    /// Loại vũ khí của Hunter theo đặc tả Dungeon Eater.md (Đóng gói cục bộ trong HunterPlayer).
    /// </summary>
    public enum WeaponType
    {
        Sword, // Kiếm cận chiến, Combo 3 hit, Cleave, Whirlwind
        Bow    // Cung tầm xa, Quick Shot, Charged Shot, Piercing Arrow
    }

    /// <summary>
    /// Trạng thái hoạt động của Hunter trong Combat (Đóng gói cục bộ trong HunterPlayer).
    /// </summary>
    public enum HunterCombatState
    {
        Normal,
        Attacking,
        ChargingBow,
        Dashing,
        Hurt,
        Dead
    }

    [Header("--- Chỉ số Sinh tồn (Vitals) ---")]
    [Tooltip("Máu tối đa (cố định 100 theo đặc tả)")]
    [SerializeField] private float maxHealth = 100f;
    [Tooltip("Thể lực tối đa (cố định 100 theo đặc tả)")]
    [SerializeField] private float maxStamina = 100f;
    [Tooltip("Thể lực tiêu tốn mỗi lần Dash (20 theo đặc tả)")]
    [SerializeField] private float dashStaminaCost = 20f;
    [Tooltip("Tốc độ hồi thể lực (10 điểm/giây theo đặc tả)")]
    [SerializeField] private float staminaRegenRate = 10f;
    [Tooltip("Thời gian chờ trước khi bắt đầu hồi thể lực (0.5 giây theo đặc tả)")]
    [SerializeField] private float staminaRegenDelay = 0.5f;

    [Header("--- Di chuyển (Movement) ---")]
    [Tooltip("Tốc độ chạy cơ bản")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("--- Cơ chế Dash (Phòng thủ / Né đòn) ---")]
    [Tooltip("Tốc độ lướt khi Dash")]
    [SerializeField] private float dashSpeed = 13f;
    [Tooltip("Thời gian lướt của Dash")]
    [SerializeField] private float dashDuration = 0.2f;
    [Tooltip("Thời gian hồi giữa 2 lần Dash")]
    [SerializeField] private float dashCooldown = 0.4f;

    [Header("--- Hệ thống Vũ khí & Tấn công ---")]
    [SerializeField] private WeaponType currentWeapon = WeaponType.Sword;
    [Tooltip("LayerMask của kẻ địch để gây sát thương")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Kiếm (Sword Combat)")]
    [SerializeField] private float swordAttackRange = 1.1f;
    [SerializeField] private float swordAttackRadius = 0.6f;
    [SerializeField] private float swordLightDamage = 20f;
    [SerializeField] private float swordHeavyDamage = 40f;
    [SerializeField] private float swordSkillDamage = 35f;
    [SerializeField] private float comboResetTime = 0.8f;

    [Header("Cung (Bow Combat)")]
    [SerializeField] private GameObject normalArrowPrefab;
    [SerializeField] private GameObject magicArrowPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float arrowSpeed = 14f;
    [SerializeField] private float maxChargeTime = 1.2f;

    [Header("--- Bình máu (Potions) ---")]
    [Tooltip("Số lượng lọ máu mang theo")]
    [SerializeField] private int potionCount = 3;
    [Tooltip("Lượng máu hồi phục mỗi lọ (20 HP theo đặc tả)")]
    [SerializeField] private float potionHealAmount = 20f;

    [Header("--- Tương tác (Interact / Pickup) ---")]
    [SerializeField] private float interactionDistance = 0.7f;
    [SerializeField] private float interactionRadius = 0.4f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("--- Animator Parameter Names ---")]
    [SerializeField] private string moveXParam = "MoveX";
    [SerializeField] private string moveYParam = "MoveY";
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string dashTrigger = "Dash";
    [SerializeField] private string weaponTypeParam = "WeaponType"; // 0: Sword, 1: Bow

    // Runtime States
    private float currentHealth;
    private float currentStamina;
    private float staminaRegenTimer;
    private float dashCooldownTimer;
    private bool isInvulnerable; // i-frame bất tử
    private HunterCombatState combatState = HunterCombatState.Normal;

    // Movement & Facing
    private Vector2 moveInput;
    private Vector2 lastFacingDirection = Vector2.down;
    private Vector2 dashDirection;

    // Combo system
    private int comboIndex = 0;
    private float comboTimer = 0f;

    // Bow charging
    private float currentChargeTime = 0f;

    // Component references
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Hashes
    private int hashMoveX;
    private int hashMoveY;
    private int hashIsMoving;
    private int hashAttack;
    private int hashDash;
    private int hashWeaponType;

    private bool hasParamMoveX;
    private bool hasParamMoveY;
    private bool hasParamIsMoving;
    private bool hasParamAttack;
    private bool hasParamDash;
    private bool hasParamWeaponType;

    // Events phục vụ UI (HUD Máu, Thể lực, Vũ khí, Bình máu)
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action<WeaponType> OnWeaponSwapped;
    public event Action<int> OnPotionCountChanged;

    // Public properties
    public float CurrentHealth => currentHealth;
    public float CurrentStamina => currentStamina;
    public WeaponType CurrentWeapon => currentWeapon;
    public int PotionCount => potionCount;
    public bool IsInvulnerable => isInvulnerable;
    public Vector2 FacingDirection => lastFacingDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        InitializeAnimatorHashes();
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        OnWeaponSwapped?.Invoke(currentWeapon);
        OnPotionCountChanged?.Invoke(potionCount);

        UpdateAnimator(Vector2.zero, false);
    }

    private void Update()
    {
        if (combatState == HunterCombatState.Dead) return;

        HandleTimers();
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (combatState == HunterCombatState.Dead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (combatState == HunterCombatState.Dashing)
        {
            rb.velocity = dashDirection * dashSpeed;
        }
        else if (combatState == HunterCombatState.Normal)
        {
            rb.velocity = moveInput * moveSpeed;
        }
        else
        {
            // Trong lúc ra đòn hoặc tụ lực, giảm/khóa di chuyển
            rb.velocity = Vector2.zero;
        }
    }

    #region Input & State Handling

    private void HandleInput()
    {
        // 1. Nhận input di chuyển
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v);

        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        if (moveInput.sqrMagnitude > 0.01f && combatState == HunterCombatState.Normal)
        {
            lastFacingDirection = moveInput.normalized;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.01f && combatState == HunterCombatState.Normal;
        UpdateAnimator(moveInput, isMoving);

        // 2. Dash (Phím Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryDash();
        }

        // 3. Đổi vũ khí (Phím Q)
        if (Input.GetKeyDown(KeyCode.Q) && combatState == HunterCombatState.Normal)
        {
            SwapWeapon();
        }

        // 4. Dùng bình máu (Phím R)
        if (Input.GetKeyDown(KeyCode.R) && combatState == HunterCombatState.Normal)
        {
            UseHealthPotion();
        }

        // 5. Tương tác / Nhặt đồ (Phím F)
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryInteract();
        }

        // 6. Tấn công (Combat Inputs)
        HandleCombatActions();
    }

    private void HandleCombatActions()
    {
        if (combatState != HunterCombatState.Normal && combatState != HunterCombatState.ChargingBow) return;

        // Light Attack: Chuột trái (LMB) hoặc phím J
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J))
        {
            if (currentWeapon == WeaponType.Sword)
            {
                PerformSwordLightAttack();
            }
            else
            {
                PerformBowQuickShot();
            }
        }

        // Heavy Attack: Chuột phải (RMB) hoặc phím K
        if (currentWeapon == WeaponType.Sword)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.K))
            {
                PerformSwordCleave();
            }
        }
        else if (currentWeapon == WeaponType.Bow)
        {
            // Cung tụ lực (Hold RMB / K)
            if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.K))
            {
                combatState = HunterCombatState.ChargingBow;
                currentChargeTime += Time.deltaTime;
                currentChargeTime = Mathf.Min(currentChargeTime, maxChargeTime);
            }

            if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.K))
            {
                if (combatState == HunterCombatState.ChargingBow)
                {
                    PerformBowChargedShot(currentChargeTime);
                    currentChargeTime = 0f;
                    combatState = HunterCombatState.Normal;
                }
            }
        }

        // Weapon Skill (Phím E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            PerformWeaponSkill();
        }
    }

    #endregion

    #region Dash & Stamina System

    private void TryDash()
    {
        if (dashCooldownTimer > 0f) return;
        if (currentStamina < dashStaminaCost) return;
        if (combatState != HunterCombatState.Normal) return;

        // Tiêu hao thể lực
        currentStamina -= dashStaminaCost;
        staminaRegenTimer = staminaRegenDelay; // Reset thời gian chờ hồi thể lực
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);

        // Hướng Dash: theo hướng đang di chuyển, nếu đứng yên thì theo hướng đang nhìn
        dashDirection = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : lastFacingDirection;

        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        combatState = HunterCombatState.Dashing;
        isInvulnerable = true; // Kích hoạt i-frame bất tử né đòn
        dashCooldownTimer = dashCooldown;

        if (hasParamDash)
        {
            animator.SetTrigger(hashDash);
        }

        yield return new WaitForSeconds(dashDuration);

        isInvulnerable = false;
        combatState = HunterCombatState.Normal;
    }

    private void HandleTimers()
    {
        // Dash cooldown
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Combo reset timer
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                comboIndex = 0;
            }
        }

        // Hồi phục thể lực (10 điểm/giây sau 0.5s không tiêu hao)
        if (staminaRegenTimer > 0f)
        {
            staminaRegenTimer -= Time.deltaTime;
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
    }

    #endregion

    #region Combat Mechanics (Kiếm & Cung)

    private void SwapWeapon()
    {
        currentWeapon = (currentWeapon == WeaponType.Sword) ? WeaponType.Bow : WeaponType.Sword;
        comboIndex = 0;
        currentChargeTime = 0f;

        if (hasParamWeaponType)
        {
            animator.SetInteger(hashWeaponType, (int)currentWeapon);
        }

        OnWeaponSwapped?.Invoke(currentWeapon);
    }

    // --- KIẾM ---
    private void PerformSwordLightAttack()
    {
        combatState = HunterCombatState.Attacking;
        comboIndex = (comboIndex % 3) + 1; // 1, 2, 3
        comboTimer = comboResetTime;

        TriggerAttackAnimation();

        // Gây sát thương theo combo hit
        float damage = swordLightDamage + (comboIndex - 1) * 5f;
        DealMeleeDamage(swordAttackRange, swordAttackRadius, damage);

        StartCoroutine(ResetAttackState(0.25f));
    }

    private void PerformSwordCleave()
    {
        combatState = HunterCombatState.Attacking;
        TriggerAttackAnimation();

        // Cleave quét hình cung rộng hơn, sát thương lớn
        DealMeleeDamage(swordAttackRange * 1.3f, swordAttackRadius * 1.4f, swordHeavyDamage);

        StartCoroutine(ResetAttackState(0.45f));
    }

    private void PerformSwordSkillWhirlwind()
    {
        combatState = HunterCombatState.Attacking;
        TriggerAttackAnimation();

        // Whirlwind quét tròn 360 độ xung quanh người
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, swordAttackRange * 1.5f, enemyLayer);
        foreach (var hit in hits)
        {
            ApplyDamageToTarget(hit, swordSkillDamage);
        }

        StartCoroutine(ResetAttackState(0.5f));
    }

    // --- CUNG ---
    private void PerformBowQuickShot()
    {
        combatState = HunterCombatState.Attacking;
        TriggerAttackAnimation();

        SpawnArrow(lastFacingDirection, arrowSpeed, 15f, false);
        StartCoroutine(ResetAttackState(0.3f));
    }

    private void PerformBowChargedShot(float chargeTime)
    {
        combatState = HunterCombatState.Attacking;
        TriggerAttackAnimation();

        float chargeRatio = chargeTime / maxChargeTime;
        float damage = Mathf.Lerp(20f, 50f, chargeRatio);
        float speed = Mathf.Lerp(arrowSpeed, arrowSpeed * 1.5f, chargeRatio);

        SpawnArrow(lastFacingDirection, speed, damage, chargeRatio >= 0.8f);
        StartCoroutine(ResetAttackState(0.35f));
    }

    private void PerformBowSkillPiercingArrow()
    {
        combatState = HunterCombatState.Attacking;
        TriggerAttackAnimation();

        // Mũi tên phép xuyên thấu
        SpawnArrow(lastFacingDirection, arrowSpeed * 1.4f, 40f, true);
        StartCoroutine(ResetAttackState(0.4f));
    }

    private void PerformWeaponSkill()
    {
        if (currentWeapon == WeaponType.Sword)
        {
            PerformSwordSkillWhirlwind();
        }
        else
        {
            PerformBowSkillPiercingArrow();
        }
    }

    private void DealMeleeDamage(float distance, float radius, float damage)
    {
        Vector2 center = (Vector2)transform.position + (lastFacingDirection * distance);
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyLayer);
        foreach (var hit in hits)
        {
            ApplyDamageToTarget(hit, damage);
        }
    }

    private void ApplyDamageToTarget(Collider2D targetCollider, float damage)
    {
        // Gửi damage qua SendMessage hoặc Interface IDamageable
        targetCollider.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }

    private void SpawnArrow(Vector2 direction, float speed, float damage, bool isPiercing)
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + (Vector3)(direction * 0.5f);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        GameObject arrowObj = null;
        if (normalArrowPrefab != null)
        {
            arrowObj = Instantiate(normalArrowPrefab, spawnPos, rot);
            Rigidbody2D arrowRb = arrowObj.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.velocity = direction * speed;
            }
        }
    }

    private IEnumerator ResetAttackState(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (combatState == HunterCombatState.Attacking)
        {
            combatState = HunterCombatState.Normal;
        }
    }

    #endregion

    #region Health, Potions & Damage Taking

    /// <summary>
    /// Nhận sát thương từ quái vật hoặc bẫy trong dungeon.
    /// </summary>
    public void TakeDamage(float damage, Vector2 knockbackDirection = default)
    {
        if (isInvulnerable || combatState == HunterCombatState.Dead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageFeedbackRoutine(knockbackDirection));
        }
    }

    private IEnumerator DamageFeedbackRoutine(Vector2 knockbackDir)
    {
        isInvulnerable = true;

        // Flash màu đỏ báo hiệu trúng đòn (Hit feedback)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }

        // Knockback nhẹ
        if (knockbackDir != Vector2.zero)
        {
            rb.velocity = knockbackDir.normalized * 4f;
        }

        yield return new WaitForSeconds(0.12f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        // i-frame nhấp nháy bảo vệ ngắn sau khi nhận đòn
        for (int i = 0; i < 3; i++)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.08f);
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.08f);
        }

        isInvulnerable = false;
    }

    /// <summary>
    /// Sử dụng bình máu (Phím R): Hồi 20 HP theo đặc tả.
    /// </summary>
    public void UseHealthPotion()
    {
        if (potionCount <= 0) return;
        if (currentHealth >= maxHealth) return;

        potionCount--;
        currentHealth = Mathf.Min(maxHealth, currentHealth + potionHealAmount);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnPotionCountChanged?.Invoke(potionCount);
    }

    private void Die()
    {
        combatState = HunterCombatState.Dead;
        rb.velocity = Vector2.zero;

        // Thông báo Game Loop Manager (Mất 50% nguyên liệu, hồi sinh ở cổng dungeon theo đặc tả)
        SendMessage("OnPlayerDied", SendMessageOptions.DontRequireReceiver);
    }

    #endregion

    #region Interaction & Animation Helpers

    private void TryInteract()
    {
        Vector2 checkPos = (Vector2)transform.position + (lastFacingDirection * interactionDistance);
        Collider2D hit = Physics2D.OverlapCircle(checkPos, interactionRadius, interactableLayer);
        if (hit != null)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(null); // Có thể truyền Hunter nếu cần
                return;
            }
            hit.SendMessage("OnInteract", this, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void UpdateAnimator(Vector2 direction, bool isMoving)
    {
        if (animator == null) return;

        if (hasParamMoveX)
        {
            animator.SetFloat(hashMoveX, isMoving ? direction.x : lastFacingDirection.x);
        }

        if (hasParamMoveY)
        {
            animator.SetFloat(hashMoveY, isMoving ? direction.y : lastFacingDirection.y);
        }

        if (hasParamIsMoving)
        {
            animator.SetBool(hashIsMoving, isMoving);
        }
    }

    private void TriggerAttackAnimation()
    {
        if (animator != null && hasParamAttack)
        {
            animator.SetTrigger(hashAttack);
        }
    }

    private void InitializeAnimatorHashes()
    {
        if (animator == null) return;

        hashMoveX = Animator.StringToHash(moveXParam);
        hashMoveY = Animator.StringToHash(moveYParam);
        hashIsMoving = Animator.StringToHash(isMovingParam);
        hashAttack = Animator.StringToHash(attackTrigger);
        hashDash = Animator.StringToHash(dashTrigger);
        hashWeaponType = Animator.StringToHash(weaponTypeParam);

        foreach (var param in animator.parameters)
        {
            if (param.name == moveXParam) hasParamMoveX = true;
            else if (param.name == moveYParam) hasParamMoveY = true;
            else if (param.name == isMovingParam) hasParamIsMoving = true;
            else if (param.name == attackTrigger) hasParamAttack = true;
            else if (param.name == dashTrigger) hasParamDash = true;
            else if (param.name == weaponTypeParam) hasParamWeaponType = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vùng tương tác phím F
        Vector2 interactPos = (Vector2)transform.position + (lastFacingDirection * interactionDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactPos, interactionRadius);

        // Vùng chém cận chiến của Kiếm
        Vector2 attackPos = (Vector2)transform.position + (lastFacingDirection * swordAttackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, swordAttackRadius);
    }

    #endregion
}
