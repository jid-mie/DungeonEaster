using System.Collections;
using UnityEngine;
using NguyenVanHai.Combat;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// AI Nấm Bào Tử (Tầm xa - Thay thế Bộ Xương Cung Thủ theo xác nhận của User):
    /// - Máu 30, Tốc độ 2.0, Sát thương 10.
    /// - Giữ khoảng cách 5-8 units, bắn đạn bào tử độc thẳng vào vị trí Player (tần suất 1.5s/viên).
    /// - Rơi: Mẩu xương vụn / Nấm hang phát quang.
    /// </summary>
    public class SporeShooterAI : EnemyBase
    {
        [Header("--- Cơ chế Bắn Tầm xa ---")]
        [SerializeField] private float minKeepDistance = 4.0f;
        [SerializeField] private float maxShootDistance = 8.0f;
        [SerializeField] private float projectileSpeed = 6.0f;
        [SerializeField] private float telegraphDuration = 0.45f;
        [SerializeField] private Sprite sporeBulletSprite;

        protected override void Awake()
        {
            maxHealth = 30f;
            moveSpeed = 2.0f;
            attackDamage = 10f;
            detectRange = 9.0f;
            attackRange = 8.0f;
            attackCooldown = 1.5f;

            dropItemId = "bone_fragment";
            dropItemName = "Mẩu xương vụn";
            dropCount = 1;

            base.Awake();
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking) return;

            if (distanceToPlayer <= maxShootDistance)
            {
                // Giữ khoảng cách: Nếu người chơi áp sát quá gần thì lùi lại
                if (distanceToPlayer < minKeepDistance)
                {
                    Vector2 retreatDir = ((Vector2)transform.position - (Vector2)targetPlayer.transform.position).normalized;
                    rb.velocity = retreatDir * moveSpeed;
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }

                // Bắn khi hồi chiêu xong
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    StartCoroutine(ShootSporeRoutine());
                }
            }
            else if (distanceToPlayer <= detectRange)
            {
                // Tiếp cận đến tầm bắn
                Vector2 approachDir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;
                rb.velocity = approachDir * moveSpeed;
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }

        private IEnumerator ShootSporeRoutine()
        {
            isAttacking = true;
            currentState = AIState.Attack;
            rb.velocity = Vector2.zero;

            // Telegraph 0.45s: Phình to mũ nấm báo hiệu
            Vector3 originalScale = transform.localScale;
            transform.localScale = originalScale * 1.2f;

            TrySetAnimBool("IsAttack", true);
            if (anim != null) anim.Play("Attack", 0, 0f);

            yield return new WaitForSeconds(telegraphDuration);

            transform.localScale = originalScale;

            if (targetPlayer != null && IsAlive)
            {
                Vector2 shootDir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;
                SpawnSporeProjectile(shootDir);
            }

            TrySetAnimBool("IsAttack", false);
            lastAttackTime = Time.time;
            isAttacking = false;
            currentState = AIState.Idle;
        }

        private void SpawnSporeProjectile(Vector2 dir)
        {
            var go = new GameObject("SporeBullet");
            go.transform.position = transform.position + (Vector3)(dir * 0.4f);

            var bulletSr = go.AddComponent<SpriteRenderer>();
            if (sporeBulletSprite != null) bulletSr.sprite = sporeBulletSprite;
            bulletSr.color = new Color(0.8f, 0.4f, 1f, 1f); // Tím độc
            bulletSr.sortingOrder = 4050;

            var proj = go.AddComponent<SporeProjectile>();
            proj.Initialize(dir, attackDamage, projectileSpeed);
        }
    }
}
