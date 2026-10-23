using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NguyenVanHai.Enemies;

namespace NguyenVanHai.UI
{
    /// <summary>
    /// Màn hình Kết thúc trận đấu trong Dungeon:
    /// 1. Màn hình Thua trận (Game Over): Khi Player HP = 0, thông báo mất 50% nguyên liệu theo TaiLieu.md, có nút Chơi lại.
    /// 2. Màn hình Chiến thắng (Victory): Khi diệt xong Boss King Boar, chúc mừng nhận được Nạc vai heo rừng, có nút Chơi lại / Về làng.
    /// </summary>
    public class DungeonEndScreens : MonoBehaviour
    {
        [Header("Sprites màn hình kết thúc (nếu có)")]
        [SerializeField] private Sprite gameOverSprite;
        [SerializeField] private Sprite victorySprite;

        private GameObject gameOverPanel;
        private GameObject victoryPanel;
        private HunterPlayer player;

        private void Awake()
        {
            BuildEndScreensUI();
        }

        private void Start()
        {
            player = FindObjectOfType<HunterPlayer>();
            if (player != null)
            {
                player.OnHealthChanged += CheckPlayerDeath;
            }

            BossKingBoarAI.OnBossDefeated += ShowVictoryScreen;
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnHealthChanged -= CheckPlayerDeath;
            }

            BossKingBoarAI.OnBossDefeated -= ShowVictoryScreen;
        }

        private void CheckPlayerDeath(float current, float max)
        {
            if (current <= 0f)
            {
                ShowGameOverScreen();
            }
        }

        public void ShowGameOverScreen()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }

        public void ShowVictoryScreen()
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToVillage()
        {
            Time.timeScale = 1f;
            // Kiểm tra xem scene làng có trong BuildSettings không, nếu không thì restart
            if (Application.CanStreamedLevelBeLoaded("Village_Hau"))
            {
                SceneManager.LoadScene("Village_Hau");
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        private void BuildEndScreensUI()
        {
            var canvasGo = new GameObject("Dungeon_EndScreens_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // --- 1. Màn hình Game Over ---
            gameOverPanel = CreateRect("GameOverPanel", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var goBg = gameOverPanel.AddComponent<Image>();
            goBg.color = new Color(0.05f, 0.02f, 0.02f, 0.9f);

            var goCard = CreateRect("Card", gameOverPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 400f));
            var cardImg = goCard.AddComponent<Image>();
            cardImg.color = new Color(0.15f, 0.05f, 0.05f, 0.95f);

            var goTitle = CreateRect("Title", goCard.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -40f), new Vector2(0f, 60f));
            AddText(goTitle, "☠️ BẠN ĐÃ TỬ TRẬN", font, 36, new Color(1f, 0.25f, 0.25f), TextAnchor.MiddleCenter);

            var goDesc = CreateRect("Desc", goCard.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(500f, 80f));
            AddText(goDesc, "Bạn đã kiệt sức trong ngục tối.\n<size=15>50% số nguyên liệu thu thập được trong lượt thám hiểm này đã bị thất lạc.</size>", font, 18, new Color(0.85f, 0.85f, 0.85f), TextAnchor.MiddleCenter);

            var retryBtnGo = CreateRect("RetryBtn", goCard.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(220f, 55f));
            var retryImg = retryBtnGo.AddComponent<Image>();
            retryImg.color = new Color(0.65f, 0.15f, 0.15f, 1f);
            var retryBtn = retryBtnGo.AddComponent<Button>();
            retryBtn.onClick.AddListener(RestartScene);
            var retryTxtGo = CreateRect("Text", retryBtnGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddText(retryTxtGo, "🔄 Thử lại màn", font, 20, Color.white, TextAnchor.MiddleCenter);

            gameOverPanel.SetActive(false);

            // --- 2. Màn hình Victory ---
            victoryPanel = CreateRect("VictoryPanel", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var vicBg = victoryPanel.AddComponent<Image>();
            vicBg.color = new Color(0.02f, 0.05f, 0.02f, 0.9f);

            var vicCard = CreateRect("Card", victoryPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(650f, 440f));
            var vicCardImg = vicCard.AddComponent<Image>();
            vicCardImg.color = new Color(0.08f, 0.18f, 0.1f, 0.95f);

            var vicTitle = CreateRect("Title", vicCard.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -40f), new Vector2(0f, 60f));
            AddText(vicTitle, "🏆 CHIẾN THẮNG BÁ CHỦ ĐẦM LẦY!", font, 32, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter);

            var vicDesc = CreateRect("Desc", vicCard.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(550f, 100f));
            AddText(vicDesc, "King Boar đã bị khuất phục!\n<b>Nhận được: 1x Nạc vai heo rừng (Nguyên liệu Boss)</b>\n<size=15>Bây giờ bạn có thể mang nguyên liệu quý này về nhà hàng để sáng tạo món ăn huyền thoại.</size>", font, 18, Color.white, TextAnchor.MiddleCenter);

            var playAgainBtnGo = CreateRect("PlayAgainBtn", vicCard.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120f, 50f), new Vector2(200f, 55f));
            var paImg = playAgainBtnGo.AddComponent<Image>();
            paImg.color = new Color(0.2f, 0.6f, 0.25f, 1f);
            var paBtn = playAgainBtnGo.AddComponent<Button>();
            paBtn.onClick.AddListener(RestartScene);
            var paTxtGo = CreateRect("Text", playAgainBtnGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddText(paTxtGo, "🔄 Chơi lại", font, 18, Color.white, TextAnchor.MiddleCenter);

            var villageBtnGo = CreateRect("VillageBtn", vicCard.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(120f, 50f), new Vector2(200f, 55f));
            var vilImg = villageBtnGo.AddComponent<Image>();
            vilImg.color = new Color(0.2f, 0.45f, 0.75f, 1f);
            var vilBtn = villageBtnGo.AddComponent<Button>();
            vilBtn.onClick.AddListener(ReturnToVillage);
            var vilTxtGo = CreateRect("Text", villageBtnGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddText(vilTxtGo, "🏡 Về Làng", font, 18, Color.white, TextAnchor.MiddleCenter);

            victoryPanel.SetActive(false);
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

        private Text AddText(GameObject go, string text, Font font, int size, Color color, TextAnchor align)
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
