using UnityEngine;

namespace DungeonEater.Dungeon.Enemies
{
    public class MeleeSlime : EnemyBase
    {
        [Header("Slime Attack Settings")]
        [SerializeField] private float attackRange = 1.8f;
        [SerializeField] private float leapSpeed = 8f;
        [SerializeField] private float telegraphDuration = 0.4f; // 0.4s telegraph theo GDD
        [SerializeField] private float attackCooldown = 1.5f;

        private float currentTelegraph = 0f;
        private float currentCooldown = 0f;
        private bool isLeaping = false;
        private Vector2 leapDirection;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 45f;
            currentHealth = maxHealth;
            moveSpeed = 2.2f;
            dropItem = Core.IngredientType.MonsterMeat;
        }

        protected override void Update()
        {
            base.Update();
            if (playerTransform == null) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (currentCooldown > 0f) currentCooldown -= Time.deltaTime;

            if (currentTelegraph > 0f)
            {
                currentTelegraph -= Time.deltaTime;
                // Nhấp nháy đỏ báo hiệu đòn đánh sắp tới (Telegraph)
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(originalColor, Color.red, Mathf.PingPong(Time.time * 8, 1));
                }

                if (currentTelegraph <= 0f)
                {
                    // Thực hiện cú nhảy vồ
                    isLeaping = true;
                    if (spriteRenderer != null) spriteRenderer.color = originalColor;
                    rb.velocity = leapDirection * leapSpeed;
                    Invoke(nameof(EndLeap), 0.25f);
                    currentCooldown = attackCooldown;
                }
                return;
            }

            if (!isLeaping)
            {
                if (distance <= attackRange && currentCooldown <= 0f)
                {
                    // Bắt đầu chuẩn bị vồ (Telegraph)
                    currentTelegraph = telegraphDuration;
                    leapDirection = (playerTransform.position - transform.position).normalized;
                    rb.velocity = Vector2.zero;
                }
                else
                {
                    // Tiếp cận người chơi
                    Vector2 dir = (playerTransform.position - transform.position).normalized;
                    rb.velocity = dir * moveSpeed;
                }
            }
        }

        private void EndLeap()
        {
            isLeaping = false;
            rb.velocity = Vector2.zero;
        }
    }
}

