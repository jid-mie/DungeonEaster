using System;
using System.Collections;
using UnityEngine;
using NguyenVanHai.Combat;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho toàn bộ Quái vật và Boss trong Dungeon.
    /// - Quản lý Máu (HP), di chuyển vật lý Rigidbody2D, Animator.
    /// - Hỗ trợ Cảnh báo Đỏ Rực trên thân Quái/Boss trước khi ra đòn (PlayRedWarningRoutine).
    /// - Hỗ trợ Quãng nghỉ bắt buộc sau đòn đánh (EnterPostAttackRestRoutine) để không spam đòn liên tục.
    /// - Chống lỗi Boss bị đẩy trôi bởi va chạm vật lý hoặc Knockback (isBoss + SetPhysicsLocked).
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
        [SerializeField] protected float attackCooldown = 2.2f;

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
        protected float postAttackRestUntil = -99f;
        protected bool isAttacking = false;
        protected bool isDead = false;
        protected bool isBoss = false;
        protected Color baseTintColor = Color.white;

        public bool IsAlive => !isDead && currentHealth > 0;
        public float HealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
        public event Action<EnemyBase> OnDeath;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();
            anim = GetComponent<Animator>();

            flash = GetComponent<DamageFlash>();
            if (flash == null) flash = gameObject.AddComponent<DamageFlash>();

            currentHealth = maxHealth;
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                // Nếu là Boss: Tăng khối lượng cực đại để Player va vào không bao giờ đẩy trôi được Boss
                if (isBoss)
                {
                    rb.mass = 10000f;
                    rb.drag = 25f;
                }
                else
                {
                    rb.mass = 20f;
                    rb.drag = 10f;
                }
            }
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

            // Nếu đang trong thời gian đứng nghỉ sau đòn đánh -> Đứng yên tuyệt đối, không đuổi, không ra đòn mới
            if (!isAttacking && Time.time < postAttackRestUntil)
            {
                currentState = AIState.Idle;
                if (rb != null) rb.velocity = Vector2.zero;
                UpdateAnimator();
                return;
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

        /// <summary>
        /// Khóa/Mở vị trí vật lý Rigidbody2D:
        /// Khi Boss đứng yên gồng chiêu, bắn xa hoặc đứng nghỉ, khóa FreezeAll để không bị bất kỳ lực nào đẩy trôi.
        /// </summary>
        protected void SetPhysicsLocked(bool locked)
        {
            if (rb == null) return;
            if (locked)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        /// <summary>
        /// Hiệu ứng đổi màu thân Quái / Boss sang ĐỎ RỰC CẢNH BÁO (nhấp nháy đỏ tươi + phồng nhẹ)
        /// trong suốt thời gian chuẩn bị ra đòn (kể cả đòn bắn xa, cận chiến, lao, nhảy dậm).
        /// </summary>
        protected IEnumerator PlayRedWarningRoutine(float duration, float scaleMultiplier = 1.10f)
        {
            SetPhysicsLocked(true);

            if (flash != null)
            {
                flash.StopFlashImmediate();
                flash.IsWarningActive = true;
            }

            Vector3 origScale = transform.localScale;
            Color redBright = new Color(1f, 0.15f, 0.15f, 1f);
            Color redDeep = new Color(1f, 0.42f, 0.12f, 1f);

            float elapsed = 0f;
            float safeDur = Mathf.Max(0.1f, duration);

            while (elapsed < safeDur && IsAlive)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / safeDur);

                // Nhấp nháy đỏ rực nhanh dần khi sắp ra đòn
                float pulse = (Mathf.Sin(elapsed * Mathf.Lerp(14f, 28f, t)) + 1f) * 0.5f;
                if (sr != null)
                {
                    sr.color = Color.Lerp(redBright, redDeep, pulse);
                }

                float curScale = Mathf.Lerp(1f, scaleMultiplier, Mathf.Sin(t * Mathf.PI));
                transform.localScale = new Vector3(origScale.x * curScale, origScale.y * curScale, origScale.z);

                yield return null;
            }

            transform.localScale = origScale;
            if (sr != null)
            {
                sr.color = baseTintColor;
            }
            if (flash != null)
            {
                flash.BaseColor = baseTintColor;
                flash.IsWarningActive = false;
            }

            SetPhysicsLocked(false);
        }

        /// <summary>
        /// Quãng nghỉ bắt buộc sau mỗi đòn đánh:
        /// Quái/Boss đứng yên tại chỗ thở dốc trong restDuration giây, khóa mọi đòn kế tiếp để Player có cơ hội phản công.
        /// </summary>
        protected IEnumerator EnterPostAttackRestRoutine(float restDuration)
        {
            SetPhysicsLocked(true);
            currentState = AIState.Idle;
            postAttackRestUntil = Time.time + restDuration;
            lastAttackTime = Time.time;

            yield return new WaitForSeconds(restDuration);

            SetPhysicsLocked(false);
            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = AIState.Idle;
        }

        protected virtual void UpdateAnimator()
        {
            if (anim == null || !anim.isActiveAndEnabled) return;

            Vector2 vel = rb != null ? rb.velocity : Vector2.zero;
            bool moving = vel.sqrMagnitude > 0.05f && !isAttacking && Time.time >= postAttackRestUntil;

            TrySetAnimFloat("MoveX", vel.normalized.x);
            TrySetAnimFloat("MoveY", vel.normalized.y);
            TrySetAnimBool("IsMoving", moving);
        }

        public virtual void TakeDamage(float dmg, Vector2 knockback = default)
        {
            if (isDead) return;

            currentHealth -= dmg;
            if (flash != null) flash.Flash();

            // Boss miễn nhiễm 100% với đẩy lùi (Knockback). Quái thường chỉ bị đẩy nhẹ khi không đang gồng chiêu.
            if (!isBoss && !isAttacking && knockback != Vector2.zero && rb != null)
            {
                rb.AddForce(knockback * 0.4f, ForceMode2D.Impulse);
            }

            if (anim != null)
            {
                TrySetAnimBool("IsAttacked", true);
                TrySetAnimBool("isAttacked", true);

                if (anim.HasState(0, Animator.StringToHash("attacked")))
                    anim.Play("attacked", 0, 0f);
                else if (anim.HasState(0, Animator.StringToHash("slimeAttacked")))
                    anim.Play("slimeAttacked", 0, 0f);

                StartCoroutine(ResetAttackedAnim(0.2f));
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator ResetAttackedAnim(float delay)
        {
            yield return new WaitForSeconds(delay);
            TrySetAnimBool("IsAttacked", false);
            TrySetAnimBool("isAttacked", false);
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;
            currentState = AIState.Dead;

            SetPhysicsLocked(false);
            if (rb != null) rb.velocity = Vector2.zero;
            if (col != null) col.enabled = false;
            if (flash != null) flash.IsWarningActive = false;

            SpawnLoot();

            OnDeath?.Invoke(this);

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
