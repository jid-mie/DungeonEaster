using UnityEngine;
using System;

namespace DungeonEater.Core
{
    public enum GameState
    {
        Dungeon,
        Restaurant,
        GameOver,
        Victory
    }

    /// <summary>
    /// Singleton quản lý luồng trạng thái toàn cục, vòng lặp game (Dungeon <-> Restaurant)
    /// và hệ thống tiền tệ / danh tiếng theo tài liệu GDD môn Thiết kế và phát triển Game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Dungeon;
        public GameState CurrentState => currentState;

        [Header("Economy & Meta")]
        [SerializeField] private int gold = 50;
        [SerializeField] private int reputation = 0;
        public int Gold => gold;
        public int Reputation => reputation;

        [Header("Story Flags")]
        public bool hasMetElder = false;

        // Events
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnGoldChanged;
        public event Action<int> OnReputationChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetState(GameState.Dungeon);
        }

        public void SetState(GameState newState)
        {
            currentState = newState;
            Debug.Log($"[GameManager] Chuyển đổi trạng thái game sang: {currentState}");
            OnStateChanged?.Invoke(currentState);
        }

        public void AddGold(int amount)
        {
            gold += amount;
            Debug.Log($"[GameManager] Nhận +{amount} Vàng. Tổng: {gold}");
            OnGoldChanged?.Invoke(gold);
        }

        public bool SpendGold(int amount)
        {
            if (gold < amount) return false;
            gold -= amount;
            Debug.Log($"[GameManager] Tiêu thụ -{amount} Vàng. Còn lại: {gold}");
            OnGoldChanged?.Invoke(gold);
            return true;
        }

        public void AddReputation(int amount)
        {
            reputation += amount;
            Debug.Log($"[GameManager] Nhận +{amount} Danh tiếng. Tổng: {reputation}");
            OnReputationChanged?.Invoke(reputation);
        }

        public void ReduceReputation(int amount)
        {
            reputation = Mathf.Max(0, reputation - amount);
            Debug.Log($"[GameManager] Bị trừ -{amount} Danh tiếng. Còn lại: {reputation}");
            OnReputationChanged?.Invoke(reputation);
        }

        /// <summary>
        /// Chuyển từ Dungeon về nhà hàng Làng Dun
        /// </summary>
        public void ReturnToRestaurant()
        {
            SetState(GameState.Restaurant);
        }

        /// <summary>
        /// Bắt đầu một Run mới trong Dungeon
        /// </summary>
        public void EnterDungeon()
        {
            SetState(GameState.Dungeon);
        }

        /// <summary>
        /// Xử lý khi nhân vật cạn HP trong Dungeon (Người chơi được dân làng cứu về quán, giữ nguyên vàng & danh tiếng)
        /// </summary>
        public void HandlePlayerDefeated()
        {
            Debug.LogWarning("[GameManager] Người chơi bị hạ gục! Dân làng Dun đưa bạn trở về quán.");
            SetState(GameState.Restaurant);
        }
    }
}

