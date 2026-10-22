using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Ca phục vụ (GDD 3.2.2): kéo dài 10 phút, tối đa 20 khách. Spawn khách ở cửa, dẫn tới bàn trống,
    /// gán order ngẫu nhiên trong các recipe đã mở khóa.
    /// </summary>
    public class TavernShift : MonoBehaviour
    {
        public static TavernShift Instance { get; private set; }

        [Header("Ca phục vụ")]
        [SerializeField] private bool startOnPlay = true;
        [SerializeField] private float shiftDuration = 600f;
        [SerializeField] private int maxCustomers = 20;
        [Tooltip("Khoảng thời gian ngẫu nhiên giữa 2 lượt khách (giây)")]
        [SerializeField] private Vector2 spawnInterval = new Vector2(6f, 12f);
        [SerializeField] private float firstCustomerDelay = 2f;

        [Header("Đường đi")]
        [Tooltip("Cửa quán: khách xuất hiện và biến mất ở đây")]
        [SerializeField] private Transform spawnPoint;
        [Tooltip("Các điểm khách đi qua sau khi vào cửa (lối đi chính), theo thứ tự")]
        [SerializeField] private List<Transform> entryWaypoints = new List<Transform>();

        [Header("Khách & bàn")]
        [SerializeField] private List<CustomerProfile> profiles = new List<CustomerProfile>();
        [Tooltip("Để trống sẽ tự tìm mọi TavernTable trong scene")]
        [SerializeField] private List<TavernTable> tables = new List<TavernTable>();
        [SerializeField] private float customerScale = 0.5f;

        private float timeLeft;
        private float spawnTimer;
        private readonly List<TavernCustomer> activeCustomers = new List<TavernCustomer>();

        public bool IsRunning { get; private set; }
        public float TimeLeft => Mathf.Max(0f, timeLeft);
        public int MaxCustomers => maxCustomers;
        public int SpawnedCount { get; private set; }
        public int ServedCount { get; private set; }
        public int AngryCount { get; private set; }
        public int EarnedGold { get; private set; }
        public int EarnedReputation { get; private set; }
        public IReadOnlyList<TavernCustomer> ActiveCustomers => activeCustomers;

        public event Action OnStatsChanged;
        public event Action OnShiftEnded;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (tables.Count == 0) tables.AddRange(FindObjectsOfType<TavernTable>());
            tables.Sort((a, b) => a.TableNumber.CompareTo(b.TableNumber));
            if (startOnPlay) StartShift();
        }

        public void StartShift()
        {
            if (IsRunning) return;
            IsRunning = true;
            timeLeft = shiftDuration;
            spawnTimer = firstCustomerDelay;
            SpawnedCount = ServedCount = AngryCount = EarnedGold = EarnedReputation = 0;
            if (TavernUI.Instance != null) TavernUI.Instance.ShowToast("Quán mở cửa! Ca phục vụ bắt đầu.", TavernUI.ColorGood);
            OnStatsChanged?.Invoke();
        }

        private void EndShift()
        {
            IsRunning = false;
            if (TavernUI.Instance != null)
            {
                TavernUI.Instance.ShowToast(
                    $"Hết ca! Phục vụ {ServedCount}/{SpawnedCount} khách, +{EarnedGold} vàng, {EarnedReputation:+#;-#;0} danh tiếng.",
                    TavernUI.ColorGood, 6f);
            }
            OnShiftEnded?.Invoke();
            OnStatsChanged?.Invoke();
        }

        private void Update()
        {
            if (!IsRunning) return;

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                // Khách đang trong quán vẫn được phục vụ tiếp, chỉ dừng đón khách mới
                EndShift();
                return;
            }

            if (SpawnedCount >= maxCustomers) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f) return;

            if (TrySpawnCustomer())
            {
                spawnTimer = UnityEngine.Random.Range(spawnInterval.x, spawnInterval.y);
            }
            else
            {
                spawnTimer = 1f; // hết bàn trống / chưa có recipe -> thử lại sau
            }
        }

        private bool TrySpawnCustomer()
        {
            var freeTables = tables.FindAll(t => t != null && t.IsFree);
            if (freeTables.Count == 0 || profiles.Count == 0 || Kitchen.Instance == null) return false;

            var menu = Kitchen.Instance.GetUnlockedRecipes();
            if (menu.Count == 0) return false;

            var table = freeTables[UnityEngine.Random.Range(0, freeTables.Count)];
            var profile = profiles[UnityEngine.Random.Range(0, profiles.Count)];
            var order = menu[UnityEngine.Random.Range(0, menu.Count)];

            var entryPath = new List<Vector3>();
            entryPath.Add(spawnPoint != null ? spawnPoint.position : transform.position);
            foreach (var w in entryWaypoints)
            {
                if (w != null) entryPath.Add(w.position);
            }

            var customer = TavernCustomer.Create(profile, order, table, entryPath, this, customerScale);
            activeCustomers.Add(customer);
            SpawnedCount++;
            OnStatsChanged?.Invoke();
            return true;
        }

        public void NotifyServed(TavernCustomer customer, ServeOutcome outcome)
        {
            ServedCount++;
            EarnedGold += outcome.gold + outcome.tip;
            EarnedReputation += outcome.reputation;
            OnStatsChanged?.Invoke();
        }

        public void NotifyAngry(TavernCustomer customer, int penalty)
        {
            AngryCount++;
            EarnedReputation -= penalty;
            if (TavernUI.Instance != null)
            {
                TavernUI.Instance.ShowToast($"{customer.Profile.displayName} ở bàn {customer.Table.TableNumber} chờ quá lâu và bỏ về! (-{penalty} danh tiếng)", TavernUI.ColorBad);
            }
            OnStatsChanged?.Invoke();
        }

        public void NotifyCustomerGone(TavernCustomer customer)
        {
            activeCustomers.Remove(customer);
            OnStatsChanged?.Invoke();
        }

        private void OnDrawGizmos()
        {
            if (spawnPoint == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
            Vector3 prev = spawnPoint.position;
            foreach (var w in entryWaypoints)
            {
                if (w == null) continue;
                Gizmos.DrawLine(prev, w.position);
                prev = w.position;
            }
        }
    }
}
