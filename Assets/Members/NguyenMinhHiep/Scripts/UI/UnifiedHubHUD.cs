using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NguyenMinhHiep.Core;
using NguyenMinhHiep.Data;

namespace NguyenMinhHiep.UI
{
    /// <summary>
    /// Giao diện HUD cố định trên màn hình của Unified Canvas Hub (tích hợp cho cả 3 Scene: Dungeon, Village, Restaurant).
    /// Lấy format chuẩn từ NgoVanHien (TavernHUD.cs + QuickBar.cs + UIKit.cs):
    /// - Góc trái trên: Khung gỗ 9-slice hiển thị Tên khu vực, Thanh Máu (HP), Thanh Thể Lực (Stamina), Vàng (Gold) và Danh Tiếng (Reputation).
    /// - Góc trái dưới: Khung Vũ khí (Kiếm & Cung, cấp độ Lv.1-4, thông số sát thương từ JSON, phím [Q] chuyển đổi).
    /// - Chính giữa dưới: Thanh Quick Bar 4 ô vật phẩm tiêu hao ([1]-[4], [Tab], [R] dùng bình máu).
    /// - Góc phải trên: Bảng phím tắt & nút mở nhanh Túi đồ [I].
    /// </summary>
    public class UnifiedHubHUD : MonoBehaviour
    {
        // Top-Left: Scene, HP, Stamina, Gold, Reputation
        private Text sceneBadgeText;
        private Image hpFillBar;
        private Text hpValueText;
        private Image staminaFillBar;
        private Text staminaValueText;
        private Text goldValueText;
        private Text repValueText;

        // Bottom-Left: Weapon Widget (Sword & Bow)
        private Image swordSlotBg;
        private Image bowSlotBg;
        private Text activeWeaponTitleText;
        private Text activeWeaponStatsText;

        // Bottom-Center: QuickBar 4 slots
        private class QuickSlotView
        {
            public Image bg;
            public Image icon;
            public Text countText;
            public Text nameText;
        }
        private readonly List<QuickSlotView> quickSlotViews = new List<QuickSlotView>();

        // Toast Notification
        private GameObject toastPanel;
        private Text toastText;
        private float toastTimer;

        private UnifiedGameHubCanvas masterHub;

