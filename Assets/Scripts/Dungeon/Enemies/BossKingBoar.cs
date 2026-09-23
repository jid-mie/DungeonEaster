using UnityEngine;
using DungeonEater.Core;

namespace DungeonEater.Dungeon.Enemies
{
    public class BossKingBoar : EnemyBase
    {
        [Header("Boss Skills")]
        [SerializeField] private float chargeSpeed = 12f;
        [SerializeField] private float chargeDamage = 30f;
        [SerializeField] private float stompRadius = 4.5f;
        [SerializeField] private float stompDamage = 25f;

        [Header("Telegraph & Action Timer")]
        [SerializeField] private float actionCooldown = 3.5f;
        private float actionTimer = 2f;
        private bool isExecutingSkill = false;

        private enum BossState { IdleChase, TelegraphCharge, Charging, TelegraphStomp, Stomping }
        private BossState state = BossState.IdleChase;
        private float stateTimer = 0f;
        private Vector2 chargeDirection;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 380f;
            currentHealth = maxHealth;
            moveSpeed = 2.4f;
            dropItem = Core.IngredientType.BossRib;
        }

        protected override void Update()
        {
            base.Update();
            if (playerTransform == null) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;

            switch (state)
            {
                case BossState.IdleChase:
                    rb.velocity = dirToPlayer * moveSpeed;
                    actionTimer -= Time.deltaTime;
                    if (actionTimer <= 0f)
                    {
                        actionTimer = actionCooldown;
                        // Random chọn chiêu: Húc càn hoặc Dậm đất
                        if (Random.value < 0.5f)
                        {
                            StartTelegraphCharge(dirToPlayer);
                        }
                        else
                        {
                            StartTelegraphStomp();
                        }
                    }
                    break;

                case BossState.TelegraphCharge:
                    stateTimer -= Time.deltaTime;
                    rb.velocity = Vector2.zero;
                    // Chớp cảnh báo đỏ
                    if (spriteRenderer != null) spriteRenderer.color = Color.Lerp(originalColor, Color.red, Mathf.PingPong(Time.time * 10, 1));
                    if (stateTimer <= 0f)
                    {
                        ExecuteCharge();
                    }
                    break;

                case BossState.Charging:
                    stateTimer -= Time.deltaTime;
                    rb.velocity = chargeDirection * chargeSpeed;
                    if (stateTimer <= 0f)
                    {
                        EndSkill();
                    }
                    break;

                case BossState.TelegraphStomp:
                    stateTimer -= Time.deltaTime;
                    rb.velocity = Vector2.zero;
                    if (spriteRenderer != null) spriteRenderer.color = Color.Lerp(originalColor, Color.yellow, Mathf.PingPong(Time.time * 10, 1));
                    if (stateTimer <= 0f)
                    {
                        ExecuteStomp();
                    }
                    break;
            }
        }

        private void StartTelegraphCharge(Vector2 dir)
        {
            state = BossState.TelegraphCharge;
            stateTimer = 0.6f; // Telegraph 0.6s
            chargeDirection = dir;
            Debug.Log("[Boss] Cảnh báo: Chuẩn bị Húc Càn (Charge)!");
        }

        private void ExecuteCharge()
        {
            state = BossState.Charging;
            stateTimer = 0.55f;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            SoundManager.Instance?.PlaySwordHeavy();
            CameraFollow.Instance?.TriggerScreenShake(0.2f, 0.3f);
        }

        private void StartTelegraphStomp()
        {
            state = BossState.TelegraphStomp;
            stateTimer = 0.65f;
            Debug.Log("[Boss] Cảnh báo: Chuẩn bị Dậm Đất (Ground Stomp)!");
        }

        private void ExecuteStomp()
        {
            state = BossState.IdleChase;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            SoundManager.Instance?.PlayExplosion();
            CameraFollow.Instance?.TriggerScreenShake(0.35f, 0.5f);

            // Sát thương sóng xung kích
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, stompRadius);
            foreach (var hit in hits)
            {
                var player = hit.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(stompDamage);
                }
            }
            Debug.Log("[Boss] Dậm đất rung chuyển hầm ngục!");
        }

        private void EndSkill()
        {
            state = BossState.IdleChase;
            rb.velocity = Vector2.zero;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }

        protected override void Die()
        {
            Debug.Log("🏆 [Boss] King Boar đã bị tiêu diệt!");
            GameManager.Instance?.AddGold(120);
            SoundManager.Instance?.PlayCoin();

            // Rơi thêm nhiều thịt
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(IngredientType.MonsterMeat, 2);
            }

            base.Die();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stompRadius);
        }
    }
}

