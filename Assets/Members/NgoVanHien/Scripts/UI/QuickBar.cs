using UnityEngine;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Thanh quick item của quán (giao diện dùng chung HudQuickBarView): hiện các ô quick item của hòm đồ
    /// kèm phím số. Ô chứa món mà khách ở bàn trước mặt đang gọi được viền vàng.
    /// </summary>
    public class QuickBar : MonoBehaviour
    {
        private TavernUI owner;
        private HudQuickBarView view;

        public void Build(TavernUI ui)
        {
            owner = ui;
            int count = PlayerInventory.Instance != null ? PlayerInventory.Instance.QuickSlotCount : RestaurantDatabase.Inventory.quickSlotCount;
            view = HudQuickBarView.Create(transform, count);
        }

        /// <summary>Nháy sáng ô k khi người chơi bấm phím tương ứng.</summary>
        public void Flash(int k)
        {
            view.Flash(k);
        }

        private void Update()
        {
            var inv = PlayerInventory.Instance;
            if (inv == null) return;

            // món khách trước mặt đang gọi (nếu có) -> viền vàng ô tương ứng
            ItemData wanted = null;
            var facingTable = owner.FindFacingInteractable(out _) as TavernTable;
            var customer = facingTable != null ? facingTable.Customer : null;
            if (customer != null && customer.State == CustomerState.Waiting) wanted = customer.Order.dish;

            for (int k = 0; k < view.Count; k++)
            {
                int index = inv.QuickSlotIndex(k);
                var slot = index < inv.Slots.Count ? inv.Slots[index] : null;
                bool empty = slot == null || slot.IsEmpty;
                view.SetSlot(k, empty ? null : slot.item.icon, empty ? 0 : slot.count, empty ? null : slot.item.displayName);
                view.SetWanted(k, !empty && wanted != null && slot.item == wanted);
            }
        }
    }
}
