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
    /// Giao diện Hòm đồ (Túi đồ / Balo) của người chơi khi nhấn phím [I] trong làng.
    /// Cho phép xem 10 ô hành trang, 4 ô Quick item, điểm Reputation (1000) và Vàng (10000).
    /// </summary>
    public class VillageInventoryUI : MonoBehaviour
    {
        public static VillageInventoryUI Instance { get; private set; }

        private RectTransform rootPanel;
        private RectTransform window;

        private Text goldText;
        private Text repText;
        private Text capacityText;

        private readonly List<Image> slotFrames = new List<Image>();
        private readonly List<Image> slotIcons = new List<Image>();
        private readonly List<Text> slotCounts = new List<Text>();

        private readonly List<Image> quickIcons = new List<Image>();
        private readonly List<Text> quickCounts = new List<Text>();

        private Image detailIcon;
        private Text detailNameText;
        private Text detailDescText;
        private Button assignQuickBtn;

        private int selectedIndex = -1;

        public bool IsOpen => rootPanel != null && rootPanel.gameObject.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildUI();
            CloseInventory();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (VillageWallet.Instance != null)
            {
                VillageWallet.Instance.OnChanged += RefreshUI;
            }
            if (VillageInventory.Instance != null)
            {
                VillageInventory.Instance.OnInventoryChanged += RefreshUI;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                // Nếu Shop hoặc Dialogue đang mở thì không toggle bằng I
                if (VillageShopUI.Instance != null && VillageShopUI.Instance.IsOpen) return;
                if (VillageDialogueUI.Instance != null && VillageDialogueUI.Instance.IsOpen) return;

                ToggleInventory();
            }
            else if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseInventory();
            }
        }

        private void BuildUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            rootPanel = HauUITheme.CreateBox(transform, "InventoryOverlay", new Color(0, 0, 0, 0.6f), true).rectTransform;
            HauUITheme.FillParent(rootPanel);

            float winW = 680f;
            float winH = 680f;
            window = HauUITheme.CreatePanel(rootPanel, "InventoryWindow", HauUITheme.WoodDarkest, true).rectTransform;
            HauUITheme.Anchor(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(winW, winH));

            // Header
            var header = HauUITheme.CreatePanel(window, "Header", HauUITheme.WoodNormal, false);
            HauUITheme.Anchor(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -6), new Vector2(-12, 54));

            var titleLbl = HauUITheme.CreateLabel(header.transform, "Title", "🎒 HÀNH TRANG & TÚI ĐỒ (BALO)", 24, HauUITheme.GoldYellow, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(titleLbl.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18, 0), new Vector2(400, 36));

            var closeBtn = HauUITheme.CreateButton(header.transform, "CloseBtn", "✕ [ESC/I]", 16, HauUITheme.BtnRed, CloseInventory);
            HauUITheme.Anchor(closeBtn.image.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12, 0), new Vector2(110, 36));

            // Stats bar
            var statsBar = HauUITheme.CreatePanel(window, "Stats", HauUITheme.WoodDark, false);
            HauUITheme.Anchor(statsBar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -66), new Vector2(-24, 46));

            goldText = HauUITheme.CreateLabel(statsBar.transform, "Gold", "💰 VÀNG: 10,000", 20, HauUITheme.GoldYellow, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(goldText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(15, 0), new Vector2(200, 30));

            repText = HauUITheme.CreateLabel(statsBar.transform, "Rep", "⭐ DANH TIẾNG: 1,000", 18, HauUITheme.RepCyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(repText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(230, 0), new Vector2(230, 30));

            capacityText = HauUITheme.CreateLabel(statsBar.transform, "Cap", "4/10 Ô", 17, HauUITheme.TextDim, TextAnchor.MiddleRight, FontStyle.Bold);
            HauUITheme.Anchor(capacityText.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-15, 0), new Vector2(120, 30));

            // Grid 10 slots
            var gridBox = HauUITheme.CreatePanel(window, "GridBox", HauUITheme.WoodDark, false);
            HauUITheme.Anchor(gridBox.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -120), new Vector2(-24, 195));

            float slotW = 95f;
            float padX = 22f;
            float padY = 14f;
            float stepX = (winW - 24f - padX * 2 - slotW) / 4f;
            float stepY = slotW + 12f;

            slotFrames.Clear();
            slotIcons.Clear();
            slotCounts.Clear();

            for (int i = 0; i < VillageInventory.DefaultSlotCount; i++)
            {
                int index = i;
                int col = i % 5;
                int row = i / 5;

                float x = padX + col * stepX;
                float y = -padY - row * stepY;

                var frame = HauUITheme.CreatePanel(gridBox.transform, $"Slot_{i}", HauUITheme.SlotBg, true);
                HauUITheme.Anchor(frame.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y), new Vector2(slotW, slotW));

                var icon = HauUITheme.CreateIcon(frame.transform, "Icon", null);
                HauUITheme.FillParent(icon.rectTransform, 10, 10, 10, 10);
                icon.enabled = false;

                var count = HauUITheme.CreateLabel(frame.transform, "Count", "", 18, HauUITheme.TextWhite, TextAnchor.LowerRight, FontStyle.Bold);
                HauUITheme.Anchor(count.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-4, 4), new Vector2(40, 22));

                var btn = frame.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() => OnSelectSlot(index));

                slotFrames.Add(frame);
                slotIcons.Add(icon);
                slotCounts.Add(count);
            }

            // Quick bar
            var qBox = HauUITheme.CreatePanel(window, "QuickBar", HauUITheme.WoodDark, false);
            HauUITheme.Anchor(qBox.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -325), new Vector2(-24, 88));

            var qTitle = HauUITheme.CreateLabel(qBox.transform, "QTitle", "THANH QUICK ITEM (4 Ô):", 16, HauUITheme.TextCream, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(qTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(15, -4), new Vector2(-30, 22));

            quickIcons.Clear();
            quickCounts.Clear();
            float qSize = 56f;
            float qPadX = 30f;
            float qStepX = (winW - 24f - qPadX * 2 - qSize) / 3f;

            for (int q = 0; q < VillageInventory.QuickSlotCount; q++)
            {
                int qIndex = q;
                float x = qPadX + q * qStepX;

                var qFrame = HauUITheme.CreatePanel(qBox.transform, $"Quick_{q}", HauUITheme.SlotBg, true);
                HauUITheme.Anchor(qFrame.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(x, 8), new Vector2(qSize, qSize));

                var numLbl = HauUITheme.CreateLabel(qFrame.transform, "Num", $"[{q + 1}]", 14, HauUITheme.GoldYellow, TextAnchor.UpperLeft, FontStyle.Bold);
                HauUITheme.Anchor(numLbl.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(4, -2), new Vector2(25, 18));

                var qIcon = HauUITheme.CreateIcon(qFrame.transform, "Icon", null);
                HauUITheme.FillParent(qIcon.rectTransform, 6, 6, 6, 6);
                qIcon.enabled = false;

                var qCount = HauUITheme.CreateLabel(qFrame.transform, "Count", "", 14, HauUITheme.TextWhite, TextAnchor.LowerRight, FontStyle.Bold);
                HauUITheme.Anchor(qCount.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-3, 3), new Vector2(30, 16));

                var btn = qFrame.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    if (VillageInventory.Instance != null && VillageInventory.Instance.QuickSlotIndices[qIndex] != -1)
                    {
                        VillageInventory.Instance.ClearQuickSlot(qIndex);
                        RefreshUI();
                    }
                });

                quickIcons.Add(qIcon);
                quickCounts.Add(qCount);
            }

            // Detail Panel
            var detail = HauUITheme.CreatePanel(window, "Detail", HauUITheme.WoodDarkest, false);
            HauUITheme.Anchor(detail.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0f), new Vector2(0, 15), new Vector2(-24, -430));

            detailIcon = HauUITheme.CreateIcon(detail.transform, "DetailIcon", null);
            HauUITheme.Anchor(detailIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(45, 0), new Vector2(70, 70));

            detailNameText = HauUITheme.CreateLabel(detail.transform, "Name", "Chọn 1 ô hành trang để xem chi tiết", 20, HauUITheme.GoldYellow, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(detailNameText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(95, -12), new Vector2(-105, 30));

            detailDescText = HauUITheme.CreateLabel(detail.transform, "Desc", "Dùng phím [WASD] để di chuyển trong làng, [E] để tương tác với NPC hoặc vào nhà Trưởng làng.", 15, HauUITheme.TextDim, TextAnchor.UpperLeft);
            HauUITheme.Anchor(detailDescText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(95, 45), new Vector2(-105, -46));

            assignQuickBtn = HauUITheme.CreateButton(detail.transform, "QuickBtn", "GÁN VÀO QUICK ITEM", 16, HauUITheme.BtnBlue, OnAssignQuick);
            HauUITheme.Anchor(assignQuickBtn.image.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-15, 12), new Vector2(200, 40));
            assignQuickBtn.gameObject.SetActive(false);
        }

        public void ToggleInventory()
        {
            if (IsOpen) CloseInventory();
            else OpenInventory();
        }

        public void OpenInventory()
        {
            if (rootPanel != null) rootPanel.gameObject.SetActive(true);
            selectedIndex = -1;
            RefreshUI();
            UpdateDetail();
        }

        public void CloseInventory()
        {
            if (rootPanel != null) rootPanel.gameObject.SetActive(false);
        }

        private void OnSelectSlot(int index)
        {
            selectedIndex = index;
            RefreshUI();
            UpdateDetail();
        }

        private void OnAssignQuick()
        {
            var inv = VillageInventory.Instance;
            if (inv == null || selectedIndex < 0) return;

            int targetQ = 0;
            for (int q = 0; q < VillageInventory.QuickSlotCount; q++)
            {
                if (inv.QuickSlotIndices[q] == -1)
                {
                    targetQ = q;
                    break;
                }
            }

            inv.SetQuickSlot(targetQ, selectedIndex);
            RefreshUI();
        }

        private void RefreshUI()
        {
            var wallet = VillageWallet.Instance;
            var inv = VillageInventory.Instance;

            if (wallet != null)
            {
                if (goldText != null) goldText.text = $"💰 VÀNG: {wallet.Gold:N0}";
                if (repText != null) repText.text = $"⭐ DANH TIẾNG: {wallet.Reputation:N0}";
            }

            if (inv != null)
            {
                if (capacityText != null)
                    capacityText.text = $"{inv.GetUsedSlotCount()}/{VillageInventory.DefaultSlotCount} Ô";

                for (int i = 0; i < slotFrames.Count; i++)
                {
                    var slot = inv.Slots[i];
                    slotFrames[i].color = (i == selectedIndex) ? HauUITheme.SlotSelect : HauUITheme.SlotBg;

                    if (!slot.IsEmpty)
                    {
                        slotIcons[i].sprite = slot.item.icon != null ? slot.item.icon : HauUITheme.White;
                        slotIcons[i].enabled = true;
                        slotCounts[i].text = $"{slot.count}/5";
                    }
                    else
                    {
                        slotIcons[i].enabled = false;
                        slotCounts[i].text = "";
                    }
                }

                for (int q = 0; q < quickIcons.Count; q++)
                {
                    var itemSlot = inv.GetQuickSlotItem(q);
                    if (itemSlot != null && !itemSlot.IsEmpty)
                    {
                        quickIcons[q].sprite = itemSlot.item.icon != null ? itemSlot.item.icon : HauUITheme.White;
                        quickIcons[q].enabled = true;
                        quickCounts[q].text = $"{itemSlot.count}";
                    }
                    else
                    {
                        quickIcons[q].enabled = false;
                        quickCounts[q].text = "";
                    }
                }
            }
        }

        private void UpdateDetail()
        {
            var inv = VillageInventory.Instance;
            if (inv == null || selectedIndex < 0 || selectedIndex >= inv.Slots.Count)
            {
                detailNameText.text = "Chọn 1 ô hành trang để xem chi tiết";
                detailDescText.text = "Dùng phím [WASD] để di chuyển trong làng, [E] để tương tác với NPC hoặc vào nhà Trưởng làng.";
                detailIcon.enabled = false;
                assignQuickBtn.gameObject.SetActive(false);
                return;
            }

            var slot = inv.Slots[selectedIndex];
            if (slot.IsEmpty)
            {
                detailNameText.text = "Ô hành trang trống";
                detailDescText.text = "Ô này chưa có vật phẩm.";
                detailIcon.enabled = false;
                assignQuickBtn.gameObject.SetActive(false);
            }
            else
            {
                var it = slot.item;
                detailNameText.text = $"{it.displayName} <color=#94a3b8>[{it.category}]</color>";
                detailDescText.text = $"{it.description}\nSố lượng: {slot.count}/5 stack | Giá bán: {it.sellPrice} Vàng";
                detailIcon.sprite = it.icon != null ? it.icon : HauUITheme.White;
                detailIcon.enabled = true;
                assignQuickBtn.gameObject.SetActive(true);
            }
        }
    }
}
