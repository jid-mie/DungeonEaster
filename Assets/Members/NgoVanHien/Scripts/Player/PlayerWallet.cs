using System;
using UnityEngine;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Vàng (Gold) và Danh tiếng (Reputation) của người chơi.
    /// Theo GDD: Gold nhận từ bán món, Reputation nhận từ kết quả phục vụ, cả hai KHÔNG mất khi chết
    /// -> object này sống xuyên scene (DontDestroyOnLoad).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerWallet : MonoBehaviour
    {
        public static PlayerWallet Instance { get; private set; }


        private int gold;
        private int reputation;

        public int Gold => gold;
        public int Reputation => reputation;

        /// <summary>Gọi mỗi khi vàng hoặc danh tiếng thay đổi.</summary>
        public event Action OnChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
            var cfg = RestaurantDatabase.Wallet;
            gold = cfg.startingGold;
            reputation = cfg.startingReputation;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Đưa vàng/danh tiếng về giá trị khởi đầu trong restaurant_config.json.</summary>
        public void ResetToStart()
        {
            var cfg = RestaurantDatabase.Wallet;
            gold = cfg.startingGold;
            reputation = cfg.startingReputation;
            OnChanged?.Invoke();
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            gold += amount;
            OnChanged?.Invoke();
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0 || gold < amount) return false;
            gold -= amount;
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>Cộng (delta &gt; 0) hoặc trừ (delta &lt; 0) danh tiếng, không xuống dưới 0.</summary>
        public void AddReputation(int delta)
        {
            if (delta == 0) return;
            reputation = Mathf.Max(0, reputation + delta);
            OnChanged?.Invoke();
        }
    }
}