        public void Initialize(UnifiedGameHubCanvas hub)
        {
            masterHub = hub;
            BuildHUDLayout();
            BindEvents();
            RefreshAll();
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
                data.OnWalletChanged += RefreshWallet;
                data.OnVitalsChanged += RefreshVitals;
                data.OnWeaponChanged += RefreshWeapons;
                data.OnInventoryChanged += RefreshQuickBar;
                data.OnNotification += ShowToast;
            }
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void UnbindEvents()
        {
            var data = SharedGameDataManager.Instance;
            if (data != null)
            {
                data.OnWalletChanged -= RefreshWallet;
                data.OnVitalsChanged -= RefreshVitals;
                data.OnWeaponChanged -= RefreshWeapons;
                data.OnInventoryChanged -= RefreshQuickBar;
                data.OnNotification -= ShowToast;
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshSceneBadge(scene.name);
            RefreshAll();
        }

        private void Update()
        {
            if (toastTimer > 0f)
            {
                toastTimer -= Time.unscaledDeltaTime;
                if (toastTimer <= 0f && toastPanel != null)
                {
                    toastPanel.SetActive(false);
                }
            }
        }

        // =====================================================================
        // DỰNG GIAO DIỆN THEO FORMAT NGÔ VĂN HIẾN (TavernHUD + QuickBar)
        // =====================================================================

        private void BuildHUDLayout()
        {
            BuildTopLeftStatusCard();
            BuildBottomLeftWeaponCard();
            BuildBottomCenterQuickBar();
            BuildTopRightControlsCard();
            BuildToastBanner();
        }

        /// <summary>
        /// 1. Góc trên bên trái: Máu, Thể lực, Tiền (Vàng), Danh tiếng (Format gỗ 9-slice của TavernHUD.cs).
        /// </summary>
        private void BuildTopLeftStatusCard()
        {
            var card = HiepUIKit.CreatePanel(transform, "TopLeft_StatsAndWalletCard", HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(card.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(20, -20), new Vector2(360, 218));

            var inner = HiepUIKit.CreatePanel(card.transform, "Inner", HiepUIKit.WoodMid, true);
            HiepUIKit.Stretch(inner.rectTransform, 6, 6, 6, 6);

            // Header tên Scene hiện tại
            var sceneHeader = HiepUIKit.CreatePanel(inner.transform, "SceneHeader", HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(sceneHeader.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -6), new Vector2(-16, 28));

            sceneBadgeText = HiepUIKit.CreateLabel(sceneHeader.transform, "SceneBadge", "⚔️ KHU VỰC: HẦM NGỤC", 13,
                HiepUIKit.AccentGold, TextAnchor.MiddleCenter, FontStyle.Bold);
            HiepUIKit.Stretch(sceneBadgeText.rectTransform, 6, 6, 2, 2);

            // Thanh Máu (HP Bar)
            BuildVitalBarRow(inner.transform, "HPRow", -40f, "❤️ MÁU (HP)", HiepUIKit.HpBarBg, HiepUIKit.HpBarFill,
                out hpFillBar, out hpValueText);

            // Thanh Thể Lực (Stamina Bar)
            BuildVitalBarRow(inner.transform, "StaminaRow", -76f, "⚡ THỂ LỰC", HiepUIKit.StaminaBarBg, HiepUIKit.StaminaBarFill,
                out staminaFillBar, out staminaValueText);

            // Dòng Vàng & Danh tiếng (2 cột ngang chuẩn format TavernHUD.cs của Ngô Văn Hiến)
            Sprite goldSprite = LoadEditorOrFallbackSprite("Assets/Members/NgoVanHien/Sprites/Icons/Icon_Gold.png", "gold_coin", "#FFD24A");
            Sprite repSprite = LoadEditorOrFallbackSprite("Assets/Members/NgoVanHien/Sprites/Icons/Icon_Reputation.png", "rep_star", "#63B4F6");

            goldValueText = BuildCurrencySubCard(inner.transform, "GoldCard", new Vector2(8, -114), new Vector2(162, 42),
                goldSprite, "TIỀN VÀNG", HiepUIKit.AccentGold);

            repValueText = BuildCurrencySubCard(inner.transform, "RepCard", new Vector2(178, -114), new Vector2(162, 42),
                repSprite, "DANH TIẾNG", HiepUIKit.AccentBlue);

            // Dòng chân: Nút mở nhanh Hòm đồ & thông tin sức chứa
            var bagBtn = HiepUIKit.CreateButton(inner.transform, "OpenInventoryButton",
                "🎒 TÚI ĐỒ & VẬT PHẨM [I]", 13, HiepUIKit.WoodLight, HiepUIKit.TextLight, () =>
                {
                    if (masterHub != null) masterHub.ToggleInventory();
                });
            HiepUIKit.Anchor(bagBtn.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0),
                new Vector2(0, 8), new Vector2(-16, 34));
        }

        private void BuildVitalBarRow(Transform parent, string name, float yOffset, string labelTitle,
            Color barBgColor, Color barFillColor, out Image fillImg, out Text valTxt)
        {
            var row = HiepUIKit.CreatePanel(parent, name, HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(row.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, yOffset), new Vector2(-16, 30));

            var lbl = HiepUIKit.CreateLabel(row.transform, "Title", labelTitle, 12, HiepUIKit.TextLight,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            HiepUIKit.Anchor(lbl.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f),
                new Vector2(8, 0), new Vector2(96, 0));

            var barBg = HiepUIKit.CreatePanel(row.transform, "BarBg", barBgColor, true);
            HiepUIKit.Anchor(barBg.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 0.5f),
                new Vector2(104, 0), new Vector2(-112, 20));

            fillImg = HiepUIKit.CreateFillBar(barBg.transform, "Fill", barFillColor);
            HiepUIKit.Stretch(fillImg.rectTransform, 2, 2, 2, 2);

            valTxt = HiepUIKit.CreateLabel(barBg.transform, "Value", "100 / 100", 12, Color.white,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            HiepUIKit.Stretch(valTxt.rectTransform, 0, 0, 0, 0);
        }

        private Text BuildCurrencySubCard(Transform parent, string name, Vector2 topLeftPos, Vector2 size,
            Sprite iconSprite, string title, Color valueColor)
        {
            var row = HiepUIKit.CreatePanel(parent, name, HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(row.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                topLeftPos, size);

            var iconRt = HiepUIKit.CreateRect(row.transform, "Icon");
            HiepUIKit.Anchor(iconRt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(8, 0), new Vector2(26, 26));
            var iconImg = iconRt.gameObject.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.preserveAspect = true;

            var lbl = HiepUIKit.CreateLabel(row.transform, "Title", title, 10, HiepUIKit.TextMuted,
                TextAnchor.UpperLeft, FontStyle.Bold);
            HiepUIKit.Anchor(lbl.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(38, -4), new Vector2(-42, 14));

            var val = HiepUIKit.CreateLabel(row.transform, "Value", "0", 16, valueColor,
                TextAnchor.LowerLeft, FontStyle.Bold);
            HiepUIKit.Anchor(val.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0),
                new Vector2(38, 4), new Vector2(-42, 22));
            return val;
        }

        /// <summary>
        /// 2. Góc dưới bên trái: Khung Vũ khí (Kiếm & Cung) theo Bảng 7.5 báo cáo.
        /// </summary>
        private void BuildBottomLeftWeaponCard()
        {
            var card = HiepUIKit.CreatePanel(transform, "BottomLeft_WeaponCard", HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(card.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
                new Vector2(20, 18), new Vector2(330, 114));

            var inner = HiepUIKit.CreatePanel(card.transform, "Inner", HiepUIKit.WoodMid, true);
            HiepUIKit.Stretch(inner.rectTransform, 5, 5, 5, 5);

            // 2 Ô biểu tượng vũ khí (Kiếm & Cung)
            swordSlotBg = BuildWeaponSlotIcon(inner.transform, "SwordSlot", new Vector2(10, -10), "⚔️", "KIẾM", 0);
            bowSlotBg = BuildWeaponSlotIcon(inner.transform, "BowSlot", new Vector2(72, -10), "🏹", "CUNG", 1);

            // Thông tin chi tiết vũ khí đang chọn
            activeWeaponTitleText = HiepUIKit.CreateLabel(inner.transform, "WeaponTitle",
                "⚔️ KIẾM THỢ SĂN (Lv.1)", 14, HiepUIKit.AccentGold, TextAnchor.UpperLeft, FontStyle.Bold);
            HiepUIKit.Anchor(activeWeaponTitleText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(138, -8), new Vector2(-146, 22));

            activeWeaponStatsText = HiepUIKit.CreateLabel(inner.transform, "WeaponStats",
                "Đánh thường: 15 | Mạnh: 30\nKỹ năng [E]: 40 DMG\n<color=#FFD24A>[Q] Đổi Vũ khí</color>", 11,
                HiepUIKit.TextLight, TextAnchor.UpperLeft);
            HiepUIKit.Anchor(activeWeaponStatsText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(138, -30), new Vector2(-146, 68));
        }

        private Image BuildWeaponSlotIcon(Transform parent, string name, Vector2 pos, string symbol, string shortLabel, int weaponIndex)
        {
            var box = HiepUIKit.CreatePanel(parent, name, HiepUIKit.SlotIdle, true);
            HiepUIKit.Anchor(box.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                pos, new Vector2(56, 84));

            var symTxt = HiepUIKit.CreateLabel(box.transform, "Symbol", symbol, 24, HiepUIKit.TextLight,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            HiepUIKit.Anchor(symTxt.rectTransform, new Vector2(0, 0.35f), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);

            var nameTxt = HiepUIKit.CreateLabel(box.transform, "ShortName", shortLabel, 11, HiepUIKit.TextMuted,
                TextAnchor.LowerCenter, FontStyle.Bold);
            HiepUIKit.Anchor(nameTxt.rectTransform, new Vector2(0, 0), new Vector2(1, 0.35f), new Vector2(0.5f, 0),
                new Vector2(0, 5), Vector2.zero);

            var trigger = box.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(_ =>
            {
                if (SharedGameDataManager.Instance != null)
                {
                    SharedGameDataManager.Instance.SetActiveWeapon(weaponIndex);
                }
            });
            trigger.triggers.Add(entry);

            return box;
        }

        /// <summary>
        /// 3. Chính giữa phía dưới: Thanh Quick Bar 4 ô vĩnh viễn (Chuẩn format QuickBar.cs của Ngô Văn Hiến).
        /// </summary>
        private void BuildBottomCenterQuickBar()
        {
            const int slotCount = 4;
            const float slotSize = 68f;
            const float gap = 8f;
            const float pad = 10f;
            float width = slotCount * slotSize + (slotCount - 1) * gap + pad * 2;
            float height = slotSize + pad * 2 + 22f;

            var bg = HiepUIKit.CreatePanel(transform, "BottomCenter_QuickBar", HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(bg.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 16), new Vector2(width, height));

            var inner = HiepUIKit.CreatePanel(bg.transform, "Inner", HiepUIKit.WoodMid, true);
            HiepUIKit.Stretch(inner.rectTransform, 4, 4, 4, 4);

            var title = HiepUIKit.CreateLabel(inner.transform, "Title",
                "THANH DÙNG NHANH  •  [1-4] Dùng vật phẩm  •  [R] Uống Bình Máu", 11,
                HiepUIKit.AccentGold, TextAnchor.MiddleCenter, FontStyle.Bold);
            HiepUIKit.Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, -3), new Vector2(0, 16));

            quickSlotViews.Clear();
            float startX = -((slotCount - 1) * (slotSize + gap)) * 0.5f;
            for (int i = 0; i < slotCount; i++)
            {
                var slotBg = HiepUIKit.CreatePanel(inner.transform, $"QuickSlot_{i}", HiepUIKit.SlotIdle, true);
                HiepUIKit.Anchor(slotBg.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                    new Vector2(startX + i * (slotSize + gap), 8), new Vector2(slotSize, slotSize));

                var iconRt = HiepUIKit.CreateRect(slotBg.transform, "Icon");
                HiepUIKit.Stretch(iconRt, 10, 10, 14, 14);
                var iconImg = iconRt.gameObject.AddComponent<Image>();
                iconImg.preserveAspect = true;
                iconImg.enabled = false;

                var keyTxt = HiepUIKit.CreateLabel(slotBg.transform, "Key", $"[{i + 1}]", 11, HiepUIKit.AccentGold,
                    TextAnchor.UpperLeft, FontStyle.Bold);
                HiepUIKit.Stretch(keyTxt.rectTransform, 4, 4, 3, 3);

                var countTxt = HiepUIKit.CreateLabel(slotBg.transform, "Count", "", 13, HiepUIKit.TextLight,
                    TextAnchor.LowerRight, FontStyle.Bold);
                HiepUIKit.Stretch(countTxt.rectTransform, 4, 5, 3, 14);

                var nameTxt = HiepUIKit.CreateLabel(slotBg.transform, "Name", "Trống", 9, HiepUIKit.TextMuted,
                    TextAnchor.LowerCenter);
                HiepUIKit.Anchor(nameTxt.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0),
                    new Vector2(0, 2), new Vector2(-4, 12));

                int captured = i;
                var trigger = slotBg.gameObject.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entry.callback.AddListener(ev =>
                {
                    var pev = (PointerEventData)ev;
                    if (pev.button == PointerEventData.InputButton.Right)
                    {
                        SharedGameDataManager.Instance?.QuickTransferSlot(true, captured);
                    }
                    else
                    {
                        SharedGameDataManager.Instance?.UseQuickSlotItem(captured);
                    }
                });
                trigger.triggers.Add(entry);

                quickSlotViews.Add(new QuickSlotView
                {
                    bg = slotBg,
                    icon = iconImg,
                    countText = countTxt,
                    nameText = nameTxt
                });
            }
        }

