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
        public static PlayerController Instance { get; private set; }

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float dashSpeed = 14f;
        [SerializeField] private float dashDuration = 0.22f;
        [SerializeField] private float dashCooldown = 0.7f;
        [SerializeField] private float staminaCostPerDash = 20f;

        [Header("Health & Stamina")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina = 100f;
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
        private float attackActionTimer = 0f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
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
            Instance = this;
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (weaponSystem == null) weaponSystem = GetComponentInChildren<WeaponSystem>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            currentHealth = maxHealth;
            currentStamina = maxStamina;
        }

        private void Start()
        {
            if (currentHealth <= 0f) currentHealth = maxHealth;
            if (currentStamina <= 0f) currentStamina = maxStamina;
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
            // Điều khiển 8 hướng WASD
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            movementInput = new Vector2(moveX, moveY).normalized;

            if (movementInput.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = movementInput;

                // Lật hướng nhìn theo trục ngang của hướng di chuyển
                if (attackActionTimer <= 0f && spriteRenderer != null && Mathf.Abs(moveX) > 0.05f)
                {
                    spriteRenderer.flipX = (moveX < 0f);
                }
            }

            if (animator != null)
            {
                animator.SetBool("isMoving", movementInput.sqrMagnitude > 0.01f);
                animator.SetBool("isDashing", isDashing);
            }

            // Dash né đòn (Space)
            if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0f && currentStamina >= staminaCostPerDash && !isDashing)
            {
                StartDash();
            }

            // Đổi vũ khí Kiếm <-> Cung (Q)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SwitchWeapon();
            }

            // Xử lý các đòn tấn công (Đồng bộ 100% với Hướng Di Chuyển & Nhìn của nhân vật)
            if (!isDashing && weaponSystem != null)
            {
                Vector2 attackDirection = GetCurrentAttackDirection();

                // Đòn đánh thường: Chuột trái (LMB) hoặc phím J
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J))
                {
                    ApplyFacing(attackDirection);
                    if (weaponSystem.PerformLightAttack(currentWeapon, attackDirection))
                    {
                        TriggerAttackAnimation(false);
                    }
                }
                // Đòn đánh mạnh / Tụ lực: Chuột phải (RMB) hoặc phím K
                else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.K))
                {
                    ApplyFacing(attackDirection);
                    if (weaponSystem.PerformHeavyAttack(currentWeapon, attackDirection))
                    {
                        TriggerAttackAnimation(false);
                    }
                }
                // Tuyệt kỹ vũ khí (E)
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    ApplyFacing(attackDirection);
                    if (weaponSystem.PerformSkillAttack(currentWeapon, attackDirection))
                    {
                        TriggerAttackAnimation(true);
                    }
                }
            }

            // Nhặt vật phẩm / Tương tác (F)
            if (Input.GetKeyDown(KeyCode.F))
            {
                TryInteractOrPickup();
            }
        }

        /// <summary>
        /// Xác định hướng ra đòn khớp tuyệt đối với hướng di chuyển của nhân vật (WASD / lastMoveDirection)
        /// </summary>
        private Vector2 GetCurrentAttackDirection()
        {
            Vector2 rawDir = (movementInput.sqrMagnitude > 0.01f) ? movementInput : lastMoveDirection;
            float facing = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;

            // Nếu người chơi di chuyển hoặc dừng lại ở hướng thuần UP hoặc thuần DOWN,
            // đòn đánh được hơi chếch góc 20 độ theo hướng nhìn ngang (facing * 0.35f)
            // để mũi tên và vệt kiếm xuất phát chuẩn xác từ bàn tay vung vũ khí thay vì xuyên qua đỉnh đầu/chân.
            if (Mathf.Abs(rawDir.x) < 0.15f)
            {
                return new Vector2(facing * 0.35f, Mathf.Sign(rawDir.y) * 0.94f).normalized;
            }

            return rawDir.normalized;
        }

        private void ApplyFacing(Vector2 direction)
        {
            if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.05f)
            {
                spriteRenderer.flipX = (direction.x < 0f);
            }
        }

        private void TriggerAttackAnimation(bool isSkill)
        {
            attackActionTimer = 0.22f; // Giữ tư thế ra đòn, giảm trôi nhân vật

            if (animator == null) return;

            if (currentWeapon == WeaponType.Sword)
            {
                animator.ResetTrigger("swordAttack");
                animator.SetTrigger("swordAttack");

                if (isSkill)
                {
                    StartCoroutine(WhirlwindSpinVisual(0.25f));
                }
            }
            else
            {
                animator.ResetTrigger("bowShoot");
                animator.SetTrigger("bowShoot");
            }
        }

        private System.Collections.IEnumerator WhirlwindSpinVisual(float duration)
        {
            float elapsed = 0f;
            float step = 0.06f;
            while (elapsed < duration)
            {
                if (spriteRenderer != null) spriteRenderer.flipX = !spriteRenderer.flipX;
                yield return new WaitForSeconds(step);
                elapsed += step;
            }
            if (spriteRenderer != null) spriteRenderer.flipX = (lastMoveDirection.x < 0f);
        }

        private void MovePlayer()
        {
            if (isDashing)
            {
                rb.velocity = lastMoveDirection * dashSpeed;
            }
            else if (attackActionTimer > 0f)
            {
                // Khi vung kiếm: hãm tốc độ (30% moveSpeed) để nhân vật trụ tấn công vững vàng,
                // không bị trôi tuột vượt qua hiệu ứng vệt kiếm (Slash VFX)
                rb.velocity = movementInput * (moveSpeed * 0.3f);
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
            if (attackActionTimer > 0f)
            {
                attackActionTimer -= Time.deltaTime;
            }

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
