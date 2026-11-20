using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Cửa sổ hòm đồ dùng chung cho 3 scene (style gốc: hòm đồ quán của Hiến): lưới túi đồ + hàng quick item,
    /// banner hướng dẫn, khung thông tin, (tùy scene) vàng + danh tiếng. Đặt lệch phải để không che gameplay.
    /// Chỉ lo hiển thị: scene nào dùng thì tự đẩy dữ liệu ô và xử lý bấm ô qua callback.
    /// Chỉ số ô: [0..bagCount-1] là túi, tiếp theo là quick item.
    /// </summary>
    public class HudInventoryWindow : MonoBehaviour
    {
        private const int Columns = 5;
        private const float SlotSize = 88f;
        private const float SlotGap = 10f;

        private RectTransform window;
        private Text title;
        private Text subtitle;
        private RectTransform banner;
        private Image bannerIcon;
        private Text bannerText;
        private RectTransform bannerBarBack;
        private Image bannerBar;
        private Text infoText;
        private Text goldText;
        private Text repText;
        private int bagCount;
        private readonly List<HudSlotView> slotViews = new List<HudSlotView>();
        private readonly List<bool> slotEmpty = new List<bool>();

        public bool IsOpen => window != null && window.gameObject.activeSelf;
        public int BagCount => bagCount;
        public int SlotCount => slotViews.Count;

        /// <summary>Màu viền vàng nhấp nháy cho ô cần chú ý.</summary>
        public static Color PulseColor
        {
            get
            {
                float pulse = 0.65f + 0.35f * Mathf.PingPong(Time.unscaledTime * 2f, 1f);
                return new Color(UIKit.Highlight.r, UIKit.Highlight.g, UIKit.Highlight.b, pulse);
            }
        }

        public static HudInventoryWindow Create(Transform parent, int bagSlots, int quickSlots, string quickHint,
            bool showWallet, Sprite goldIcon, Sprite reputationIcon, Action<int> onSlotClicked, Action onClose)
        {
            var rt = UIKit.Rect("InventoryWindowRoot", parent);
            UIKit.Stretch(rt, 0, 0, 0, 0);
            var view = rt.gameObject.AddComponent<HudInventoryWindow>();
            view.Build(bagSlots, quickSlots, quickHint, showWallet, goldIcon, reputationIcon, onSlotClicked, onClose);
            return view;
        }

        private void Build(int bagSlots, int quickSlots, string quickHint, bool showWallet, Sprite goldIcon, Sprite reputationIcon,
            Action<int> onSlotClicked, Action onClose)
        {
            bagCount = bagSlots;
            int bagRows = Mathf.CeilToInt(bagSlots / (float)Columns);
            float gridWidth = Columns * SlotSize + (Columns - 1) * SlotGap;
            float bagHeight = bagRows * SlotSize + Mathf.Max(0, bagRows - 1) * SlotGap;
            int quickRows = Mathf.Max(1, Mathf.CeilToInt(quickSlots / (float)Columns));
            float quickHeight = quickRows * SlotSize + (quickRows - 1) * SlotGap;
            float width = gridWidth + 64f;

            // vị trí từ trên xuống
            float bagLabelY = -192f;
            float bagY = bagLabelY - 28f;
            float quickLabelY = bagY - bagHeight - 14f;
            float quickY = quickLabelY - 28f;
            float infoY = quickY - quickHeight - 16f;
            const float infoHeight = 96f;
            float height = -infoY + infoHeight + (showWallet ? 76f : 28f);

            window = UIKit.Panel(transform, "Window", UIKit.Wood).rectTransform;
            UIKit.Place(window, new Vector2(1f, 0.5f), new Vector2(-40f, 20f), new Vector2(width, height));
            // chặn click xuyên qua
            window.GetComponent<Image>().raycastTarget = true;

            title = UIKit.Label(window, "Title", "HÒM ĐỒ", 36, UIKit.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UIKit.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(32f, -18f), new Vector2(width - 120f, 44f));
            subtitle = UIKit.Label(window, "Subtitle", "", 20, UIKit.TextDim, TextAnchor.UpperLeft);
            UIKit.Place(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(32f, -62f), new Vector2(width - 120f, 26f));

            var close = UIKit.Button(window, "Close", "X", 26, UIKit.ButtonRed, onClose);
            UIKit.Place((RectTransform)close.transform, new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(52f, 52f));

            // --- banner hướng dẫn / phục vụ
            banner = UIKit.Panel(window, "Banner", UIKit.WoodDark).rectTransform;
            UIKit.Place(banner, new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(gridWidth, 80f));
            bannerIcon = UIKit.Icon(banner, "Icon", null);
            UIKit.Place(bannerIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(64f, 64f));
            bannerText = UIKit.Label(banner, "Text", "", 20, UIKit.Text, TextAnchor.UpperLeft);
            UIKit.Place(bannerText.rectTransform, new Vector2(0f, 1f), new Vector2(14f, -8f), new Vector2(gridWidth - 28f, 50f));
            bannerBar = UIKit.Bar(banner, "PatienceBack", UIKit.Good, out bannerBarBack);
            UIKit.Place(bannerBarBack, new Vector2(0f, 0f), new Vector2(86f, 10f), new Vector2(gridWidth - 104f, 10f));
            bannerBarBack.gameObject.SetActive(false);

            // --- túi đồ
            SectionLabel("BagLabel", "TÚI ĐỒ", bagLabelY, width);
            var bagGrid = CreateGrid("BagGrid", bagY, gridWidth, bagHeight);
            for (int i = 0; i < bagSlots; i++) AddSlot(bagGrid, i, null, UIKit.Slot, onSlotClicked);

            // --- quick item
            string hint = string.IsNullOrEmpty(quickHint) ? "" : $"  <color=#{UIKit.Hex(UIKit.TextDim)}>· {quickHint}</color>";
            SectionLabel("QuickLabel", "QUICK ITEM" + hint, quickLabelY, width);
            var quickGrid = CreateGrid("QuickGrid", quickY, gridWidth, quickHeight);
            for (int k = 0; k < quickSlots; k++) AddSlot(quickGrid, bagSlots + k, (k + 1).ToString(), UIKit.WoodLight, onSlotClicked);

            // --- thông tin
            var info = UIKit.Panel(window, "Info", UIKit.WoodDark);
            UIKit.Place(info.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, infoY), new Vector2(gridWidth, infoHeight));
            infoText = UIKit.Label(info.transform, "Text", "", 19, UIKit.Text, TextAnchor.UpperLeft);
            UIKit.Stretch(infoText.rectTransform, 14, 8, 14, 10);

            // --- vàng & danh tiếng
            if (showWallet)
            {
                goldText = FooterValue(goldIcon, UIKit.Gold, 32f);
                repText = FooterValue(reputationIcon, UIKit.Rep, 32f + gridWidth * 0.5f);
            }

            window.gameObject.SetActive(false);
        }

        private void SectionLabel(string objName, string text, float y, float windowWidth)
        {
            var label = UIKit.Label(window, objName, text, 19, UIKit.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UIKit.Place(label.rectTransform, new Vector2(0f, 1f), new Vector2(32f, y), new Vector2(windowWidth - 64f, 24f));
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

        private void AddSlot(RectTransform grid, int index, string keyLabel, Color frameColor, Action<int> onSlotClicked)
        {
            var rt = UIKit.Rect("Slot" + index, grid);
            Action click = () => { if (onSlotClicked != null) onSlotClicked(index); };
            slotViews.Add(HudSlotView.Create(rt, frameColor, keyLabel, 6f, 10f, 21, click));
            slotEmpty.Add(true);
        }

        private Text FooterValue(Sprite icon, Color color, float x)
        {
            var img = UIKit.Icon(window, "FooterIcon", icon);
            UIKit.Place(img.rectTransform, new Vector2(0f, 0f), new Vector2(x, 18f), new Vector2(40f, 40f));
            var t = UIKit.Label(window, "FooterValue", "", 26, color, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIKit.Place(t.rectTransform, new Vector2(0f, 0f), new Vector2(x + 48f, 18f), new Vector2(200f, 40f));
            return t;
        }

        public void Show()
        {
            window.gameObject.SetActive(true);
        }

        public void Hide()
        {
            window.gameObject.SetActive(false);
        }

        public void SetHeader(string titleText, string subtitleText)
        {
            title.text = titleText;
            subtitle.text = subtitleText;
        }

        /// <param name="icon">null = banner chỉ có chữ (kéo chữ sang trái cho gọn).</param>
        /// <param name="barRatio">&lt; 0 = ẩn thanh tiến độ (vd thanh kiên nhẫn của khách).</param>
        public void SetBanner(Sprite icon, string text, float barRatio = -1f)
        {
            bannerIcon.enabled = icon != null;
            if (icon != null) bannerIcon.sprite = icon;
            float textX = icon != null ? 86f : 14f;
            bannerText.rectTransform.anchoredPosition = new Vector2(textX, -8f);
            bannerText.rectTransform.sizeDelta = new Vector2(banner.sizeDelta.x - textX - 14f, 50f);
            bannerText.text = text;

            bool showBar = barRatio >= 0f;
            bannerBarBack.gameObject.SetActive(showBar);
            if (showBar)
            {
                UIKit.SetBar(bannerBar, barRatio);
                bannerBar.color = barRatio > 0.5f ? UIKit.Good : (barRatio > 0.25f ? UIKit.Highlight : UIKit.Bad);
            }
        }

        public void SetSlot(int index, Sprite icon, int count, string fallbackName = null)
        {
            if (index < 0 || index >= slotViews.Count) return;
            bool empty = count <= 0;
            slotEmpty[index] = empty;
            var view = slotViews[index];
            view.SetContent(icon, count, fallbackName);
            view.SetFrameColor(index >= bagCount ? UIKit.WoodLight : UIKit.Slot, empty ? 0.6f : 1f);
        }

        /// <summary>Viền sáng (đang chọn / cần chú ý) và làm mờ icon (không dùng được trong ngữ cảnh hiện tại).</summary>
        public void SetSlotState(int index, bool highlighted, Color highlightColor, bool dimmed = false)
        {
            if (index < 0 || index >= slotViews.Count) return;
            slotViews[index].SetHighlight(highlighted, highlightColor);
            slotViews[index].SetDimmed(dimmed && !slotEmpty[index]);
        }

        public void SetInfo(string text)
        {
            infoText.text = text;
        }

        public void SetWallet(int gold, int reputation)
        {
            if (goldText != null) goldText.text = UIKit.FormatNumber(gold);
            if (repText != null) repText.text = UIKit.FormatNumber(reputation);
        }
    }
}
