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
        [Header("--- Cơ chế Cào Chém Cận Chiến ---")]
        [SerializeField] private float clawAttackRange = 1.2f;
        [SerializeField] private float telegraphTime = 0.4f;
        [SerializeField] private float slashRadius = 1.0f;

        protected override void Awake()
        {
            maxHealth = 60f;
            moveSpeed = 1.8f;
            attackDamage = 12f;
            attackRange = 1.2f;
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

            // Telegraph 0.4s: Giương càng báo hiệu
            Vector2 attackDir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;

            TrySetAnimBool("IsAttack", true);
            if (anim != null) anim.Play("bugattack", 0, 0f);

            // Nháy đỏ nhẹ cảnh báo đòn đánh
            if (sr != null) sr.color = new Color(1f, 0.6f, 0.6f, 1f);

            yield return new WaitForSeconds(telegraphTime);

            if (sr != null) sr.color = Color.white;

            // Vung đòn cào xé
            if (targetPlayer != null && IsAlive)
            {
                float currentDist = Vector2.Distance(transform.position, targetPlayer.transform.position);
                if (currentDist <= slashRadius + 0.3f)
                {
                    targetPlayer.TakeDamage(attackDamage);
                }
            }

            TrySetAnimBool("IsAttack", false);
            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = AIState.Idle;
        }
    }
}
