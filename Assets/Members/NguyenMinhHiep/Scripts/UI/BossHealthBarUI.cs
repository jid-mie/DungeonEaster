using UnityEngine;
using UnityEngine.UI;
using NguyenMinhHiep.Bosses;

namespace NguyenMinhHiep.UI
{
    /// <summary>
    /// Giao diện Thanh máu Boss (Boss Health Bar uGUI) đặt tại đỉnh màn hình:
    /// - Hiển thị Tên Boss, Danh hiệu, Số Máu hiện tại/tối đa và phần trăm máu.
    /// - Hỗ trợ tự động hiển thị khi Boss xuất hiện và biến mất khi Boss bị hạ gục.
    /// - Hỗ trợ hiển thị thanh máu kép (Dual Boss Bar) cho Đôi Hiệp Sĩ Tầng 4.
    /// </summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        public static BossHealthBarUI Instance { get; private set; }

        private GameObject rootPanel;
        private Image fillBarMain;
        private Text titleTextMain;
        private Text healthTextMain;

        // Dual Boss Bar
        private GameObject dualPanel;
        private Image fillBarKnight1;
        private Image fillBarKnight2;
        private Text titleTextKnight1;
        private Text titleTextKnight2;

        private BossBase currentBoss;
        private DualKnightsBoss dualBoss;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            BuildUI();
        }

        private void OnEnable()
        {
            BossBase.OnBossSpawnedGeneric += HandleBossSpawned;
            BossBase.OnBossDefeatedGeneric += HandleBossDefeated;
            BossBase.OnBossHealthChangedGeneric += HandleBossHealthChanged;
        }

        private void OnDisable()
        {
            BossBase.OnBossSpawnedGeneric -= HandleBossSpawned;
            BossBase.OnBossDefeatedGeneric -= HandleBossDefeated;
            BossBase.OnBossHealthChangedGeneric -= HandleBossHealthChanged;
        }

        private void HandleBossSpawned(BossBase boss)
        {
            if (boss is DualKnightsBoss db)
            {
                dualBoss = db;
                currentBoss = null;
                ShowDualBoss(db);
            }
            else
            {
                currentBoss = boss;
                dualBoss = null;
                ShowSingleBoss(boss);
            }
        }

        private void HandleBossDefeated(BossBase boss)
        {
            if (boss == currentBoss || (dualBoss != null && boss.transform.IsChildOf(dualBoss.transform)))
            {
                HideAll();
            }
        }

        private void HandleBossHealthChanged(BossBase boss, float cur, float max)
        {
            if (boss == currentBoss && fillBarMain != null)
            {
                fillBarMain.fillAmount = Mathf.Clamp01(cur / max);
                if (healthTextMain != null)
                {
                    healthTextMain.text = $"{Mathf.CeilToInt(cur)} / {Mathf.CeilToInt(max)} ({Mathf.CeilToInt((cur / max) * 100f)}%)";
                }
            }
        }

        private void ShowSingleBoss(BossBase boss)
        {
            if (dualPanel != null) dualPanel.SetActive(false);
            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
                if (titleTextMain != null)
                {
                    titleTextMain.text = $"👑 {boss.BossName.ToUpper()} - {boss.BossTitle.ToUpper()} (TẦNG {boss.DungeonFloor})";
                }
                if (fillBarMain != null)
                {
                    fillBarMain.fillAmount = boss.HealthPercent;
                }
                if (healthTextMain != null)
                {
                    healthTextMain.text = $"{Mathf.CeilToInt(boss.CurrentHealth)} / {Mathf.CeilToInt(boss.MaxHealth)}";
                }
            }
        }

        private void ShowDualBoss(DualKnightsBoss db)
        {
            if (rootPanel != null) rootPanel.SetActive(false);
            if (dualPanel != null)
            {
                dualPanel.SetActive(true);
                if (titleTextKnight1 != null) titleTextKnight1.text = "⚔️ HIỆP SĨ KIẾM";
                if (titleTextKnight2 != null) titleTextKnight2.text = "🛡️ HIỆP SĨ GIÁO";
            }
        }

        public void UpdateDualKnights(float hp1, float max1, float hp2, float max2)
        {
            if (fillBarKnight1 != null) fillBarKnight1.fillAmount = Mathf.Clamp01(hp1 / max1);
            if (fillBarKnight2 != null) fillBarKnight2.fillAmount = Mathf.Clamp01(hp2 / max2);
        }

        private void HideAll()
        {
            if (rootPanel != null) rootPanel.SetActive(false);
            if (dualPanel != null) dualPanel.SetActive(false);
            currentBoss = null;
            dualBoss = null;
        }

        private void BuildUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("BossHUD_Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGo.AddComponent<GraphicRaycaster>();
                transform.SetParent(canvasGo.transform, false);
            }

            var defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // --- 1. SINGLE BOSS PANEL (Đỉnh màn hình) ---
            rootPanel = CreateUIObject("SingleBossBarPanel", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -35f), new Vector2(720f, 65f));
            AddBackground(rootPanel, new Color(0.12f, 0.05f, 0.05f, 0.92f));

            var titleGo = CreateUIObject("BossTitle", rootPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 22f), new Vector2(680f, 30f));
            titleTextMain = AddText(titleGo, "👑 BOSS TITLE", defaultFont, 18, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleCenter);

            var barBg = CreateUIObject("BarBackground", rootPanel.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(10f, 10f), new Vector2(700f, 45f));
            AddBackground(barBg, new Color(0.25f, 0.05f, 0.05f, 1f));

            var fillGo = CreateUIObject("BarFill", barBg.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            fillBarMain = AddImageFill(fillGo, new Color(0.92f, 0.2f, 0.2f, 1f));

            var hpTextGo = CreateUIObject("HP_Text", barBg.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            healthTextMain = AddText(hpTextGo, "HP / MaxHP", defaultFont, 16, Color.white, TextAnchor.MiddleCenter);

            rootPanel.SetActive(false);

            // --- 2. DUAL BOSS PANEL (Thanh máu kép cho Tầng 4) ---
            dualPanel = CreateUIObject("DualBossBarPanel", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -35f), new Vector2(850f, 65f));
            AddBackground(dualPanel, new Color(0.1f, 0.1f, 0.15f, 0.92f));

            // Cột 1: Hiệp sĩ Kiếm
            var k1Bg = CreateUIObject("K1_Bg", dualPanel.transform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(15f, 0f), new Vector2(395f, 40f));
            AddBackground(k1Bg, new Color(0.2f, 0.05f, 0.05f, 1f));
            var k1Fill = CreateUIObject("K1_Fill", k1Bg.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            fillBarKnight1 = AddImageFill(k1Fill, new Color(0.9f, 0.25f, 0.25f, 1f));
            var k1TextGo = CreateUIObject("K1_Title", k1Bg.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 22f), new Vector2(390f, 25f));
            titleTextKnight1 = AddText(k1TextGo, "⚔️ HIỆP SĨ KIẾM (600 HP)", defaultFont, 15, Color.white, TextAnchor.MiddleCenter);

            // Cột 2: Hiệp sĩ Giáo
            var k2Bg = CreateUIObject("K2_Bg", dualPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(25f, 0f), new Vector2(395f, 40f));
            AddBackground(k2Bg, new Color(0.05f, 0.1f, 0.2f, 1f));
            var k2Fill = CreateUIObject("K2_Fill", k2Bg.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            fillBarKnight2 = AddImageFill(k2Fill, new Color(0.25f, 0.55f, 0.95f, 1f));
            var k2TextGo = CreateUIObject("K2_Title", k2Bg.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 22f), new Vector2(390f, 25f));
            titleTextKnight2 = AddText(k2TextGo, "🛡️ HIỆP SĨ GIÁO (600 HP)", defaultFont, 15, Color.white, TextAnchor.MiddleCenter);

            dualPanel.SetActive(false);
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

