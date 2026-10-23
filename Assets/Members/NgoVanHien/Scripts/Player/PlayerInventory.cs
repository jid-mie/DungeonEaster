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
    /// Gồm 10 ô túi + 5 ô quick item (phím 1-5). Mảng Slots: [0..bag-1] là túi, sau đó là quick item.
    /// Món ăn ưu tiên vào ô quick item, nguyên liệu ưu tiên vào túi.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        [SerializeField, Min(1)] private int bagSlotCount = 10;
        [SerializeField, Min(0)] private int quickSlotCount = 5;

        [Header("Đồ có sẵn khi bắt đầu (mock để debug)")]
        [SerializeField] private List<ItemAmount> startingItems = new List<ItemAmount>();

        private InventorySlot[] slots;

        public int BagSlotCount => bagSlotCount;
        public int QuickSlotCount => quickSlotCount;
        public int SlotCount => bagSlotCount + quickSlotCount;
        public bool IsQuickSlot(int index) => index >= bagSlotCount && index < SlotCount;
        /// <summary>Chỉ số trong Slots của ô quick item thứ k (0-based).</summary>
        public int QuickSlotIndex(int k) => bagSlotCount + k;
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

            slots = CreateSlots(SlotCount);
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
            Add(slots, item, amount, bagSlotCount);
            OnChanged?.Invoke();
            return true;
        }

        public bool TryRemove(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return false;
            if (Count(item) < amount) return false;
            Remove(slots, item, amount, bagSlotCount);
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>Lấy đồ từ đúng 1 ô (vd bấm vào ô để giao món).</summary>
        public bool TryRemoveAt(int index, int amount)
        {
            if (index < 0 || index >= slots.Length || amount <= 0) return false;
            var s = slots[index];
            if (s.IsEmpty || s.count < amount) return false;
            s.count -= amount;
            if (s.count <= 0) s.Clear();
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Chuyển đồ giữa 2 ô: ô đích trống -> chuyển sang; cùng loại -> gộp tới maxStack (dư giữ lại ô cũ);
        /// khác loại -> đổi chỗ.
        /// </summary>
        public bool Move(int from, int to)
        {
            if (from == to || from < 0 || to < 0 || from >= slots.Length || to >= slots.Length) return false;
            var a = slots[from];
            var b = slots[to];
            if (a.IsEmpty) return false;

            if (b.IsEmpty)
            {
                b.item = a.item;
                b.count = a.count;
                a.Clear();
            }
            else if (b.item == a.item)
            {
                int put = Mathf.Min(a.count, a.item.maxStack - b.count);
                if (put <= 0) return false;
                b.count += put;
                a.count -= put;
                if (a.count <= 0) a.Clear();
            }
            else
            {
                var item = a.item;
                int count = a.count;
                a.item = b.item;
                a.count = b.count;
                b.item = item;
                b.count = count;
            }
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
                Remove(copy, r.item, r.amount, bagSlotCount);
            }

            if (add.item != null && add.amount > 0)
            {
                if (SpaceFor(copy, add.item) < add.amount) return null;
                Add(copy, add.item, add.amount, bagSlotCount);
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

        private static void Add(InventorySlot[] target, ItemData item, int amount, int bagCount)
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
            // Món ăn vào quick item trước (mang ra bàn giao nhanh), nguyên liệu vào túi trước
            foreach (int i in SlotOrder(target.Length, bagCount, item.IsDish))
            {
                if (amount <= 0) return;
                var s = target[i];
                if (!s.IsEmpty) continue;
                int put = Mathf.Min(amount, item.maxStack);
                s.item = item;
                s.count = put;
                amount -= put;
            }
        }

        private static IEnumerable<int> SlotOrder(int total, int bagCount, bool quickFirst)
        {
            if (quickFirst)
            {
                for (int i = bagCount; i < total; i++) yield return i;
                for (int i = 0; i < bagCount; i++) yield return i;
            }
            else
            {
                for (int i = 0; i < total; i++) yield return i;
            }
        }

        private static void Remove(InventorySlot[] target, ItemData item, int amount, int bagCount)
        {
            // Lấy trong túi trước (từ ô cuối lên), đồ đã xếp ở quick item lấy sau cùng
            for (int pass = 0; pass < 2 && amount > 0; pass++)
            {
                int from = pass == 0 ? bagCount - 1 : target.Length - 1;
                int to = pass == 0 ? 0 : bagCount;
                for (int i = from; i >= to && amount > 0; i--)
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
}
