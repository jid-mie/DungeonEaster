using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// HUD quán: vàng + danh tiếng, đồng hồ ca phục vụ, danh sách order đang chờ, nhắc phím.
    /// </summary>
    public class TavernHUD : MonoBehaviour
    {
        private const int MaxOrderRows = 8;

        private Text goldText;
        private Text repText;
        private Text shiftTitle;
        private Text shiftStats;
        private RectTransform ordersPanel;
        private readonly List<OrderRow> orderRows = new List<OrderRow>();

        private int shownGold = int.MinValue;
        private int shownRep = int.MinValue;

        private class OrderRow
        {
            public GameObject go;
            public Image icon;
            public Text label;
            public Image barFill;
        }

        public void Build(TavernUI owner)
        {
            var self = (RectTransform)transform;

            // --- Vàng & danh tiếng (trên trái)
            var wallet = UIKit.Panel(self, "Wallet", UIKit.Wood);
            UIKit.Place(wallet.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(380f, 128f));
            goldText = WalletRow(wallet.transform, owner.GoldIcon, "Vàng", UIKit.Gold, -14f);
            repText = WalletRow(wallet.transform, owner.ReputationIcon, "Danh tiếng", UIKit.Rep, -66f);

            // --- Ca phục vụ (giữa trên)
            var shift = UIKit.Panel(self, "Shift", UIKit.Wood);
            UIKit.Place(shift.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(520f, 92f));
            shiftTitle = UIKit.Label(shift.transform, "Title", "", 30, UIKit.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIKit.Stretch(shiftTitle.rectTransform, 12, 44, 12, 8);
            shiftStats = UIKit.Label(shift.transform, "Stats", "", 22, UIKit.TextDim, TextAnchor.MiddleCenter);
            UIKit.Stretch(shiftStats.rectTransform, 12, 8, 12, 50);

            // --- Order đang chờ (trái, dưới ví)
            ordersPanel = UIKit.Panel(self, "Orders", UIKit.Wood).rectTransform;
            UIKit.Place(ordersPanel, new Vector2(0f, 1f), new Vector2(24f, -168f), new Vector2(380f, 60f));
            var ordersTitle = UIKit.Label(ordersPanel, "Title", "ORDER ĐANG CHỜ", 20, UIKit.TextDim, TextAnchor.UpperLeft, FontStyle.Bold);
            UIKit.Place(ordersTitle.rectTransform, new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(300f, 26f));
            for (int i = 0; i < MaxOrderRows; i++) orderRows.Add(CreateOrderRow(ordersPanel, i));

            // --- Nhắc phím (dưới trái)
            var hint = UIKit.Label(self, "Hint",
                "<b>[WASD]</b> Di chuyển    <b>[F]</b> Tương tác (bếp / bàn khách)    <b>[I]</b> Hòm đồ    <b>[Esc]</b> Đóng",
                20, UIKit.Text, TextAnchor.LowerLeft);
            UIKit.Place(hint.rectTransform, new Vector2(0f, 0f), new Vector2(24f, 18f), new Vector2(1100f, 30f));
        }

        private static Text WalletRow(Transform parent, Sprite icon, string caption, Color color, float y)
        {
            var img = UIKit.Icon(parent, caption + "Icon", icon);
            UIKit.Place(img.rectTransform, new Vector2(0f, 1f), new Vector2(14f, y), new Vector2(48f, 48f));
            if (icon == null)
            {
                // không có icon -> chấm màu thay thế
                img.sprite = TavernSprites.Frame;
                img.type = Image.Type.Sliced;
                img.color = color;
                img.enabled = true;
            }
            var cap = UIKit.Label(parent, caption + "Caption", caption, 18, UIKit.TextDim, TextAnchor.UpperLeft);
            UIKit.Place(cap.rectTransform, new Vector2(0f, 1f), new Vector2(74f, y + 2f), new Vector2(240f, 22f));
            var value = UIKit.Label(parent, caption + "Value", "0", 28, color, TextAnchor.UpperLeft, FontStyle.Bold);
            UIKit.Place(value.rectTransform, new Vector2(0f, 1f), new Vector2(74f, y - 18f), new Vector2(240f, 34f));
            return value;
        }

        private static OrderRow CreateOrderRow(RectTransform parent, int index)
        {
            var row = new OrderRow();
            var rt = UIKit.Rect("Order" + index, parent);
            UIKit.Place(rt, new Vector2(0f, 1f), new Vector2(12f, -40f - index * 52f), new Vector2(356f, 48f));
            row.go = rt.gameObject;

            var bg = UIKit.Panel(rt, "Bg", UIKit.Slot);
            UIKit.Stretch(bg.rectTransform, 0, 0, 0, 0);
            row.icon = UIKit.Icon(rt, "Icon", null);
            UIKit.Place(row.icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(40f, 40f));
            row.label = UIKit.Label(rt, "Label", "", 17, UIKit.Text, TextAnchor.UpperLeft);
            row.label.horizontalOverflow = HorizontalWrapMode.Overflow;
            UIKit.Place(row.label.rectTransform, new Vector2(0f, 1f), new Vector2(54f, -4f), new Vector2(296f, 24f));

            var barBack = UIKit.Box(rt, "BarBack", new Color(0.13f, 0.08f, 0.05f));
            UIKit.Place(barBack.rectTransform, new Vector2(0f, 0f), new Vector2(54f, 8f), new Vector2(290f, 10f));
            row.barFill = UIKit.Box(barBack.transform, "Fill", UIKit.Good);
            row.barFill.rectTransform.anchorMin = Vector2.zero;
            row.barFill.rectTransform.anchorMax = new Vector2(1f, 1f);
            row.barFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            row.barFill.rectTransform.offsetMin = Vector2.zero;
            row.barFill.rectTransform.offsetMax = Vector2.zero;

            row.go.SetActive(false);
            return row;
        }

        private void Update()
        {
            var wallet = PlayerWallet.Instance;
            if (wallet != null)
            {
                if (wallet.Gold != shownGold)
                {
                    shownGold = wallet.Gold;
                    goldText.text = UIKit.FormatNumber(shownGold);
                }
                if (wallet.Reputation != shownRep)
                {
                    shownRep = wallet.Reputation;
                    repText.text = UIKit.FormatNumber(shownRep);
                }
            }

            UpdateShift();
            UpdateOrders();
        }

        private void UpdateShift()
        {
            var shift = TavernShift.Instance;
            if (shift == null)
            {
                shiftTitle.text = "QUÁN ĐÓNG CỬA";
                shiftStats.text = "";
                return;
            }

            int seconds = Mathf.CeilToInt(shift.TimeLeft);
            shiftTitle.text = shift.IsRunning
                ? $"CA PHỤC VỤ  {seconds / 60:00}:{seconds % 60:00}"
                : "HẾT CA PHỤC VỤ";
            shiftStats.text = $"Khách {shift.SpawnedCount}/{shift.MaxCustomers}   ·   Đã phục vụ {shift.ServedCount}   ·   Bỏ về {shift.AngryCount}";
        }

        private void UpdateOrders()
        {
            var shift = TavernShift.Instance;
            int shown = 0;
            if (shift != null)
            {
                foreach (var c in shift.ActiveCustomers)
                {
                    if (c == null || c.State != CustomerState.Waiting || shown >= orderRows.Count) continue;
                    var row = orderRows[shown++];
                    row.go.SetActive(true);
                    row.icon.sprite = c.Order.Icon;
                    row.icon.enabled = row.icon.sprite != null;
                    int have = PlayerInventory.Instance != null ? PlayerInventory.Instance.Count(c.Order.dish) : 0;
                    // số trong ngoặc = số phần món này đang có trong hòm đồ
                    string haveColor = UIKit.Hex(have > 0 ? UIKit.Good : UIKit.Bad);
                    row.label.text = $"Bàn {c.Table.TableNumber} · {c.Order.DisplayName} <color=#{haveColor}>({have})</color>";
                    float r = c.PatienceRatio;
                    row.barFill.rectTransform.anchorMax = new Vector2(r, 1f);
                    row.barFill.color = r > 0.5f ? UIKit.Good : (r > 0.25f ? UIKit.Highlight : UIKit.Bad);
                }
            }
            for (int i = shown; i < orderRows.Count; i++) orderRows[i].go.SetActive(false);

            ordersPanel.gameObject.SetActive(shown > 0);
            ordersPanel.sizeDelta = new Vector2(ordersPanel.sizeDelta.x, 46f + shown * 52f);
        }
    }
}
