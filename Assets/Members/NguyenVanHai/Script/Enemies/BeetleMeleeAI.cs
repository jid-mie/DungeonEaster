using System.Collections;
using UnityEngine;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// AI Bọ Thiết Giáp (Cận chiến Cào chém - Thay thế Thây Ma theo xác nhận của User):
    /// - Máu 60, Tốc độ 1.8, Sát thương 12.
    /// - Áp sát và tung đòn cào xé hình vòng cung cự ly 1.2 units (tần suất 1.2s/đòn), có báo hiệu 0.4s.
    /// - 100% rơi: Thớ thịt khô.
    /// </summary>
    public class BeetleMeleeAI : EnemyBase
    {
        [Header("--- Cơ chế Cào Chém Cận Chiến (Bán nguyệt 180 độ) ---")]
        [SerializeField] private float clawAttackRange = 1.3f;
        [SerializeField] private float clawArcAngle = 180f; // Bán nguyệt 180 độ theo hướng tấn công
        [SerializeField] private float telegraphTime = 0.4f;
        [Tooltip("Gán 4 Sprite từ Assets/Sprites/Detailed/VFX_Slash_Arc_0..3 để phát vệt cào")]
        [SerializeField] private Sprite[] clawSlashSprites;
        [SerializeField] private Color slashColor = new Color(1f, 0.35f, 0.2f, 1f); // Màu móng vuốt đỏ cam sắc bén

        protected override void Awake()
        {
            maxHealth = 60f;
            moveSpeed = 1.8f;
            attackDamage = 12f;
            attackRange = 1.3f;
            attackCooldown = 1.2f;

            dropItemId = "dried_meat";
            dropItemName = "Thớ thịt khô";
            dropCount = 1;

            base.Awake();
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking) return;

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

            // Hướng tấn công về phía Player
            Vector2 attackDir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;

            TrySetAnimBool("IsAttack", true);
            if (anim != null) anim.Play("bugattack", 0, 0f);

            // Telegraph 0.4s: Nháy đỏ nhẹ cảnh báo đòn đánh
            if (sr != null) sr.color = new Color(1f, 0.6f, 0.6f, 1f);

            yield return new WaitForSeconds(telegraphTime);

            if (sr != null) sr.color = Color.white;

            // Sinh hiệu ứng vệt cào bán nguyệt VFX
            SpawnClawVFX(attackDir);

            // Vung đòn cào xé hình bán nguyệt 180 độ
            if (targetPlayer != null && IsAlive)
            {
                Vector2 toPlayer = (Vector2)targetPlayer.transform.position - (Vector2)transform.position;
                float dist = toPlayer.magnitude;

                if (dist <= clawAttackRange)
                {
                    float angle = Vector2.Angle(attackDir, toPlayer.normalized);
                    // Góc nằm trong bán nguyệt (180 độ / 2 = 90 độ mỗi bên)
                    if (angle <= clawArcAngle * 0.5f)
                    {
                        targetPlayer.TakeDamage(attackDamage);
                    }
                }
            }

            TrySetAnimBool("IsAttack", false);
            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = AIState.Idle;
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
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.9f); // Màu cam bán nguyệt quái

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
