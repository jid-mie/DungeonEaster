using UnityEngine;
using UnityEngine.UI;
using NguyenVanHai.Enemies;

namespace NguyenVanHai.UI
{
    /// <summary>
    /// HUD chiến đấu của Dungeon:
    /// 1. Thanh Máu (HP Bar đỏ gothic) & Thể lực (Stamina Bar vàng/xanh ngọc).
    /// 2. Hiển thị vũ khí đang dùng (Kiếm/Cung) và hướng dẫn phím tắt (Q, LMB, RMB, E).
    /// 3. Hàng Quick Slot 4 ô vĩnh viễn (phím 1-4) và phím R dùng bình máu theo TaiLieu.md.
    /// 4. Thanh máu Boss King Boar ở đỉnh màn hình khi Boss kích hoạt.
    /// </summary>
    public class DungeonHUD : MonoBehaviour
    {
        private HunterPlayer player;

        // UI References
        private Image hpBarFill;
        private Text hpText;
        private Image staminaBarFill;
        private Text staminaText;
        private Text weaponText;
        private Text potionText;

        // Boss Bar
        private GameObject bossBarPanel;
        private Image bossBarFill;
        private Text bossNameText;
        private BossKingBoarAI activeBoss;

        private void Awake()
        {
            BuildHUDUI();
        }

        private void Start()
        {
            player = FindObjectOfType<HunterPlayer>();
            if (player != null)
            {
                player.OnHealthChanged += UpdateHealth;
                player.OnStaminaChanged += UpdateStamina;
                player.OnWeaponSwapped += UpdateWeapon;
                player.OnPotionCountChanged += UpdatePotion;

                UpdateHealth(player.CurrentHealth, 100f);
                UpdateStamina(player.CurrentStamina, 100f);
                UpdateWeapon(player.CurrentWeapon);
                UpdatePotion(player.PotionCount);
            }

            BossKingBoarAI.OnBossSpawned += ShowBossBar;
            BossKingBoarAI.OnBossDefeated += HideBossBar;
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnHealthChanged -= UpdateHealth;
                player.OnStaminaChanged -= UpdateStamina;
                player.OnWeaponSwapped -= UpdateWeapon;
                player.OnPotionCountChanged -= UpdatePotion;
            }

            BossKingBoarAI.OnBossSpawned -= ShowBossBar;
            BossKingBoarAI.OnBossDefeated -= HideBossBar;
        }

        private void Update()
        {
            if (activeBoss != null && activeBoss.IsAlive && bossBarFill != null)
            {
                bossBarFill.fillAmount = Mathf.Lerp(bossBarFill.fillAmount, activeBoss.HealthPercent, Time.deltaTime * 5f);
            }
        }

