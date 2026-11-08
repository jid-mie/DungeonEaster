using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NguyenMinhHiep.Core;
using NguyenMinhHiep.Data;

namespace NguyenMinhHiep.UI
{
    /// <summary>
    /// Bảng Hòm Đồ & Kho Nguyên Liệu Chung (Unified Inventory Panel) dùng cho cả 3 Scene:
    /// - Lấy chuẩn format giao diện của NgoVanHien (InventoryPanel.cs + UIKit.cs):
    ///   Khung gỗ 9-slice bên phải màn hình, thẻ thông tin giấy da (Parchment), lưới ô 5 cột.
    /// - Hiển thị đầy đủ:
    ///   1. Tiền (Vàng), Danh tiếng (Reputation), Sức chứa Balo (10 ô mặc định, +5 mỗi Boss).
    ///   2. Trang bị Vũ khí (Kiếm & Cung Lv.1-4) kèm nút nâng cấp theo Bảng 7.5 báo cáo.
    ///   3. Lưới Balo (Bag) & Thanh Dùng Nhanh (Quick Bar 4 ô): Click chọn 2 ô để di chuyển/hoán đổi, hoặc Chuột phải (RMB) để trang bị nhanh.
    ///   4. Chi tiết vật phẩm đọc trực tiếp từ JSON (items_catalog.json) + Kho Tra Cứu JSON để lấy nhanh bất kỳ vật phẩm nào trong báo cáo.
    /// </summary>
    public class UnifiedInventoryPanel : MonoBehaviour
    {
        private class SlotView
        {
            public bool isQuick;
            public int index;
            public Image bg;
            public Image icon;
            public Text countText;
            public Text nameText;
        }

        private readonly List<SlotView> bagViews = new List<SlotView>();
        private readonly List<SlotView> quickViews = new List<SlotView>();

        private RectTransform bagGridContainer;
        private Text headerCapacityText;
        private Text walletSummaryText;
        private Text weaponSummaryText;

        // Thẻ thông tin chi tiết vật phẩm (Parchment Card chuẩn NgoVanHien)
        private Image detailIcon;
        private Text detailTitleText;
        private Text detailMetaText;
        private Text detailDescText;
        private Button useOrEquipButton;
        private Text useOrEquipButtonLabel;

        // Cửa sổ tra cứu nhanh danh mục JSON (Bảng 7.1, 7.2, 7.5, 7.7)
        private GameObject jsonCatalogSubPanel;
        private RectTransform catalogListContent;
        private string activeCatalogFilter = "All";

        private bool hasSelectedSlot;
        private bool selectedIsQuick;
        private int selectedIndex = -1;

        public bool IsOpen => gameObject.activeSelf;

        public void Initialize()
        {
            BuildPanelLayout();
            BindEvents();
            Refresh();
        }

        private void OnDestroy()
        {
            UnbindEvents();
        }

        private void BindEvents()
        {
            var data = SharedGameDataManager.Instance;
            if (data != null)
            {
                data.OnInventoryChanged += Refresh;
                data.OnWalletChanged += RefreshHeaderStrip;
                data.OnWeaponChanged += RefreshHeaderStrip;
            }
        }

        private void UnbindEvents()
        {
            var data = SharedGameDataManager.Instance;
            if (data != null)
            {
                data.OnInventoryChanged -= Refresh;
                data.OnWalletChanged -= RefreshHeaderStrip;
                data.OnWeaponChanged -= RefreshHeaderStrip;
            }
        }

        // =====================================================================
        // DỰNG GIAO DIỆN THEO FORMAT NGÔ VĂN HIẾN (InventoryPanel.cs)
        // =====================================================================

        private void BuildPanelLayout()
        {
            // Khung chính bên phải màn hình (giống hệt vị trí InventoryPanel của NgoVanHien)
            var rootImg = HiepUIKit.CreatePanel(transform, "InventoryRootWindow", HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(rootImg.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-24, 15), new Vector2(720, 650));

            var inner = HiepUIKit.CreatePanel(rootImg.transform, "Inner", HiepUIKit.WoodMid, true);
            HiepUIKit.Stretch(inner.rectTransform, 8, 8, 8, 8);

            // 1. Tiêu đề + Nút Đóng [X] + Nút Mở Danh Mục JSON
            var title = HiepUIKit.CreateLabel(inner.transform, "Title", "🎒 TÚI ĐỒ & KHO NGUYÊN LIỆU CHUNG", 19,
                HiepUIKit.AccentGold, TextAnchor.MiddleLeft, FontStyle.Bold);
            HiepUIKit.Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(16, -10), new Vector2(-240, 28));

