using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NguyenVanHai.Enemies;

namespace NguyenVanHai.UI
{
    /// <summary>
    /// Bảng Hòm Đồ (Balo) trong Dungeon:
    /// - Mô hình hóa từ Inventory của Restaurant_Hien:
    /// - Gồm 10 ô hành trang (lưới 5x2) + 4 ô Quick Slots (vĩnh viễn 4 ô theo TaiLieu.md mục 3.2.1).
    /// - Phím [I] để mở/đóng hòm đồ.
    /// - Quy tắc TaiLieu.md: Không thể mở Inventory khi đang bị quái vật target/truy đuổi.
    /// - Click chuột phải (RMB) để gắn / tháo Quick item.
    /// </summary>
    public class DungeonInventoryUI : MonoBehaviour
    {
        [System.Serializable]
        public class InventoryItemEntry
        {
            public string id;
            public string name;
            public int count;
            public int maxStack = 5;
            public Sprite icon;
            public string description;

            public bool IsEmpty => string.IsNullOrEmpty(id) || count <= 0;
            public void Clear() { id = ""; name = ""; count = 0; icon = null; description = ""; }
        }

        private const int BagSlotsCount = 10;
        private const int QuickSlotsCount = 4;

        private InventoryItemEntry[] bagSlots = new InventoryItemEntry[BagSlotsCount];
        private InventoryItemEntry[] quickSlots = new InventoryItemEntry[QuickSlotsCount];

        // UI Panels
        private GameObject windowPanel;
        private GameObject quickBarPanel;
        private Text infoText;

        private List<Image> bagSlotIconImages = new List<Image>();
        private List<Text> bagSlotCountTexts = new List<Text>();
        private List<Image> quickSlotIconImages = new List<Image>();
        private List<Text> quickSlotCountTexts = new List<Text>();

        public bool IsOpen => windowPanel != null && windowPanel.activeSelf;

        private void Awake()
        {
            InitializeData();
            BuildInventoryUI();
        }

        private void InitializeData()
        {
            for (int i = 0; i < BagSlotsCount; i++) bagSlots[i] = new InventoryItemEntry();
            for (int i = 0; i < QuickSlotsCount; i++) quickSlots[i] = new InventoryItemEntry();

            // Đồ khởi đầu: 3 bình máu ở ô quick slot 1
            quickSlots[0].id = "health_potion";
            quickSlots[0].name = "Bình máu hồi phục";
            quickSlots[0].count = 3;
            quickSlots[0].description = "Hồi phục 20 điểm máu cho người chơi khi sử dụng (Phím [R] hoặc [1]).";
        }

        private void Start()
        {
            RefreshUI();
        }

        private void Update()
        {
            // Phím [I]: Bật/Tắt Hòm đồ
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (IsOpen)
                {
                    Close();
                }
                else
                {
                    TryOpen();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) && IsOpen)
            {
                Close();
            }

