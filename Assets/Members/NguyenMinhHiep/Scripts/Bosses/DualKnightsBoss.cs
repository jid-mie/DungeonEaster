using System.Collections;
using UnityEngine;
using NguyenMinhHiep.Combat;
using NguyenMinhHiep.UI;

namespace NguyenMinhHiep.Bosses
{
    /// <summary>
    /// Boss Tầng 4: ĐÔI HIỆP SĨ (Dual Boss - Song kiếm cấm vệ) theo TaiLieu.md mục 7.4:
    /// - Gồm 2 con hiệp sĩ xuất hiện cùng lúc, mỗi con 600 HP (Tổng 1200 HP), tốc độ 3.8 u/s.
    /// - Hiệp sĩ Kiếm (Sword Knight): Chém 3 nhát liên hoàn áp sát (35 DMG/hit).
    /// - Hiệp sĩ Giáo (Spear Knight): Đâm thẳng tầm xa xuyên thấu (45 DMG).
    /// - Hiệu ứng Tương tác (Dual Synergy): Khi 1 trong 2 con chết, con còn lại sẽ HÓA CUỒNG (Hồi 200 HP, tăng 35% tốc độ & 30% sát thương).
    /// - Rơi nguyên liệu: 100% rơi 1x Trứng thiết giáp quái khi đánh bại cả 2 hiệp sĩ.
    /// </summary>
    public class DualKnightsBoss : BossBase
    {
        [Header("--- Tham chiếu 2 Hiệp Sĩ ---")]
        [SerializeField] private SwordKnightMember swordKnight;
        [SerializeField] private SpearKnightMember spearKnight;

        protected override void Awake()
        {
            bossName = "Đôi Hiệp Sĩ Cấm Vệ";
            bossTitle = "Song kiếm cấm vệ";
            dungeonFloor = 4;

            maxHealth = 1200f;
            currentHealth = 1200f;
            dropItemId = "knight_egg";
            dropItemName = "Trứng thiết giáp quái";
            dropCount = 1;

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            // Khởi tạo 2 hiệp sĩ nếu chưa có
            if (swordKnight == null) swordKnight = GetComponentInChildren<SwordKnightMember>();
            if (spearKnight == null) spearKnight = GetComponentInChildren<SpearKnightMember>();

            if (swordKnight != null) swordKnight.Initialize(this);
            if (spearKnight != null) spearKnight.Initialize(this);
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            // Controller tổng không di chuyển trực tiếp, 2 hiệp sĩ con sẽ tự di chuyển
            float totalHp = 0f;
            if (swordKnight != null && swordKnight.IsAlive) totalHp += swordKnight.CurrentHP;
            if (spearKnight != null && spearKnight.IsAlive) totalHp += spearKnight.CurrentHP;

            currentHealth = totalHp;

            if (BossHealthBarUI.Instance != null && swordKnight != null && spearKnight != null)
            {
                BossHealthBarUI.Instance.UpdateDualKnights(
                    swordKnight.CurrentHP, swordKnight.MaxHP,
                    spearKnight.CurrentHP, spearKnight.MaxHP
                );
            }

            // Kiểm tra chiến thắng khi cả 2 đã chết
            bool k1Dead = swordKnight == null || !swordKnight.IsAlive;
            bool k2Dead = spearKnight == null || !spearKnight.IsAlive;

            if (k1Dead && k2Dead && !isDead)
            {
                Die();
            }
        }

        public void NotifyMemberDeath(KnightMemberBase deadKnight)
        {
            // Con còn lại Hóa Cuồng
            if (deadKnight == swordKnight && spearKnight != null && spearKnight.IsAlive)
            {
                spearKnight.Enrage();
            }
            else if (deadKnight == spearKnight && swordKnight != null && swordKnight.IsAlive)
            {
                swordKnight.Enrage();
            }
        }
    }

    /// <summary>
    /// Lớp cơ sở cho từng thành viên kỵ sĩ
    /// </summary>
    public abstract class KnightMemberBase : MonoBehaviour
    {
        [SerializeField] protected float maxHP = 600f;
        [SerializeField] protected float currentHP = 600f;
        [SerializeField] protected float moveSpeed = 3.8f;
        [SerializeField] protected float attackCooldown = 2.0f;
        [SerializeField] protected bool isEnraged = false;

        protected DualKnightsBoss parentController;
        protected Rigidbody2D rb;
        protected SpriteRenderer sr;
        protected Transform targetPlayer;
        protected float lastAttackTime = -99f;
        protected bool isAttacking = false;
        protected bool isDead = false;

        public float CurrentHP => currentHP;
        public float MaxHP => maxHP;
        public bool IsAlive => !isDead && currentHP > 0f;

        public virtual void Initialize(DualKnightsBoss controller)
        {
            parentController = controller;
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            currentHP = maxHP;

            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        protected virtual void Update()
        {
            if (isDead) return;

            if (targetPlayer == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) targetPlayer = p.transform;
            }

            if (targetPlayer == null) return;

            if (sr != null && !isAttacking)
            {
                sr.flipX = (targetPlayer.position.x - transform.position.x) < 0f;
            }

            float dist = Vector2.Distance(transform.position, targetPlayer.position);
            UpdateKnightAI(dist);
        }

