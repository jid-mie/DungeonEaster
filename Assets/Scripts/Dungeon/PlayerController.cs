using UnityEngine;
using DungeonEater.Core;

namespace DungeonEater.Dungeon
{
    public enum WeaponType
    {
        Sword,  // Kiếm cận chiến: Combo 3 hit, Cleave, Whirlwind
        Bow     // Cung tầm xa: Quick shot, Charged shot, Piercing Arrow
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float dashSpeed = 14f;
        [SerializeField] private float dashDuration = 0.22f;
        [SerializeField] private float dashCooldown = 0.7f;
        [SerializeField] private float staminaCostPerDash = 20f;

        [Header("Health & Stamina")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina;
        [SerializeField] private float staminaRegenRate = 18f;

        [Header("Weapon & Combat")]
        [SerializeField] private WeaponType currentWeapon = WeaponType.Sword;
        [SerializeField] private WeaponSystem weaponSystem;
        [SerializeField] private Animator animator;

        [Header("State Flags")]
        private bool isDashing = false;
        private bool isInvulnerable = false;
        private float dashTimer = 0f;
        private float dashCooldownTimer = 0f;
        private float invulnerableTimer = 0f;

        private Rigidbody2D rb;
        private Vector2 movementInput;
        private Vector2 lastMoveDirection = Vector2.right;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public WeaponType CurrentWeapon => currentWeapon;
        public bool IsInvulnerable => isInvulnerable;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (weaponSystem == null) weaponSystem = GetComponentInChildren<WeaponSystem>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            currentHealth = maxHealth;
            currentStamina = maxStamina;
        }

        private void Update()
        {
            HandleInput();
            UpdateTimers();
            RegenStamina();
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void HandleInput()
        {
            // 8-directional movement input (WASD / Arrows)
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            movementInput = new Vector2(moveX, moveY).normalized;

            if (movementInput.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = movementInput;
                // Flip sprite facing direction
                transform.localScale = new Vector3(moveX > 0 ? 1 : (moveX < 0 ? -1 : transform.localScale.x), 1, 1);
            }

            if (animator != null)
            {
                animator.SetBool("isMoving", movementInput.sqrMagnitude > 0.01f);
                animator.SetBool("isDashing", isDashing);
            }

            // Dash (Space)
            if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0f && currentStamina >= staminaCostPerDash && !isDashing)
            {
                StartDash();
            }

            // Switch Weapon (Q)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SwitchWeapon();
            }

            // Combat inputs
            if (!isDashing && weaponSystem != null)
            {
                // Calculate precise aim direction (Mouse cursor in world space, or last move direction if using keyboard)
                Vector2 aimDirection = lastMoveDirection;
                if (Camera.main != null)
                {
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 mouseDir = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
                    if (mouseDir.sqrMagnitude > 0.01f)
                    {
                        aimDirection = mouseDir;
                    }
                }

                // Light Attack: LMB or J
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J))
                {
                    // Flip toward attack direction immediately
                    if (Mathf.Abs(aimDirection.x) > 0.05f)
                    {
                        transform.localScale = new Vector3(aimDirection.x > 0 ? 1 : -1, 1, 1);
                    }

                    if (animator != null) animator.SetTrigger(currentWeapon == WeaponType.Sword ? "swordAttack" : "bowShoot");
                    weaponSystem.PerformLightAttack(currentWeapon, aimDirection);
                }
                // Heavy Attack: RMB or K
                else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.K))
                {
                    if (Mathf.Abs(aimDirection.x) > 0.05f)
                    {
                        transform.localScale = new Vector3(aimDirection.x > 0 ? 1 : -1, 1, 1);
                    }

                    if (animator != null) animator.SetTrigger(currentWeapon == WeaponType.Sword ? "swordAttack" : "bowShoot");
                    weaponSystem.PerformHeavyAttack(currentWeapon, aimDirection);
                }
                // Weapon Skill: E
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    if (Mathf.Abs(aimDirection.x) > 0.05f)
                    {
                        transform.localScale = new Vector3(aimDirection.x > 0 ? 1 : -1, 1, 1);
                    }

                    if (animator != null) animator.SetTrigger(currentWeapon == WeaponType.Sword ? "swordAttack" : "bowShoot");
                    weaponSystem.PerformSkillAttack(currentWeapon, aimDirection);
                }
            }

            // Pickup / Interact (F)
            if (Input.GetKeyDown(KeyCode.F))
            {
                TryInteractOrPickup();
            }
        }

        private void MovePlayer()
        {
            if (isDashing)
            {
                rb.velocity = lastMoveDirection * dashSpeed;
            }
            else
            {
                rb.velocity = movementInput * moveSpeed;
            }
        }

        private void StartDash()
        {
            isDashing = true;
            isInvulnerable = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            invulnerableTimer = dashDuration + 0.08f; // i-frame lasts slightly longer than dash
            currentStamina -= staminaCostPerDash;

            SoundManager.Instance?.PlayDash();
            Debug.Log("[Player] Dash kích hoạt i-frame né sát thương!");
        }

        private void SwitchWeapon()
        {
            currentWeapon = (currentWeapon == WeaponType.Sword) ? WeaponType.Bow : WeaponType.Sword;
            SoundManager.Instance?.PlayPickup();
            Debug.Log($"[Player] Chuyển vũ khí sang: {currentWeapon}");
        }

        private void UpdateTimers()
        {
            if (dashTimer > 0f)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f) isDashing = false;
            }

            if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;

            if (invulnerableTimer > 0f)
            {
                invulnerableTimer -= Time.deltaTime;
                if (invulnerableTimer <= 0f) isInvulnerable = false;
            }
        }

        private void RegenStamina()
        {
            if (!isDashing && currentStamina < maxStamina)
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
            }
        }

        public void TakeDamage(float damage)
        {
            if (isInvulnerable) return; // Né hoàn toàn nhờ i-frame

            currentHealth -= damage;
            if (animator != null) animator.SetTrigger("hurt");
            SoundManager.Instance?.PlayHit();
            CameraFollow.Instance?.TriggerScreenShake(0.18f, 0.25f);

            // Brief invulnerability post-damage
            isInvulnerable = true;
            invulnerableTimer = 0.4f;

            Debug.Log($"[Player] Nhận {damage} sát thương. Máu còn: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Die();
            }
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        private void Die()
        {
            Debug.LogWarning("[Player] Bạn đã ngã xuống trong hầm ngục!");
            GameManager.Instance?.HandlePlayerDefeated();
        }

        private void TryInteractOrPickup()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.8f);
            foreach (var col in colliders)
            {
                var loot = col.GetComponent<LootItem>();
                if (loot != null)
                {
                    loot.Collect();
                    return;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1.8f);
        }
    }
}

