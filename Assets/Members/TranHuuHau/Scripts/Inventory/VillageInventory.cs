using System;
using System.Collections.Generic;
using UnityEngine;
using DungeonEater.TranHuuHau.Data;

namespace DungeonEater.TranHuuHau.Inventory
{
    [Serializable]
    public class InventorySlot
    {
        public VillageItemData item;
        public int count;

        public bool IsEmpty => item == null || count <= 0;

        public void Clear()
        {
            item = null;
            count = 0;
        }
    }

    /// <summary>
    /// Hệ thống Hòm đồ (Balo) và Thanh Quick Item theo chuẩn GDD mục 3.2.1:
    /// - Mặc định có 10 ô hành trang (backpack slots).
    /// - Mỗi ô chứa tối đa 5 stacks đồ.
    /// - Thanh Quick Item có 4 ô vĩnh viễn, cho phép gán đồ từ hòm đồ và tháo gỡ.
    /// - Có sẵn các vật phẩm mock để test và debug ngay khi vào làng.
    /// </summary>
    public class VillageInventory : MonoBehaviour
    {
        public static VillageInventory Instance { get; private set; }

        public const int DefaultSlotCount = 10;
        public const int QuickSlotCount = 4;
        public const int MaxStackLimit = 5;

        [Header("Danh sách ô chứa đồ (10 ô)")]
        [SerializeField] private InventorySlot[] slots = new InventorySlot[DefaultSlotCount];

        [Header("Thanh Quick Item (4 ô)")]
        [SerializeField] private int[] quickSlotBackpackIndices = new int[QuickSlotCount];

        public IReadOnlyList<InventorySlot> Slots => slots;
        public IReadOnlyList<int> QuickSlotIndices => quickSlotBackpackIndices;

        public event Action OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeSlots();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void InitializeSlots()
        {
            if (slots == null || slots.Length != DefaultSlotCount)
            {
                slots = new InventorySlot[DefaultSlotCount];
            }

            for (int i = 0; i < DefaultSlotCount; i++)
            {
                if (slots[i] == null) slots[i] = new InventorySlot();
            }

            if (quickSlotBackpackIndices == null || quickSlotBackpackIndices.Length != QuickSlotCount)
            {
                quickSlotBackpackIndices = new int[QuickSlotCount];
            }

            for (int i = 0; i < QuickSlotCount; i++)
            {
                quickSlotBackpackIndices[i] = -1; // -1 nghĩa là chưa gán
            }

            // Gán các item mock ban đầu để debug
            SeedMockItems();
        }

        private void SeedMockItems()
        {
            // Slot 0: 3 Bình Máu
            var potion = VillageItemData.GetItem("potion_heal");
            if (potion != null) slots[0] = new InventorySlot { item = potion, count = 3 };

            // Slot 1: 4 Nấm Hang
            var mush = VillageItemData.GetItem("mat_mushroom");
            if (mush != null) slots[1] = new InventorySlot { item = mush, count = 4 };

            // Slot 2: 3 Mẩu Xương
            var bone = VillageItemData.GetItem("mat_bone");
            if (bone != null) slots[2] = new InventorySlot { item = bone, count = 3 };

            // Slot 3: 2 Chất Nhầy Dẻo
            var slime = VillageItemData.GetItem("mat_slime");
            if (slime != null) slots[3] = new InventorySlot { item = slime, count = 2 };

            // Gán Quick Item 1 trỏ vào Bình Máu (slot 0)
            quickSlotBackpackIndices[0] = 0;
        }

        public int GetUsedSlotCount()
        {
            int used = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty) used++;
            }
            return used;
        }

        public bool CanAddItem(VillageItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            int remaining = amount;
            // 1. Kiểm tra các slot đã có item cùng loại còn chỗ stack (< MaxStackLimit)
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty && slots[i].item.id == item.id)
                {
                    int space = Mathf.Min(item.maxStack, MaxStackLimit) - slots[i].count;
                    if (space > 0)
                    {
                        remaining -= space;
                        if (remaining <= 0) return true;
                    }
                }
            }

            // 2. Kiểm tra các slot trống
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty)
                {
                    remaining -= Mathf.Min(item.maxStack, MaxStackLimit);
                    if (remaining <= 0) return true;
                }
            }

            return false;
        }

        public bool AddItem(VillageItemData item, int amount = 1)
        {
            if (!CanAddItem(item, amount)) return false;

            int remaining = amount;
            int maxPerSlot = Mathf.Min(item.maxStack, MaxStackLimit);

            // Gộp vào các ô đang có
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty && slots[i].item.id == item.id && slots[i].count < maxPerSlot)
                {
                    int add = Mathf.Min(maxPerSlot - slots[i].count, remaining);
                    slots[i].count += add;
                    remaining -= add;
                    if (remaining <= 0) break;
                }
            }

            // Đặt vào ô mới
            if (remaining > 0)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i].IsEmpty)
                    {
                        int add = Mathf.Min(maxPerSlot, remaining);
                        slots[i].item = item;
                        slots[i].count = add;
                        remaining -= add;
                        if (remaining <= 0) break;
                    }
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool RemoveItem(int slotIndex, int amount = 1)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return false;
            var slot = slots[slotIndex];
            if (slot.IsEmpty || slot.count < amount) return false;

            slot.count -= amount;
            if (slot.count <= 0)
            {
                slot.Clear();
                // Nếu slot này được gán quick item thì xóa gán
                for (int q = 0; q < QuickSlotCount; q++)
                {
                    if (quickSlotBackpackIndices[q] == slotIndex)
                    {
                        quickSlotBackpackIndices[q] = -1;
                    }
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool SetQuickSlot(int quickIndex, int backpackSlotIndex)
        {
            if (quickIndex < 0 || quickIndex >= QuickSlotCount) return false;
            if (backpackSlotIndex < 0 || backpackSlotIndex >= slots.Length) return false;
            if (slots[backpackSlotIndex].IsEmpty) return false;

            // Xóa gán cũ nếu đã ở quick slot khác
            for (int i = 0; i < QuickSlotCount; i++)
            {
                if (quickSlotBackpackIndices[i] == backpackSlotIndex)
                {
                    quickSlotBackpackIndices[i] = -1;
                }
            }

            quickSlotBackpackIndices[quickIndex] = backpackSlotIndex;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void ClearQuickSlot(int quickIndex)
        {
            if (quickIndex >= 0 && quickIndex < QuickSlotCount)
            {
                quickSlotBackpackIndices[quickIndex] = -1;
                OnInventoryChanged?.Invoke();
            }
        }

        public InventorySlot GetQuickSlotItem(int quickIndex)
        {
            if (quickIndex < 0 || quickIndex >= QuickSlotCount) return null;
            int bpIndex = quickSlotBackpackIndices[quickIndex];
            if (bpIndex < 0 || bpIndex >= slots.Length) return null;
            return slots[bpIndex];
        }
    }
}
