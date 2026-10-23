using System;
using System.Collections;
using UnityEngine;
using NguyenVanHai.Combat;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho toàn bộ quái vật trong Dungeon.
    /// Quản lý Máu (HP), di chuyển vật lý Rigidbody2D, Animator (MoveX, MoveY, IsMoving, IsAttack),
    /// nhấp nháy trúng đòn (DamageFlash), đẩy lùi (Knockback), và rơi nguyên liệu khi chết.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("--- Chỉ số Sinh tồn (Vitals) ---")]
        [SerializeField] protected float maxHealth = 40f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float moveSpeed = 2.5f;
        [SerializeField] protected float attackDamage = 8f;

        [Header("--- Phạm vi Phát hiện & Tấn công ---")]
        [SerializeField] protected float detectRange = 8f;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float attackCooldown = 1.2f;

        [Header("--- Rơi đồ khi chết (Loot Drop) ---")]
        [SerializeField] protected string dropItemId = "slime_gel";
        [SerializeField] protected string dropItemName = "Chất nhầy dẻo";
        [SerializeField] protected int dropCount = 1;
        [SerializeField] protected Sprite dropIcon;

        // Trạng thái AI
        public enum AIState { Idle, Chase, Attack, Hurt, Dead }
        [SerializeField] protected AIState currentState = AIState.Idle;

        // Components
        protected Rigidbody2D rb;
        protected Collider2D col;
        protected SpriteRenderer sr;
        protected Animator anim;
        protected DamageFlash flash;
        protected HunterPlayer targetPlayer;

        protected float lastAttackTime = -99f;
        protected bool isAttacking = false;
        protected bool isDead = false;

        public bool IsAlive => !isDead && currentHealth > 0;
        public float HealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
        public event Action<EnemyBase> OnDeath;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();
            anim = GetComponent<Animator>();

            // Tự động thêm DamageFlash nếu chưa có
            flash = GetComponent<DamageFlash>();
            if (flash == null) flash = gameObject.AddComponent<DamageFlash>();

            currentHealth = maxHealth;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        protected virtual void Start()
        {
            FindPlayer();
        }

        protected virtual void Update()
        {
            if (isDead) return;

            if (targetPlayer == null || !targetPlayer.isActiveAndEnabled)
            {
                FindPlayer();
                if (targetPlayer == null) return;
            }

            float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);

            UpdateAI(dist);
            UpdateAnimator();
        }

        protected void FindPlayer()
        {
            targetPlayer = FindObjectOfType<HunterPlayer>();
        }

        protected abstract void UpdateAI(float distanceToPlayer);

        protected virtual void UpdateAnimator()
        {
            if (anim == null || !anim.isActiveAndEnabled) return;

            Vector2 vel = rb.velocity;
            bool moving = vel.sqrMagnitude > 0.05f;

            // Đồng bộ bộ tham số chuẩn: MoveX, MoveY, IsMoving
            TrySetAnimFloat("MoveX", vel.normalized.x);
            TrySetAnimFloat("MoveY", vel.normalized.y);
            TrySetAnimBool("IsMoving", moving);
        }

        public virtual void TakeDamage(float dmg, Vector2 knockback = default)
        {
            if (isDead) return;

            currentHealth -= dmg;
            if (flash != null) flash.Flash();

            // Hiệu ứng giật lùi (Knockback)
            if (knockback != Vector2.zero && rb != null)
            {
                rb.AddForce(knockback, ForceMode2D.Impulse);
            }

            // Gọi clip trúng đòn nếu có
            if (anim != null)
            {
                anim.Play("attacked", 0, 0f);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;
            currentState = AIState.Dead;

            rb.velocity = Vector2.zero;
            if (col != null) col.enabled = false;

            // Rơi nguyên liệu
            SpawnLoot();

            OnDeath?.Invoke(this);

            // Mờ dần rồi hủy GameObject
            StartCoroutine(FadeAndDestroyRoutine());
        }

        protected virtual void SpawnLoot()
        {
            var dropGo = new GameObject($"Loot_{dropItemId}");
            dropGo.transform.position = transform.position;
            var loot = dropGo.AddComponent<LootDrop>();
            loot.Setup(dropItemId, dropItemName, dropCount, dropIcon);

            var dropSr = dropGo.GetComponent<SpriteRenderer>();
            if (dropIcon != null) dropSr.sprite = dropIcon;
            dropSr.sortingOrder = 4000;
        }

        protected IEnumerator FadeAndDestroyRoutine()
        {
            float elapsed = 0f;
            float duration = 0.5f;
            Color startColor = sr != null ? sr.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(1f, 0f, elapsed / duration);
                if (sr != null) sr.color = new Color(startColor.r, startColor.g, startColor.b, a);
                yield return null;
            }

            Destroy(gameObject);
        }

        protected void TrySetAnimBool(string paramName, bool val)
        {
            if (anim == null) return;
            foreach (var p in anim.parameters)
            {
                if (p.name == paramName && p.type == AnimatorControllerParameterType.Bool)
                {
                    anim.SetBool(paramName, val);
                    return;
                }
            }
        }

        protected void TrySetAnimFloat(string paramName, float val)
        {
            if (anim == null) return;
            foreach (var p in anim.parameters)
            {
                if (p.name == paramName && p.type == AnimatorControllerParameterType.Float)
                {
                    anim.SetFloat(paramName, val);
                    return;
                }
            }
        }
    }
}
