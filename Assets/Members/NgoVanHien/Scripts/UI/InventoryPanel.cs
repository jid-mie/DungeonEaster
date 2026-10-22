using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Bảng hòm đồ. 2 chế độ:
    /// - Xem (phím I): xem nguyên liệu/món đang có, vàng, danh tiếng.
    /// - Phục vụ (F tại bàn khách): ô chứa món khách gọi được viền vàng, bấm vào món để giao cho khách.
    /// Đặt lệch phải màn hình để không che khách/order (guideline: tránh popup lớn chặn order khi đang ca).
    /// </summary>
    public class InventoryPanel : MonoBehaviour
    {
        private const int Columns = 4;
        private const float SlotSize = 100f;
        private const float SlotGap = 12f;

        private TavernUI owner;
        private RectTransform window;
        private Text title;
        private Text subtitle;
        private RectTransform serveBanner;
        private Image serveIcon;
        private Text serveText;
        private Image servePatience;
        private RectTransform grid;
        private Text infoText;
        private Text goldText;
        private Text repText;
        private readonly List<SlotView> slotViews = new List<SlotView>();

        private TavernCustomer serveTarget;
        private int selectedIndex = -1;

        public bool IsOpen => window != null && window.gameObject.activeSelf;

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

            int slotCount = PlayerInventory.Instance != null ? PlayerInventory.Instance.SlotCount : 16;
            int rows = Mathf.CeilToInt(slotCount / (float)Columns);
            float gridWidth = Columns * SlotSize + (Columns - 1) * SlotGap;
            float gridHeight = rows * SlotSize + (rows - 1) * SlotGap;
            float width = gridWidth + 64f;
            float height = 96f + 92f + gridHeight + 24f + 110f + 64f;

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

            // --- banner phục vụ
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

            // --- lưới ô
            grid = UIKit.Rect("Grid", window);
            UIKit.Place(grid, new Vector2(0.5f, 1f), new Vector2(0f, -196f), new Vector2(gridWidth, gridHeight));
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(SlotSize, SlotSize);
            layout.spacing = new Vector2(SlotGap, SlotGap);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = Columns;
            for (int i = 0; i < slotCount; i++) slotViews.Add(CreateSlot(grid, i));

            // --- thông tin
            var info = UIKit.Panel(window, "Info", UIKit.WoodDark);
            UIKit.Place(info.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(gridWidth, 110f));
            infoText = UIKit.Label(info.transform, "Text", "", 19, UIKit.Text, TextAnchor.UpperLeft);
            UIKit.Stretch(infoText.rectTransform, 14, 8, 14, 10);

            // --- vàng & danh tiếng
            goldText = FooterValue(window, owner.GoldIcon, UIKit.Gold, 32f);
            repText = FooterValue(window, owner.ReputationIcon, UIKit.Rep, 32f + gridWidth * 0.5f);

            window.gameObject.SetActive(false);
        }

        private static Text FooterValue(RectTransform parent, Sprite icon, Color color, float x)
        {
            var img = UIKit.Icon(parent, "FooterIcon", icon);
            UIKit.Place(img.rectTransform, new Vector2(0f, 0f), new Vector2(x, 18f), new Vector2(40f, 40f));
            var t = UIKit.Label(parent, "FooterValue", "", 26, color, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIKit.Place(t.rectTransform, new Vector2(0f, 0f), new Vector2(x + 48f, 18f), new Vector2(200f, 40f));
            return t;
        }

        private SlotView CreateSlot(RectTransform parent, int index)
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
            UIKit.Stretch(view.icon.rectTransform, 12, 12, 12, 12);

            view.count = UIKit.Label(rt, "Count", "", 22, UIKit.Text, TextAnchor.LowerRight, FontStyle.Bold);
            UIKit.Stretch(view.count.rectTransform, 4, 2, 8, 4);
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
            subtitle.text = $"Đã dùng {inv.UsedSlots}/{inv.SlotCount} ô";

            servePatience.transform.parent.gameObject.SetActive(serving);
            // chế độ xem không có icon -> kéo chữ sang trái cho gọn
            float textX = serving ? 86f : 14f;
            serveText.rectTransform.anchoredPosition = new Vector2(textX, -8f);
            serveText.rectTransform.sizeDelta = new Vector2(serveBanner.rect.width - textX - 14f, 50f);
            if (!serving)
            {
                serveIcon.enabled = false;
                serveText.text = $"<color=#{UIKit.Hex(UIKit.TextDim)}>Món nấu ở bếp sẽ vào hòm đồ.\nRa bàn khách bấm <b>F</b> để chọn món phục vụ.</color>";
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
                view.highlight.color = isWanted ? new Color(UIKit.Highlight.r, UIKit.Highlight.g, UIKit.Highlight.b, pulse) : UIKit.TextDim;
                view.frame.color = empty ? new Color(UIKit.Slot.r, UIKit.Slot.g, UIKit.Slot.b, 0.6f) : UIKit.Slot;
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
                return $"<b>{item.displayName}</b>  <color=#{UIKit.Hex(UIKit.TextDim)}>· {item.CategoryLabel} · có {inv.Count(item)}</color>\n{item.description}";
            }
            if (serving) return "Chọn đúng món khách gọi. Các ô mờ là nguyên liệu/món khác, khách không nhận.";
            return "Bấm vào 1 ô để xem thông tin. Món nấu ở bếp sẽ được cho vào đây, mang ra bàn bấm F để phục vụ.";
        }

        private void OnSlotClicked(int index)
        {
            var inv = PlayerInventory.Instance;
            if (inv == null || index >= inv.Slots.Count) return;
            var slot = inv.Slots[index];
            selectedIndex = slot.IsEmpty ? -1 : index;

            if (serveTarget == null || slot.IsEmpty) return;

            var customer = serveTarget;
            var outcome = customer.TryServe(slot.item);
            switch (outcome.result)
            {
                case ServeResult.Served:
                    string tip = outcome.tip > 0 ? $" (gồm {outcome.tip} tiền boa)" : "";
                    owner.ShowToast($"Đã phục vụ {customer.Order.DisplayName} cho bàn {customer.Table.TableNumber}: +{outcome.gold + outcome.tip} vàng{tip}, +{outcome.reputation} danh tiếng.", UIKit.Good);
                    Close();
                    break;
                case ServeResult.WrongDish:
                    owner.ShowToast($"Khách không gọi {slot.item.displayName}! Khách muốn {customer.Order.DisplayName}.", UIKit.Bad);
                    break;
                case ServeResult.DishNotInInventory:
                    owner.ShowToast("Không còn món này trong hòm đồ.", UIKit.Bad);
                    break;
                case ServeResult.NotWaiting:
                    Close();
                    break;
            }
        }
    }
}
