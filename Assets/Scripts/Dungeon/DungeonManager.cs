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
                // Đợt 1: 2 Slime cận chiến + 1 Nấm Spore tầm xa trong lòng sàn đấu
                SpawnEnemy(slimePrefab, new Vector2(-2.2f, 0.0f));
                SpawnEnemy(slimePrefab, new Vector2(2.4f, 0.0f));
                SpawnEnemy(sporePrefab, new Vector2(0.0f, 0.9f));
            }
            else if (wave == 2)
            {
                // Đợt 2: Phối hợp Melee Slime + 2 Kamikaze Beetle bộc phá + 1 Ranged Spore
                SpawnEnemy(slimePrefab, new Vector2(-2.2f, -0.6f));
                SpawnEnemy(beetlePrefab, new Vector2(-1.2f, 0.5f));
                SpawnEnemy(beetlePrefab, new Vector2(1.2f, 0.5f));
                SpawnEnemy(sporePrefab, new Vector2(2.2f, -0.6f));
            }
            else if (wave == 3)
            {
                // Đợt 3: Trùm Vua Lợn Rừng (King Boar) ở trung tâm + 2 Hộ vệ Bọ bộc phá
                SpawnEnemy(bossPrefab, new Vector2(0.0f, 0.3f));
                SpawnEnemy(beetlePrefab, new Vector2(-2.0f, -1.0f));
                SpawnEnemy(beetlePrefab, new Vector2(2.0f, -1.0f));
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