        /// <summary>
        /// 4. Góc trên bên phải: Hướng dẫn phím tắt chuẩn format TavernHUD.cs của Ngô Văn Hiến.
        /// </summary>
        private void BuildTopRightControlsCard()
        {
            var hint = HiepUIKit.CreatePanel(transform, "TopRight_ControlsHint", new Color(0.16f, 0.10f, 0.06f, 0.88f), true);
            HiepUIKit.Anchor(hint.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-20, -20), new Vector2(300, 108));

            var txt = HiepUIKit.CreateLabel(hint.transform, "Text",
                "<b>ĐIỀU KHIỂN CHUNG (HUB 3 SCENE)</b>\n" +
                "[WASD] Di chuyển   [Space] Lướt né\n" +
                "[Q] Đổi Kiếm/Cung   [R] Bình máu\n" +
                "[I] Mở Túi đồ & JSON   [1-4] Dùng nhanh",
                12, HiepUIKit.TextLight, TextAnchor.UpperLeft);
            HiepUIKit.Stretch(txt.rectTransform, 10, 10, 8, 8);
        }

        private void BuildToastBanner()
        {
            var panel = HiepUIKit.CreatePanel(transform, "HubToastBanner", HiepUIKit.WoodDark, true);
            HiepUIKit.Anchor(panel.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -100), new Vector2(560, 44));
            toastPanel = panel.gameObject;

