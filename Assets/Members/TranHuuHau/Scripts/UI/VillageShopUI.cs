using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DungeonEater.TranHuuHau.Common;
using DungeonEater.TranHuuHau.Data;
using DungeonEater.TranHuuHau.Inventory;

namespace DungeonEater.TranHuuHau.UI
{
    /// <summary>
    /// Giao diện Màn hình Mua Đồ tại Nhà Trưởng Làng theo yêu cầu người dùng:
    /// - Cột bên trái: Cửa hàng Trưởng làng (Bình máu, Lựu đạn nổ/băng/độc, Nguyên liệu).
    /// - Cột bên phải: Hòm đồ (Giỏ đồ) người chơi: Vàng (10,000), Danh tiếng (1,000), 10 ô hành trang, 4 ô quick item.
    /// - Cơ chế: Mua đồ (trừ tiền, cộng đồ nếu còn chỗ), Bán đồ (trừ đồ, cộng tiền), gán Quick item.
    /// </summary>
    public class VillageShopUI : MonoBehaviour
    {
        public static VillageShopUI Instance { get; private set; }

        private RectTransform rootPanel;
        private RectTransform window;

        // Cột trái (Shop)
        private RectTransform shopListContainer;

        // Cột phải (Inventory)
        private Text walletGoldText;
        private Text walletRepText;
        private Text capacityText;
        private readonly List<InventorySlotUI> inventorySlotUIs = new List<InventorySlotUI>();
        private readonly List<QuickSlotUI> quickSlotUIs = new List<QuickSlotUI>();

        // Khung chi tiết & Bán đồ bên phải
        private RectTransform detailPanel;
        private Image detailIcon;
        private Text detailNameText;
        private Text detailDescText;
        private Text detailSellPriceText;
        private Button sellButton;
        private Button assignQuickButton;
        private Text toastMessageText;
        private float toastTimer = 0f;

        private int selectedBackpackIndex = -1;

        public bool IsOpen => rootPanel != null && rootPanel.gameObject.activeSelf;
        public event Action OnShopClosed;

        private class InventorySlotUI
        {
            public int index;
            public Image slotFrame;
            public Image slotIcon;
            public Text countText;
            public Button button;
        }

        private class QuickSlotUI
        {
            public int quickIndex;
            public Image slotFrame;
            public Image slotIcon;
            public Text numberText;
            public Text countText;
            public Button button;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildUI();
            CloseShop();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (VillageWallet.Instance != null)
            {
                VillageWallet.Instance.OnChanged += RefreshRightPanel;
            }
            if (VillageInventory.Instance != null)
            {
                VillageInventory.Instance.OnInventoryChanged += RefreshRightPanel;
            }
        }

        private void Update()
        {
            if (!IsOpen) return;

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            {
                CloseShop();
            }

            if (toastTimer > 0f)
            {
                toastTimer -= Time.unscaledDeltaTime;
                if (toastTimer <= 0f && toastMessageText != null)
                {
                    toastMessageText.text = "";
                }
            }
        }

        // ========================== DỰNG GIAO DIỆN UGUI ==========================

        private void BuildUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var cGo = new GameObject("VillageShopCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = cGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = cGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                transform.SetParent(canvas.transform, false);
            }

            // Nền tối mờ toàn màn hình
            rootPanel = HauUITheme.CreateBox(transform, "ShopOverlay", new Color(0, 0, 0, 0.72f), true).rectTransform;
            HauUITheme.FillParent(rootPanel);

            // Cửa sổ chính
            float winW = 1240f;
            float winH = 720f;
            window = HauUITheme.CreatePanel(rootPanel, "ShopWindow", HauUITheme.WoodDarkest, true).rectTransform;
            HauUITheme.Anchor(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(winW, winH));

            // Thanh tiêu đề
            var header = HauUITheme.CreatePanel(window, "Header", HauUITheme.WoodNormal, false);
            HauUITheme.Anchor(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -6), new Vector2(-12, 64));