        private void UpdateHealth(float current, float max)
        {
            if (hpBarFill != null) hpBarFill.fillAmount = Mathf.Clamp01(current / max);
            if (hpText != null) hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        private void UpdateStamina(float current, float max)
        {
            if (staminaBarFill != null) staminaBarFill.fillAmount = Mathf.Clamp01(current / max);
            if (staminaText != null) staminaText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        private void UpdateWeapon(HunterPlayer.WeaponType weapon)
        {
            if (weaponText != null)
            {
                if (weapon == HunterPlayer.WeaponType.Sword)
                {
                    weaponText.text = "⚔️ [KIẾM]\n<size=13>LMB: Combo 3 hit | RMB: Cleave | E: Whirlwind\n[Q] Đổi sang Cung</size>";
                }
                else
                {
                    weaponText.text = "🏹 [CUNG TẦM XA]\n<size=13>LMB: Bắn nhanh | Hold LMB: Gồng lực | E: Xuyên thấu\n[Q] Đổi sang Kiếm</size>";
                }
            }
        }

        private void UpdatePotion(int count)
        {
            if (potionText != null)
            {
                potionText.text = $"🧪 Bình máu: x{count}  (Phím [R] hồi 20 HP)";
            }
        }

        private void ShowBossBar(BossKingBoarAI boss)
        {
            activeBoss = boss;
            if (bossBarPanel != null) bossBarPanel.SetActive(true);
        }

        private void HideBossBar()
        {
            activeBoss = null;
            if (bossBarPanel != null) bossBarPanel.SetActive(false);
        }

        /// <summary>
        /// Dựng toàn bộ Canvas HUD bằng mã nguồn C# chuẩn uGUI, không phụ thuộc prefab hay font ngoài.
        /// </summary>
        private void BuildHUDUI()
        {
            // 1. Kiểm tra hoặc tạo Canvas
            var canvasGo = new GameObject("Dungeon_HUD_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // Font mặc định
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 2. Cụm Máu & Thể lực (Góc trên bên trái)
            var statsPanel = CreateUIObject("PlayerStatsPanel", canvasGo.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(360f, 110f));
            AddBackground(statsPanel, new Color(0.1f, 0.1f, 0.12f, 0.85f));

            // Thanh Máu
            var hpBg = CreateUIObject("HP_Background", statsPanel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(15f, -15f), new Vector2(330f, 26f));
            AddBackground(hpBg, new Color(0.2f, 0.05f, 0.05f, 1f));

            var hpFillGo = CreateUIObject("HP_Fill", hpBg.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            hpBarFill = AddImageFill(hpFillGo, new Color(0.85f, 0.15f, 0.15f, 1f));

            var hpTextGo = CreateUIObject("HP_Text", hpBg.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            hpText = AddText(hpTextGo, "100 / 100", defaultFont, 16, Color.white, TextAnchor.MiddleCenter);

            // Thanh Thể Lực
            var stBg = CreateUIObject("Stamina_Background", statsPanel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(15f, -48f), new Vector2(330f, 20f));
            AddBackground(stBg, new Color(0.1f, 0.15f, 0.05f, 1f));

            var stFillGo = CreateUIObject("Stamina_Fill", stBg.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            staminaBarFill = AddImageFill(stFillGo, new Color(0.2f, 0.8f, 0.25f, 1f));

            var stTextGo = CreateUIObject("Stamina_Text", stBg.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            staminaText = AddText(stTextGo, "100 / 100", defaultFont, 14, Color.white, TextAnchor.MiddleCenter);

            // Potion counter
            var potGo = CreateUIObject("Potion_Text", statsPanel.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(15f, 12f), new Vector2(330f, 24f));
            potionText = AddText(potGo, "🧪 Bình máu: x3  (Phím [R] hồi 20 HP)", defaultFont, 14, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleLeft);

            // 3. Cụm hiển thị Vũ khí (Góc dưới bên trái)
            var weaponPanel = CreateUIObject("WeaponPanel", canvasGo.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 40f), new Vector2(420f, 85f));
            AddBackground(weaponPanel, new Color(0.1f, 0.1f, 0.12f, 0.85f));

            var wpTextGo = CreateUIObject("Weapon_Text", weaponPanel.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(15f, 10f), new Vector2(390f, 65f));
            weaponText = AddText(wpTextGo, "⚔️ [KIẾM]\n<size=13>LMB: Combo 3 hit | RMB: Cleave | E: Whirlwind\n[Q] Đổi sang Cung</size>", defaultFont, 16, Color.white, TextAnchor.MiddleLeft);

            // 4. Thanh Máu Boss (Đỉnh màn hình giữa)
            bossBarPanel = CreateUIObject("BossBarPanel", canvasGo.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(650f, 50f));
            AddBackground(bossBarPanel, new Color(0.12f, 0.05f, 0.05f, 0.9f));

            var bossNameGo = CreateUIObject("BossName", bossBarPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 18f), new Vector2(500f, 26f));
            bossNameText = AddText(bossNameGo, "👑 KING BOAR - BÁ CHỦ ĐẦM LẦY", defaultFont, 18, new Color(1f, 0.3f, 0.3f), TextAnchor.MiddleCenter);

            var bossFillBg = CreateUIObject("BossFillBg", bossBarPanel.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(8f, 8f), new Vector2(634f, 34f));
            AddBackground(bossFillBg, new Color(0.3f, 0.05f, 0.05f, 1f));

            var bossFillGo = CreateUIObject("BossFill", bossFillBg.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            bossBarFill = AddImageFill(bossFillGo, new Color(0.9f, 0.15f, 0.15f, 1f));

            bossBarPanel.SetActive(false); // Ẩn mặc định
        }

        private GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
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

        private Image AddBackground(GameObject go, Color color)
        {
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private Image AddImageFill(GameObject go, Color color)
        {
            var img = go.AddComponent<Image>();
            img.color = color;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Left;
            img.fillAmount = 1f;
            return img;
        }

        private Text AddText(GameObject go, string content, Font font, int fontSize, Color color, TextAnchor align)
        {
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.font = font;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.supportRichText = true;
            return txt;
        }
    }
}
