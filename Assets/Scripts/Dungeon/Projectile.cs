using UnityEngine;
using DungeonEater.Dungeon.Enemies;

namespace DungeonEater.Dungeon
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float damage = 18f;
        [SerializeField] private float speed = 14f;
        [SerializeField] private int pierceCount = 1;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private bool isPlayerProjectile = true;

        private Vector2 velocity;

        public void Initialize(Vector2 direction, float projSpeed, float projDamage, int pierce, bool isSpecial)
        {
            speed = projSpeed;
            damage = projDamage;
            pierceCount = pierce;
            velocity = direction.normalized * speed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isPlayerProjectile)
            {
                var enemy = collision.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, velocity.normalized * 2f);
                    pierceCount--;
                    if (pierceCount <= 0)
                    {
                        Destroy(gameObject);
                    }
                }
            }
            else
            {
                // Enemy projectile hitting player
                var player = collision.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }
    }
}

