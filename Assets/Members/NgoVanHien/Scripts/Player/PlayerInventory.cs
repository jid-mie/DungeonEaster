using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonEater.Tavern
{
    [Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;

        public bool IsEmpty => item == null || count <= 0;

        public void Clear()
        {
            item = null;
            count = 0;
        }
    }

    /// <summary>
    /// Hòm đồ (Balo) dạng ô: mỗi ô chứa 1 loại vật phẩm, xếp chồng tới ItemData.maxStack.
    /// GDD: "Balo có giới hạn slot; người chơi phải bỏ hoặc ưu tiên loot khi đầy" -> mọi thao tác thêm
    /// đều all-or-nothing, không bao giờ âm thầm xóa đồ.
    /// Chứa cả nguyên liệu lẫn món đã nấu (món nấu xong vào hòm đồ, mang ra bàn phục vụ).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        [SerializeField, Min(1)] private int slotCount = 16;

        [Header("Đồ có sẵn khi bắt đầu (mock để debug)")]
        [SerializeField] private List<ItemAmount> startingItems = new List<ItemAmount>();

        private InventorySlot[] slots;

        public int SlotCount => slotCount;
        public IReadOnlyList<InventorySlot> Slots => slots;
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

            slots = CreateSlots(slotCount);
            foreach (var start in startingItems)
            {
                if (start.item == null) continue;
                if (!TryAdd(start.item, start.amount))
                {
                    Debug.LogWarning($"[PlayerInventory] Không đủ ô để thêm đồ khởi đầu {start.amount}x {start.item.displayName}.");
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public int UsedSlots
        {
            get
            {
                int used = 0;
                foreach (var s in slots) if (!s.IsEmpty) used++;
                return used;
            }
        }

        public int Count(ItemData item)
        {
            int total = 0;
            foreach (var s in slots)
            {
                if (s.item == item) total += s.count;
            }
            return total;
        }

        /// <summary>Số lượng tối đa của item còn nhét vừa hòm đồ.</summary>
        public int SpaceFor(ItemData item)
        {
            return SpaceFor(slots, item);
        }

        public bool TryAdd(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return false;
            if (SpaceFor(item) < amount) return false;
            Add(slots, item, amount);
            OnChanged?.Invoke();
            return true;
        }

        public bool TryRemove(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return false;
            if (Count(item) < amount) return false;
            Remove(slots, item, amount);
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Kiểm tra đổi đồ (vd nấu ăn: trừ nguyên liệu, thêm món). Tính trên bản sao vì nguyên liệu
        /// dùng hết có thể giải phóng ô cho món mới.
        /// </summary>
        public bool CanExchange(IList<ItemAmount> remove, ItemAmount add)
        {
            return Simulate(remove, add) != null;
        }

        public bool TryExchange(IList<ItemAmount> remove, ItemAmount add)
        {
            var result = Simulate(remove, add);
            if (result == null) return false;
            slots = result;
            OnChanged?.Invoke();
            return true;
        }

        private InventorySlot[] Simulate(IList<ItemAmount> remove, ItemAmount add)
        {
            var copy = CreateSlots(slots.Length);
            for (int i = 0; i < slots.Length; i++)
            {
                copy[i].item = slots[i].item;
                copy[i].count = slots[i].count;
            }

            foreach (var r in remove)
            {
                if (r.item == null) continue;
                int have = 0;
                foreach (var s in copy) if (s.item == r.item) have += s.count;
                if (have < r.amount) return null;
                Remove(copy, r.item, r.amount);
            }

            if (add.item != null && add.amount > 0)
            {
                if (SpaceFor(copy, add.item) < add.amount) return null;
                Add(copy, add.item, add.amount);
            }
            return copy;
        }

        private static InventorySlot[] CreateSlots(int count)
        {
            var result = new InventorySlot[count];
            for (int i = 0; i < count; i++) result[i] = new InventorySlot();
            return result;
        }

        private static int SpaceFor(InventorySlot[] target, ItemData item)
        {
            if (item == null) return 0;
            int space = 0;
            foreach (var s in target)
            {
                if (s.IsEmpty) space += item.maxStack;
                else if (s.item == item) space += Mathf.Max(0, item.maxStack - s.count);
            }
            return space;
        }

        private static void Add(InventorySlot[] target, ItemData item, int amount)
        {
            // Lấp các ô đang chứa cùng loại trước, sau đó mới mở ô trống
            foreach (var s in target)
            {
                if (amount <= 0) return;
                if (s.IsEmpty || s.item != item) continue;
                int put = Mathf.Min(amount, item.maxStack - s.count);
                s.count += put;
                amount -= put;
            }
            foreach (var s in target)
            {
                if (amount <= 0) return;
                if (!s.IsEmpty) continue;
                int put = Mathf.Min(amount, item.maxStack);
                s.item = item;
                s.count = put;
                amount -= put;
            }
        }

        private static void Remove(InventorySlot[] target, ItemData item, int amount)
        {
            // Lấy từ ô cuối lên để các ô đầu giữ nguyên vị trí
            for (int i = target.Length - 1; i >= 0 && amount > 0; i--)
            {
                var s = target[i];
                if (s.item != item) continue;
                int take = Mathf.Min(amount, s.count);
                s.count -= take;
                amount -= take;
                if (s.count <= 0) s.Clear();
            }
        }
    }
}
