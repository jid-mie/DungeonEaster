using System.Collections;
using UnityEngine;
using NguyenVanHai.Combat;
using NguyenVanHai.Data;

namespace NguyenVanHai.Enemies
{
    /// <summary>
    /// AI Nấm Bào Tử (Tầm xa):
    /// - Đọc toàn bộ thông số từ dungeon_combat_config.json (id: "spore_shooter").
    /// - Trước khi bắn: Đứng yên hóa Đỏ Rực Cảnh Báo để người chơi dễ dàng nhận biết.
    /// - Sau khi bắn: Nghỉ theo restAfterAttack trong JSON.
    /// </summary>
    public class SporeShooterAI : EnemyBase
    {
        [Header("--- Cơ chế Bắn Tầm xa ---")]
        [SerializeField] private float minKeepDistance = 4.0f;
        [SerializeField] private float maxShootDistance = 8.0f;
        [SerializeField] private float projectileSpeed = 5.5f;
        [SerializeField] private float telegraphDuration = 0.85f;
        [SerializeField] private float restAfterShoot = 1.35f;
        [Tooltip("Gán 4 Sprite từ Assets/Sprites/Detailed/VFX_Poison_Spore_0..3")]
        [SerializeField] private Sprite[] sporeAnimSprites;
        [SerializeField] private Sprite sporeBulletSprite;

        protected override void Awake()
        {
            isBoss = false;
            var cfg = DungeonCombatDatabase.GetEnemy("spore_shooter");
            if (cfg != null)
            {
                maxHealth = cfg.maxHealth;
                moveSpeed = cfg.moveSpeed;
                attackDamage = cfg.attackDamage;
                detectRange = cfg.detectRange;
                attackRange = cfg.attackRange;
                attackCooldown = cfg.attackCooldown;
                telegraphDuration = cfg.telegraphTime;
                restAfterShoot = cfg.restAfterAttack;
                minKeepDistance = cfg.minKeepDistance;
                maxShootDistance = cfg.maxShootDistance;
                projectileSpeed = cfg.projectileSpeed;
                dropItemId = cfg.dropItemId;
                dropItemName = cfg.dropItemName;
                dropCount = cfg.dropCount;
            }
            else
            {
                maxHealth = 30f;
                moveSpeed = 2.0f;
                attackDamage = 10f;
                detectRange = 9.0f;
                attackRange = 8.0f;
                attackCooldown = 2.5f;
                dropItemId = "bone_fragment";
                dropItemName = "Mẩu xương vụn";
                dropCount = 1;
            }

            base.Awake();
        }

        protected override void UpdateAI(float distanceToPlayer)
        {
            if (isAttacking || Time.time < postAttackRestUntil) return;

            if (distanceToPlayer <= maxShootDistance)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    StartCoroutine(ShootSporeRoutine());
                    return;
                }

                if (distanceToPlayer < minKeepDistance)
                {
                    Vector2 retreatDir = ((Vector2)transform.position - (Vector2)targetPlayer.transform.position).normalized;
                    rb.velocity = retreatDir * moveSpeed;
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
            }
            else if (distanceToPlayer <= detectRange)
            {
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

            TrySetAnimBool("IsAttack", true);
            if (anim != null) anim.Play("Attack", 0, 0f);

            // 1. Đổi màu thân Nấm sang ĐỎ RỰC CẢNH BÁO + phình mũ nấm theo telegraphDuration từ JSON
            yield return StartCoroutine(PlayRedWarningRoutine(telegraphDuration, 1.20f));

            // 2. Bắn đạn bào tử
            if (targetPlayer != null && IsAlive)
            {
                Vector2 shootDir = ((Vector2)targetPlayer.transform.position - (Vector2)transform.position).normalized;
                SpawnSporeProjectile(shootDir);
            }

            TrySetAnimBool("IsAttack", false);

            // 3. Quãng nghỉ bắt buộc sau khi bắn theo restAfterShoot từ JSON
            yield return StartCoroutine(EnterPostAttackRestRoutine(restAfterShoot));
        }

        private void SpawnSporeProjectile(Vector2 dir)
        {
            var go = new GameObject("SporeBullet");
            go.transform.position = transform.position + (Vector3)(dir * 0.4f);

            var bulletSr = go.AddComponent<SpriteRenderer>();
            if (sporeAnimSprites != null && sporeAnimSprites.Length > 0)
            {
                bulletSr.sprite = sporeAnimSprites[0];
            }
            else if (sporeBulletSprite != null)
            {
                bulletSr.sprite = sporeBulletSprite;
                bulletSr.color = new Color(0.8f, 0.4f, 1f, 1f);
            }
            bulletSr.sortingLayerName = "Projectiles_VFX";
            bulletSr.sortingOrder = 5500;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.isTrigger = true;

            var proj = go.AddComponent<SporeProjectile>();
            proj.Initialize(dir, attackDamage, projectileSpeed, sporeAnimSprites);
        }
    }
}