            // Phím 1-4 dùng Quick Item
            for (int k = 0; k < QuickSlotsCount; k++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + k) || Input.GetKeyDown(KeyCode.Keypad1 + k))
                {
                    UseQuickItem(k);
                }
            }
        }

        public bool TryOpen()
        {
            // Kiểm tra quy tắc TaiLieu.md mục 3.2.1: "Không thể mở inventory khi đang bị quái target"
            if (IsPlayerTargetedByEnemy())
            {
                ShowFloatingWarning("⚠️ Không thể mở hòm đồ khi đang bị quái vật truy đuổi!");
                return false;
            }

            if (windowPanel != null)
            {
                windowPanel.SetActive(true);
                RefreshUI();
            }
            return true;
        }

        public void Close()
        {
            if (windowPanel != null)
            {
                windowPanel.SetActive(false);
            }
        }

        private bool IsPlayerTargetedByEnemy()
        {
            var enemies = FindObjectsOfType<EnemyBase>();
            var player = FindObjectOfType<HunterPlayer>();
            if (player == null) return false;

            foreach (var e in enemies)
            {
                if (e.IsAlive)
                {
                    float dist = Vector2.Distance(e.transform.position, player.transform.position);
                    if (dist <= 7.0f) return true; // Đang trong tầm áp sát
                }
            }
            return false;
        }

        public void AddItem(string id, string name, int amount, Sprite icon = null)
        {
            // 1. Thêm gộp vào ô đã có
            for (int i = 0; i < BagSlotsCount; i++)
            {
                if (bagSlots[i].id == id && bagSlots[i].count < bagSlots[i].maxStack)
                {
                    bagSlots[i].count += amount;
                    RefreshUI();
                    return;
                }
            }

            // 2. Thêm vào ô trống đầu tiên
            for (int i = 0; i < BagSlotsCount; i++)
            {
                if (bagSlots[i].IsEmpty)
                {
                    bagSlots[i].id = id;
                    bagSlots[i].name = name;
                    bagSlots[i].count = amount;
                    bagSlots[i].icon = icon;
                    bagSlots[i].description = $"Nguyên liệu quý thu thập từ ngục tối dùng để nấu ăn tại nhà hàng.";
                    RefreshUI();
                    return;
                }
            }
        }

        private void UseQuickItem(int index)
        {
            if (index < 0 || index >= QuickSlotsCount) return;
            var slot = quickSlots[index];
            if (slot.IsEmpty) return;

            if (slot.id == "health_potion")
            {
                var player = FindObjectOfType<HunterPlayer>();
                if (player != null)
                {
                    player.UseHealthPotion();
                    slot.count--;
                    if (slot.count <= 0) slot.Clear();
                    RefreshUI();
                }
            }
        }

        private void RefreshUI()
        {
            // Cập nhật 10 ô balo
            for (int i = 0; i < BagSlotsCount && i < bagSlotIconImages.Count; i++)
            {
                var slot = bagSlots[i];
                if (!slot.IsEmpty)
                {
                    bagSlotIconImages[i].enabled = true;
                    if (slot.icon != null) bagSlotIconImages[i].sprite = slot.icon;
                    bagSlotCountTexts[i].text = slot.count > 1 ? slot.count.ToString() : "";
                }
                else
                {
                    bagSlotIconImages[i].enabled = false;
                    bagSlotCountTexts[i].text = "";
                }
            }

            // Cập nhật 4 ô quick bar
            for (int i = 0; i < QuickSlotsCount && i < quickSlotIconImages.Count; i++)
            {
                var slot = quickSlots[i];
                if (!slot.IsEmpty)
                {
                    quickSlotIconImages[i].enabled = true;
                    if (slot.icon != null) quickSlotIconImages[i].sprite = slot.icon;
                    quickSlotCountTexts[i].text = slot.count > 1 ? slot.count.ToString() : "";
                }
                else
                {
                    quickSlotIconImages[i].enabled = false;
                    quickSlotCountTexts[i].text = "";
                }
            }
        }

        private string warningMsg = "";
        private float warningTimer = 0f;

        private void ShowFloatingWarning(string msg)
        {
            warningMsg = msg;
            warningTimer = 2.0f;
        }

        private void OnGUI()
        {
            if (warningTimer > 0f)
            {
                warningTimer -= Time.deltaTime;
                GUI.backgroundColor = new Color(0.8f, 0.1f, 0.1f, 0.9f);
                GUI.contentColor = Color.white;
                GUI.Box(new Rect(Screen.width / 2 - 250, 120, 500, 40), warningMsg);
            }
        }

        /// <summary>
        /// Dựng giao diện Hòm đồ uGUI tự động, chuẩn tông gỗ/đá tối gothic.
        /// </summary>
        private void BuildInventoryUI()
        {
            var canvasGo = new GameObject("Dungeon_Inventory_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Hàng Quick Bar (Góc dưới giữa màn hình)
            quickBarPanel = CreateRect("QuickBarPanel", canvasGo.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(360f, 80f));
            AddImageBg(quickBarPanel, new Color(0.12f, 0.08f, 0.06f, 0.9f));

            for (int i = 0; i < QuickSlotsCount; i++)
            {
                float x = -135f + (i * 90f);
                var slotGo = CreateRect($"QuickSlot_{i}", quickBarPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(76f, 64f));
                AddImageBg(slotGo, new Color(0.25f, 0.18f, 0.12f, 1f));

                // Số phím [1..4]
                var keyGo = CreateRect("KeyText", slotGo.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(5f, -5f), new Vector2(25f, 20f));
                AddUIText(keyGo, $"[{i + 1}]", font, 12, new Color(1f, 0.85f, 0.4f), TextAnchor.UpperLeft);

                // Icon
                var iconGo = CreateRect("Icon", slotGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44f, 44f));
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.enabled = false;
                quickSlotIconImages.Add(iconImg);

                // Số lượng
                var countGo = CreateRect("Count", slotGo.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-6f, 4f), new Vector2(30f, 20f));
                var countTxt = AddUIText(countGo, "", font, 14, Color.white, TextAnchor.LowerRight);
                quickSlotCountTexts.Add(countTxt);
            }

            // 2. Cửa sổ Hòm đồ chính (Lệch phải màn hình, chuẩn theo mẫu Restaurant_Hien)
            windowPanel = CreateRect("InventoryWindow", canvasGo.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-60f, 20f), new Vector2(520f, 620f));
            AddImageBg(windowPanel, new Color(0.14f, 0.1f, 0.08f, 0.95f));

            // Tiêu đề
            var titleGo = CreateRect("Title", windowPanel.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -20f), new Vector2(-48f, 40f));
            AddUIText(titleGo, "🎒 HÒM ĐỒ DUNGEON (PHÍM [I])", font, 24, new Color(1f, 0.88f, 0.5f), TextAnchor.UpperLeft);

            // Nút đóng X
            var closeGo = CreateRect("CloseBtn", windowPanel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(40f, 40f));
            AddImageBg(closeGo, new Color(0.7f, 0.2f, 0.2f, 1f));
            var closeBtn = closeGo.AddComponent<Button>();
            closeBtn.onClick.AddListener(Close);
            var closeTxtGo = CreateRect("Text", closeGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddUIText(closeTxtGo, "X", font, 20, Color.white, TextAnchor.MiddleCenter);

            // Lưới 10 ô Balo (5 cột x 2 hàng)
            var gridGo = CreateRect("BagGrid", windowPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(460f, 190f));
            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(80f, 80f);
            grid.spacing = new Vector2(15f, 15f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;

            for (int i = 0; i < BagSlotsCount; i++)
            {
                var slotGo = CreateRect($"BagSlot_{i}", gridGo.transform, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(80f, 80f));
                AddImageBg(slotGo, new Color(0.24f, 0.16f, 0.12f, 1f));

                var iconGo = CreateRect("Icon", slotGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(56f, 56f));
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.enabled = false;
                bagSlotIconImages.Add(iconImg);

                var countGo = CreateRect("Count", slotGo.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-6f, 4f), new Vector2(35f, 22f));
                var countTxt = AddUIText(countGo, "", font, 16, Color.white, TextAnchor.LowerRight);
                bagSlotCountTexts.Add(countTxt);
            }

            // Bảng thông tin chi tiết item bên dưới
            var descPanel = CreateRect("DescPanel", windowPanel.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(460f, 260f));
            AddImageBg(descPanel, new Color(0.1f, 0.07f, 0.05f, 1f));

            var descTextGo = CreateRect("DescText", descPanel.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(15f, 15f), new Vector2(-30f, -30f));
            infoText = AddUIText(descTextGo, "<b>HƯỚNG DẪN CHIẾN ĐẤU</b>\n• Thu thập nguyên liệu từ quái vật để nấu ăn tại nhà hàng.\n• Balo chứa tối đa 10 ô, mỗi ô 5 stack.\n• Nhấn <b>[RMB]</b> vào ô để gán nhanh xuống hàng Quick Slot.", font, 16, new Color(0.9f, 0.85f, 0.75f), TextAnchor.UpperLeft);

            windowPanel.SetActive(false); // Ẩn mặc định
        }

        private GameObject CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go;
        }

        private Image AddImageBg(GameObject go, Color color)
        {
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private Text AddUIText(GameObject go, string text, Font font, int size, Color color, TextAnchor align)
        {
            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.font = font;
            txt.fontSize = size;
            txt.color = color;
            txt.alignment = align;
            txt.supportRichText = true;
            return txt;
        }
    }
}
