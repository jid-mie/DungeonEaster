using UnityEngine;
using System.Collections.Generic;
using DungeonEater.Core;

namespace DungeonEater.Restaurant
{
    public class RestaurantManager : MonoBehaviour
    {
        public static RestaurantManager Instance { get; private set; }

        [Header("Tables & Furniture")]
        [SerializeField] private List<DiningTable> tables = new List<DiningTable>();
        [SerializeField] private GameObject customerPrefab;
        [SerializeField] private Transform customerSpawnPoint;

        [Header("Shift Settings")]
        [SerializeField] private bool isServiceActive = false;
        [SerializeField] private float shiftDuration = 120f; // 2 phút cho bản demo
        private float currentShiftTimer = 0f;
        [SerializeField] private float customerSpawnInterval = 8f;
        private float spawnTimer = 0f;

        public bool IsServiceActive => isServiceActive;
        public float RemainingShiftTime => Mathf.Max(0f, currentShiftTimer);

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SoundManager.Instance?.PlayBgm(SoundManager.Instance.restaurantBgm);
        }

        private void Update()
        {
            if (!isServiceActive) return;

            currentShiftTimer -= Time.deltaTime;
            if (currentShiftTimer <= 0f)
            {
                EndServiceShift();
                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = customerSpawnInterval;
                TrySpawnCustomer();
            }
        }

        public void StartServiceShift()
        {
            if (isServiceActive) return;

            isServiceActive = true;
            currentShiftTimer = shiftDuration;
            spawnTimer = 2f;
            Debug.Log("🔔 [Restaurant] Bắt đầu ca phục vụ nhà hàng!");
        }

        public void EndServiceShift()
        {
            isServiceActive = false;
            Debug.Log("🌙 [Restaurant] Kết thúc ca phục vụ nhà hàng! Người chơi có thể nâng cấp hoặc quay lại Dungeon.");
        }

        private void TrySpawnCustomer()
        {
            // Tìm bàn trống
            DiningTable emptyTable = tables.Find(t => !t.IsOccupied);
            if (emptyTable == null) return;

            // Chọn công thức ngẫu nhiên
            var recipes = CookingSystem.Instance.Recipes;
            if (recipes.Count == 0) return;
            var chosenRecipe = recipes[Random.Range(0, recipes.Count)];

            Vector3 spawnPos = customerSpawnPoint != null ? customerSpawnPoint.position : emptyTable.transform.position;
            GameObject custObj = null;

            if (customerPrefab != null)
            {
                custObj = Instantiate(customerPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                // Fallback runtime
                custObj = new GameObject("Customer_" + emptyTable.TableId);
                custObj.transform.position = spawnPos;
                var sr = custObj.AddComponent<SpriteRenderer>();
                sr.color = Color.cyan;
                custObj.AddComponent<CustomerAI>();
            }

            var cust = custObj.GetComponent<CustomerAI>();
            if (cust != null)
            {
                cust.Initialize(emptyTable, chosenRecipe);
            }
        }

        /// <summary>
        /// Tương tác phục vụ khi người chơi nhấn phím F gần bàn ăn
        /// </summary>
        public void InteractWithNearestTable(Vector2 playerPos)
        {
            foreach (var table in tables)
            {
                if (table.IsOccupied && Vector2.Distance(playerPos, table.transform.position) < 2.0f)
                {
                    table.CurrentCustomer.TryServe();
                    return;
                }
            }
        }

        public void AddTable(DiningTable newTable)
        {
            if (!tables.Contains(newTable))
            {
                tables.Add(newTable);
            }
        }
    }
}