            var titleLbl = HauUITheme.CreateLabel(header.transform, "Title", "🏪 TIỆM TIẾP TẾ TRƯỞNG LÀNG GODWIN", 28, HauUITheme.GoldYellow, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(titleLbl.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(25, 0), new Vector2(700, 40));

            // Nút đóng [X]
            var closeBtn = HauUITheme.CreateButton(header.transform, "CloseBtn", "✕ ĐÓNG [ESC]", 18, HauUITheme.BtnRed, CloseShop);
            HauUITheme.Anchor(closeBtn.image.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-15, 0), new Vector2(150, 42));

            // Đường phân cách dọc chia đôi 2 cột (Trái: Shop, Phải: Inventory)
            var colSplit = HauUITheme.CreateBox(window, "ColSplit", HauUITheme.WoodBorder, false);
            HauUITheme.Anchor(colSplit.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0, -38), new Vector2(3, -90));

            // ================== CỘT BÊN TRÁI: CỬA HÀNG ==================
            BuildLeftShopColumn(window);

            // ================== CỘT BÊN PHẢI: GIỎ ĐỒ / HÀNH TRANG ==================
            BuildRightInventoryColumn(window);

            // Thông báo Toast feedback ở cạnh đáy
            toastMessageText = HauUITheme.CreateLabel(window, "ToastText", "", 20, HauUITheme.GoldYellow, TextAnchor.MiddleCenter, FontStyle.Bold);
            HauUITheme.Anchor(toastMessageText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 18), new Vector2(800, 30));
        }

        private void BuildLeftShopColumn(RectTransform parent)
        {
            var colLeft = HauUITheme.CreateRect("ShopColumn", parent);
            HauUITheme.Anchor(colLeft, new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, 1f), new Vector2(20, -78), new Vector2(-30, -96));

            // Tiêu đề cột Shop
            var subHeader = HauUITheme.CreateLabel(colLeft, "ShopSubHeader", "DANH MỤC MUA VẬT PHẨM (TRƯỞNG LÀNG BÁN)", 20, HauUITheme.TextCream, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(subHeader.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(8, 0), new Vector2(-16, 30));

            // Container chứa danh sách vật phẩm
            var scrollArea = HauUITheme.CreatePanel(colLeft, "ScrollArea", HauUITheme.WoodDark, false);
            HauUITheme.Anchor(scrollArea.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0, -18), new Vector2(0, -36));

            shopListContainer = HauUITheme.CreateRect("ListContainer", scrollArea.transform);
            HauUITheme.FillParent(shopListContainer, 8, 8, 8, 8);

            // Sinh các hàng vật phẩm shop
            PopulateShopItems();
        }

        private void PopulateShopItems()
        {
            var catalog = VillageItemData.GetShopCatalog();
            float itemH = 82f;
            float gap = 8f;

            for (int i = 0; i < catalog.Count; i++)
            {
                var item = catalog[i];
                float topY = -i * (itemH + gap);

                var row = HauUITheme.CreatePanel(shopListContainer, $"ShopRow_{item.id}", HauUITheme.SlotBg, true);
                HauUITheme.Anchor(row.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, topY), new Vector2(0, itemH));

                // Icon
                var iconBox = HauUITheme.CreatePanel(row.transform, "IconBox", HauUITheme.WoodNormal, false);
                HauUITheme.Anchor(iconBox.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40, 0), new Vector2(64, 64));

                var iconImg = HauUITheme.CreateIcon(iconBox.transform, "Icon", item.icon);
                HauUITheme.FillParent(iconImg.rectTransform, 6, 6, 6, 6);

                // Tên & Nhãn loại
                var nameLbl = HauUITheme.CreateLabel(row.transform, "Name", $"<b>{item.displayName}</b> <color=#94a3b8>[{item.category}]</color>", 19, HauUITheme.TextWhite, TextAnchor.MiddleLeft);
                HauUITheme.Anchor(nameLbl.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(85, -8), new Vector2(-200, 26));

                // Mô tả
                var descLbl = HauUITheme.CreateLabel(row.transform, "Desc", item.description, 14, HauUITheme.TextDim, TextAnchor.UpperLeft);
                HauUITheme.Anchor(descLbl.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(85, 8), new Vector2(-200, -36));

                // Giá tiền
                var priceLbl = HauUITheme.CreateLabel(row.transform, "Price", $"💰 {item.buyPrice}", 19, HauUITheme.GoldYellow, TextAnchor.MiddleRight, FontStyle.Bold);
                HauUITheme.Anchor(priceLbl.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-115, 0), new Vector2(90, 36));

                // Nút Mua
                var buyBtn = HauUITheme.CreateButton(row.transform, "BuyBtn", "MUA", 17, HauUITheme.BtnGreen, () => BuyItem(item));
                HauUITheme.Anchor(buyBtn.image.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12, 0), new Vector2(90, 44));
            }
        }

        private void BuildRightInventoryColumn(RectTransform parent)
        {
            var colRight = HauUITheme.CreateRect("InventoryColumn", parent);
            HauUITheme.Anchor(colRight, new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(20, -78), new Vector2(-20, -96));

            // Thanh thông tin Vàng + Danh tiếng của người chơi (Mock 10,000 vàng, 1,000 rep)
            var statsBar = HauUITheme.CreatePanel(colRight, "StatsBar", HauUITheme.WoodDark, false);
            HauUITheme.Anchor(statsBar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 0), new Vector2(0, 52));

            walletGoldText = HauUITheme.CreateLabel(statsBar.transform, "GoldText", "💰 VÀNG: 10,000", 21, HauUITheme.GoldYellow, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(walletGoldText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16, 0), new Vector2(230, 36));

            walletRepText = HauUITheme.CreateLabel(statsBar.transform, "RepText", "⭐ DANH TIẾNG: 1,000", 19, HauUITheme.RepCyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(walletRepText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(250, 0), new Vector2(230, 36));

            capacityText = HauUITheme.CreateLabel(statsBar.transform, "CapText", "🎒 4/10 Ô", 18, HauUITheme.TextDim, TextAnchor.MiddleRight, FontStyle.Bold);
            HauUITheme.Anchor(capacityText.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16, 0), new Vector2(120, 36));

            // Tiêu đề Lưới 10 ô hòm đồ
            var bpTitle = HauUITheme.CreateLabel(colRight, "BpTitle", "HÀNH TRANG (10 Ô - TỐI ĐA 5 STACK/Ô):", 17, HauUITheme.TextCream, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(bpTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(4, -60), new Vector2(-8, 24));

            // Lưới 10 ô hành trang (2 hàng x 5 cột)
            var gridBox = HauUITheme.CreatePanel(colRight, "BackpackGrid", HauUITheme.WoodDark, false);
            HauUITheme.Anchor(gridBox.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -88), new Vector2(0, 180));

            float slotSize = 78f;
            float padX = 20f;
            float padY = 12f;
            float stepX = (570f - padX * 2 - slotSize) / 4f;
            float stepY = slotSize + 10f;

            inventorySlotUIs.Clear();
            for (int i = 0; i < VillageInventory.DefaultSlotCount; i++)
            {
                int col = i % 5;
                int row = i / 5;
                int slotIndex = i;

                float xPos = padX + col * stepX;
                float yPos = -padY - row * stepY;

                var slotFrame = HauUITheme.CreatePanel(gridBox.transform, $"Slot_{i}", HauUITheme.SlotBg, true);
                HauUITheme.Anchor(slotFrame.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(xPos, yPos), new Vector2(slotSize, slotSize));

                var iconImg = HauUITheme.CreateIcon(slotFrame.transform, "Icon", null);
                HauUITheme.FillParent(iconImg.rectTransform, 8, 8, 8, 8);
                iconImg.enabled = false;

                var countLbl = HauUITheme.CreateLabel(slotFrame.transform, "Count", "", 16, HauUITheme.TextWhite, TextAnchor.LowerRight, FontStyle.Bold);
                HauUITheme.Anchor(countLbl.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-4, 4), new Vector2(40, 20));

                var btn = slotFrame.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() => OnSelectBackpackSlot(slotIndex));

                inventorySlotUIs.Add(new InventorySlotUI
                {
                    index = i,
                    slotFrame = slotFrame,
                    slotIcon = iconImg,
                    countText = countLbl,
                    button = btn
                });
            }

            // Thanh 4 ô Quick Item
            var qTitle = HauUITheme.CreateLabel(colRight, "QTitle", "THANH QUICK ITEM (4 Ô):", 17, HauUITheme.TextCream, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(qTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(4, -276), new Vector2(-8, 24));

            var quickBox = HauUITheme.CreatePanel(colRight, "QuickBox", HauUITheme.WoodDark, false);
            HauUITheme.Anchor(quickBox.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -304), new Vector2(0, 80));

            quickSlotUIs.Clear();
            float qSize = 62f;
            float qPadX = 35f;
            float qStepX = (570f - qPadX * 2 - qSize) / 3f;

            for (int q = 0; q < VillageInventory.QuickSlotCount; q++)
            {
                int qIndex = q;
                float x = qPadX + q * qStepX;

                var qFrame = HauUITheme.CreatePanel(quickBox.transform, $"QuickSlot_{q}", HauUITheme.SlotBg, true);
                HauUITheme.Anchor(qFrame.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, 0), new Vector2(qSize, qSize));

                var numLbl = HauUITheme.CreateLabel(qFrame.transform, "Number", $"[{q + 1}]", 14, HauUITheme.GoldYellow, TextAnchor.UpperLeft, FontStyle.Bold);
                HauUITheme.Anchor(numLbl.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(4, -2), new Vector2(30, 20));

                var qIcon = HauUITheme.CreateIcon(qFrame.transform, "Icon", null);
                HauUITheme.FillParent(qIcon.rectTransform, 6, 6, 6, 6);
                qIcon.enabled = false;

                var qCount = HauUITheme.CreateLabel(qFrame.transform, "Count", "", 14, HauUITheme.TextWhite, TextAnchor.LowerRight, FontStyle.Bold);
                HauUITheme.Anchor(qCount.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-3, 3), new Vector2(30, 16));

                var qBtn = qFrame.gameObject.AddComponent<Button>();
                qBtn.onClick.AddListener(() => OnClickQuickSlot(qIndex));

                quickSlotUIs.Add(new QuickSlotUI
                {
                    quickIndex = q,
                    slotFrame = qFrame,
                    slotIcon = qIcon,
                    numberText = numLbl,
                    countText = qCount,
                    button = qBtn
                });
            }

            // Khung Chi Tiết & Bán đồ bên dưới
            detailPanel = HauUITheme.CreatePanel(colRight, "DetailPanel", HauUITheme.WoodDarkest, false).rectTransform;
            HauUITheme.Anchor(detailPanel, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0f), new Vector2(0, 0), new Vector2(0, -394));

            detailIcon = HauUITheme.CreateIcon(detailPanel.transform, "DetailIcon", null);
            HauUITheme.Anchor(detailIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(42, 0), new Vector2(60, 60));

            detailNameText = HauUITheme.CreateLabel(detailPanel.transform, "DetailName", "Chọn 1 ô hành trang để xem / bán đồ", 18, HauUITheme.GoldYellow, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(detailNameText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(85, -10), new Vector2(-95, 26));

            detailDescText = HauUITheme.CreateLabel(detailPanel.transform, "DetailDesc", "Bấm vào các ô hành trang ở trên để thao tác.", 14, HauUITheme.TextDim, TextAnchor.UpperLeft);
            HauUITheme.Anchor(detailDescText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(85, 45), new Vector2(-95, -42));

            detailSellPriceText = HauUITheme.CreateLabel(detailPanel.transform, "DetailSellPrice", "", 16, HauUITheme.GoldYellow, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(detailSellPriceText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(85, 12), new Vector2(180, 28));

            sellButton = HauUITheme.CreateButton(detailPanel.transform, "SellBtn", "BÁN 1 CÁI", 16, HauUITheme.BtnGreen, OnSellSelectedSlot);
            HauUITheme.Anchor(sellButton.image.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-12, 10), new Vector2(130, 38));
            sellButton.gameObject.SetActive(false);

            assignQuickButton = HauUITheme.CreateButton(detailPanel.transform, "AssignQuickBtn", "GÁN QUICK [1]", 15, HauUITheme.BtnBlue, OnAssignToQuick);
            HauUITheme.Anchor(assignQuickButton.image.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-152, 10), new Vector2(140, 38));
            assignQuickButton.gameObject.SetActive(false);
        }

        // ========================== CÁC HÀM XỬ LÝ LOGIC MUA / BÁN ==========================

        public void OpenShop()
        {
            if (rootPanel != null) rootPanel.gameObject.SetActive(true);
            selectedBackpackIndex = -1;
            RefreshRightPanel();
            UpdateDetailPanel();
        }

        public void CloseShop()
        {
            if (rootPanel != null) rootPanel.gameObject.SetActive(false);
            OnShopClosed?.Invoke();
        }

        private void BuyItem(VillageItemData item)
        {
            var wallet = VillageWallet.Instance;
            var inv = VillageInventory.Instance;

            if (wallet == null || inv == null) return;

            if (wallet.Gold < item.buyPrice)
            {
                ShowToast($"<color=#ef4444>Không đủ vàng! Cần {item.buyPrice} vàng (hiện có: {wallet.Gold})</color>");
                return;
            }

            if (!inv.CanAddItem(item, 1))
            {
                ShowToast("<color=#ef4444>Hành trang đã đầy (10 ô, tối đa 5 stack/ô)!</color>");
                return;
            }

            wallet.SpendGold(item.buyPrice);
            inv.AddItem(item, 1);
            ShowToast($"<color=#86efac>Đã mua thành công 1x {item.displayName}! (-{item.buyPrice} Vàng)</color>");
            RefreshRightPanel();
        }

        private void OnSelectBackpackSlot(int index)
        {
            selectedBackpackIndex = index;
            RefreshRightPanel();
            UpdateDetailPanel();
        }

        private void OnSellSelectedSlot()
        {
            var inv = VillageInventory.Instance;
            var wallet = VillageWallet.Instance;

            if (inv == null || wallet == null || selectedBackpackIndex < 0 || selectedBackpackIndex >= inv.Slots.Count)
                return;

            var slot = inv.Slots[selectedBackpackIndex];
            if (slot.IsEmpty) return;

            int price = slot.item.sellPrice;
            string name = slot.item.displayName;

            inv.RemoveItem(selectedBackpackIndex, 1);
            wallet.AddGold(price);

            ShowToast($"<color=#86efac>Đã bán 1x {name} nhận lại +{price} Vàng!</color>");
            RefreshRightPanel();
            UpdateDetailPanel();
        }

        private void OnAssignToQuick()
        {
            var inv = VillageInventory.Instance;
            if (inv == null || selectedBackpackIndex < 0) return;

            // Tìm quick slot trống đầu tiên, hoặc gán vào slot 0
            int targetQuick = 0;
            for (int q = 0; q < VillageInventory.QuickSlotCount; q++)
            {
                if (inv.QuickSlotIndices[q] == -1)
                {
                    targetQuick = q;
                    break;
                }
            }

            inv.SetQuickSlot(targetQuick, selectedBackpackIndex);
            ShowToast($"<color=#38bdf8>Đã gán {inv.Slots[selectedBackpackIndex].item.displayName} vào Quick Item [{targetQuick + 1}]!</color>");
            RefreshRightPanel();
        }

        private void OnClickQuickSlot(int quickIndex)
        {
            var inv = VillageInventory.Instance;
            if (inv == null) return;

            if (inv.QuickSlotIndices[quickIndex] != -1)
            {
                inv.ClearQuickSlot(quickIndex);
                ShowToast($"<color=#fca5a5>Đã tháo vật phẩm khỏi Quick Item [{quickIndex + 1}]</color>");
                RefreshRightPanel();
            }
        }

        private void RefreshRightPanel()
        {
            var wallet = VillageWallet.Instance;
            var inv = VillageInventory.Instance;

            if (wallet != null)
            {
                if (walletGoldText != null) walletGoldText.text = $"💰 VÀNG: {wallet.Gold:N0}";
                if (walletRepText != null) walletRepText.text = $"⭐ DANH TIẾNG: {wallet.Reputation:N0}";
            }

            if (inv != null)
            {
                if (capacityText != null)
                    capacityText.text = $"🎒 {inv.GetUsedSlotCount()}/{VillageInventory.DefaultSlotCount} Ô";

                // Cập nhật 10 ô hòm đồ
                for (int i = 0; i < inventorySlotUIs.Count; i++)
                {
                    var ui = inventorySlotUIs[i];
                    var slot = inv.Slots[i];

                    bool isSelected = (i == selectedBackpackIndex);
                    ui.slotFrame.color = isSelected ? HauUITheme.SlotSelect : HauUITheme.SlotBg;

                    if (!slot.IsEmpty)
                    {
                        ui.slotIcon.sprite = slot.item.icon != null ? slot.item.icon : HauUITheme.White;
                        ui.slotIcon.enabled = true;
                        ui.countText.text = $"{slot.count}/5";
                    }
                    else
                    {
                        ui.slotIcon.enabled = false;
                        ui.countText.text = "";
                    }
                }

                // Cập nhật 4 ô quick item
                for (int q = 0; q < quickSlotUIs.Count; q++)
                {
                    var qUI = quickSlotUIs[q];
                    var itemSlot = inv.GetQuickSlotItem(q);

                    if (itemSlot != null && !itemSlot.IsEmpty)
                    {
                        qUI.slotIcon.sprite = itemSlot.item.icon != null ? itemSlot.item.icon : HauUITheme.White;
                        qUI.slotIcon.enabled = true;
                        qUI.countText.text = $"{itemSlot.count}";
                    }
                    else
                    {
                        qUI.slotIcon.enabled = false;
                        qUI.countText.text = "";
                    }
                }
            }
        }

        private void UpdateDetailPanel()
        {
            var inv = VillageInventory.Instance;
            if (inv == null || selectedBackpackIndex < 0 || selectedBackpackIndex >= inv.Slots.Count)
            {
                detailNameText.text = "Chọn 1 ô hành trang để xem / bán đồ";
                detailDescText.text = "Bấm vào các ô hành trang ở trên để thao tác.";
                detailSellPriceText.text = "";
                detailIcon.enabled = false;
                sellButton.gameObject.SetActive(false);
                assignQuickButton.gameObject.SetActive(false);
                return;
            }

            var slot = inv.Slots[selectedBackpackIndex];
            if (slot.IsEmpty)
            {
                detailNameText.text = "Ô hành trang trống";
                detailDescText.text = "Hãy mua thêm vật phẩm hoặc thu thập nguyên liệu từ hầm ngục.";
                detailSellPriceText.text = "";
                detailIcon.enabled = false;
                sellButton.gameObject.SetActive(false);
                assignQuickButton.gameObject.SetActive(false);
            }
            else
            {
                var it = slot.item;
                detailNameText.text = $"{it.displayName} <color=#94a3b8>[{it.category}]</color>";
                detailDescText.text = $"{it.description}\nSố lượng đang có: {slot.count} / {it.maxStack} stack.";
                detailSellPriceText.text = $"Giá bán lại: 💰 {it.sellPrice} Vàng";
                detailIcon.sprite = it.icon != null ? it.icon : HauUITheme.White;
                detailIcon.enabled = true;
                sellButton.gameObject.SetActive(true);
                assignQuickButton.gameObject.SetActive(true);
            }
        }

        private void ShowToast(string message)
        {
            if (toastMessageText != null)
            {
                toastMessageText.text = message;
                toastTimer = 2.8f;
            }
        }
    }
}
