using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Bảng chọn món ở quầy nấu (GDD 3.2.2 + guideline UI nhà hàng):
    /// - Danh sách recipe phân biệt rõ: chưa mở khóa / thiếu nguyên liệu / hòm đồ đầy / nấu được.
    /// - Chọn số lượng, xem trước nguyên liệu tiêu hao, số món nhận được và doanh thu dự kiến.
    /// - Bấm NẤU: trừ nguyên liệu, món vào hòm đồ ngay (chưa có minigame).
    /// </summary>
    public class CookingPanel : MonoBehaviour
    {
        private const float Width = 1040f;
        private const float Height = 720f;
        private const int MaxIngredientRows = 4;

        private TavernUI owner;
        private RectTransform window;
        private readonly List<RecipeRow> rows = new List<RecipeRow>();
        private readonly List<IngredientRow> ingredientRows = new List<IngredientRow>();

        private Image detailIcon;
        private Text detailName;
        private Text detailPrice;
        private Text detailDesc;
        private Text quantityText;
        private Text previewText;
        private Button cookButton;
        private Text cookLabel;
        private Button minusButton;
        private Button plusButton;
        private Button maxButton;

        private RecipeData selected;
        private int quantity = 1;

        public bool IsOpen => window != null && window.gameObject.activeSelf;

        private class RecipeRow
        {
            public RecipeData recipe;
            public Image frame;
            public Image icon;
            public Text name;
            public Text status;
        }

        private class IngredientRow
        {
            public GameObject go;
            public Image icon;
            public Text name;
            public Text amount;
        }

        public void Build(TavernUI ui)
        {
            owner = ui;
            var self = (RectTransform)transform;

            window = UIKit.Panel(self, "Window", UIKit.Wood).rectTransform;
            UIKit.Place(window, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(Width, Height));

            var title = UIKit.Label(window, "Title", "BẾP NẤU — CHỌN MÓN", 36, UIKit.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UIKit.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(28f, -20f), new Vector2(700f, 44f));
            var close = UIKit.Button(window, "Close", "X", 26, UIKit.ButtonRed, Close);
            UIKit.Place((RectTransform)close.transform, new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(52f, 52f));

            // --- cột trái: danh sách recipe
            var recipes = Kitchen.Instance != null ? Kitchen.Instance.Recipes : (IReadOnlyList<RecipeData>)new List<RecipeData>();
            for (int i = 0; i < recipes.Count; i++)
            {
                if (recipes[i] == null) continue;
                rows.Add(CreateRecipeRow(recipes[i], rows.Count));
            }

            // --- cột phải: chi tiết
            var detail = UIKit.Panel(window, "Detail", UIKit.WoodDark).rectTransform;
            UIKit.Place(detail, new Vector2(0f, 1f), new Vector2(420f, -84f), new Vector2(Width - 444f, Height - 108f));

            detailIcon = UIKit.Icon(detail, "Icon", null);
            UIKit.Place(detailIcon.rectTransform, new Vector2(0f, 1f), new Vector2(18f, -16f), new Vector2(120f, 120f));
            detailName = UIKit.Label(detail, "Name", "", 32, UIKit.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UIKit.Place(detailName.rectTransform, new Vector2(0f, 1f), new Vector2(156f, -18f), new Vector2(420f, 40f));
            detailPrice = UIKit.Label(detail, "Price", "", 21, UIKit.Gold, TextAnchor.UpperLeft);
            UIKit.Place(detailPrice.rectTransform, new Vector2(0f, 1f), new Vector2(156f, -60f), new Vector2(420f, 28f));
            detailDesc = UIKit.Label(detail, "Desc", "", 19, UIKit.TextDim, TextAnchor.UpperLeft);
            UIKit.Place(detailDesc.rectTransform, new Vector2(0f, 1f), new Vector2(156f, -92f), new Vector2(420f, 50f));

            var ingHeader = UIKit.Label(detail, "IngredientsHeader", "NGUYÊN LIỆU CẦN", 20, UIKit.TextDim, TextAnchor.UpperLeft, FontStyle.Bold);
            UIKit.Place(ingHeader.rectTransform, new Vector2(0f, 1f), new Vector2(18f, -150f), new Vector2(400f, 26f));
            for (int i = 0; i < MaxIngredientRows; i++) ingredientRows.Add(CreateIngredientRow(detail, i));

            // số lượng
            var qtyLabel = UIKit.Label(detail, "QtyLabel", "Số lượng", 22, UIKit.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIKit.Place(qtyLabel.rectTransform, new Vector2(0f, 1f), new Vector2(18f, -392f), new Vector2(120f, 48f));
            minusButton = UIKit.Button(detail, "Minus", "-", 30, UIKit.ButtonBrown, () => ChangeQuantity(-1));
            UIKit.Place((RectTransform)minusButton.transform, new Vector2(0f, 1f), new Vector2(140f, -392f), new Vector2(56f, 48f));
            var qtyBox = UIKit.Panel(detail, "QtyBox", UIKit.Slot);
            UIKit.Place(qtyBox.rectTransform, new Vector2(0f, 1f), new Vector2(204f, -392f), new Vector2(80f, 48f));
            quantityText = UIKit.Label(qtyBox.transform, "Value", "1", 28, UIKit.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIKit.Stretch(quantityText.rectTransform, 0, 0, 0, 0);
            plusButton = UIKit.Button(detail, "Plus", "+", 30, UIKit.ButtonBrown, () => ChangeQuantity(1));
            UIKit.Place((RectTransform)plusButton.transform, new Vector2(0f, 1f), new Vector2(292f, -392f), new Vector2(56f, 48f));
            maxButton = UIKit.Button(detail, "Max", "Tối đa", 22, UIKit.ButtonBrown, SetMaxQuantity);
            UIKit.Place((RectTransform)maxButton.transform, new Vector2(0f, 1f), new Vector2(356f, -392f), new Vector2(120f, 48f));

            // xem trước
            var preview = UIKit.Panel(detail, "Preview", UIKit.Slot);
            UIKit.Place(preview.rectTransform, new Vector2(0f, 1f), new Vector2(18f, -452f), new Vector2(Width - 480f, 74f));
            previewText = UIKit.Label(preview.transform, "Text", "", 19, UIKit.Text, TextAnchor.UpperLeft);
            UIKit.Stretch(previewText.rectTransform, 12, 6, 12, 8);

            cookButton = UIKit.Button(detail, "Cook", "NẤU", 28, UIKit.ButtonGreen, CookSelected, out cookLabel);
            UIKit.Place((RectTransform)cookButton.transform, new Vector2(0f, 0f), new Vector2(18f, 18f), new Vector2(Width - 480f, 60f));

            window.gameObject.SetActive(false);
        }

        private RecipeRow CreateRecipeRow(RecipeData recipe, int index)
        {
            var row = new RecipeRow { recipe = recipe };
            row.frame = UIKit.Panel(window, "Recipe_" + recipe.name, UIKit.Slot);
            UIKit.Place(row.frame.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -84f - index * 108f), new Vector2(380f, 100f));
            var button = row.frame.gameObject.AddComponent<Button>();
            button.targetGraphic = row.frame;
            button.onClick.AddListener(() => Select(recipe));

            row.icon = UIKit.Icon(row.frame.transform, "Icon", recipe.Icon);
            UIKit.Place(row.icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(76f, 76f));
            row.name = UIKit.Label(row.frame.transform, "Name", recipe.DisplayName, 22, UIKit.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UIKit.Place(row.name.rectTransform, new Vector2(0f, 1f), new Vector2(100f, -12f), new Vector2(270f, 28f));
            var price = UIKit.Label(row.frame.transform, "Price", $"{recipe.goldPrice} vàng · +{recipe.reputation} danh tiếng", 17, UIKit.Gold, TextAnchor.UpperLeft);
            UIKit.Place(price.rectTransform, new Vector2(0f, 1f), new Vector2(100f, -42f), new Vector2(270f, 22f));
            row.status = UIKit.Label(row.frame.transform, "Status", "", 17, UIKit.Good, TextAnchor.UpperLeft);
            UIKit.Place(row.status.rectTransform, new Vector2(0f, 1f), new Vector2(100f, -66f), new Vector2(270f, 22f));
            return row;
        }

        private IngredientRow CreateIngredientRow(RectTransform parent, int index)
        {
            var row = new IngredientRow();
            var rt = UIKit.Rect("Ingredient" + index, parent);
            UIKit.Place(rt, new Vector2(0f, 1f), new Vector2(18f, -180f - index * 50f), new Vector2(Width - 480f, 46f));
            row.go = rt.gameObject;
            var bg = UIKit.Panel(rt, "Bg", UIKit.Slot);
            UIKit.Stretch(bg.rectTransform, 0, 0, 0, 0);
            row.icon = UIKit.Icon(rt, "Icon", null);
            UIKit.Place(row.icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(38f, 38f));
            row.name = UIKit.Label(rt, "Name", "", 20, UIKit.Text, TextAnchor.MiddleLeft);
            UIKit.Place(row.name.rectTransform, new Vector2(0f, 0.5f), new Vector2(54f, 0f), new Vector2(340f, 40f));
            row.amount = UIKit.Label(rt, "Amount", "", 20, UIKit.Text, TextAnchor.MiddleRight, FontStyle.Bold);
            UIKit.Place(row.amount.rectTransform, new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(200f, 40f));
            return row;
        }

        public void Open()
        {
            window.gameObject.SetActive(true);
            if (selected == null || !rows.Exists(r => r.recipe == selected))
            {
                // ưu tiên chọn sẵn món đang có khách gọi, không thì món đầu tiên
                selected = FindRequestedRecipe() ?? (rows.Count > 0 ? rows[0].recipe : null);
            }
            quantity = 1;
            Refresh();
            owner.NotifyModalChanged();
        }

        public void Close()
        {
            window.gameObject.SetActive(false);
            owner.NotifyModalChanged();
        }

        private static RecipeData FindRequestedRecipe()
        {
            var shift = TavernShift.Instance;
            if (shift == null) return null;
            foreach (var c in shift.ActiveCustomers)
            {
                if (c != null && c.State == CustomerState.Waiting && c.Order.unlocked) return c.Order;
            }
            return null;
        }

        private void Select(RecipeData recipe)
        {
            selected = recipe;
            quantity = 1;
            Refresh();
        }

        private void ChangeQuantity(int delta)
        {
            if (selected == null || Kitchen.Instance == null) return;
            int max = Mathf.Max(1, Kitchen.Instance.MaxCookable(selected));
            quantity = Mathf.Clamp(quantity + delta, 1, max);
            Refresh();
        }

        private void SetMaxQuantity()
        {
            if (selected == null || Kitchen.Instance == null) return;
            quantity = Mathf.Max(1, Kitchen.Instance.MaxCookable(selected));
            Refresh();
        }

        private void CookSelected()
        {
            var kitchen = Kitchen.Instance;
            if (selected == null || kitchen == null) return;

            var check = kitchen.Check(selected, quantity);
            if (check != CookCheck.Ok)
            {
                owner.ShowToast(Kitchen.Describe(check), UIKit.Bad);
                return;
            }
            if (kitchen.Cook(selected, quantity))
            {
                int made = selected.outputAmount * quantity;
                owner.ShowToast($"Đã nấu {made} phần {selected.DisplayName} — đã cho vào hòm đồ.", UIKit.Good);
                quantity = 1;
                Refresh();
            }
        }

        private void Update()
        {
            // nguyên liệu/hòm đồ có thể đổi khi đang mở (vd khách bỏ về không ảnh hưởng, nhưng giữ đồng bộ)
            if (IsOpen && Time.frameCount % 10 == 0) Refresh();
        }

        private void Refresh()
        {
            var kitchen = Kitchen.Instance;
            var inv = PlayerInventory.Instance;
            if (kitchen == null || inv == null) return;

            foreach (var row in rows)
            {
                var r = row.recipe;
                bool isSelected = r == selected;
                int max = kitchen.MaxCookable(r);
                var check = kitchen.Check(r, 1);
                if (!r.unlocked)
                {
                    row.status.text = "Chưa mở khóa";
                    row.status.color = UIKit.TextDim;
                }
                else if (check == CookCheck.Ok)
                {
                    row.status.text = $"Đủ nguyên liệu · nấu được {max}";
                    row.status.color = UIKit.Good;
                }
                else
                {
                    row.status.text = Kitchen.Describe(check);
                    row.status.color = UIKit.Bad;
                }
                row.icon.color = r.unlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f, 0.8f);
                row.frame.color = isSelected ? UIKit.WoodLight : UIKit.Slot;
            }

            if (selected == null)
            {
                detailName.text = "Chưa có công thức";
                cookButton.interactable = false;
                return;
            }

            int maxCook = kitchen.MaxCookable(selected);
            quantity = Mathf.Clamp(quantity, 1, Mathf.Max(1, maxCook));
            var selectedCheck = kitchen.Check(selected, quantity);

            detailIcon.sprite = selected.Icon;
            detailIcon.enabled = selected.Icon != null;
            detailIcon.color = selected.unlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f, 0.8f);
            detailName.text = selected.DisplayName;
            detailPrice.text = $"Giá bán {selected.goldPrice} vàng · +{selected.reputation} danh tiếng / phần";
            detailDesc.text = selected.dish != null ? selected.dish.description : "";

            for (int i = 0; i < ingredientRows.Count; i++)
            {
                var view = ingredientRows[i];
                bool used = i < selected.ingredients.Count && selected.ingredients[i].item != null;
                view.go.SetActive(used);
                if (!used) continue;

                var ing = selected.ingredients[i];
                int need = ing.amount * quantity;
                int have = inv.Count(ing.item);
                view.icon.sprite = ing.item.icon;
                view.icon.enabled = ing.item.icon != null;
                view.name.text = $"{ing.item.displayName}  <color=#{UIKit.Hex(UIKit.TextDim)}>x{ing.amount}</color>";
                view.amount.text = $"{have} / {need}";
                view.amount.color = have >= need ? UIKit.Good : UIKit.Bad;
            }

            quantityText.text = quantity.ToString();
            minusButton.interactable = quantity > 1;
            plusButton.interactable = quantity < maxCook;
            maxButton.interactable = maxCook > 1;

            previewText.text = BuildPreview(selected, quantity, selectedCheck);

            cookButton.interactable = selectedCheck == CookCheck.Ok;
            cookLabel.text = selectedCheck == CookCheck.Ok
                ? $"NẤU {quantity} PHẦN"
                : Kitchen.Describe(selectedCheck).ToUpperInvariant();
        }

        private static string BuildPreview(RecipeData recipe, int qty, CookCheck check)
        {
            var sb = new StringBuilder();
            sb.Append("Tiêu hao: ");
            bool first = true;
            foreach (var ing in Kitchen.ScaledIngredients(recipe, qty))
            {
                if (!first) sb.Append(", ");
                sb.Append(ing.amount).Append(' ').Append(ing.item.displayName);
                first = false;
            }
            sb.Append("\nNhận được: ").Append(recipe.outputAmount * qty).Append(' ').Append(recipe.DisplayName).Append(" → hòm đồ");
            int revenue = recipe.goldPrice * recipe.outputAmount * qty;
            int rep = recipe.reputation * recipe.outputAmount * qty;
            sb.Append($"\n<color=#{UIKit.Hex(UIKit.Gold)}>Doanh thu dự kiến: {UIKit.FormatNumber(revenue)} vàng · +{rep} danh tiếng</color>");
            if (check == CookCheck.InventoryFull)
            {
                sb.Append($"  <color=#{UIKit.Hex(UIKit.Bad)}>(hòm đồ không đủ chỗ)</color>");
            }
            return sb.ToString();
        }
    }
}
