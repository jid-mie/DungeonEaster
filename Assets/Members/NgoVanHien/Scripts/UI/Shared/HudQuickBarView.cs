using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Thanh quick item giữa dưới màn hình dùng chung cho 3 scene. Chỉ lo phần hiển thị:
    /// mỗi scene tự đẩy dữ liệu ô (icon, số lượng) từ hòm đồ của mình qua SetSlot.
    /// </summary>
    public class HudQuickBarView : MonoBehaviour
    {
        public const float SlotSize = 76f;
        public const float SlotGap = 8f;
        public const float BottomMargin = 14f;
        public const float BarHeight = SlotSize + 28f;

        private readonly List<HudSlotView> views = new List<HudSlotView>();
        private readonly List<float> flashUntil = new List<float>();
        private readonly List<bool> wanted = new List<bool>();
        private readonly List<bool> empty = new List<bool>();

        public int Count => views.Count;

        public static HudQuickBarView Create(Transform parent, int count)
        {
            var bar = UIKit.Panel(parent, "QuickBar", UIKit.Wood);
            bar.raycastTarget = false;
            var view = bar.gameObject.AddComponent<HudQuickBarView>();
            view.Build(count);
            return view;
        }

        private void Build(int count)
        {
            float width = count * SlotSize + Mathf.Max(0, count - 1) * SlotGap + 28f;
            UIKit.Place((RectTransform)transform, new Vector2(0.5f, 0f), new Vector2(0f, BottomMargin), new Vector2(width, BarHeight));

            for (int k = 0; k < count; k++)
            {
                var rt = UIKit.Rect("Quick" + k, transform);
                UIKit.Place(rt, new Vector2(0f, 0.5f), new Vector2(14f + k * (SlotSize + SlotGap), 0f), new Vector2(SlotSize, SlotSize));
                views.Add(HudSlotView.Create(rt, UIKit.WoodLight, (k + 1).ToString(), 5f, 9f, 20, null));
                flashUntil.Add(0f);
                wanted.Add(false);
                empty.Add(true);
            }
        }

        /// <param name="fallbackName">Tên hiện thay icon khi vật phẩm chưa có sprite.</param>
        public void SetSlot(int k, Sprite icon, int count, string fallbackName = null)
        {
            if (k < 0 || k >= views.Count) return;
            empty[k] = count <= 0;
            views[k].SetContent(icon, count, fallbackName);
        }

        public void ClearSlot(int k)
        {
            SetSlot(k, null, 0);
        }

        /// <summary>Viền vàng nhấp nháy (vd ô chứa món khách trước mặt đang gọi).</summary>
        public void SetWanted(int k, bool on)
        {
            if (k >= 0 && k < wanted.Count) wanted[k] = on;
        }

        /// <summary>Nháy sáng ô k khi người chơi bấm phím tương ứng.</summary>
        public void Flash(int k)
        {
            if (k >= 0 && k < flashUntil.Count) flashUntil[k] = Time.unscaledTime + 0.25f;
        }

        private void Update()
        {
            float pulse = 0.65f + 0.35f * Mathf.PingPong(Time.unscaledTime * 2f, 1f);
            for (int k = 0; k < views.Count; k++)
            {
                bool flashing = Time.unscaledTime < flashUntil[k];
                bool isWanted = wanted[k] && !empty[k];
                views[k].SetHighlight(flashing || isWanted,
                    flashing ? UIKit.Text : new Color(UIKit.Highlight.r, UIKit.Highlight.g, UIKit.Highlight.b, pulse));
                views[k].SetFrameColor(UIKit.WoodLight, empty[k] ? 0.55f : 1f);
            }
        }
    }

    /// <summary>1 ô vật phẩm (khung, viền sáng, icon, số lượng, nhãn phím) dùng chung cho quick bar và hòm đồ.</summary>
    public class HudSlotView
    {
        public Image frame;
        public Image highlight;
        public Image icon;
        public Text count;
        public Text fallback;

        public static HudSlotView Create(RectTransform rt, Color frameColor, string keyLabel, float highlightPad, float iconPad,
            int countFontSize, System.Action onClick)
        {
            var view = new HudSlotView();

            view.highlight = UIKit.Panel(rt, "Highlight", UIKit.Highlight);
            UIKit.Stretch(view.highlight.rectTransform, -highlightPad, -highlightPad, -highlightPad, -highlightPad);
            view.highlight.raycastTarget = false;
            view.highlight.enabled = false;

            view.frame = UIKit.Panel(rt, "Frame", frameColor);
            UIKit.Stretch(view.frame.rectTransform, 0, 0, 0, 0);
            if (onClick != null)
            {
                var button = view.frame.gameObject.AddComponent<Button>();
                button.targetGraphic = view.frame;
                button.onClick.AddListener(() => onClick());
            }
            else
            {
                view.frame.raycastTarget = false;
            }

            view.icon = UIKit.Icon(rt, "Icon", null);
            UIKit.Stretch(view.icon.rectTransform, iconPad, iconPad, iconPad, iconPad);

            view.fallback = UIKit.Label(rt, "Name", "", 13, UIKit.Text, TextAnchor.MiddleCenter);
            UIKit.Stretch(view.fallback.rectTransform, 5, 14, 5, 14);

            view.count = UIKit.Label(rt, "Count", "", countFontSize, UIKit.Text, TextAnchor.LowerRight, FontStyle.Bold);
            UIKit.Stretch(view.count.rectTransform, 4, 2, 7, 3);

            if (keyLabel != null)
            {
                var key = UIKit.Label(rt, "Key", keyLabel, 18, UIKit.Highlight, TextAnchor.UpperLeft, FontStyle.Bold);
                UIKit.Stretch(key.rectTransform, 7, 3, 4, 3);
            }
            return view;
        }

        public void SetContent(Sprite sprite, int amount, string fallbackName)
        {
            bool isEmpty = amount <= 0;
            icon.enabled = !isEmpty && sprite != null;
            if (icon.enabled) icon.sprite = sprite;
            fallback.text = !isEmpty && sprite == null && !string.IsNullOrEmpty(fallbackName) ? fallbackName : "";
            count.text = isEmpty ? "" : amount.ToString();
        }

        public void SetHighlight(bool on, Color color)
        {
            highlight.enabled = on;
            if (on) highlight.color = color;
        }

        public void SetFrameColor(Color color, float alpha)
        {
            frame.color = new Color(color.r, color.g, color.b, alpha);
        }

        public void SetDimmed(bool dimmed)
        {
            icon.color = dimmed ? new Color(1f, 1f, 1f, 0.35f) : Color.white;
        }
    }
}
