using UnityEngine;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Bảng hòm đồ của quán (giao diện dùng chung HudInventoryWindow): ô túi + ô quick item. 2 chế độ:
    /// - Xem (phím I): bấm 1 ô rồi bấm ô khác để chuyển / gộp / đổi chỗ (vd đưa món xuống quick item).
    /// - Phục vụ (F tại bàn khách): ô chứa món khách gọi được viền vàng, bấm vào món để giao cho khách.
    /// </summary>
    public class InventoryPanel : MonoBehaviour
    {
        private TavernUI owner;
        private HudInventoryWindow view;

        private TavernCustomer serveTarget;
        private int selectedIndex = -1;

        public bool IsOpen => view != null && view.IsOpen;
        public TavernCustomer ServeTarget => serveTarget;

        public void Build(TavernUI ui)
        {
            owner = ui;
            var inv = PlayerInventory.Instance;
            var cfg = RestaurantDatabase.Inventory;
            int bagCount = inv != null ? inv.BagSlotCount : cfg.bagSlotCount;
            int quickCount = inv != null ? inv.QuickSlotCount : cfg.quickSlotCount;

            view = HudInventoryWindow.Create(transform, bagCount, quickCount, $"phím 1-{quickCount} để giao nhanh",
                true, owner.GoldIcon, owner.ReputationIcon, OnSlotClicked, Close);
        }

        public void Open()
        {
            serveTarget = null;
            Show();
        }

        public void OpenForServing(TavernCustomer customer)
        {
            serveTarget = customer;
            Show();
        }

        private void Show()
        {
            selectedIndex = -1;
            view.Show();
            Refresh();
            owner.NotifyModalChanged();
        }

        public void Close()
        {
            serveTarget = null;
            selectedIndex = -1;
            view.Hide();
            owner.NotifyModalChanged();
        }

        private void Update()
        {
            if (!IsOpen) return;

            if (serveTarget != null && serveTarget.State != CustomerState.Waiting)
            {
                // khách hết kiên nhẫn bỏ về trong lúc đang chọn món
                Close();
                return;
            }
            Refresh();
        }

        private void Refresh()
        {
            var inv = PlayerInventory.Instance;
            if (inv == null) return;

            bool serving = serveTarget != null;
            ItemData wanted = serving ? serveTarget.Order.dish : null;

            view.SetHeader(serving ? $"PHỤC VỤ BÀN {serveTarget.Table.TableNumber}" : "HÒM ĐỒ",
                $"Đã dùng {inv.UsedSlots}/{inv.SlotCount} ô · mỗi ô tối đa {RestaurantDatabase.Inventory.defaultMaxStack}");

            if (!serving)
            {
                view.SetBanner(null,
                    $"<color=#{UIKit.Hex(UIKit.TextDim)}>Món nấu xong tự vào ô quick item.\nTrước bàn khách: bấm <b>F</b> hoặc phím <b>1-{inv.QuickSlotCount}</b> để giao.</color>");
            }
            else
            {
                int have = inv.Count(wanted);
                string haveLine = have > 0
                    ? $"<color=#{UIKit.Hex(UIKit.Good)}>Có {have} phần — bấm ô viền vàng để giao.</color>"
                    : $"<color=#{UIKit.Hex(UIKit.Bad)}>Chưa có món này — ra bếp nấu trước.</color>";
                view.SetBanner(wanted.icon,
                    $"{serveTarget.Profile.displayName} gọi: <b>{wanted.displayName}</b>\n{haveLine}", serveTarget.PatienceRatio);
            }

            Color pulse = HudInventoryWindow.PulseColor;
            for (int i = 0; i < view.SlotCount; i++)
            {
                var slot = i < inv.Slots.Count ? inv.Slots[i] : null;
                bool empty = slot == null || slot.IsEmpty;
                view.SetSlot(i, empty ? null : slot.item.icon, empty ? 0 : slot.count, empty ? null : slot.item.displayName);

                bool isWanted = serving && !empty && slot.item == wanted;
                view.SetSlotState(i, isWanted || i == selectedIndex, isWanted ? pulse : UIKit.Text, serving && !isWanted);
            }

            view.SetInfo(BuildInfo(inv, serving));
            if (PlayerWallet.Instance != null) view.SetWallet(PlayerWallet.Instance.Gold, PlayerWallet.Instance.Reputation);
        }

        private string BuildInfo(PlayerInventory inv, bool serving)
        {
            var slot = selectedIndex >= 0 && selectedIndex < inv.Slots.Count ? inv.Slots[selectedIndex] : null;
            if (slot != null && !slot.IsEmpty)
            {
                var item = slot.item;
                return $"<b>{item.displayName}</b>  <color=#{UIKit.Hex(UIKit.TextDim)}>· {item.CategoryLabel} · có {inv.Count(item)}</color>\n" +
                       $"{item.description}\n<color=#{UIKit.Hex(UIKit.Highlight)}>Bấm ô khác để chuyển / đổi chỗ.</color>";
            }
            if (serving) return "Chọn đúng món khách gọi. Các ô mờ là nguyên liệu/món khác, khách không nhận.";
            return "Bấm 1 ô để chọn, rồi bấm ô khác để chuyển, gộp hoặc đổi chỗ (vd đưa món xuống quick item).";
        }

        private void OnSlotClicked(int index)
        {
            var inv = PlayerInventory.Instance;
            if (inv == null || index >= inv.Slots.Count) return;
            var slot = inv.Slots[index];

            if (serveTarget != null)
            {
                if (slot.IsEmpty) return;
                var customer = serveTarget;
                var item = slot.item;
                var outcome = customer.TryServe(item, index);
                owner.ReportServe(outcome, customer, item);
                if (outcome.result == ServeResult.Served || outcome.result == ServeResult.NotWaiting) Close();
                return;
            }

            // chế độ xem: chọn ô nguồn -> bấm ô đích để chuyển / gộp / đổi chỗ
            if (selectedIndex < 0)
            {
                selectedIndex = slot.IsEmpty ? -1 : index;
            }
            else if (selectedIndex == index)
            {
                selectedIndex = -1;
            }
            else
            {
                inv.Move(selectedIndex, index);
                selectedIndex = -1;
            }
            Refresh();
        }
    }
}
