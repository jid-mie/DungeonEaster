using System;
using System.Collections;
using UnityEngine;
using NguyenMinhHiep.Combat;

namespace NguyenMinhHiep.Bosses
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho toàn bộ Boss theo quy chuẩn TaiLieu.md:
    /// - Quản lý Máu (HP), Sát thương (ATK), Tốc độ (Speed), Trạng thái Cuồng nộ (Enrage Phase).
    /// - Xử lý nhấp nháy trúng đòn (DamageFlash), tự động lật mặt theo hướng người chơi.
    /// - Tự động phát sự kiện tới BossHealthBarUI hiển thị thanh máu trên đỉnh màn hình.
    /// - Rơi nguyên liệu Boss quý hiếm (100% tỷ lệ) khi tử trận.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class BossBase : MonoBehaviour
    {
        [Header("--- Thông tin Boss ---")]
        [SerializeField] protected string bossName = "Boss";
        [SerializeField] protected string bossTitle = "Danh hiệu";
        [SerializeField] protected int dungeonFloor = 1;

        [Header("--- Chỉ số Cân bằng (TaiLieu.md) ---")]
        [SerializeField] protected float maxHealth = 500f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float moveSpeed = 3.0f;
        [SerializeField] protected float attackDamage = 25f;

        [Header("--- Khoảng cách & Hồi chiêu ---")]
        [SerializeField] protected float detectRange = 16f;
        [SerializeField] protected float attackRange = 3.5f;
        [SerializeField] protected float attackCooldown = 2.5f;

        [Header("--- Giai đoạn Cuồng nộ (Enrage Phase) ---")]
        [SerializeField] protected bool canEnrage = true;
        [SerializeField] protected float enrageHpPercent = 0.5f;
        [SerializeField] protected bool isEnraged = false;

        [Header("--- Nguyên liệu rơi khi chết ---")]
        [SerializeField] protected string dropItemId = "boss_material";
        [SerializeField] protected string dropItemName = "Nguyên liệu Boss";
        [SerializeField] protected int dropCount = 1;
        [SerializeField] protected Sprite dropIcon;

        // Trạng thái AI
        public enum BossState { Idle, Chase, Attack, Stunned, Dead }
        [SerializeField] protected BossState currentState = BossState.Idle;

        // Components
        protected Rigidbody2D rb;
        protected Collider2D col;
        protected SpriteRenderer sr;
        protected Transform targetPlayer;

        protected float lastAttackTime = -99f;
        protected bool isAttacking = false;
        protected bool isDead = false;

        // Public Properties
        public string BossName => bossName;
        public string BossTitle => bossTitle;
        public int DungeonFloor => dungeonFloor;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
        public bool IsAlive => !isDead && currentHealth > 0;
        public bool IsEnraged => isEnraged;

        // Events
        public static event Action<BossBase> OnBossSpawnedGeneric;
        public static event Action<BossBase> OnBossDefeatedGeneric;
        public static event Action<BossBase, float, float> OnBossHealthChangedGeneric;
        public event Action OnEnraged;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();

            currentHealth = maxHealth;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (sr != null)
            {
                sr.sortingOrder = 3000; // Entities
            }
        }

        protected virtual void Start()
        {
            FindPlayer();
            OnBossSpawnedGeneric?.Invoke(this);
            OnBossHealthChangedGeneric?.Invoke(this, currentHealth, maxHealth);

            // Bắt đầu hiệu ứng thở / nhún nhẹ để Boss sống động
            StartCoroutine(BreathingMotionRoutine());
        }

        protected virtual void Update()
        {
            if (isDead) return;

            if (targetPlayer == null || !targetPlayer.gameObject.activeInHierarchy)
            {
                FindPlayer();
                if (targetPlayer == null) return;
            }

            // Tự động lật mặt theo hướng người chơi
            if (sr != null && !isAttacking)
            {
                float dx = targetPlayer.position.x - transform.position.x;
                if (Mathf.Abs(dx) > 0.1f)
                {
                    sr.flipX = dx < 0f;
                }
            }

            float dist = Vector2.Distance(transform.position, targetPlayer.position);
            UpdateAI(dist);
        }

        protected abstract void UpdateAI(float distanceToPlayer);

        protected void FindPlayer()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                targetPlayer = p.transform;
                return;
            }

            // Tìm HunterPlayer của Hai
            var hunter = FindObjectOfType<NguyenVanHai.HunterPlayer>();
            if (hunter != null)
            {
                targetPlayer = hunter.transform;
                return;
            }

            // Tìm PlayerController
            var pc = FindObjectOfType<DungeonEater.Dungeon.PlayerController>();
            if (pc != null)
            {
                targetPlayer = pc.transform;
            }
        }

        public virtual void TakeDamage(float dmg, Vector2 knockback = default)
        {
            if (isDead) return;

            currentHealth -= dmg;
            currentHealth = Mathf.Max(0f, currentHealth);

            StartCoroutine(DamageFlashRoutine());

            OnBossHealthChangedGeneric?.Invoke(this, currentHealth, maxHealth);

            // Kiểm tra Cuồng Nộ
            if (canEnrage && !isEnraged && HealthPercent <= enrageHpPercent)
            {
                TriggerEnrage();
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void TriggerEnrage()
        {
            isEnraged = true;
            moveSpeed *= 1.25f;
            attackCooldown *= 0.75f;
            attackDamage *= 1.2f;

            if (sr != null)
            {
                sr.color = new Color(1f, 0.6f, 0.6f, 1f);
            }

            OnEnraged?.Invoke();
            Debug.Log($"[Boss] 💥 {bossName} đã HÓA CUỒNG (ENRAGED)!");
        }

        protected IEnumerator DamageFlashRoutine()
        {
            if (sr == null) yield break;
            Color orig = isEnraged ? new Color(1f, 0.6f, 0.6f, 1f) : Color.white;
            sr.color = new Color(1f, 0.2f, 0.2f, 1f);
            yield return new WaitForSeconds(0.08f);
            if (sr != null) sr.color = orig;
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;
            currentState = BossState.Dead;

            rb.velocity = Vector2.zero;
            if (col != null) col.enabled = false;

            SpawnBossLoot();

            OnBossDefeatedGeneric?.Invoke(this);

            StartCoroutine(DeathDisappearRoutine());
        }

        protected virtual void SpawnBossLoot()
        {
            BossLootItem.Spawn(transform.position, dropItemId, dropItemName, dropCount, dropIcon);
            Debug.Log($"[Boss] 🎁 {bossName} đã bị hạ gục! Rơi ra 1x {dropItemName}.");
        }

        protected IEnumerator DeathDisappearRoutine()
        {
            float elapsed = 0f;
            float duration = 1.2f;
            Color orig = sr != null ? sr.color : Color.white;
            Vector3 origScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (sr != null)
                {
                    sr.color = new Color(orig.r, orig.g, orig.b, 1f - t);
                }
                transform.localScale = Vector3.Lerp(origScale, origScale * 1.2f, t);
                yield return null;
            }

            Destroy(gameObject);
        }

        protected IEnumerator BreathingMotionRoutine()
        {
            Vector3 initialScale = transform.localScale;
            while (!isDead)
            {
                if (!isAttacking)
                {
                    float wave = Mathf.Sin(Time.time * 3f) * 0.04f;
                    transform.localScale = new Vector3(initialScale.x * (1f + wave), initialScale.y * (1f - wave), initialScale.z);
                }
                yield return null;
            }
        }

        protected void DealDamageToPlayer(float dmg)
        {
            if (targetPlayer == null) return;
            targetPlayer.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);
        }
    }
}

