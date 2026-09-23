using UnityEngine;
using System.Collections.Generic;
using DungeonEater.Core;
using DungeonEater.Dungeon.Enemies;

namespace DungeonEater.Dungeon
{
    public class DungeonManager : MonoBehaviour
    {
        public static DungeonManager Instance { get; private set; }

        [Header("Wave Management")]
        [SerializeField] private int currentWave = 1;
        [SerializeField] private int maxWaves = 3;
        [SerializeField] private List<EnemyBase> activeEnemies = new List<EnemyBase>();

        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject slimePrefab;
        [SerializeField] private GameObject sporePrefab;
        [SerializeField] private GameObject beetlePrefab;
        [SerializeField] private GameObject bossPrefab;

        [Header("Portal Settings")]
        [SerializeField] private GameObject portalObject;
        [SerializeField] private Transform portalSpawnPoint;

        public int CurrentWave => currentWave;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            StartWave(currentWave);
            SoundManager.Instance?.PlayBgm(SoundManager.Instance.dungeonBgm);
        }

        public void StartWave(int wave)
        {
            currentWave = wave;
            Debug.Log($"⚔ [DungeonManager] Bắt đầu Đợt {currentWave}/{maxWaves}!");

            if (wave == 1)
            {
                SpawnEnemy(slimePrefab, new Vector2(-4, 2));
                SpawnEnemy(slimePrefab, new Vector2(4, 2));
                SpawnEnemy(sporePrefab, new Vector2(0, 4));
                SpawnEnemy(slimePrefab, new Vector2(-2.2f, 0.0f));
                SpawnEnemy(slimePrefab, new Vector2(2.4f, 0.0f));
                SpawnEnemy(sporePrefab, new Vector2(0.2f, 0.8f));
            }
            else if (wave == 2)
            {
                SpawnEnemy(slimePrefab, new Vector2(-5, 1));
                SpawnEnemy(beetlePrefab, new Vector2(-2, 3));
                SpawnEnemy(beetlePrefab, new Vector2(2, 3));
                SpawnEnemy(sporePrefab, new Vector2(5, 2));
                SpawnEnemy(slimePrefab, new Vector2(-2.2f, -0.6f));
                SpawnEnemy(beetlePrefab, new Vector2(-1.0f, 0.5f));
                SpawnEnemy(beetlePrefab, new Vector2(1.4f, 0.5f));
                SpawnEnemy(sporePrefab, new Vector2(2.4f, -0.6f));
            }
            else if (wave == 3)
            {
                // Boss Wave
                SpawnEnemy(bossPrefab, new Vector2(0, 3));
                SpawnEnemy(beetlePrefab, new Vector2(-4, 1));
                SpawnEnemy(beetlePrefab, new Vector2(4, 1));
                // Boss Wave - Boss xuất hiện ngay trung tâm đấu trường cổ ngữ
                SpawnEnemy(bossPrefab, new Vector2(0.2f, 0.2f));
                SpawnEnemy(beetlePrefab, new Vector2(-2.0f, -1.2f));
                SpawnEnemy(beetlePrefab, new Vector2(2.2f, -1.2f));
            }
        }

        private void SpawnEnemy(GameObject prefab, Vector2 position)
        {
            GameObject enemyObj = null;
            if (prefab != null)
            {
                enemyObj = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                // Fallback runtime spawn
                enemyObj = new GameObject("RuntimeEnemy");
                enemyObj.transform.position = position;
                enemyObj.AddComponent<CircleCollider2D>();
                var rb = enemyObj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                var sr = enemyObj.AddComponent<SpriteRenderer>();
                sr.color = Color.green;
                enemyObj.AddComponent<MeleeSlime>();
            }

            var enemyComp = enemyObj.GetComponent<EnemyBase>();
            if (enemyComp != null)
            {
                activeEnemies.Add(enemyComp);
            }
        }

        public void OnEnemyKilled(EnemyBase enemy)
        {
            activeEnemies.Remove(enemy);
            Debug.Log($"[DungeonManager] Tiêu diệt 1 quái. Quái còn lại trong đợt: {activeEnemies.Count}");

            if (activeEnemies.Count == 0)
            {
                if (currentWave < maxWaves)
                {
                    Invoke(nameof(ProceedToNextWave), 2.0f);
                }
                else
                {
                    OpenVillagePortal();
                }
            }
        }

        private void ProceedToNextWave()
        {
            StartWave(currentWave + 1);
        }

        private void OpenVillagePortal()
        {
            Debug.Log("🌀 [DungeonManager] Dungeon hoàn thành! Cổng trở về làng Dun đã kích hoạt!");
            if (portalObject != null)
            {
                portalObject.SetActive(true);
            }
        }

        /// <summary>
        /// Được gọi khi người chơi bước vào portal hoặc tương tác F
        /// </summary>
        public void TeleportToVillage()
        {
            GameManager.Instance?.ReturnToRestaurant();
        }
    }
}

