using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Thanh quick item giữa dưới màn hình: hiện 5 ô quick item của hòm đồ kèm phím 1-5.
    /// Ô chứa món mà khách ở bàn trước mặt đang gọi được viền vàng.
    /// </summary>
    public class QuickBar : MonoBehaviour
    {
        private const float SlotSize = 76f;
        private const float SlotGap = 8f;

        private TavernUI owner;
        private readonly List<SlotView> views = new List<SlotView>();
        private readonly List<float> flashUntil = new List<float>();

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
            int count = PlayerInventory.Instance != null ? PlayerInventory.Instance.QuickSlotCount : 5;
            float width = count * SlotSize + (count - 1) * SlotGap + 28f;

            var bar = UIKit.Panel(transform, "Bar", UIKit.Wood);
            UIKit.Place(bar.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(width, SlotSize + 28f));

            for (int k = 0; k < count; k++)
            {
                var view = new SlotView();
                var rt = UIKit.Rect("Quick" + k, bar.transform);
                UIKit.Place(rt, new Vector2(0f, 0.5f), new Vector2(14f + k * (SlotSize + SlotGap), 0f), new Vector2(SlotSize, SlotSize));

                view.highlight = UIKit.Panel(rt, "Highlight", UIKit.Highlight);
                UIKit.Stretch(view.highlight.rectTransform, -5, -5, -5, -5);
                view.frame = UIKit.Panel(rt, "Frame", UIKit.WoodLight);
                UIKit.Stretch(view.frame.rectTransform, 0, 0, 0, 0);
                view.icon = UIKit.Icon(rt, "Icon", null);
                UIKit.Stretch(view.icon.rectTransform, 9, 9, 9, 9);
                view.count = UIKit.Label(rt, "Count", "", 20, UIKit.Text, TextAnchor.LowerRight, FontStyle.Bold);
                UIKit.Stretch(view.count.rectTransform, 4, 2, 7, 3);
                var key = UIKit.Label(rt, "Key", (k + 1).ToString(), 18, UIKit.Highlight, TextAnchor.UpperLeft, FontStyle.Bold);
                UIKit.Stretch(key.rectTransform, 7, 3, 4, 3);

                views.Add(view);
                flashUntil.Add(0f);
            }
        }

        /// <summary>Nháy sáng ô k khi người chơi bấm phím tương ứng.</summary>
        public void Flash(int k)
        {
            if (k >= 0 && k < flashUntil.Count) flashUntil[k] = Time.unscaledTime + 0.25f;
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

            float pulse = 0.65f + 0.35f * Mathf.PingPong(Time.unscaledTime * 2f, 1f);
            for (int k = 0; k < views.Count; k++)
            {
                var view = views[k];
                int index = inv.QuickSlotIndex(k);
                var slot = index < inv.Slots.Count ? inv.Slots[index] : null;
                bool empty = slot == null || slot.IsEmpty;

                view.icon.enabled = !empty && slot.item.icon != null;
                if (!empty) view.icon.sprite = slot.item.icon;
                view.count.text = empty ? "" : slot.count.ToString();

                bool flashing = Time.unscaledTime < flashUntil[k];
                bool isWanted = !empty && wanted != null && slot.item == wanted;
                view.highlight.enabled = flashing || isWanted;
                view.highlight.color = flashing ? UIKit.Text : new Color(UIKit.Highlight.r, UIKit.Highlight.g, UIKit.Highlight.b, pulse);
                view.frame.color = empty ? new Color(UIKit.WoodLight.r, UIKit.WoodLight.g, UIKit.WoodLight.b, 0.55f) : UIKit.WoodLight;
            }
        }
    }
}
