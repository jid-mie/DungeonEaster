using UnityEngine;
using DungeonEater.Restaurant;

namespace DungeonEater.NPC
{
    /// <summary>
    /// Manages spawning customers with random sprite variants in the Restaurant scene.
    /// Attach to the RestaurantManager or a dedicated CustomerSpawner GameObject.
    /// </summary>
    public class CustomerSpawner : MonoBehaviour
    {
        [Header("Customer Sprites")]
        [SerializeField] private Sprite[] customerIdleSprites;
        [SerializeField] private Sprite[] customerEatingSprites;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 8f;
        [SerializeField] private int maxCustomers = 20;
        [SerializeField] private float shiftDuration = 600f; // 10 minutes

        private float spawnTimer;
        private float shiftTimer;
        private int customersSpawned = 0;
        private bool isShiftActive = false;

        private DiningTable[] tables;

        private void Start()
        {
            tables = FindObjectsOfType<DiningTable>();
        }

        public void StartShift()
        {
            isShiftActive = true;
            shiftTimer = shiftDuration;
            customersSpawned = 0;
            spawnTimer = 2f; // First customer comes after 2 seconds
            Debug.Log("🏪 [Ca phục vụ] Bắt đầu! Chờ khách...");
        }

        public void StopShift()
        {
            isShiftActive = false;
            Debug.Log("🏪 [Ca phục vụ] Kết thúc!");
        }

        private void Update()
        {
            if (!isShiftActive) return;

            shiftTimer -= Time.deltaTime;
            if (shiftTimer <= 0f)
            {
                StopShift();
                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f && customersSpawned < maxCustomers)
            {
                TrySpawnCustomer();
                // Random interval between customers (5-12 seconds)
                spawnTimer = Random.Range(5f, 12f);
            }
        }

        private void TrySpawnCustomer()
        {
            // Find an empty table
            DiningTable emptyTable = null;
            foreach (DiningTable table in tables)
            {
                if (!table.IsOccupied)
                {
                    emptyTable = table;
                    break;
                }
            }

            if (emptyTable == null)
            {
                Debug.Log("🚫 Hết bàn trống! Khách phải đợi...");
                return;
            }

            // Create customer GameObject
            GameObject customerObj = new GameObject("Customer_" + customersSpawned);
            customerObj.transform.position = emptyTable.transform.position + new Vector3(0, 0.3f, 0);

            // Add SpriteRenderer with random customer sprite
            SpriteRenderer sr = customerObj.AddComponent<SpriteRenderer>();
            if (customerIdleSprites != null && customerIdleSprites.Length > 0)
            {
                int randIdx = Random.Range(0, customerIdleSprites.Length);
                sr.sprite = customerIdleSprites[randIdx];
            }
            sr.sortingOrder = 50;

            // Add CustomerAI component
            CustomerAI ai = customerObj.AddComponent<CustomerAI>();

            // Assign random recipe
            CookingSystem cooking = CookingSystem.Instance;
            if (cooking != null && cooking.Recipes != null && cooking.Recipes.Count > 0)
            {
                int recipeIdx = Random.Range(0, cooking.Recipes.Count);
                Recipe recipe = cooking.Recipes[recipeIdx];
                float extraPatience = Random.Range(0f, 10f);
                ai.Initialize(emptyTable, recipe, extraPatience);
            }

            customersSpawned++;
            Debug.Log($"👤 [Khách #{customersSpawned}] Đã vào quán!");
        }
    }
}

