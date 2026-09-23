using UnityEngine;
using DungeonEater.Core;

namespace DungeonEater.Dungeon
{
    [RequireComponent(typeof(Collider2D))]
    public class LootItem : MonoBehaviour
    {
        [SerializeField] private IngredientType itemType = IngredientType.MonsterMeat;
        [SerializeField] private float magnetDistance = 2.5f;
        [SerializeField] private float magnetSpeed = 6f;

        private Transform playerTransform;

        private void Start()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null) playerTransform = player.transform;
        }

        private void Update()
        {
            if (playerTransform == null) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance < magnetDistance)
            {
                // Hút nhẹ về phía người chơi
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);

                if (distance < 0.6f)
                {
                    Collect();
                }
            }
        }

        public void SetItemType(IngredientType type)
        {
            itemType = type;
        }

        public void Collect()
        {
            if (InventoryManager.Instance != null && InventoryManager.Instance.AddItem(itemType, 1))
            {
                SoundManager.Instance?.PlayPickup();
                Destroy(gameObject);
            }
        }
    }
}