            toastText = HiepUIKit.CreateLabel(panel.transform, "ToastText", "", 14, HiepUIKit.AccentGold,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            HiepUIKit.Stretch(toastText.rectTransform, 12, 12, 4, 4);

            toastPanel.SetActive(false);
        }

        // =====================================================================
        // CẬP NHẬT DỮ LIỆU TỪ SHARED GAME DATA MANAGER
        // =====================================================================

        public void RefreshAll()
        {
            RefreshSceneBadge(SceneManager.GetActiveScene().name);
            RefreshVitals();
            RefreshWallet();
            RefreshWeapons();
            RefreshQuickBar();
        }

        public void RefreshSceneBadge(string sceneName)
        {
            if (sceneBadgeText == null) return;
            string lower = (sceneName ?? "").ToLowerInvariant();
            if (lower.Contains("village") || lower.Contains("hau"))
            {
                sceneBadgeText.text = "🏘️ KHU VỰC: NGÔI LÀNG LONDO";
            }
            else if (lower.Contains("restaurant") || lower.Contains("tavern") || lower.Contains("hien"))
            {
                sceneBadgeText.text = "🍲 KHU VỰC: NHÀ HÀNG QUÁI VẬT";
            }
            else
            {
                sceneBadgeText.text = "⚔️ KHU VỰC: HẦM NGỤC (DUNGEON)";
            }
        }

