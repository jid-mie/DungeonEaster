using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Bảng hòm đồ: 10 ô túi + 5 ô quick item. 2 chế độ:
    /// - Xem (phím I): bấm 1 ô rồi bấm ô khác để chuyển / gộp / đổi chỗ (vd đưa món xuống quick item).
    /// - Phục vụ (F tại bàn khách): ô chứa món khách gọi được viền vàng, bấm vào món để giao cho khách.
    /// Đặt lệch phải màn hình để không che khách/order (guideline: tránh popup lớn chặn order khi đang ca).
    /// </summary>
    public class InventoryPanel : MonoBehaviour
    {
        private const int Columns = 5;
        private const float SlotSize = 88f;
        private const float SlotGap = 10f;

        private TavernUI owner;
        private RectTransform window;
        private Text title;
        private Text subtitle;
        private RectTransform serveBanner;
        private Image serveIcon;
        private Text serveText;
        private Image servePatience;
        private Text infoText;
        private Text goldText;
        private Text repText;
        private readonly List<SlotView> slotViews = new List<SlotView>();

        private TavernCustomer serveTarget;
        private int selectedIndex = -1;

        public bool IsOpen => window != null && window.gameObject.activeSelf;
        public TavernCustomer ServeTarget => serveTarget;

        private class SlotView
        {
            public Image frame;
            public Image highlight;
            public Image icon;
            public Text count;
        }

        public void Build(TavernUI ui)
        {
            owner = ui;
            var self = (RectTransform)transform;

            var inv = PlayerInventory.Instance;
            int bagCount = inv != null ? inv.BagSlotCount : 10;
            int quickCount = inv != null ? inv.QuickSlotCount : 5;
            int bagRows = Mathf.CeilToInt(bagCount / (float)Columns);
            float gridWidth = Columns * SlotSize + (Columns - 1) * SlotGap;
            float bagHeight = bagRows * SlotSize + (bagRows - 1) * SlotGap;
            float width = gridWidth + 64f;

            // vị trí từ trên xuống
            float bagLabelY = -192f;
            float bagY = bagLabelY - 28f;
            float quickLabelY = bagY - bagHeight - 14f;
            float quickY = quickLabelY - 28f;
            float infoY = quickY - SlotSize - 16f;
            const float infoHeight = 96f;
            float height = -infoY + infoHeight + 76f;

            window = UIKit.Panel(self, "Window", UIKit.Wood).rectTransform;
            UIKit.Place(window, new Vector2(1f, 0.5f), new Vector2(-40f, 20f), new Vector2(width, height));
            // chặn click xuyên qua
            window.GetComponent<Image>().raycastTarget = true;

            title = UIKit.Label(window, "Title", "HÒM ĐỒ", 36, UIKit.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UIKit.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(32f, -18f), new Vector2(width - 120f, 44f));
            subtitle = UIKit.Label(window, "Subtitle", "", 20, UIKit.TextDim, TextAnchor.UpperLeft);
            UIKit.Place(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(32f, -62f), new Vector2(width - 120f, 26f));

            var close = UIKit.Button(window, "Close", "X", 26, UIKit.ButtonRed, Close);
            UIKit.Place((RectTransform)close.transform, new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(52f, 52f));

            // --- banner phục vụ / hướng dẫn
            serveBanner = UIKit.Panel(window, "ServeBanner", UIKit.WoodDark).rectTransform;
            UIKit.Place(serveBanner, new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(gridWidth, 80f));
            serveIcon = UIKit.Icon(serveBanner, "Icon", null);
            UIKit.Place(serveIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(64f, 64f));
            serveText = UIKit.Label(serveBanner, "Text", "", 20, UIKit.Text, TextAnchor.UpperLeft);
            UIKit.Place(serveText.rectTransform, new Vector2(0f, 1f), new Vector2(86f, -8f), new Vector2(gridWidth - 100f, 50f));
            var barBack = UIKit.Box(serveBanner, "PatienceBack", new Color(0.13f, 0.08f, 0.05f));
            UIKit.Place(barBack.rectTransform, new Vector2(0f, 0f), new Vector2(86f, 10f), new Vector2(gridWidth - 104f, 10f));
            servePatience = UIKit.Box(barBack.transform, "Fill", UIKit.Good);
            servePatience.rectTransform.anchorMin = Vector2.zero;
            servePatience.rectTransform.anchorMax = Vector2.one;
            servePatience.rectTransform.pivot = new Vector2(0f, 0.5f);
            servePatience.rectTransform.offsetMin = servePatience.rectTransform.offsetMax = Vector2.zero;

            // --- túi đồ
            SectionLabel("BagLabel", "TÚI ĐỒ", bagLabelY);
            var bagGrid = CreateGrid("BagGrid", bagY, gridWidth, bagHeight);
            for (int i = 0; i < bagCount; i++) slotViews.Add(CreateSlot(bagGrid, i, null));

            // --- quick item
            SectionLabel("QuickLabel", $"QUICK ITEM  <color=#{UIKit.Hex(UIKit.TextDim)}>· phím 1-{quickCount} để giao nhanh</color>", quickLabelY);
            var quickGrid = CreateGrid("QuickGrid", quickY, gridWidth, SlotSize);
            for (int k = 0; k < quickCount; k++) slotViews.Add(CreateSlot(quickGrid, bagCount + k, (k + 1).ToString()));

            // --- thông tin
            var info = UIKit.Panel(window, "Info", UIKit.WoodDark);
            UIKit.Place(info.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, infoY), new Vector2(gridWidth, infoHeight));
            infoText = UIKit.Label(info.transform, "Text", "", 19, UIKit.Text, TextAnchor.UpperLeft);
            UIKit.Stretch(infoText.rectTransform, 14, 8, 14, 10);

            // --- vàng & danh tiếng
            goldText = FooterValue(window, owner.GoldIcon, UIKit.Gold, 32f);
            repText = FooterValue(window, owner.ReputationIcon, UIKit.Rep, 32f + gridWidth * 0.5f);

            window.gameObject.SetActive(false);
        }

        private void SectionLabel(string objName, string text, float y)
        {
            var label = UIKit.Label(window, objName, text, 19, UIKit.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UIKit.Place(label.rectTransform, new Vector2(0f, 1f), new Vector2(32f, y), new Vector2(window.sizeDelta.x - 64f, 24f));
        }

        private RectTransform CreateGrid(string objName, float y, float width, float height)
        {
            var grid = UIKit.Rect(objName, window);
            UIKit.Place(grid, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(width, height));
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(SlotSize, SlotSize);
            layout.spacing = new Vector2(SlotGap, SlotGap);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = Columns;
            return grid;
        }

        private static Text FooterValue(RectTransform parent, Sprite icon, Color color, float x)
        {
            var img = UIKit.Icon(parent, "FooterIcon", icon);
            UIKit.Place(img.rectTransform, new Vector2(0f, 0f), new Vector2(x, 18f), new Vector2(40f, 40f));
            var t = UIKit.Label(parent, "FooterValue", "", 26, color, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIKit.Place(t.rectTransform, new Vector2(0f, 0f), new Vector2(x + 48f, 18f), new Vector2(200f, 40f));
            return t;
        }

        private SlotView CreateSlot(RectTransform parent, int index, string keyLabel)
        {
            var view = new SlotView();
            var rt = UIKit.Rect("Slot" + index, parent);

            view.highlight = UIKit.Panel(rt, "Highlight", UIKit.Highlight);
            UIKit.Stretch(view.highlight.rectTransform, -6, -6, -6, -6);
            view.highlight.raycastTarget = false;

            view.frame = UIKit.Panel(rt, "Frame", UIKit.Slot);
            UIKit.Stretch(view.frame.rectTransform, 0, 0, 0, 0);
            var button = view.frame.gameObject.AddComponent<Button>();
            button.targetGraphic = view.frame;
            int captured = index;
            button.onClick.AddListener(() => OnSlotClicked(captured));

            view.icon = UIKit.Icon(rt, "Icon", null);
            UIKit.Stretch(view.icon.rectTransform, 10, 10, 10, 10);

            view.count = UIKit.Label(rt, "Count", "", 21, UIKit.Text, TextAnchor.LowerRight, FontStyle.Bold);
            UIKit.Stretch(view.count.rectTransform, 4, 2, 8, 4);

            if (keyLabel != null)
            {
                var key = UIKit.Label(rt, "Key", keyLabel, 18, UIKit.Highlight, TextAnchor.UpperLeft, FontStyle.Bold);
                UIKit.Stretch(key.rectTransform, 8, 4, 4, 4);
            }
            return view;
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
            window.gameObject.SetActive(true);
            Refresh();
            owner.NotifyModalChanged();
        }

        public void Close()
        {
            serveTarget = null;
            selectedIndex = -1;
            window.gameObject.SetActive(false);
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

            title.text = serving ? $"PHỤC VỤ BÀN {serveTarget.Table.TableNumber}" : "HÒM ĐỒ";
            subtitle.text = $"Đã dùng {inv.UsedSlots}/{inv.SlotCount} ô · mỗi ô tối đa 5";

            servePatience.transform.parent.gameObject.SetActive(serving);
            // chế độ xem không có icon -> kéo chữ sang trái cho gọn
            float textX = serving ? 86f : 14f;
            serveText.rectTransform.anchoredPosition = new Vector2(textX, -8f);
            serveText.rectTransform.sizeDelta = new Vector2(serveBanner.rect.width - textX - 14f, 50f);
            if (!serving)
            {
                serveIcon.enabled = false;
                serveText.text = $"<color=#{UIKit.Hex(UIKit.TextDim)}>Món nấu xong tự vào ô quick item.\nTrước bàn khách: bấm <b>F</b> hoặc phím <b>1-5</b> để giao.</color>";
            }
            else
            {
                int have = inv.Count(wanted);
                serveIcon.sprite = wanted.icon;
                serveIcon.enabled = wanted.icon != null;
                string haveLine = have > 0
                    ? $"<color=#{UIKit.Hex(UIKit.Good)}>Có {have} phần — bấm ô viền vàng để giao.</color>"
                    : $"<color=#{UIKit.Hex(UIKit.Bad)}>Chưa có món này — ra bếp nấu trước.</color>";
                serveText.text = $"{serveTarget.Profile.displayName} gọi: <b>{wanted.displayName}</b>\n{haveLine}";
                float r = serveTarget.PatienceRatio;
                servePatience.rectTransform.anchorMax = new Vector2(r, 1f);
                servePatience.color = r > 0.5f ? UIKit.Good : (r > 0.25f ? UIKit.Highlight : UIKit.Bad);
            }

            float pulse = 0.65f + 0.35f * Mathf.PingPong(Time.unscaledTime * 2f, 1f);
            for (int i = 0; i < slotViews.Count; i++)
            {
                var view = slotViews[i];
                var slot = i < inv.Slots.Count ? inv.Slots[i] : null;
                bool empty = slot == null || slot.IsEmpty;

                view.icon.enabled = !empty && slot.item.icon != null;
                if (!empty) view.icon.sprite = slot.item.icon;
                view.count.text = empty ? "" : slot.count.ToString();

                bool isWanted = serving && !empty && slot.item == wanted;
                bool dimmed = serving && !empty && !isWanted;
                view.icon.color = dimmed ? new Color(1f, 1f, 1f, 0.35f) : Color.white;
                view.highlight.enabled = isWanted || i == selectedIndex;
                view.highlight.color = isWanted ? new Color(UIKit.Highlight.r, UIKit.Highlight.g, UIKit.Highlight.b, pulse) : UIKit.Text;
                Color frameColor = inv.IsQuickSlot(i) ? UIKit.WoodLight : UIKit.Slot;
                view.frame.color = empty ? new Color(frameColor.r, frameColor.g, frameColor.b, 0.6f) : frameColor;
            }

            infoText.text = BuildInfo(inv, serving);

            if (PlayerWallet.Instance != null)
            {
                goldText.text = UIKit.FormatNumber(PlayerWallet.Instance.Gold);
                repText.text = UIKit.FormatNumber(PlayerWallet.Instance.Reputation);
            }
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
                var outcome = customer.TryServe(slot.item, index);
                owner.ReportServe(outcome, customer, slot.item);
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
