using System.Collections;
using UnityEngine;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// AI Bọ Thiết Giáp (Cận chiến Cào chém):
    /// - Máu 60, Tốc độ 1.8, Sát thương 12.
    /// - Trước khi cào: Đứng yên hóa Đỏ Rực Cảnh Báo trong 0.80s.
    /// - Sau khi cào: Đứng nghỉ tại chỗ 1.25s rồi mới truy đuổi tiếp.
    /// </summary>
    public class BeetleMeleeAI : EnemyBase
    {
        [Header("--- Cơ chế Cào Chém Cận Chiến (Bán nguyệt 180 độ) ---")]
        [SerializeField] private float clawAttackRange = 1.3f;
        [SerializeField] private float clawArcAngle = 180f;
        [SerializeField] private float telegraphTime = 0.80f;
        [SerializeField] private float restAfterAttack = 1.25f;
        [Tooltip("Gán 4 Sprite từ Assets/Sprites/Detailed/VFX_Slash_Arc_0..3 để phát vệt cào")]
        [SerializeField] private Sprite[] clawSlashSprites;
        [SerializeField] private Color slashColor = new Color(1f, 0.35f, 0.2f, 1f);

        protected override void Awake()
        {
            isBoss = false;
            var cfg = NguyenVanHai.Data.DungeonCombatDatabase.GetEnemy("beetle");
            if (cfg != null)
            {
                maxHealth = cfg.maxHealth;
                moveSpeed = cfg.moveSpeed;
                attackDamage = cfg.attackDamage;
                detectRange = cfg.detectRange;
                attackRange = cfg.attackRange;
                attackCooldown = cfg.attackCooldown;
                telegraphTime = cfg.telegraphTime;
                restAfterAttack = cfg.restAfterAttack;
                clawAttackRange = cfg.clawAttackRange > 0f ? cfg.clawAttackRange : cfg.attackRange;
                clawArcAngle = cfg.clawArcAngle > 0f ? cfg.clawArcAngle : 180f;
                dropItemId = cfg.dropItemId;
                dropItemName = cfg.dropItemName;
                dropCount = cfg.dropCount;
            }

            base.Awake();
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking || Time.time < postAttackRestUntil) return;

            if (distanceToPlayer <= clawAttackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(ClawAttackRoutine());
            }
            else if (distanceToPlayer <= detectRange)
            {
                currentState = AIState.Chase;
                Vector2 dir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;
                rb.velocity = dir * moveSpeed;
            }
            else
            {
                currentState = AIState.Idle;
                rb.velocity = Vector2.zero;
            }
        }

        private IEnumerator ClawAttackRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;
            rb.velocity = Vector2.zero;

            Vector2 attackDir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;

            TrySetAnimBool("IsAttack", true);
            if (anim != null) anim.Play("bugattack", 0, 0f);

            // 1. Cảnh báo Đỏ Rực theo telegraphTime từ JSON
            yield return StartCoroutine(PlayRedWarningRoutine(telegraphTime, 1.12f));

            // 2. Sinh hiệu ứng vệt cào bán nguyệt VFX & Gây sát thương
            SpawnClawVFX(attackDir);

            if (targetPlayer != null && IsAlive)
            {
                Vector2 toPlayer = (Vector2)targetPlayer.transform.position - (Vector2)transform.position;
                float dist = toPlayer.magnitude;

                if (dist <= clawAttackRange)
                {
                    float angle = Vector2.Angle(attackDir, toPlayer.normalized);
                    if (angle <= clawArcAngle * 0.5f)
                    {
                        targetPlayer.TakeDamage(attackDamage);
                    }
                }
            }

            TrySetAnimBool("IsAttack", false);

            // 3. Quãng nghỉ bắt buộc theo restAfterAttack từ JSON
            yield return StartCoroutine(EnterPostAttackRestRoutine(restAfterAttack));
        }

        private void SpawnClawVFX(Vector2 dir)
        {
            if (clawSlashSprites == null || clawSlashSprites.Length == 0) return;

            var vfxGo = new GameObject("ClawSlash_VFX");
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            vfxGo.transform.position = transform.position + (Vector3)(dir * 0.5f);
            vfxGo.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            var vfxSr = vfxGo.AddComponent<SpriteRenderer>();
            vfxSr.color = slashColor;
            vfxSr.sortingLayerName = "Projectiles_VFX";
            vfxSr.sortingOrder = 6000;

            StartCoroutine(PlayClawVfxRoutine(vfxGo, vfxSr));
        }

        private IEnumerator PlayClawVfxRoutine(GameObject vfxGo, SpriteRenderer vfxSr)
        {
            float frameDuration = 0.035f;
            for (int i = 0; i < clawSlashSprites.Length; i++)
            {
                if (vfxGo == null) yield break;
                vfxSr.sprite = clawSlashSprites[i];
                yield return new WaitForSeconds(frameDuration);
            }
            if (vfxGo != null) Destroy(vfxGo);
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 facing = targetPlayer != null
                ? (Vector2)(targetPlayer.transform.position - transform.position).normalized
                : Vector2.down;

            float baseAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.9f);

            int segments = 16;
            float halfArc = clawArcAngle * 0.5f;
            Vector3 center = transform.position;
            Vector3 startPoint = center + new Vector3(Mathf.Cos((baseAngle - halfArc) * Mathf.Deg2Rad), Mathf.Sin((baseAngle - halfArc) * Mathf.Deg2Rad), 0f) * clawAttackRange;
            Gizmos.DrawLine(center, startPoint);

            Vector3 prevPoint = startPoint;
            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = baseAngle - halfArc + ((halfArc * 2f) * i / segments);
                Vector3 nextPoint = center + new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad), 0f) * clawAttackRange;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
            Gizmos.DrawLine(center, prevPoint);
        }
    }
}
