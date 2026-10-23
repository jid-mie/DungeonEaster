using System;
using UnityEngine;

namespace DungeonEater.TranHuuHau.Inventory
{
    /// <summary>
    /// Quản lý Tiền Vàng (Gold) và Điểm Danh Tiếng (Reputation) của người chơi.
    /// Khởi tạo mock 10,000 Vàng và 1,000 Danh tiếng để debug theo đúng yêu cầu người dùng.
    /// </summary>
    public class VillageWallet : MonoBehaviour
    {
        public static VillageWallet Instance { get; private set; }

        [Header("Mock Values theo yêu cầu")]
        [SerializeField] private int initialGold = 10000;
        [SerializeField] private int initialReputation = 1000;

        private int gold;
        private int reputation;

        public int Gold => gold;
        public int Reputation => reputation;

        public event Action OnChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            gold = initialGold;
            reputation = initialReputation;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            gold += amount;
            Debug.Log($"[VillageWallet] Nhận +{amount} Vàng. Tổng: {gold}");
            OnChanged?.Invoke();
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0) return true;
            if (gold < amount) return false;

            gold -= amount;
            Debug.Log($"[VillageWallet] Tiêu -{amount} Vàng. Còn lại: {gold}");
            OnChanged?.Invoke();
            return true;
        }

        public void AddReputation(int amount)
        {
            if (amount == 0) return;
            reputation = Mathf.Max(0, reputation + amount);
            string sign = amount > 0 ? "+" : "";
            Debug.Log($"[VillageWallet] Cập nhật Danh tiếng ({sign}{amount}). Tổng: {reputation}");
            OnChanged?.Invoke();
        }

        public void ResetToMock()
        {
            gold = initialGold;
            reputation = initialReputation;
            OnChanged?.Invoke();
        }
    }
}
