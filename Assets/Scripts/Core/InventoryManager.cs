using UnityEngine;
using System;
using System.Collections.Generic;

namespace DungeonEater.Core
{
    public enum IngredientType
    {
        MonsterMeat,    // Thịt quái dã thú (từ Slime, quái thường)
        CaveMushroom,   // Nấm hang sâu (từ Nấm Spore)
        BombCore,       // Lõi bọ phát nổ (từ Bomb Beetle)
        BossRib         // Sườn Vua Thú (từ Boss King Boar)
    }

    /// <summary>
    /// Quản lý túi đồ (Balo), giới hạn slot chứa đồ và nguyên liệu nấu ăn
    /// thu thập từ hầm ngục theo thiết kế GDD.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Backpack Settings")]
        [SerializeField] private int maxCapacity = 10;
        public int MaxCapacity => maxCapacity;

        // Lưu trữ số lượng từng loại nguyên liệu
        private Dictionary<IngredientType, int> items = new Dictionary<IngredientType, int>();

        public event Action OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Khởi tạo các loại item
            foreach (IngredientType type in Enum.GetValues(typeof(IngredientType)))
            {
                items[type] = 0;
            }

            // Cho sẵn 2 thịt và 2 nấm để người chơi có thể thử nấu ngay khi vào quán
            items[IngredientType.MonsterMeat] = 2;
            items[IngredientType.CaveMushroom] = 2;
        }

        public int GetTotalItemCount()
        {
            int total = 0;
            foreach (var kvp in items)
            {
                total += kvp.Value;
            }
            return total;
        }

        public int GetItemCount(IngredientType type)
        {
            return items.ContainsKey(type) ? items[type] : 0;
        }

        public bool AddItem(IngredientType type, int amount = 1)
        {
            if (GetTotalItemCount() + amount > maxCapacity)
            {
                Debug.LogWarning($"[InventoryManager] Balo đã đầy ({GetTotalItemCount()}/{maxCapacity})! Không thể nhặt thêm {type}.");
                return false;
            }

            items[type] = GetItemCount(type) + amount;
            Debug.Log($"[InventoryManager] Đã thêm {amount}x {type}. Hiện có: {items[type]}");
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool ConsumeItem(IngredientType type, int amount = 1)
        {
            int current = GetItemCount(type);
            if (current < amount) return false;

            items[type] = current - amount;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void IncreaseCapacity(int additionalSlots)
        {
            maxCapacity += additionalSlots;
            Debug.Log($"[InventoryManager] Sức chứa Balo được nâng cấp lên: {maxCapacity} ô.");
            OnInventoryChanged?.Invoke();
        }

        public string GetItemDisplayName(IngredientType type)
        {
            switch (type)
            {
                case IngredientType.MonsterMeat: return "Thịt quái dã thú";
                case IngredientType.CaveMushroom: return "Nấm hang sâu";
                case IngredientType.BombCore: return "Lõi bọ phát nổ";
                case IngredientType.BossRib: return "Sườn Vua Thú (Hiếm)";
                default: return type.ToString();
            }
        }
    }
}