            headerCapacityText = HiepUIKit.CreateLabel(inner.transform, "SubTitle",
                "Click trái 2 ô để đổi chỗ • Click phải (RMB) để gắn/tháo QuickBar • [I / Esc] Đóng", 11,
                HiepUIKit.TextMuted, TextAnchor.MiddleLeft);
            HiepUIKit.Anchor(headerCapacityText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(16, -36), new Vector2(-240, 18));

            var catalogBtn = HiepUIKit.CreateButton(inner.transform, "CatalogBtn", "📖 DANH MỤC JSON", 12,
                HiepUIKit.WoodLight, HiepUIKit.AccentGold, ToggleJsonCatalogSubPanel);
            HiepUIKit.Anchor(catalogBtn.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-54, -12), new Vector2(148, 32));

            var closeBtn = HiepUIKit.CreateButton(inner.transform, "CloseBtn", "✕", 16,
                HiepUIKit.AccentRed, HiepUIKit.TextLight, () => gameObject.SetActive(false));
            HiepUIKit.Anchor(closeBtn.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-12, -12), new Vector2(34, 32));

            // 2. Thanh Tóm tắt Tiền tệ & Vũ khí (Wallet + Weapons Bar)
            BuildWalletAndWeaponStrip(inner.transform);

            // 3. Khu vực Lưới Balo (Bag Slots - 5 cột)
            var bagSectionTitle = HiepUIKit.CreateLabel(inner.transform, "BagTitle",
                "BALO HÀNH TRANG (Tối đa 10 stack Nguyên liệu / 5 stack Tiêu hao & Món ăn)", 12,
                HiepUIKit.TextLight, TextAnchor.MiddleLeft, FontStyle.Bold);
            HiepUIKit.Anchor(bagSectionTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(16, -114), new Vector2(-32, 20));

            var bagBg = HiepUIKit.CreatePanel(inner.transform, "BagGridFrame", HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(bagBg.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -136), new Vector2(-32, 184));

            var bagViewport = HiepUIKit.CreateRect(bagBg.transform, "BagViewport");
            HiepUIKit.Stretch(bagViewport, 4, 4, 4, 4);
            bagViewport.gameObject.AddComponent<RectMask2D>();

            bagGridContainer = HiepUIKit.CreateRect(bagViewport, "BagContent");
            HiepUIKit.Anchor(bagGridContainer, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                Vector2.zero, new Vector2(0, 176));

            var bagScroll = bagBg.gameObject.AddComponent<ScrollRect>();
            bagScroll.viewport = bagViewport;
            bagScroll.content = bagGridContainer;
            bagScroll.horizontal = false;
            bagScroll.vertical = true;
            bagScroll.movementType = ScrollRect.MovementType.Clamped;
            bagScroll.scrollSensitivity = 24f;

            RebuildBagSlotViews(10);

            // 4. Khu vực Thanh Dùng Nhanh (Quick Bar - 4 ô vĩnh viễn theo GDD 3.2.1)
            var quickTitle = HiepUIKit.CreateLabel(inner.transform, "QuickTitle",
                "THANH VẬT PHẨM DÙNG NHANH (QUICK BAR - 4 Ô VĨNH VIỄN [Phím 1-4])", 12,
                HiepUIKit.AccentGold, TextAnchor.MiddleLeft, FontStyle.Bold);
            HiepUIKit.Anchor(quickTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(16, -326), new Vector2(-32, 20));

            var quickFrame = HiepUIKit.CreatePanel(inner.transform, "QuickGridFrame", HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(quickFrame.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -348), new Vector2(-32, 88));

            BuildQuickSlotViews(quickFrame.rectTransform);

            // 5. Thẻ Chi Tiết Vật Phẩm Bằng Giấy Da (Parchment Detail Card chuẩn NgoVanHien)
            BuildDetailParchmentCard(inner.transform);

            // 6. Cửa sổ con Tra cứu Danh mục JSON (27 Vật phẩm + 2 Vũ khí)
            BuildJsonCatalogSubPanel(rootImg.transform);
        }

        private void BuildWalletAndWeaponStrip(Transform parent)
        {
            var strip = HiepUIKit.CreatePanel(parent, "WalletWeaponStrip", HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(strip.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -58), new Vector2(-32, 50));

            walletSummaryText = HiepUIKit.CreateLabel(strip.transform, "WalletSummary",
                "💰 Vàng: <b>500 G</b>   |   ⭐ Danh tiếng: <b>20</b>", 13,
                HiepUIKit.AccentGold, TextAnchor.MiddleLeft, FontStyle.Bold);
            HiepUIKit.Anchor(walletSummaryText.rectTransform, new Vector2(0, 0), new Vector2(0.46f, 1), new Vector2(0, 0.5f),
                new Vector2(12, 0), new Vector2(-12, 0));

            weaponSummaryText = HiepUIKit.CreateLabel(strip.transform, "WeaponSummary",
                "⚔️ Kiếm Lv.1   |   🏹 Cung Lv.1", 12,
                HiepUIKit.TextLight, TextAnchor.MiddleLeft, FontStyle.Bold);
            HiepUIKit.Anchor(weaponSummaryText.rectTransform, new Vector2(0.46f, 0), new Vector2(0.76f, 1), new Vector2(0, 0.5f),
                new Vector2(4, 0), new Vector2(-8, 0));

            var upgSwordBtn = HiepUIKit.CreateButton(strip.transform, "UpgradeSwordBtn", "⬆️ Rèn Kiếm", 11,
                HiepUIKit.WoodLight, HiepUIKit.AccentGold, () =>
                {
                    SharedGameDataManager.Instance?.UpgradeWeapon(true);
                });
            HiepUIKit.Anchor(upgSwordBtn.GetComponent<RectTransform>(), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-86, 0), new Vector2(74, 34));

            var upgBowBtn = HiepUIKit.CreateButton(strip.transform, "UpgradeBowBtn", "⬆️ Rèn Cung", 11,
                HiepUIKit.WoodLight, HiepUIKit.AccentBlue, () =>
                {
                    SharedGameDataManager.Instance?.UpgradeWeapon(false);
                });
            HiepUIKit.Anchor(upgBowBtn.GetComponent<RectTransform>(), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-8, 0), new Vector2(74, 34));
        }

        private void RebuildBagSlotViews(int totalSlots)
        {
            foreach (var v in bagViews)
            {
                if (v.bg != null) Destroy(v.bg.gameObject);
            }
            bagViews.Clear();

            const int cols = 5;
            int rows = Mathf.CeilToInt(totalSlots / (float)cols);
            float cellW = 120f;
            float cellH = rows <= 2 ? 78f : 52f;
            float gapX = 10f;
            float gapY = rows <= 2 ? 8f : 6f;
            float topPad = 4f;

            for (int i = 0; i < totalSlots; i++)
            {
                int r = i / cols;
                int c = i % cols;
                var pos = new Vector2(10f + c * (cellW + gapX), -topPad - r * (cellH + gapY));
                bagViews.Add(CreateSlotView(bagGridContainer, false, i, pos, new Vector2(cellW, cellH), $"Ô {i + 1}"));
            }

            float contentHeight = topPad * 2f + rows * cellH + Mathf.Max(0, rows - 1) * gapY;
            bagGridContainer.sizeDelta = new Vector2(0f, Mathf.Max(176f, contentHeight));
        }

        private void BuildQuickSlotViews(RectTransform container)
        {
            quickViews.Clear();
            const int count = 4;
            float cellW = 122f;
            float cellH = 72f;
            float gapX = 12f;

            for (int i = 0; i < count; i++)
            {
                var pos = new Vector2(14f + i * (cellW + gapX), -8f);
                quickViews.Add(CreateSlotView(container, true, i, pos, new Vector2(cellW, cellH), $"Phím [{i + 1}]"));
            }

            // Gợi ý phím R cạnh 4 ô Quick Slot
            var hintBox = HiepUIKit.CreatePanel(container, "QuickHintBox", HiepUIKit.WoodMid, true);
            HiepUIKit.Anchor(hintBox.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(14f + 4 * (cellW + gapX), -8f), new Vector2(-28f - 4 * (cellW + gapX), cellH));

            var hintTxt = HiepUIKit.CreateLabel(hintBox.transform, "HintText",
                "<b>MẸO CHIẾN ĐẤU</b>\n• Phím <b>[1-4]</b>: Dùng ô tương ứng\n• Phím <b>[R]</b>: Uống nhanh Bình máu\n• <b>Chuột phải</b>: Gắn/Tháo nhanh",
                10, HiepUIKit.TextLight, TextAnchor.MiddleLeft);
            HiepUIKit.Stretch(hintTxt.rectTransform, 8, 8, 4, 4);
        }

        private SlotView CreateSlotView(Transform parent, bool isQuick, int index, Vector2 topLeftPos, Vector2 size, string badgeText)
        {
            var bg = HiepUIKit.CreatePanel(parent, $"{(isQuick ? "Quick" : "Bag")}_{index}", HiepUIKit.SlotIdle, true);
            HiepUIKit.Anchor(bg.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), topLeftPos, size);

            var iconRt = HiepUIKit.CreateRect(bg.transform, "Icon");
            HiepUIKit.Anchor(iconRt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(8, 2), new Vector2(40, 40));
            var iconImg = iconRt.gameObject.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.enabled = false;

            var badgeLbl = HiepUIKit.CreateLabel(bg.transform, "Badge", badgeText, 10, HiepUIKit.AccentGold,
                TextAnchor.UpperLeft, FontStyle.Bold);
            HiepUIKit.Stretch(badgeLbl.rectTransform, 6, 6, 3, 3);

            var countLbl = HiepUIKit.CreateLabel(bg.transform, "Count", "", 13, HiepUIKit.TextLight,
                TextAnchor.UpperRight, FontStyle.Bold);
            HiepUIKit.Stretch(countLbl.rectTransform, 6, 6, 3, 3);

            var nameLbl = HiepUIKit.CreateLabel(bg.transform, "Name", "Trống", 11, HiepUIKit.TextLight,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            HiepUIKit.Anchor(nameLbl.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f),
                new Vector2(52, -4), new Vector2(-58, -18));

            var view = new SlotView
            {
                isQuick = isQuick,
                index = index,
                bg = bg,
                icon = iconImg,
                countText = countLbl,
                nameText = nameLbl
            };

            var trigger = bg.gameObject.AddComponent<EventTrigger>();
            var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            clickEntry.callback.AddListener(ev =>
            {
                var pev = (PointerEventData)ev;
                OnSlotClicked(view, pev.button == PointerEventData.InputButton.Right);
            });
            trigger.triggers.Add(clickEntry);

            return view;
        }

        private void BuildDetailParchmentCard(Transform parent)
        {
            var card = HiepUIKit.CreatePanel(parent, "ItemDetailParchment", HiepUIKit.Parchment, true);
            HiepUIKit.Anchor(card.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0),
                new Vector2(0, 12), new Vector2(-32, 180));

            var iconFrame = HiepUIKit.CreatePanel(card.transform, "IconFrame", HiepUIKit.WoodMid, true);
            HiepUIKit.Anchor(iconFrame.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(14, -14), new Vector2(76, 76));

            var iconRt = HiepUIKit.CreateRect(iconFrame.transform, "DetailIcon");
            HiepUIKit.Stretch(iconRt, 10, 10, 10, 10);
            detailIcon = iconRt.gameObject.AddComponent<Image>();
            detailIcon.preserveAspect = true;
            detailIcon.enabled = false;

            detailTitleText = HiepUIKit.CreateLabel(card.transform, "DetailTitle",
                "Chọn một ô vật phẩm để xem định nghĩa từ JSON", 16,
                HiepUIKit.TextDark, TextAnchor.UpperLeft, FontStyle.Bold);
            HiepUIKit.Anchor(detailTitleText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(102, -12), new Vector2(-270, 24));

            detailMetaText = HiepUIKit.CreateLabel(card.transform, "DetailMeta",
                "Bảng dữ liệu: TaiLieu.md (Mục 7.1 Nguyên liệu • 7.2 Món ăn • 7.5 Vũ khí • 7.7 Tiêu hao)", 12,
                new Color32(0x6e, 0x47, 0x2b, 0xff), TextAnchor.UpperLeft, FontStyle.Bold);
            HiepUIKit.Anchor(detailMetaText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(102, -36), new Vector2(-270, 20));

            detailDescText = HiepUIKit.CreateLabel(card.transform, "DetailDesc",
                "• Nhấp chuột trái vào 1 ô để xem thông tin chi tiết, công thức nấu và giá trị kinh tế.\n" +
                "• Nhấp chuột trái tiếp vào ô thứ 2 để di chuyển hoặc gộp stack.\n" +
                "• Nhấp chuột phải (RMB) để chuyển nhanh giữa Balo và Thanh Quick Bar.", 12,
                HiepUIKit.TextDark, TextAnchor.UpperLeft);
            HiepUIKit.Anchor(detailDescText.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0),
                new Vector2(102, 12), new Vector2(-118, -62));

            useOrEquipButton = HiepUIKit.CreateButton(card.transform, "UseOrEquipBtn", "TRANG BỊ / DÙNG", 12,
                HiepUIKit.AccentGreen, HiepUIKit.TextDark, OnUseOrEquipSelectedClicked);
            HiepUIKit.Anchor(useOrEquipButton.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-14, -14), new Vector2(150, 36));
            useOrEquipButtonLabel = useOrEquipButton.GetComponentInChildren<Text>();
            useOrEquipButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Cửa sổ phụ cho phép duyệt toàn bộ 27 vật phẩm định nghĩa trong items_catalog.json
        /// và bấm [+ Thêm vào Balo] để kiểm thử nhanh ở bất kỳ Scene nào.
        /// </summary>
        private void BuildJsonCatalogSubPanel(Transform parent)
        {
            var panel = HiepUIKit.CreatePanel(parent, "JsonCatalogBrowser", HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(panel.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-12, 0), new Vector2(460, 650));
            jsonCatalogSubPanel = panel.gameObject;

            var inner = HiepUIKit.CreatePanel(panel.transform, "Inner", HiepUIKit.WoodMid, true);
            HiepUIKit.Stretch(inner.rectTransform, 6, 6, 6, 6);

            var title = HiepUIKit.CreateLabel(inner.transform, "Title",
                "📖 CƠ SỞ DỮ LIỆU VẬT PHẨM JSON (BÁO CÁO MỤC 7)", 15,
                HiepUIKit.AccentGold, TextAnchor.MiddleLeft, FontStyle.Bold);
            HiepUIKit.Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(12, -10), new Vector2(-24, 24));

            // Các nút lọc danh mục: Tất cả | Nguyên liệu (13) | Món ăn (10) | Tiêu hao (4)
            BuildFilterButton(inner.transform, "All", "Tất cả (27)", new Vector2(12, -38), 96);
            BuildFilterButton(inner.transform, "Ingredient", "Nguyên liệu (13)", new Vector2(114, -38), 114);
            BuildFilterButton(inner.transform, "Dish", "Món ăn (10)", new Vector2(234, -38), 96);
            BuildFilterButton(inner.transform, "Consumable", "Tiêu hao (4)", new Vector2(336, -38), 98);

            // ScrollView chứa danh sách vật phẩm từ JSON
            var scrollBg = HiepUIKit.CreatePanel(inner.transform, "ScrollBg", HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(scrollBg.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                new Vector2(0, -24), new Vector2(-20, -84));

            var viewport = HiepUIKit.CreateRect(scrollBg.transform, "Viewport");
            HiepUIKit.Stretch(viewport, 4, 4, 4, 4);
            viewport.gameObject.AddComponent<RectMask2D>();

            catalogListContent = HiepUIKit.CreateRect(viewport, "Content");
            HiepUIKit.Anchor(catalogListContent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                Vector2.zero, new Vector2(0, 800));

            var scroll = scrollBg.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = catalogListContent;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 28f;

            PopulateCatalogRows();
            jsonCatalogSubPanel.SetActive(false);
        }

        private void BuildFilterButton(Transform parent, string filterKey, string label, Vector2 pos, float width)
        {
            var btn = HiepUIKit.CreateButton(parent, $"Filter_{filterKey}", label, 11,
                HiepUIKit.WoodLight, HiepUIKit.TextLight, () =>
                {
                    activeCatalogFilter = filterKey;
                    PopulateCatalogRows();
                });
            HiepUIKit.Anchor(btn.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                pos, new Vector2(width, 26));
        }

        private void PopulateCatalogRows()
        {
            if (catalogListContent == null) return;
            for (int i = catalogListContent.childCount - 1; i >= 0; i--)
            {
                Destroy(catalogListContent.GetChild(i).gameObject);
            }

            var items = GameItemDatabase.AllItems;
            int rowIdx = 0;
            const float rowHeight = 56f;
            const float gap = 6f;

            foreach (var item in items)
            {
                if (item == null) continue;
                if (activeCatalogFilter != "All" && !string.Equals(item.category, activeCatalogFilter, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                var row = HiepUIKit.CreatePanel(catalogListContent, $"Row_{item.id}", HiepUIKit.Parchment, true);
                HiepUIKit.Anchor(row.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                    new Vector2(0, -6f - rowIdx * (rowHeight + gap)), new Vector2(-12, rowHeight));

                var iconRt = HiepUIKit.CreateRect(row.transform, "Icon");
                HiepUIKit.Anchor(iconRt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                    new Vector2(8, 0), new Vector2(38, 38));
                var iconImg = iconRt.gameObject.AddComponent<Image>();
                iconImg.sprite = item.GetIcon();
                iconImg.preserveAspect = true;

                var nameTxt = HiepUIKit.CreateLabel(row.transform, "Name",
                    $"<b>{item.displayName}</b> <color=#6E472B>({item.floor} • {item.subCategory})</color>", 12,
                    HiepUIKit.TextDark, TextAnchor.UpperLeft);
                HiepUIKit.Anchor(nameTxt.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                    new Vector2(52, -6), new Vector2(-132, 20));

                var subTxt = HiepUIKit.CreateLabel(row.transform, "Sub",
                    $"ID: {item.id} | Stack: {item.maxStack} | Giá: {item.sellPrice}G", 10,
                    new Color32(0x5a, 0x3a, 0x22, 0xff), TextAnchor.LowerLeft);
                HiepUIKit.Anchor(subTxt.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0),
                    new Vector2(52, 6), new Vector2(-132, 18));

                var capturedItem = item;
                var addBtn = HiepUIKit.CreateButton(row.transform, "AddBtn", "+ Nhận x1", 11,
                    HiepUIKit.AccentGreen, HiepUIKit.TextDark, () =>
                    {
                        SharedGameDataManager.Instance?.AddItem(capturedItem.id, 1, capturedItem.IsConsumable);
                        InspectItemDefinition(capturedItem, 1);
                    });
                HiepUIKit.Anchor(addBtn.GetComponent<RectTransform>(), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                    new Vector2(-8, 0), new Vector2(72, 34));

                rowIdx++;
            }

            catalogListContent.sizeDelta = new Vector2(0, Mathf.Max(400f, rowIdx * (rowHeight + gap) + 16f));
        }

        private void ToggleJsonCatalogSubPanel()
        {
            if (jsonCatalogSubPanel == null) return;
            bool next = !jsonCatalogSubPanel.activeSelf;
            jsonCatalogSubPanel.SetActive(next);
            if (next) PopulateCatalogRows();
        }

        // =====================================================================
        // XỬ LÝ TƯƠNG TÁC Ô VẬT PHẨM & CẬP NHẬT UI
        // =====================================================================

        private void OnSlotClicked(SlotView clicked, bool isRightClick)
        {
            var data = SharedGameDataManager.Instance;
            if (data == null) return;

            if (isRightClick)
            {
                data.QuickTransferSlot(clicked.isQuick, clicked.index);
                hasSelectedSlot = false;
                Refresh();
                return;
            }

            var clickedSlot = GetSlotData(clicked.isQuick, clicked.index);

            if (!hasSelectedSlot)
            {
                if (clickedSlot == null || clickedSlot.IsEmpty)
                {
                    InspectEmptySlot();
                    return;
                }
                hasSelectedSlot = true;
                selectedIsQuick = clicked.isQuick;
                selectedIndex = clicked.index;
                if (clicked.isQuick) data.SelectQuickSlot(clicked.index);
                InspectSlot(clickedSlot);
                RefreshSlotHighlights();
                return;
            }

            // Nếu click lại đúng ô đang chọn -> Bỏ chọn
            if (selectedIsQuick == clicked.isQuick && selectedIndex == clicked.index)
            {
                hasSelectedSlot = false;
                selectedIndex = -1;
                RefreshSlotHighlights();
                return;
            }

            // Di chuyển / Gộp stack / Đổi chỗ giữa 2 ô
            data.MoveOrSwapSlots(selectedIsQuick, selectedIndex, clicked.isQuick, clicked.index);
            hasSelectedSlot = false;
            selectedIndex = -1;

            var targetAfterMove = GetSlotData(clicked.isQuick, clicked.index);
            if (targetAfterMove != null && !targetAfterMove.IsEmpty)
            {
                InspectSlot(targetAfterMove);
            }
            Refresh();
        }

        private void OnUseOrEquipSelectedClicked()
        {
            var data = SharedGameDataManager.Instance;
            if (data == null || !hasSelectedSlot) return;

            if (selectedIsQuick)
            {
                data.UseQuickSlotItem(selectedIndex);
            }
            else
            {
                data.QuickTransferSlot(false, selectedIndex);
            }
            hasSelectedSlot = false;
            Refresh();
        }

        private SharedInventorySlot GetSlotData(bool isQuick, int index)
        {
            var data = SharedGameDataManager.Instance;
            if (data == null) return null;
            var list = isQuick ? data.QuickSlots : data.BagSlots;
            return (index >= 0 && index < list.Count) ? list[index] : null;
        }

        public void Refresh()
        {
            var data = SharedGameDataManager.Instance;
            if (data == null) return;

            if (bagViews.Count != data.BagSlots.Count)
            {
                RebuildBagSlotViews(data.BagSlots.Count);
            }

            RefreshHeaderStrip();

            for (int i = 0; i < bagViews.Count; i++)
            {
                var slot = i < data.BagSlots.Count ? data.BagSlots[i] : null;
                UpdateSlotVisual(bagViews[i], slot);
            }

            for (int i = 0; i < quickViews.Count; i++)
            {
                var slot = i < data.QuickSlots.Count ? data.QuickSlots[i] : null;
                UpdateSlotVisual(quickViews[i], slot);
            }

            RefreshSlotHighlights();
        }

        private void RefreshHeaderStrip()
        {
            var data = SharedGameDataManager.Instance;
            if (data == null) return;

            int usedBag = 0;
            foreach (var s in data.BagSlots)
            {
                if (!s.IsEmpty) usedBag++;
            }

            if (headerCapacityText != null)
            {
                headerCapacityText.text =
                    $"Sức chứa Balo: <b>{usedBag}/{data.BagSlots.Count} ô</b> (+5 ô mỗi Boss hạ gục) • Click phải (RMB) gắn nhanh xuống QuickBar";
            }

            if (walletSummaryText != null)
            {
                walletSummaryText.text = $"💰 Vàng: <b>{data.Gold:N0} G</b>   |   ⭐ Danh tiếng: <b>{data.Reputation:N0}</b>";
            }

            if (weaponSummaryText != null)
            {
                var sw = GameItemDatabase.GetWeapon("weapon_sword")?.GetLevelStats(data.SwordLevel);
                var bw = GameItemDatabase.GetWeapon("weapon_bow")?.GetLevelStats(data.BowLevel);
                int swDmg = sw != null ? sw.lightDamage : 15;
                int bwDmg = bw != null ? bw.lightDamage : 12;
                weaponSummaryText.text = $"⚔️ Kiếm <b>Lv.{data.SwordLevel}</b> ({swDmg} DMG)  |  🏹 Cung <b>Lv.{data.BowLevel}</b> ({bwDmg} DMG)";
            }
        }

        private void UpdateSlotVisual(SlotView v, SharedInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                v.icon.enabled = false;
                v.countText.text = "";
                v.nameText.text = "Trống";
                v.nameText.color = HiepUIKit.TextMuted;
            }
            else
            {
                v.icon.enabled = true;
                v.icon.sprite = slot.Icon;
                v.countText.text = $"x{slot.count}/{slot.MaxStack}";
                v.nameText.text = slot.DisplayName;
                v.nameText.color = HiepUIKit.TextLight;
            }
        }

        private void RefreshSlotHighlights()
        {
            foreach (var v in bagViews)
            {
                bool sel = hasSelectedSlot && !selectedIsQuick && selectedIndex == v.index;
                v.bg.color = sel ? HiepUIKit.SlotSelected : HiepUIKit.SlotIdle;
            }
            foreach (var v in quickViews)
            {
                bool sel = hasSelectedSlot && selectedIsQuick && selectedIndex == v.index;
                v.bg.color = sel ? HiepUIKit.SlotSelected : HiepUIKit.SlotIdle;
            }
        }

        private void InspectEmptySlot()
        {
            if (detailIcon != null) detailIcon.enabled = false;
            if (detailTitleText != null) detailTitleText.text = "Ô trống";
            if (detailMetaText != null) detailMetaText.text = "Chưa có vật phẩm trong ô này";
            if (detailDescText != null)
            {
                detailDescText.text = "Bạn có thể nhặt nguyên liệu từ quái/Boss trong Hầm Ngục, mua đồ tại Tiệm Trưởng Làng, hoặc nhấn nút [📖 DANH MỤC JSON] ở góc trên để lấy vật phẩm mẫu.";
            }
            if (useOrEquipButton != null) useOrEquipButton.gameObject.SetActive(false);
        }

        private void InspectSlot(SharedInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || slot.Definition == null)
            {
                InspectEmptySlot();
                return;
            }
            InspectItemDefinition(slot.Definition, slot.count);

            if (useOrEquipButton != null)
            {
                useOrEquipButton.gameObject.SetActive(true);
                if (useOrEquipButtonLabel != null)
                {
                    useOrEquipButtonLabel.text = selectedIsQuick ? "DÙNG NGAY" : "GẮN QUICK BAR";
                }
            }
        }

        private void InspectItemDefinition(ItemDefinitionDto def, int currentCount)
        {
            if (def == null) return;

            if (detailIcon != null)
            {
                detailIcon.enabled = true;
                detailIcon.sprite = def.GetIcon();
            }

            if (detailTitleText != null)
            {
                detailTitleText.text = $"{def.displayName}  (Đang có: x{currentCount} / Tối đa {def.maxStack} mỗi ô)";
            }

            if (detailMetaText != null)
            {
                detailMetaText.text =
                    $"Phân loại: {def.subCategory} ({def.rarity})  •  Khu vực: {def.floor}  •  Giá mua/bán: {def.buyPrice}G / {def.sellPrice}G";
            }

            if (detailDescText != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine(def.description);
                sb.Append($"• <b>Nguồn / Mở khóa:</b> {def.dropSource} ({def.unlockCondition})");

                if (def.recipeIngredients != null && def.recipeIngredients.Length > 0)
                {
                    sb.Append("\n• <b>Công thức nấu (Bảng 7.2):</b> ");
                    for (int i = 0; i < def.recipeIngredients.Length; i++)
                    {
                        var ing = def.recipeIngredients[i];
                        var ingDef = GameItemDatabase.GetItem(ing.itemId);
                        string ingName = ingDef != null ? ingDef.displayName : ing.itemId;
                        if (i > 0) sb.Append(" + ");
                        sb.Append($"{ing.count}x {ingName}");
                    }
                }
                detailDescText.text = sb.ToString();
            }
        }
    }
}