        protected abstract void UpdateKnightAI(float dist);

        public virtual void TakeDamage(float dmg, Vector2 kb = default)
        {
            if (isDead) return;
            currentHP -= dmg;
            currentHP = Mathf.Max(0f, currentHP);

            StartCoroutine(FlashRoutine());

            if (currentHP <= 0f)
            {
                Die();
            }
        }

        protected IEnumerator FlashRoutine()
        {
            if (sr == null) yield break;
            Color orig = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.08f);
            if (sr != null) sr.color = isEnraged ? new Color(1f, 0.4f, 0.4f) : Color.white;
        }

        public virtual void Enrage()
        {
            isEnraged = true;
            currentHP = Mathf.Min(maxHP, currentHP + 200f);
            moveSpeed *= 1.35f;
            attackCooldown *= 0.65f;
            if (sr != null) sr.color = new Color(1f, 0.4f, 0.4f, 1f);
            Debug.Log($"[DualKnights] ⚔️ Hiệp sĩ còn lại đã HÓA CUỒNG vì đồng đội ngã xuống!");
        }

        protected virtual void Die()
        {
            isDead = true;
            if (rb != null) rb.velocity = Vector2.zero;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            parentController.NotifyMemberDeath(this);

            StartCoroutine(FadeOutRoutine());
        }

        protected IEnumerator FadeOutRoutine()
        {
            float elapsed = 0f;
            Color c = sr != null ? sr.color : Color.white;
            while (elapsed < 1.0f)
            {
                elapsed += Time.deltaTime;
                if (sr != null) sr.color = new Color(c.r, c.g, c.b, 1f - (elapsed / 1.0f));
                yield return null;
            }
            gameObject.SetActive(false);
        }

        protected void DealDamage(float dmg)
        {
            if (targetPlayer != null)
            {
                targetPlayer.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    /// <summary>
    /// Hiệp sĩ Kiếm: Áp sát chém combo 3 hit (35 DMG/hit)
    /// </summary>
    public class SwordKnightMember : KnightMemberBase
    {
        [SerializeField] private float swordComboDamage = 35f;

        protected override void UpdateKnightAI(float dist)
        {
            if (isAttacking) return;

            if (dist <= 2.2f && Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(SwordComboRoutine());
            }
            else
            {
                Vector2 dir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;
                rb.velocity = dir * moveSpeed;
            }
        }

        private IEnumerator SwordComboRoutine()
        {
            isAttacking = true;
            rb.velocity = Vector2.zero;

            // Combo 3 nhát chém lướt tới
            for (int i = 0; i < 3; i++)
            {
                if (targetPlayer == null) break;
                Vector2 dir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;

                // Telegraph ngắn
                BossAoEIndicator.CreateLine(transform.position, dir, 2.5f, 1.0f, 0.25f, new Color(1f, 0.3f, 0.3f, 0.7f));
                yield return new WaitForSeconds(0.25f);

                // Lướt chém
                rb.velocity = dir * 6.5f;
                yield return new WaitForSeconds(0.12f);
                rb.velocity = Vector2.zero;

                if (Vector2.Distance(transform.position, targetPlayer.position) <= 2.0f)
                {
                    DealDamage(swordComboDamage);
                }

                yield return new WaitForSeconds(0.15f);
            }

            lastAttackTime = Time.time;
            isAttacking = false;
        }
    }

    /// <summary>
    /// Hiệp sĩ Giáo: Đâm thẳng xuyên thấu tầm xa (45 DMG)
    /// </summary>
    public class SpearKnightMember : KnightMemberBase
    {
        [SerializeField] private float spearThrustDamage = 45f;
        [SerializeField] private float spearRange = 5.5f;

        protected override void UpdateKnightAI(float dist)
        {
            if (isAttacking) return;

            if (dist <= spearRange && Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(SpearThrustRoutine());
            }
            else
            {
                // Giữ cự ly tầm trung (3-5m), không đứng quá sát
                Vector2 dir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;
                if (dist < 3.0f)
                {
                    rb.velocity = -dir * (moveSpeed * 0.8f); // Lùi lại giữ tầm
                }
                else
                {
                    rb.velocity = dir * moveSpeed;
                }
            }
        }

        private IEnumerator SpearThrustRoutine()
        {
            isAttacking = true;
            rb.velocity = Vector2.zero;

            Vector2 thrustDir = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;

            // Telegraph tia đâm dài 6m
            BossAoEIndicator.CreateLine(transform.position, thrustDir, spearRange, 0.8f, 0.5f, new Color(0.2f, 0.6f, 1f, 0.8f));
            yield return new WaitForSeconds(0.5f);

            // Đâm xuyên cực nhanh
            rb.velocity = thrustDir * 12.0f;
            yield return new WaitForSeconds(0.18f);
            rb.velocity = Vector2.zero;

            if (targetPlayer != null && Vector2.Distance(transform.position, targetPlayer.position) <= 2.2f)
            {
                DealDamage(spearThrustDamage);
            }

            lastAttackTime = Time.time;
            isAttacking = false;
        }
    }
}