        public void RefreshVitals()
        {
            var data = SharedGameDataManager.Instance;
            if (data == null) return;

            if (hpFillBar != null)
            {
                hpFillBar.fillAmount = Mathf.Clamp01(data.CurrentHP / Mathf.Max(1f, data.MaxHP));
            }
            if (hpValueText != null)
            {
                hpValueText.text = $"{Mathf.CeilToInt(data.CurrentHP)} / {Mathf.CeilToInt(data.MaxHP)}";
            }

            if (staminaFillBar != null)
            {
                staminaFillBar.fillAmount = Mathf.Clamp01(data.CurrentStamina / Mathf.Max(1f, data.MaxStamina));
            }
            if (staminaValueText != null)
            {
                staminaValueText.text = $"{Mathf.CeilToInt(data.CurrentStamina)} / {Mathf.CeilToInt(data.MaxStamina)}";
            }
        }

        public void RefreshWallet()
        {
            var data = SharedGameDataManager.Instance;
            if (data == null) return;

            if (goldValueText != null) goldValueText.text = $"{data.Gold:N0} G";
            if (repValueText != null) repValueText.text = $"{data.Reputation:N0}";
        }

        public void RefreshWeapons()
        {
            var data = SharedGameDataManager.Instance;
            if (data == null) return;

            bool isSword = data.ActiveWeaponIndex == 0;
            if (swordSlotBg != null)
                swordSlotBg.color = isSword ? HiepUIKit.SlotSelected : HiepUIKit.SlotIdle;
            if (bowSlotBg != null)
                bowSlotBg.color = !isSword ? HiepUIKit.SlotSelected : HiepUIKit.SlotIdle;

            var wpDef = data.ActiveWeaponDef;
            int lv = data.ActiveWeaponLevel;
            if (wpDef != null)
            {
                var stats = wpDef.GetLevelStats(lv);
                string iconPrefix = isSword ? "⚔️" : "🏹";
                if (activeWeaponTitleText != null)
                {
                    activeWeaponTitleText.text = $"{iconPrefix} {wpDef.displayName.ToUpper()} (Lv.{lv})";
                }
                if (activeWeaponStatsText != null)
                {
                    activeWeaponStatsText.text =
                        $"Đánh thường: <b>{stats.lightDamage}</b>  |  Mạnh: <b>{stats.heavyDamage}</b>\n" +
                        $"Kỹ năng [E]: <b>{stats.skillDamage} DMG</b>\n" +
                        $"<color=#FFD24A>[Q] Đổi sang {(isSword ? "Cung Tầm Xa" : "Kiếm Thợ Săn")}</color>";
                }
            }
        }

        public void RefreshQuickBar()
        {
            var data = SharedGameDataManager.Instance;
            if (data == null) return;

            for (int i = 0; i < quickSlotViews.Count; i++)
            {
                var v = quickSlotViews[i];
                var slot = i < data.QuickSlots.Count ? data.QuickSlots[i] : null;
                bool selected = i == data.SelectedQuickSlot;
                v.bg.color = selected ? HiepUIKit.SlotSelected : HiepUIKit.SlotIdle;

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
                    v.countText.text = slot.count > 1 ? $"x{slot.count}" : "x1";
                    v.nameText.text = ShortenName(slot.DisplayName, 10);
                    v.nameText.color = selected ? HiepUIKit.TextDark : HiepUIKit.TextLight;
                }
            }
        }

        public void ShowToast(string message)
        {
            if (toastPanel == null || toastText == null) return;
            toastText.text = message;
            toastPanel.SetActive(true);
            toastTimer = 2.6f;
        }

        private static string ShortenName(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= maxLen) return s;
            return s.Substring(0, maxLen - 1) + "…";
        }

        private static Sprite LoadEditorOrFallbackSprite(string assetPath, string key, string hexColor)
        {
#if UNITY_EDITOR
            var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sp != null) return sp;
#endif
            return HiepSprites.GetOrCreateIconBadge(key, hexColor, "Currency");
        }
    }
}
