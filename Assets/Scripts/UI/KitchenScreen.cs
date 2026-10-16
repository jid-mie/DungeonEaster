using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DungeonEater.Core;
using DungeonEater.Dungeon;
using DungeonEater.Restaurant;
using static DungeonEater.UI.KitchenUIKit;

namespace DungeonEater.UI
{
    /// <summary>
    /// Màn Bếp của quán Dungeon Eater (GDD 3.2.2 + concept Cooking_Minigame_Screen):
    ///  1. Chuẩn bị menu: chọn công thức đủ nguyên liệu, chọn số đĩa, xem tiêu hao & doanh thu dự kiến.
    ///  2. Nấu kỹ: minigame từng bước (Thái → Nêm → Áp chảo → Bày đĩa), combo HOÀN HẢO, chất lượng 1–3 sao.
    ///     Nấu nhanh: tạo món ngay (chất lượng Ngon) đúng như GDD "Tạo món ăn ngay".
    ///  3. Tổng kết mẻ nấu rồi ra phục vụ khách.
    /// Toàn bộ UI được dựng bằng code nên chỉ cần gọi KitchenScreen.Open().
    /// </summary>
    public class KitchenScreen : MonoBehaviour
    {
        public static KitchenScreen Instance { get; private set; }
        public static bool IsOpen => Instance != null && Instance.isOpen;
        public static event System.Action OnClosed;

        private enum Mode { Menu, Cooking, Summary }

        private class CardView
        {
            public Recipe recipe;
            public RectTransform rt;
            public Image frame;
            public GameObject lockOverlay;
            public Text stock;
            public readonly List<Text> ingCounts = new List<Text>();
        }

        private class StepRow
        {
            public Text label;
            public Image box;
            public Text mark;
            public Image star;
        }

        private class Fx
        {
            public RectTransform rt;
            public Graphic g;
            public Vector2 vel;
            public float gravity;
            public float life;
            public float maxLife;
            public float baseAlpha = 1f;
            public float grow;
            public float spin;
            public float startScale = 1f;
            public float scaleTime;
            public float killBelowY = float.NegativeInfinity;
            public System.Action onKill;
        }

        private static readonly string[] ComboPhrases = { "Nóng hổi!", "Thơm lừng!", "Xèo xèo!", "Tay nghề đỉnh cao!", "Vua bếp làng Dun!" };
        private static readonly string[] DirGlyph = { "↑", "→", "↓", "←" };
        private static readonly string[] DirKey = { "W", "D", "S", "A" };

        // Điểm neo trên ảnh nền (toạ độ chuẩn hoá 0..1 của ảnh concept 1376x768)
        private static readonly Vector2 PanPoint = new Vector2(0.50f, 0.30f);
        private static readonly Vector2 FirePoint = new Vector2(0.13f, 0.30f);
        private static readonly Vector2 StovePoint = new Vector2(0.50f, 0.10f);

        private bool isOpen;
        private Mode mode;

        // Layers
        private RectTransform root;
        private RectTransform content;
        private RectTransform fxLayer;
        private RectTransform popupLayer;
        private Image dim;
        private Image fireGlow;
        private Image stoveGlow;

        // Menu
        private GameObject menuRoot;
        private readonly List<CardView> cards = new List<CardView>();
        private Sprite cardSprite;
        private Sprite cardSelectedSprite;
        private Image detailDish;
        private Text detailName;
        private Text detailDesc;
        private RectTransform detailIngRows;
        private Text qtyText;
        private Text qtyMaxText;
        private Text previewText;
        private Button cookBtn;
        private Button quickBtn;
        private readonly List<Text> invCounts = new List<Text>();
        private Text bagText;
        private readonly List<Text> readyCounts = new List<Text>();
        private int selected;
        private int quantity = 1;

        // Cooking
        private GameObject cookRoot;
        private Text dishTitle;
        private Text dishCounter;
        private CanvasGroup comboGroup;
        private Text comboText;
        private Text comboSub;
        private float comboTimer;
        private Text stepHeader;
        private Text instruction;
        private RectTransform barGroup;
        private RectTransform barInner;
        private Image zoneGood;
        private Image zonePerfect;
        private Text lblPerfect;
        private Text lblGoodL;
        private Text lblGoodR;
        private Text lblBurntL;
        private Text lblBurntR;
        private RectTransform cursor;
        private Image chopFill;
        private Text chopLabel;
        private RectTransform seqRow;
        private readonly List<Image> seqBoxes = new List<Image>();
        private readonly List<float> seqFlash = new List<float>();
        private RectTransform timerGroup;
        private Image timerFill;
        private RectTransform timerMarker;
        private Image cardDish;
        private Text cardName;
        private RectTransform stepRowsRoot;
        private readonly List<StepRow> stepRows = new List<StepRow>();
        private RectTransform finishedRow;

        private Recipe cookRecipe;
        private int cookQty;
        private int dishesAdded;
        private int currentStep = -1;
        private int lastResult;
        private int combo;
        private int maxCombo;
        private float heatIntensity;
        private readonly List<DishQuality> batchResults = new List<DishQuality>();

        // Dish result + summary
        private GameObject dishResultRoot;
        private Image resultDish;
        private readonly List<Image> resultStars = new List<Image>();
        private Text resultQuality;
        private Text resultValue;
        private GameObject summaryRoot;
        private Text summaryTitle;
        private RectTransform summaryList;
        private Text summaryFooter;

        // FX
        private readonly List<Fx> fxList = new List<Fx>();
        private float steamTimer;
        private float emberTimer;
        private float shakeTime;
        private float shakeMag;

        private PlayerController lockedPlayer;

        // ================================================================ Public API

        public static void Open()
        {
            if (CookingSystem.Instance == null)
            {
                Debug.LogWarning("[KitchenScreen] Không tìm thấy CookingSystem trong scene, không thể mở bếp.");
                return;
            }
            if (Instance == null) Create();
            Instance.Show();
        }

        public static void Close()
        {
            if (Instance != null) Instance.Hide();
        }

        private static void Create()
        {
            var go = new GameObject("KitchenScreen") { layer = 5 };
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            Instance = go.AddComponent<KitchenScreen>();
            Instance.Build();
            go.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Show()
        {
            EnsureEventSystem();
            gameObject.SetActive(true);
            isOpen = true;
            LockPlayer(true);
            SoundManager.Instance?.PlayCook();
            Canvas.ForceUpdateCanvases();
            EnterMenu();
        }

        private void Hide()
        {
            if (!isOpen) return;
            StopAllCoroutines();
            ClearFx();
            isOpen = false;
            LockPlayer(false);
            gameObject.SetActive(false);
            OnClosed?.Invoke();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null || FindObjectOfType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void LockPlayer(bool locked)
        {
            if (locked)
            {
                lockedPlayer = PlayerController.Instance != null ? PlayerController.Instance : FindObjectOfType<PlayerController>();
                if (lockedPlayer == null) return;
                var rb = lockedPlayer.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;
                var anim = lockedPlayer.GetComponentInChildren<Animator>();
                if (anim != null && anim.runtimeAnimatorController != null) anim.SetBool("isMoving", false);
                lockedPlayer.enabled = false;
            }
            else if (lockedPlayer != null)
            {
                lockedPlayer.enabled = true;
                lockedPlayer = null;
            }
        }

        // ================================================================ Build

        private void Build()
        {
            root = Node("Root", transform).Stretch();

            // Nền bếp phủ kín màn hình, giữ tỉ lệ ảnh
            var bg = Img(root, "KitchenBackground", Load("Kitchen_Background"), Color.white);
            bg.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1920, 1072));
            var fitter = bg.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 1376f / 768f;

            fireGlow = Img(bg.transform, "FireGlow", SoftCircle, new Color(1f, 0.55f, 0.15f, 0.3f));
            fireGlow.rectTransform.anchorMin = fireGlow.rectTransform.anchorMax = FirePoint;
            fireGlow.rectTransform.sizeDelta = new Vector2(620, 560);
            stoveGlow = Img(bg.transform, "StoveGlow", SoftCircle, new Color(1f, 0.6f, 0.2f, 0.25f));
            stoveGlow.rectTransform.anchorMin = stoveGlow.rectTransform.anchorMax = StovePoint;
            stoveGlow.rectTransform.sizeDelta = new Vector2(760, 300);

            fxLayer = Node("FxLayer", bg.transform).Stretch();

            dim = Img(root, "Dim", White, new Color(0.05f, 0.03f, 0.02f, 0.55f));
            dim.rectTransform.Stretch();

            content = Node("Content", root).Stretch();
            BuildMenu();
            BuildCooking();
            BuildDishResult();
            BuildSummary();
            popupLayer = Node("Popups", root).Stretch();
        }

        private void BuildMenu()
        {
            menuRoot = Node("Menu", content).Stretch().gameObject;
            var m = menuRoot.transform;

            // Bảng hiệu
            var plaque = Img(m, "TitlePlaque", Frame(Wood, WoodDark, Gold), Color.white);
            plaque.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(880, 108));
            Txt(plaque.transform, "Title", "BẾP LÒ DUNGEON EATER", 46, GoldBright, TextAnchor.MiddleCenter, true)
                .rectTransform.Stretch(0, 0, 6, 40);
            Txt(plaque.transform, "Subtitle", "Chọn món  •  Xem nguyên liệu tiêu hao  •  Lợi nhuận dự kiến", 21, Parchment)
                .rectTransform.Stretch(0, 0, 66, 6);

            cardSprite = Frame(new Color32(58, 35, 22, 236), WoodDark, Gold);
            cardSelectedSprite = Frame(new Color32(104, 62, 30, 250), GoldBright, GoldBright);

            // Lưới thẻ công thức 2x2
            var grid = Node("RecipeGrid", m).Place(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(46, 18), new Vector2(1010, 650));
            var recipes = CookingSystem.Instance != null ? CookingSystem.Instance.Recipes : new List<Recipe>();
            for (int i = 0; i < recipes.Count && i < 4; i++)
            {
                int index = i;
                var card = new CardView { recipe = recipes[i] };
                card.frame = Img(grid, "Card_" + recipes[i].id, cardSprite, Color.white, true);
                card.rt = card.frame.rectTransform;
                card.rt.Place(new Vector2(0, 1), new Vector2(0.5f, 0.5f),
                    new Vector2(245 + (i % 2) * 520, -150 - (i / 2) * 340), new Vector2(490, 300));
                var btn = card.frame.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() => Select(index));

                var dish = Icon(card.rt, "Dish", DishSprite(recipes[i].id), new Vector2(196, 176));
                dish.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(14, 16), new Vector2(196, 176));

                Txt(card.rt, "Name", recipes[i].name, 27, GoldBright, TextAnchor.UpperLeft, true)
                    .rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(222, -20), new Vector2(252, 72));

                var ingRow = Node("Ingredients", card.rt).Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(222, -100), new Vector2(256, 80));
                for (int k = 0; k < recipes[i].ingredients.Count; k++)
                {
                    var req = recipes[i].ingredients[k];
                    var icon = Icon(ingRow, "Ing", IngredientIcon(req.type), new Vector2(50, 50));
                    icon.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(k * 86, 0), new Vector2(50, 50));
                    var count = Txt(ingRow, "Count", "", 20, Parchment, TextAnchor.UpperCenter, true);
                    count.rectTransform.Place(new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(k * 86 + 25, -50), new Vector2(84, 28));
                    card.ingCounts.Add(count);
                }

                Txt(card.rt, "Price", $"<color=#FFD45A>{recipes[i].goldPrice}</color> vàng   <color=#8FE3FF>+{recipes[i].repBonus}</color> d.tiếng", 21, Parchment, TextAnchor.MiddleLeft)
                    .rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(222, -190), new Vector2(262, 30));

                var diffRow = Node("Difficulty", card.rt).Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(222, -228), new Vector2(262, 26));
                Txt(diffRow, "Lbl", "Độ khó", 18, new Color(0.85f, 0.75f, 0.6f), TextAnchor.MiddleLeft)
                    .rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(80, 26));
                for (int d = 0; d < 4; d++)
                {
                    var pip = Img(diffRow, "Pip", Star, d < recipes[i].difficulty ? Ember : new Color(0, 0, 0, 0.45f));
                    pip.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(76 + d * 26, 0), new Vector2(22, 22));
                }

                card.stock = Txt(card.rt, "Stock", "", 19, Parchment, TextAnchor.MiddleLeft);
                card.stock.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(20, 10), new Vector2(220, 28));

                var lockImg = Img(card.rt, "Lock", White, new Color(0.05f, 0.02f, 0.02f, 0.5f));
                lockImg.rectTransform.Stretch(6, 6, 6, 6);
                var stamp = Txt(lockImg.transform, "Stamp", "THIẾU NGUYÊN LIỆU", 24, new Color(1f, 0.45f, 0.35f), TextAnchor.MiddleCenter, true);
                stamp.rectTransform.Place(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-14, 14), new Vector2(270, 40));
                stamp.rectTransform.localRotation = Quaternion.Euler(0, 0, 6f);
                card.lockOverlay = lockImg.gameObject;

                cards.Add(card);
            }

            // Bảng chi tiết công thức (giấy da)
            var detail = Img(m, "Detail", Frame(Parchment, WoodDark, new Color32(176, 130, 70, 255)), Color.white);
            var drt = detail.rectTransform.Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-46, 18), new Vector2(770, 700));
            detailName = Txt(drt, "Name", "", 36, Ink, TextAnchor.MiddleCenter, true, false);
            detailName.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -22), new Vector2(720, 50));
            detailDesc = Txt(drt, "Desc", "", 21, new Color32(96, 64, 40, 255), TextAnchor.UpperCenter, false, false);
            detailDesc.fontStyle = FontStyle.Italic;
            detailDesc.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -76), new Vector2(690, 60));
            detailDish = Icon(drt, "Dish", null, new Vector2(270, 236));
            detailDish.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(26, -146), new Vector2(270, 236));
            Txt(drt, "IngTitle", "NGUYÊN LIỆU / ĐĨA", 20, new Color32(120, 80, 46, 255), TextAnchor.MiddleLeft, true, false)
                .rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(316, -146), new Vector2(420, 28));
            detailIngRows = Node("IngRows", drt).Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(316, -180), new Vector2(430, 200));

            var sep = Img(drt, "Sep", White, new Color(Ink.r, Ink.g, Ink.b, 0.3f));
            sep.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -388), new Vector2(700, 3));

            Txt(drt, "QtyLbl", "SỐ ĐĨA", 26, Ink, TextAnchor.MiddleLeft, true, false)
                .rectTransform.Place(new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(34, -428), new Vector2(140, 50));
            var minus = Btn(drt, "Minus", "-", new Color32(120, 78, 44, 255), Color.white, 34, () => ChangeQuantity(-1));
            minus.GetComponent<RectTransform>().Place(new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(180, -428), new Vector2(60, 60));
            qtyText = Txt(drt, "Qty", "1", 40, Ink, TextAnchor.MiddleCenter, true, false);
            qtyText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(244, -428), new Vector2(80, 60));
            var plus = Btn(drt, "Plus", "+", new Color32(120, 78, 44, 255), Color.white, 34, () => ChangeQuantity(1));
            plus.GetComponent<RectTransform>().Place(new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(328, -428), new Vector2(60, 60));
            qtyMaxText = Txt(drt, "QtyMax", "", 21, new Color32(110, 74, 44, 255), TextAnchor.MiddleLeft, false, false);
            qtyMaxText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(404, -428), new Vector2(340, 50));

            previewText = Txt(drt, "Preview", "", 23, Ink, TextAnchor.UpperLeft, false, false);
            previewText.lineSpacing = 1.05f;
            previewText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(34, -466), new Vector2(700, 100));

            Txt(drt, "Note", "Nấu kỹ: chơi minigame, món có thể <b>Hoàn hảo</b> (x1.5 giá)  •  Nấu nhanh: ra món <b>Ngon</b> ngay", 17,
                new Color32(110, 74, 44, 255), TextAnchor.MiddleCenter, false, false)
                .rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 112), new Vector2(720, 26));
            cookBtn = Btn(drt, "CookBtn", "NẤU KỸ  [Enter]", new Color32(206, 92, 36, 255), Color.white, 28, StartCooking);
            cookBtn.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-180, 24), new Vector2(350, 80));
            quickBtn = Btn(drt, "QuickBtn", "NẤU NHANH  [Q]", new Color32(120, 78, 44, 255), Parchment, 24, QuickCook);
            quickBtn.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(190, 24), new Vector2(310, 80));

            // Thanh dưới: kho nguyên liệu + món sẵn sàng + phím tắt
            var bar = Img(m, "BottomBar", Frame(new Color32(46, 28, 18, 235), WoodDark, Gold), Color.white);
            var brt = bar.rectTransform;
            brt.anchorMin = new Vector2(0, 0);
            brt.anchorMax = new Vector2(1, 0);
            brt.pivot = new Vector2(0.5f, 0);
            brt.anchoredPosition = new Vector2(0, 12);
            brt.sizeDelta = new Vector2(-92, 104);

            Txt(brt, "InvLbl", "KHO NGUYÊN LIỆU", 18, Gold, TextAnchor.MiddleLeft, true)
                .rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(26, -6), new Vector2(300, 24));
            var types = (IngredientType[])System.Enum.GetValues(typeof(IngredientType));
            for (int i = 0; i < types.Length; i++)
            {
                var icon = Icon(brt, "Inv", IngredientIcon(types[i]), new Vector2(54, 54));
                icon.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(26 + i * 112, 12), new Vector2(54, 54));
                var c = Txt(brt, "InvCount", "x0", 26, Color.white, TextAnchor.MiddleLeft, true);
                c.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(82 + i * 112, 22), new Vector2(54, 36));
                invCounts.Add(c);
            }
            bagText = Txt(brt, "Bag", "", 20, Parchment, TextAnchor.MiddleLeft);
            bagText.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(476, 22), new Vector2(130, 36));

            Txt(brt, "ReadyLbl", "MÓN SẴN SÀNG PHỤC VỤ", 18, Gold, TextAnchor.MiddleLeft, true)
                .rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(640, -6), new Vector2(360, 24));
            for (int i = 0; i < cards.Count; i++)
            {
                var icon = Icon(brt, "Ready", DishSprite(cards[i].recipe.id), new Vector2(64, 58));
                icon.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(640 + i * 116, 8), new Vector2(64, 58));
                var c = Txt(brt, "ReadyCount", "0", 26, Color.white, TextAnchor.MiddleLeft, true);
                c.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(706 + i * 116, 22), new Vector2(46, 36));
                readyCounts.Add(c);
            }

            string hints = "←→↑↓ / 1-4: chọn món    - / + : số đĩa";
            if (Debug.isDebugBuild) hints += "\nF9: +nguyên liệu (chế độ test)";
            Txt(brt, "Hints", hints, 18, new Color(0.9f, 0.82f, 0.68f), TextAnchor.MiddleRight)
                .rectTransform.Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-250, 0), new Vector2(460, 80));
            var close = Btn(brt, "Close", "ĐÓNG  [Esc]", new Color32(90, 56, 34, 255), Parchment, 22, Hide);
            close.GetComponent<RectTransform>().Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-18, 0), new Vector2(210, 70));
        }

        private void BuildCooking()
        {
            cookRoot = Node("Cooking", content).Stretch().gameObject;
            var c = cookRoot.transform;

            var plaque = Img(c, "DishPlaque", Frame(Wood, WoodDark, Gold), Color.white);
            plaque.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -14), new Vector2(760, 96));
            dishTitle = Txt(plaque.transform, "Title", "", 38, GoldBright, TextAnchor.MiddleCenter, true);
            dishTitle.rectTransform.Stretch(0, 0, 4, 36);
            dishCounter = Txt(plaque.transform, "Counter", "", 21, Parchment);
            dishCounter.rectTransform.Stretch(0, 0, 58, 4);

            var comboRt = Node("Combo", c).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -164), new Vector2(1000, 124));
            comboGroup = comboRt.gameObject.AddComponent<CanvasGroup>();
            comboGroup.alpha = 0f;
            comboText = Txt(comboRt, "ComboText", "", 66, new Color32(255, 150, 40, 255), TextAnchor.MiddleCenter, true);
            comboText.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(1000, 80));
            comboText.GetComponent<Outline>().effectDistance = new Vector2(3, -3);
            comboSub = Txt(comboRt, "ComboSub", "", 38, GoldBright, TextAnchor.MiddleCenter, true);
            comboSub.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(1000, 60));

            stepHeader = Txt(c, "StepHeader", "", 34, Color.white, TextAnchor.MiddleCenter, true);
            stepHeader.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(-40, -252), new Vector2(1100, 50));

            barGroup = Node("Bar", c).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(-40, -350), new Vector2(880, 74));
            Img(barGroup, "Frame", Frame(WoodDark, WoodDark, Gold), Color.white).rectTransform.Stretch(-10, -10, -10, -10);
            barInner = Node("Inner", barGroup).Stretch(4, 4, 4, 4);
            Img(barInner, "Burnt", White, BurntRed).rectTransform.Stretch();
            zoneGood = Img(barInner, "Good", White, GoodYellow);
            zonePerfect = Img(barInner, "Perfect", White, PerfectGreen);
            lblBurntL = ZoneLabel("CHÁY");
            lblBurntR = ZoneLabel("CHÁY");
            lblGoodL = ZoneLabel("NGON");
            lblGoodR = ZoneLabel("NGON");
            lblPerfect = ZoneLabel("HOÀN HẢO");

            chopFill = Img(barInner, "ChopFill", White, Ember);
            chopFill.rectTransform.anchorMin = Vector2.zero;
            chopFill.rectTransform.anchorMax = new Vector2(0, 1);
            chopFill.rectTransform.offsetMin = chopFill.rectTransform.offsetMax = Vector2.zero;
            chopLabel = Txt(barInner, "ChopLabel", "", 30, Color.white, TextAnchor.MiddleCenter, true);
            chopLabel.rectTransform.Stretch();

            cursor = Node("Cursor", barInner);
            cursor.anchorMin = cursor.anchorMax = new Vector2(0, 0.5f);
            cursor.sizeDelta = new Vector2(10, 100);
            Img(cursor, "Shadow", White, new Color(0, 0, 0, 0.55f)).rectTransform.Stretch(-3, -3, -3, -3);
            Img(cursor, "Line", White, Color.white).rectTransform.Stretch();
            var tri = Img(cursor, "Arrow", TriangleDown, GoldBright);
            tri.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0), new Vector2(0, 2), new Vector2(40, 32));

            seqRow = Node("Sequence", c).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(-40, -350), new Vector2(880, 110));

            timerGroup = Node("Timer", c).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(-40, -414), new Vector2(880, 14));
            Img(timerGroup, "Bg", White, new Color(0, 0, 0, 0.6f)).rectTransform.Stretch(-2, -2, -2, -2);
            timerFill = Img(timerGroup, "Fill", White, Gold);
            timerFill.rectTransform.anchorMin = Vector2.zero;
            timerFill.rectTransform.anchorMax = Vector2.one;
            timerFill.rectTransform.offsetMin = timerFill.rectTransform.offsetMax = Vector2.zero;
            timerMarker = Img(timerGroup, "Marker", White, Color.white).rectTransform;
            timerMarker.anchorMin = timerMarker.anchorMax = new Vector2(0.4f, 0.5f);
            timerMarker.sizeDelta = new Vector2(4, 26);

            instruction = Txt(c, "Instruction", "", 25, Parchment, TextAnchor.MiddleCenter);
            instruction.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(-40, -456), new Vector2(1100, 40));

            // Thẻ các bước nấu bên phải (giống concept)
            var card = Img(c, "StepCard", Frame(Parchment, WoodDark, new Color32(176, 130, 70, 255)), Color.white);
            var crt = card.rectTransform.Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-34, 40), new Vector2(420, 640));
            cardDish = Icon(crt, "Dish", null, new Vector2(116, 100));
            cardDish.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(116, 100));
            cardName = Txt(crt, "Name", "", 26, Ink, TextAnchor.MiddleLeft, true, false);
            cardName.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(146, -20), new Vector2(256, 100));
            Img(crt, "Sep", White, new Color(Ink.r, Ink.g, Ink.b, 0.3f)).rectTransform
                .Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -134), new Vector2(370, 3));
            stepRowsRoot = Node("Steps", crt).Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -150), new Vector2(420, 340));
            Img(crt, "Sep2", White, new Color(Ink.r, Ink.g, Ink.b, 0.3f)).rectTransform
                .Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(370, 3));
            Txt(crt, "DoneLbl", "ĐĨA ĐÃ XONG", 19, new Color32(120, 80, 46, 255), TextAnchor.MiddleLeft, true, false)
                .rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(24, 112), new Vector2(300, 28));
            finishedRow = Node("Finished", crt).Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(20, 16), new Vector2(390, 92));

            Txt(c, "SkipHint", "Esc: nấu nhanh phần còn lại (chất lượng Ngon)", 20, Parchment, TextAnchor.MiddleLeft)
                .rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(26, 18), new Vector2(700, 36));
        }

        private Text ZoneLabel(string text)
        {
            var t = Txt(barInner, "Lbl_" + text, text, 20, Color.white, TextAnchor.MiddleCenter, true);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            return t;
        }

        private void BuildDishResult()
        {
            var rt = Node("DishResult", content).Stretch();
            dishResultRoot = rt.gameObject;
            Img(rt, "Dim", White, new Color(0, 0, 0, 0.45f)).rectTransform.Stretch();
            var glow = Img(rt, "Glow", SoftCircle, new Color(1f, 0.8f, 0.4f, 0.45f));
            glow.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(760, 620));
            resultDish = Icon(rt, "Dish", null, new Vector2(430, 370));
            resultDish.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(430, 370));
            for (int i = 0; i < 3; i++)
            {
                var s = Img(rt, "Star", Star, GoldBright);
                s.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 104, -160), new Vector2(92, 92));
                resultStars.Add(s);
            }
            resultQuality = Txt(rt, "Quality", "", 50, Color.white, TextAnchor.MiddleCenter, true);
            resultQuality.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -246), new Vector2(900, 64));
            resultValue = Txt(rt, "Value", "", 26, Parchment, TextAnchor.MiddleCenter);
            resultValue.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -298), new Vector2(900, 40));
        }

        private void BuildSummary()
        {
            var rt = Node("Summary", content).Stretch();
            summaryRoot = rt.gameObject;
            Img(rt, "Dim", White, new Color(0, 0, 0, 0.55f)).rectTransform.Stretch();
            var panel = Img(rt, "Panel", Frame(Parchment, WoodDark, new Color32(176, 130, 70, 255)), Color.white);
            var prt = panel.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(860, 560));
            summaryTitle = Txt(prt, "Title", "MẺ NẤU HOÀN TẤT!", 46, Ink, TextAnchor.MiddleCenter, true, false);
            summaryTitle.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(800, 60));
            summaryList = Node("List", prt).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -98), new Vector2(760, 300));
            summaryFooter = Txt(prt, "Footer", "", 23, Ink, TextAnchor.UpperCenter, false, false);
            summaryFooter.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 118), new Vector2(800, 84));
            var again = Btn(prt, "Again", "NẤU TIẾP  [Enter]", new Color32(206, 92, 36, 255), Color.white, 26, EnterMenu);
            again.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-190, 24), new Vector2(350, 78));
            var serve = Btn(prt, "Serve", "RA PHỤC VỤ  [Esc]", new Color32(70, 120, 60, 255), Color.white, 26, Hide);
            serve.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(190, 24), new Vector2(350, 78));
        }

        // ================================================================ Update

        private void Update()
        {
            if (!isOpen) return;
            float dt = Time.deltaTime;

            UpdateAmbient(dt);
            UpdateFx(dt);
            UpdateShake(dt);
            UpdateCombo(dt);

            switch (mode)
            {
                case Mode.Menu: MenuInput(); break;
                case Mode.Cooking:
                    if (Input.GetKeyDown(KeyCode.Escape)) SkipRemaining();
                    break;
                case Mode.Summary:
                    if (Input.GetKeyDown(KeyCode.Escape)) Hide();
                    else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) EnterMenu();
                    break;
            }
        }

        // ================================================================ Menu

        private void EnterMenu()
        {
            StopAllCoroutines();
            mode = Mode.Menu;
            menuRoot.SetActive(true);
            cookRoot.SetActive(false);
            dishResultRoot.SetActive(false);
            summaryRoot.SetActive(false);
            dim.color = new Color(0.05f, 0.03f, 0.02f, 0.55f);
            heatIntensity = 0f;

            // Ưu tiên chọn món đang nấu được
            if (cards.Count > 0 && (selected >= cards.Count || CookingSystem.Instance.GetMaxCookable(cards[selected].recipe) == 0))
            {
                int firstCookable = cards.FindIndex(cv => CookingSystem.Instance.CanCook(cv.recipe));
                selected = firstCookable >= 0 ? firstCookable : Mathf.Clamp(selected, 0, cards.Count - 1);
            }
            RefreshMenu();
        }

        private void MenuInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) { Hide(); return; }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Select(selected - 1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Select(selected + 1);
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Select(selected - 2);
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Select(selected + 2);
            for (int i = 0; i < cards.Count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)) Select(i);
            }

            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) ChangeQuantity(-1);
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus)) ChangeQuantity(1);

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) StartCooking();
            if (Input.GetKeyDown(KeyCode.Q)) QuickCook();

            if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.F9)) GrantTestIngredients();
        }

        private void Select(int index)
        {
            if (cards.Count == 0) return;
            index = Mathf.Clamp(index, 0, cards.Count - 1);
            if (index != selected)
            {
                selected = index;
                quantity = 1;
                SoundManager.Instance?.PlaySfx(SoundManager.Instance.pickupClip, 0.5f);
                Punch(cards[selected].rt, 1.08f);
            }
            RefreshMenu();
        }

        private void ChangeQuantity(int delta)
        {
            if (cards.Count == 0) return;
            int max = Mathf.Max(1, CookingSystem.Instance.GetMaxCookable(cards[selected].recipe));
            int q = Mathf.Clamp(quantity + delta, 1, max);
            if (q != quantity)
            {
                quantity = q;
                SoundManager.Instance?.PlaySfx(SoundManager.Instance.coinClip, 0.35f);
                Punch(qtyText.rectTransform, 1.3f);
            }
            RefreshMenu();
        }

        private void RefreshMenu()
        {
            var cooking = CookingSystem.Instance;
            var inv = InventoryManager.Instance;
            if (cooking == null || cards.Count == 0) return;

            for (int i = 0; i < cards.Count; i++)
            {
                var cv = cards[i];
                bool can = cooking.CanCook(cv.recipe);
                bool isSel = i == selected;
                cv.frame.sprite = isSel ? cardSelectedSprite : cardSprite;
                if (!isSel) cv.rt.localScale = Vector3.one;
                cv.lockOverlay.SetActive(!can);
                for (int k = 0; k < cv.ingCounts.Count; k++)
                {
                    var req = cv.recipe.ingredients[k];
                    int have = inv != null ? inv.GetItemCount(req.type) : 0;
                    cv.ingCounts[k].text = $"{have}/{req.count}";
                    cv.ingCounts[k].color = have >= req.count ? new Color(0.6f, 1f, 0.55f) : new Color(1f, 0.45f, 0.38f);
                }
                int ready = cooking.GetPreparedCount(cv.recipe.id);
                cv.stock.text = ready > 0 ? $"Sẵn có: <color=#9CFF8A><b>{ready}</b></color> đĩa" : "Sẵn có: 0 đĩa";
            }

            var r = cards[selected].recipe;
            int max = cooking.GetMaxCookable(r);
            quantity = Mathf.Clamp(quantity, 1, Mathf.Max(1, max));

            detailName.text = r.name;
            detailDesc.text = r.description;
            detailDish.sprite = DishSprite(r.id);

            foreach (Transform child in detailIngRows) Destroy(child.gameObject);
            for (int k = 0; k < r.ingredients.Count; k++)
            {
                var req = r.ingredients[k];
                int have = inv != null ? inv.GetItemCount(req.type) : 0;
                var icon = Icon(detailIngRows, "Icon", IngredientIcon(req.type), new Vector2(56, 56));
                icon.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -k * 64), new Vector2(56, 56));
                string haveColor = have >= req.count * quantity ? "#2F7A22" : "#B3261E";
                var line = Txt(detailIngRows, "Line",
                    $"<b>{IngredientShortName(req.type)}</b> x{req.count}   <color={haveColor}>(kho: {have})</color>",
                    23, Ink, TextAnchor.MiddleLeft, false, false);
                line.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(66, -k * 64 - 4), new Vector2(370, 48));
            }

            qtyText.text = quantity.ToString();
            qtyMaxText.text = max > 0 ? $"(tối đa {max} đĩa)" : "(chưa đủ nguyên liệu)";

            var parts = new List<string>();
            foreach (var req in r.ingredients) parts.Add($"{req.count * quantity} {IngredientShortName(req.type)}");
            int gold = r.goldPrice * quantity;
            int goldPerfect = Mathf.RoundToInt(r.goldPrice * CookingSystem.GoldMultiplier(DishQuality.Perfect)) * quantity;
            previewText.text =
                $"Tiêu hao: <b>{string.Join(", ", parts)}</b>\n" +
                $"Doanh thu dự kiến: <b><color=#8A5A00>{gold} vàng</color></b>   (hoàn hảo: tới <b>{goldPerfect}</b>)\n" +
                $"Danh tiếng: <b><color=#1F5E8A>+{r.repBonus * quantity}</color></b>   •   Bước nấu: <b>{r.steps.Count}</b>";

            cookBtn.interactable = max > 0;
            quickBtn.interactable = max > 0;

            for (int i = 0; i < invCounts.Count; i++)
            {
                invCounts[i].text = "x" + (inv != null ? inv.GetItemCount((IngredientType)i) : 0);
            }
            bagText.text = inv != null ? $"Balo {inv.GetTotalItemCount()}/{inv.MaxCapacity}" : "";
            for (int i = 0; i < readyCounts.Count; i++)
            {
                readyCounts[i].text = cooking.GetPreparedCount(cards[i].recipe.id).ToString();
            }
        }

        private void QuickCook()
        {
            if (mode != Mode.Menu || cards.Count == 0) return;
            var r = cards[selected].recipe;
            int qty = quantity;
            if (!CookingSystem.Instance.ConsumeIngredients(r, qty))
            {
                DenyFeedback();
                return;
            }
            for (int i = 0; i < qty; i++) CookingSystem.Instance.AddPreparedDish(r.id, DishQuality.Good);
            SoundManager.Instance?.PlayCook();
            SoundManager.Instance?.PlaySfx(SoundManager.Instance.serveSuccessClip, 0.6f);
            Popup($"+{qty} {r.name}  (Ngon)", GoodYellow, 40, new Vector2(0, 40));
            SteamBurst(10, false);
            quantity = 1;
            RefreshMenu();
        }

        private void GrantTestIngredients()
        {
            var inv = InventoryManager.Instance;
            if (inv == null) return;
            int need = inv.GetTotalItemCount() + 8 - inv.MaxCapacity;
            if (need > 0) inv.IncreaseCapacity(need);
            inv.AddItem(IngredientType.MonsterMeat, 3);
            inv.AddItem(IngredientType.CaveMushroom, 3);
            inv.AddItem(IngredientType.BombCore, 1);
            inv.AddItem(IngredientType.BossRib, 1);
            SoundManager.Instance?.PlayPickup();
            Popup("+ Nguyên liệu test", PerfectGreen, 36, new Vector2(0, 40));
            RefreshMenu();
        }

        private void DenyFeedback()
        {
            SoundManager.Instance?.PlaySfx(SoundManager.Instance.hitClip, 0.4f);
            Shake(0.2f, 10f);
            Popup("Chưa đủ nguyên liệu!", BurntRed, 40, new Vector2(0, 40));
        }

        // ================================================================ Cooking flow

        private void StartCooking()
        {
            if (mode != Mode.Menu || cards.Count == 0) return;
            var r = cards[selected].recipe;
            if (!CookingSystem.Instance.ConsumeIngredients(r, quantity))
            {
                DenyFeedback();
                return;
            }

            cookRecipe = r;
            cookQty = quantity;
            quantity = 1;
            dishesAdded = 0;
            combo = 0;
            maxCombo = 0;
            batchResults.Clear();
            foreach (Transform child in finishedRow) Destroy(child.gameObject);

            mode = Mode.Cooking;
            menuRoot.SetActive(false);
            cookRoot.SetActive(true);
            dishResultRoot.SetActive(false);
            summaryRoot.SetActive(false);
            dim.color = new Color(0.05f, 0.03f, 0.02f, 0.12f);
            StartCoroutine(CookRoutine());
        }

        private IEnumerator CookRoutine()
        {
            for (int d = 0; d < cookQty; d++)
            {
                SetupStepCard();
                dishTitle.text = cookRecipe.name.ToUpperInvariant();
                UpdateDishCounter(d);
                ShowWidgets(null);
                stepHeader.text = "CHO NGUYÊN LIỆU VÀO CHẢO...";
                instruction.text = "";
                SoundManager.Instance?.PlayCook();
                yield return DropIngredients();

                int points = 0;
                for (int s = 0; s < cookRecipe.steps.Count; s++)
                {
                    var step = cookRecipe.steps[s];
                    currentStep = s;
                    MarkStep(s, -1);
                    stepHeader.text = $"BƯỚC {s + 1}/{cookRecipe.steps.Count}  —  {step.label.ToUpperInvariant()}!";
                    Punch(stepHeader.rectTransform, 1.25f);
                    instruction.text = IntroText(step.type);
                    ShowWidgets(step.type);
                    PrepareWidgets(step.type);
                    yield return Wait(0.7f);

                    lastResult = 0;
                    switch (step.type)
                    {
                        case CookStepType.Chop: yield return RunChop(); break;
                        case CookStepType.Season: yield return RunSeason(); break;
                        case CookStepType.Heat: yield return RunTiming(false); break;
                        case CookStepType.Plate: yield return RunTiming(true); break;
                    }

                    points += lastResult;
                    ApplyStepResult(s, step.type, lastResult);
                    UpdateDishCounter(d);
                    heatIntensity = 0f;
                    yield return Wait(0.6f);
                }

                float ratio = points / (2f * cookRecipe.steps.Count);
                DishQuality q = ratio >= 0.8f ? DishQuality.Perfect : ratio >= 0.45f ? DishQuality.Good : DishQuality.Burnt;
                CookingSystem.Instance.AddPreparedDish(cookRecipe.id, q);
                dishesAdded++;
                batchResults.Add(q);
                AddFinishedDish(q);
                currentStep = -1;
                yield return ShowDishResult(q);
            }

            ShowSummary(false);
        }

        private void UpdateDishCounter(int d)
        {
            dishCounter.text = $"Đĩa {d + 1}/{cookQty}    •    Combo: x{combo}    •    Cao nhất: x{maxCombo}";
        }

        private void SkipRemaining()
        {
            StopAllCoroutines();
            int remaining = cookQty - dishesAdded;
            for (int i = 0; i < remaining; i++)
            {
                CookingSystem.Instance.AddPreparedDish(cookRecipe.id, DishQuality.Good);
                batchResults.Add(DishQuality.Good);
            }
            dishesAdded = cookQty;
            SoundManager.Instance?.PlayCook();
            ShowSummary(remaining > 0);
        }

        private IEnumerator Wait(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        private static bool ActionPressed()
        {
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0);
        }

        private static string IntroText(CookStepType type)
        {
            switch (type)
            {
                case CookStepType.Chop: return "Bấm liên tục <b>[SPACE]</b> để thái thật nhanh — xong trước vạch trắng là HOÀN HẢO!";
                case CookStepType.Season: return "Gõ đúng thứ tự phím mũi tên <b>(hoặc W A S D)</b> để nêm gia vị!";
                case CookStepType.Plate: return "Nhấn <b>[SPACE]</b> đúng giữa vạch để bày món thật đẹp!";
                default: return "Nhấn <b>[SPACE]</b> khi con trỏ vào vùng <color=#7CFF6A>HOÀN HẢO</color> — chần chừ là cháy!";
            }
        }

        private void ShowWidgets(CookStepType? type)
        {
            bool timing = type == CookStepType.Heat || type == CookStepType.Plate;
            bool chop = type == CookStepType.Chop;
            bool seq = type == CookStepType.Season;
            barGroup.gameObject.SetActive(timing || chop);
            zoneGood.gameObject.SetActive(timing);
            zonePerfect.gameObject.SetActive(timing);
            lblBurntL.gameObject.SetActive(timing);
            lblBurntR.gameObject.SetActive(timing);
            lblGoodL.gameObject.SetActive(timing);
            lblGoodR.gameObject.SetActive(timing);
            lblPerfect.gameObject.SetActive(timing);
            cursor.gameObject.SetActive(timing);
            chopFill.gameObject.SetActive(chop);
            chopLabel.gameObject.SetActive(chop);
            seqRow.gameObject.SetActive(seq);
            timerGroup.gameObject.SetActive(type != null);
            timerMarker.gameObject.SetActive(chop || seq);
            barGroup.localScale = Vector3.one;
        }

        private void PrepareWidgets(CookStepType type)
        {
            SetTimer(1f);
            timerFill.color = Gold;
            if (type == CookStepType.Chop)
            {
                chopFill.rectTransform.anchorMax = new Vector2(0, 1);
                chopLabel.text = "SẴN SÀNG...";
                barInner.GetChild(0).GetComponent<Image>().color = new Color32(70, 44, 28, 255);
            }
            else
            {
                barInner.GetChild(0).GetComponent<Image>().color = BurntRed;
            }
            if (type == CookStepType.Season) BuildSequence(new List<int>());
            if (type == CookStepType.Heat || type == CookStepType.Plate) SetCursor(0f);
        }

        private void SetTimer(float remaining01)
        {
            timerFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(remaining01), 1);
        }

        private void SetCursor(float pos01)
        {
            cursor.anchorMin = cursor.anchorMax = new Vector2(Mathf.Clamp01(pos01), 0.5f);
            cursor.anchoredPosition = Vector2.zero;
        }

        // ---------------------------------------------------------------- Step: timing (Áp chảo / Bày đĩa)

        private IEnumerator RunTiming(bool plate)
        {
            int diff = cookRecipe.difficulty;
            float speed = (plate ? 0.95f : 0.6f) + 0.12f * diff + 0.05f * Mathf.Min(combo, 5);
            float perfectHalf = plate ? 0.055f - 0.006f * diff : 0.075f - 0.008f * diff;
            float goodHalf = perfectHalf + (plate ? 0.12f : 0.15f);
            float center = plate ? 0.5f : Random.Range(0.3f, 0.7f);
            float limit = plate ? 4f : 5f;

            LayoutZones(center, perfectHalf, goodHalf);

            float pos = 0f, dir = 1f, t = 0f;
            while (t < limit)
            {
                t += Time.deltaTime;
                pos += dir * speed * Time.deltaTime;
                if (pos > 1f) { pos = 2f - pos; dir = -1f; }
                if (pos < 0f) { pos = -pos; dir = 1f; }
                SetCursor(pos);
                SetTimer(1f - t / limit);

                if (!plate)
                {
                    heatIntensity = t / limit;
                    timerFill.color = t > limit * 0.65f ? Color.Lerp(Gold, BurntRed, Mathf.PingPong(t * 6f, 1f)) : Gold;
                }

                if (ActionPressed())
                {
                    float d = Mathf.Abs(pos - center);
                    lastResult = d <= perfectHalf ? 2 : d <= goodHalf ? 1 : 0;
                    Punch(barGroup, 1.06f);
                    yield break;
                }
                yield return null;
            }
            lastResult = 0;
        }

        private void LayoutZones(float center, float perfectHalf, float goodHalf)
        {
            SetZone(zoneGood.rectTransform, center - goodHalf, center + goodHalf);
            SetZone(zonePerfect.rectTransform, center - perfectHalf, center + perfectHalf);
            SetZone(lblPerfect.rectTransform, center - perfectHalf, center + perfectHalf);
            SetZone(lblGoodL.rectTransform, center - goodHalf, center - perfectHalf);
            SetZone(lblGoodR.rectTransform, center + perfectHalf, center + goodHalf);
            SetZone(lblBurntL.rectTransform, 0f, Mathf.Max(0f, center - goodHalf));
            SetZone(lblBurntR.rectTransform, Mathf.Min(1f, center + goodHalf), 1f);
            lblPerfect.fontSize = perfectHalf < 0.05f ? 15 : 18;
            lblBurntL.gameObject.SetActive(center - goodHalf > 0.08f);
            lblBurntR.gameObject.SetActive(center + goodHalf < 0.92f);
        }

        private static void SetZone(RectTransform rt, float from, float to)
        {
            rt.anchorMin = new Vector2(Mathf.Clamp01(from), 0);
            rt.anchorMax = new Vector2(Mathf.Clamp01(to), 1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        // ---------------------------------------------------------------- Step: chop (Thái)

        private IEnumerator RunChop()
        {
            int diff = cookRecipe.difficulty;
            int need = 8 + 2 * diff;
            float limit = 2.4f + 0.35f * diff;
            float perfectT = limit * 0.6f;
            timerMarker.anchorMin = timerMarker.anchorMax = new Vector2(1f - perfectT / limit, 0.5f);

            int count = 0;
            float t = 0f;
            chopLabel.text = $"THÁI!  0/{need}";
            while (t < limit)
            {
                t += Time.deltaTime;
                SetTimer(1f - t / limit);
                if (ActionPressed())
                {
                    count++;
                    chopFill.rectTransform.anchorMax = new Vector2((float)count / need, 1);
                    chopLabel.text = $"THÁI!  {count}/{need}";
                    SoundManager.Instance?.PlaySfx(SoundManager.Instance.swordSlashClip, 0.45f);
                    Punch(barGroup, 1.04f);
                    ChopFx();
                    if (count >= need)
                    {
                        lastResult = t <= perfectT ? 2 : 1;
                        yield break;
                    }
                }
                yield return null;
            }
            lastResult = count >= need * 0.75f ? 1 : 0;
        }

        // ---------------------------------------------------------------- Step: season (Nêm)

        private IEnumerator RunSeason()
        {
            int diff = cookRecipe.difficulty;
            int len = Mathf.Clamp(2 + diff, 3, 6);
            var keys = new List<int>();
            for (int i = 0; i < len; i++) keys.Add(Random.Range(0, 4));
            BuildSequence(keys);

            float limit = 1.4f + 0.75f * len;
            float perfectT = limit * 0.7f;
            timerMarker.anchorMin = timerMarker.anchorMax = new Vector2(1f - perfectT / limit, 0.5f);

            int idx = 0, mistakes = 0;
            float t = 0f;
            while (t < limit)
            {
                t += Time.deltaTime;
                SetTimer(1f - t / limit);

                for (int i = 0; i < seqBoxes.Count; i++)
                {
                    seqFlash[i] = Mathf.Max(0f, seqFlash[i] - Time.deltaTime * 3f);
                    Color baseCol = i < idx ? PerfectGreen : i == idx ? Color.Lerp(Parchment, GoldBright, Mathf.PingPong(t * 3f, 1f)) : Parchment;
                    seqBoxes[i].color = Color.Lerp(baseCol, BurntRed, seqFlash[i]);
                    float s = i == idx ? 1.08f + 0.04f * Mathf.Sin(t * 12f) : 1f;
                    seqBoxes[i].rectTransform.localScale = Vector3.one * s;
                }

                int k = ReadDirection();
                if (k >= 0)
                {
                    if (k == keys[idx])
                    {
                        seqBoxes[idx].rectTransform.localScale = Vector3.one;
                        seqBoxes[idx].color = PerfectGreen;
                        SoundManager.Instance?.PlaySfx(SoundManager.Instance.pickupClip, 0.6f);
                        SpiceFx();
                        idx++;
                        if (idx >= len)
                        {
                            lastResult = mistakes == 0 && t <= perfectT ? 2 : mistakes <= 2 ? 1 : 0;
                            yield break;
                        }
                    }
                    else
                    {
                        mistakes++;
                        seqFlash[idx] = 1f;
                        SoundManager.Instance?.PlaySfx(SoundManager.Instance.hitClip, 0.35f);
                        Shake(0.12f, 6f);
                    }
                }
                yield return null;
            }
            lastResult = idx >= len / 2 && mistakes <= 2 ? 1 : 0;
        }

        private void BuildSequence(List<int> keys)
        {
            foreach (Transform child in seqRow) Destroy(child.gameObject);
            seqBoxes.Clear();
            seqFlash.Clear();
            float spacing = 116f;
            float start = -(keys.Count - 1) * spacing * 0.5f;
            for (int i = 0; i < keys.Count; i++)
            {
                var box = Img(seqRow, "Key", Frame(Color.white, WoodDark, new Color(1, 1, 1, 0.6f)), Parchment);
                box.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(start + i * spacing, 0), new Vector2(100, 100));
                var glyph = Txt(box.transform, "Glyph", DirGlyph[keys[i]], 56, Ink, TextAnchor.MiddleCenter, true, false);
                glyph.rectTransform.Stretch(0, 0, 0, 22);
                var letter = Txt(box.transform, "Key", DirKey[keys[i]], 20, new Color32(110, 74, 44, 255), TextAnchor.LowerCenter, true, false);
                letter.rectTransform.Stretch(0, 0, 0, 6);
                seqBoxes.Add(box);
                seqFlash.Add(0f);
            }
        }

        private static int ReadDirection()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) return 0;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) return 1;
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) return 2;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) return 3;
            return -1;
        }

        // ---------------------------------------------------------------- Step results

        private void ApplyStepResult(int stepIndex, CookStepType type, int result)
        {
            MarkStep(stepIndex, result);
            Vector2 popupPos = new Vector2(-40, -20);
            if (result == 2)
            {
                combo++;
                maxCombo = Mathf.Max(maxCombo, combo);
                Popup("HOÀN HẢO!", PerfectGreen, 64, popupPos);
                SoundManager.Instance?.PlayCoin();
                Shake(0.15f, 8f);
                SteamBurst(12, false);
                SparkleBurst();
                if (combo >= 2) ShowCombo();
            }
            else if (result == 1)
            {
                combo = 0;
                Popup("NGON!", GoodYellow, 56, popupPos);
                SoundManager.Instance?.PlayPickup();
                SteamBurst(5, false);
            }
            else
            {
                combo = 0;
                string miss = type == CookStepType.Heat ? "CHÁY KHÉT!" :
                              type == CookStepType.Chop ? "CHẬM QUÁ!" :
                              type == CookStepType.Season ? "SAI GIA VỊ!" : "ĐỔ VÃI!";
                Popup(miss, BurntRed, 60, popupPos);
                SoundManager.Instance?.PlaySfx(SoundManager.Instance.hitClip, 0.8f);
                Shake(0.3f, 16f);
                SteamBurst(14, true);
            }
        }

        private void ShowCombo()
        {
            comboText.text = $"COMBO x{combo}!";
            comboSub.text = ComboPhrases[Mathf.Min(combo - 2, ComboPhrases.Length - 1)];
            comboGroup.alpha = 1f;
            comboTimer = 1.8f;
            Punch((RectTransform)comboGroup.transform, 1.35f);
            SoundManager.Instance?.PlaySfx(SoundManager.Instance.serveSuccessClip, 0.5f);
        }

        private void UpdateCombo(float dt)
        {
            if (comboTimer <= 0f) return;
            comboTimer -= dt;
            comboGroup.alpha = Mathf.Clamp01(comboTimer / 0.4f);
            comboText.color = Color.Lerp(new Color32(255, 150, 40, 255), new Color32(255, 220, 90, 255), Mathf.PingPong(Time.time * 4f, 1f));
        }

        // ---------------------------------------------------------------- Step card

        private void SetupStepCard()
        {
            cardDish.sprite = DishSprite(cookRecipe.id);
            cardName.text = cookRecipe.name;
            foreach (Transform child in stepRowsRoot) Destroy(child.gameObject);
            stepRows.Clear();
            float rowH = Mathf.Min(66f, 330f / cookRecipe.steps.Count);
            for (int i = 0; i < cookRecipe.steps.Count; i++)
            {
                float y = -i * rowH - rowH * 0.5f;
                var row = new StepRow();
                Txt(stepRowsRoot, "Idx", $"{i + 1}.", 26, Ink, TextAnchor.MiddleLeft, true, false)
                    .rectTransform.Place(new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(26, y), new Vector2(40, 50));
                row.label = Txt(stepRowsRoot, "Label", cookRecipe.steps[i].label, 25, Ink, TextAnchor.MiddleLeft, false, false);
                row.label.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(66, y), new Vector2(270, 50));
                row.box = Img(stepRowsRoot, "Box", Frame(Color.white, Ink, new Color(1, 1, 1, 0.5f)), new Color32(225, 200, 156, 255));
                row.box.rectTransform.Place(new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector2(370, y), new Vector2(46, 46));
                row.mark = Txt(row.box.transform, "Mark", "", 24, Color.white, TextAnchor.MiddleCenter, true);
                row.mark.rectTransform.Stretch();
                row.star = Img(row.box.transform, "Star", Star, Color.white);
                row.star.rectTransform.Stretch(7, 7, 7, 7);
                row.star.gameObject.SetActive(false);
                stepRows.Add(row);
            }
        }

        /// <summary>result: -1 = đang làm, 0 = hỏng, 1 = ngon, 2 = hoàn hảo</summary>
        private void MarkStep(int index, int result)
        {
            if (index < 0 || index >= stepRows.Count) return;
            var row = stepRows[index];
            row.star.gameObject.SetActive(result == 2);
            row.label.fontStyle = result == -1 ? FontStyle.Bold : FontStyle.Normal;
            switch (result)
            {
                case -1: row.box.color = Ember; row.mark.text = ">"; break;
                case 0: row.box.color = BurntRed; row.mark.text = "X"; break;
                case 1: row.box.color = GoodYellow; row.mark.text = "OK"; row.mark.fontSize = 18; break;
                default: row.box.color = PerfectGreen; row.mark.text = ""; break;
            }
            Punch(row.box.rectTransform, 1.3f);
        }

        private void AddFinishedDish(DishQuality q)
        {
            int i = finishedRow.childCount;
            float x = (i % 3) * 128f;
            float y = i < 3 ? 46f : 0f;
            var item = Node("Done", finishedRow).Place(new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(x, y), new Vector2(124, 44));
            Icon(item, "Dish", DishSprite(cookRecipe.id), new Vector2(46, 42))
                .rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(46, 42));
            for (int s = 0; s < 3; s++)
            {
                var star = Img(item, "Star", Star, s <= (int)q ? GoldBright : new Color(0, 0, 0, 0.3f));
                star.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(48 + s * 24, 0), new Vector2(24, 24));
            }
        }

        // ---------------------------------------------------------------- Dish result + summary

        private IEnumerator ShowDishResult(DishQuality q)
        {
            ShowWidgets(null);
            stepHeader.text = "";
            instruction.text = "";
            dishResultRoot.SetActive(true);
            resultDish.sprite = DishSprite(cookRecipe.id);
            resultDish.color = q == DishQuality.Burnt ? new Color(0.55f, 0.45f, 0.4f) : Color.white;
            resultQuality.text = q == DishQuality.Perfect ? "HOÀN HẢO!" : q == DishQuality.Good ? "MÓN NGON!" : "HƠI CHÁY KHÉT...";
            resultQuality.color = q == DishQuality.Perfect ? PerfectGreen : q == DishQuality.Good ? GoodYellow : BurntRed;
            int gold = Mathf.RoundToInt(cookRecipe.goldPrice * CookingSystem.GoldMultiplier(q));
            int rep = Mathf.Max(1, Mathf.RoundToInt(cookRecipe.repBonus * CookingSystem.RepMultiplier(q)));
            resultValue.text = $"Giá bán dự kiến: <color=#FFD45A><b>{gold} vàng</b></color>   •   Danh tiếng: <color=#8FE3FF><b>+{rep}</b></color>";
            foreach (var s in resultStars)
            {
                s.color = new Color(0, 0, 0, 0.4f);
                s.rectTransform.localScale = Vector3.one;
            }

            SoundManager.Instance?.PlaySfx(SoundManager.Instance.serveSuccessClip, 0.9f);
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float k = EaseOutBack(Mathf.Clamp01(t / 0.35f));
                resultDish.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.3f, 1f, k);
                yield return null;
            }
            for (int i = 0; i <= (int)q; i++)
            {
                resultStars[i].color = GoldBright;
                Punch(resultStars[i].rectTransform, 1.6f);
                SoundManager.Instance?.PlaySfx(SoundManager.Instance.coinClip, 0.7f);
                yield return Wait(0.22f);
            }
            if (q == DishQuality.Perfect) SparkleBurst();

            t = 0f;
            while (t < 1.6f)
            {
                t += Time.deltaTime;
                if (t > 0.3f && (ActionPressed() || Input.GetKeyDown(KeyCode.Return))) break;
                yield return null;
            }
            dishResultRoot.SetActive(false);
        }

        private void ShowSummary(bool skipped)
        {
            mode = Mode.Summary;
            ShowWidgets(null);
            dishResultRoot.SetActive(false);
            summaryRoot.SetActive(true);
            summaryTitle.text = skipped ? "ĐÃ NẤU NHANH PHẦN CÒN LẠI" : "MẺ NẤU HOÀN TẤT!";

            foreach (Transform child in summaryList) Destroy(child.gameObject);
            int totalGold = 0, totalRep = 0;
            int perfect = 0, good = 0, burnt = 0;
            foreach (var q in batchResults)
            {
                totalGold += Mathf.RoundToInt(cookRecipe.goldPrice * CookingSystem.GoldMultiplier(q));
                totalRep += Mathf.Max(1, Mathf.RoundToInt(cookRecipe.repBonus * CookingSystem.RepMultiplier(q)));
                if (q == DishQuality.Perfect) perfect++;
                else if (q == DishQuality.Good) good++;
                else burnt++;
            }

            // Mỗi hạng chất lượng một dòng: [đĩa] ★★★ Hoàn hảo  x2
            var groups = new List<(DishQuality q, int n)>();
            if (perfect > 0) groups.Add((DishQuality.Perfect, perfect));
            if (good > 0) groups.Add((DishQuality.Good, good));
            if (burnt > 0) groups.Add((DishQuality.Burnt, burnt));
            for (int i = 0; i < groups.Count; i++)
            {
                var (q, n) = groups[i];
                float rowY = -(300f - groups.Count * 100f) * 0.5f - i * 100f;
                var row = Node("Row", summaryList).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, rowY), new Vector2(700, 96));
                Icon(row, "Dish", DishSprite(cookRecipe.id), new Vector2(110, 94))
                    .rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(110, 94));
                for (int s = 0; s < 3; s++)
                {
                    Img(row, "Star", Star, s <= (int)q ? GoldBright : new Color(0, 0, 0, 0.25f)).rectTransform
                        .Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(124 + s * 46, 0), new Vector2(42, 42));
                }
                Txt(row, "Lbl", $"<b>{CookingSystem.QualityName(q)}</b>   x{n}", 30, Ink, TextAnchor.MiddleLeft, false, false)
                    .rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(276, 0), new Vector2(420, 60));
            }

            summaryFooter.text =
                $"Combo cao nhất: <b>x{maxCombo}</b>    •    Giá trị dự kiến: <b><color=#8A5A00>{totalGold} vàng</color></b>  /  <b><color=#1F5E8A>+{totalRep}</color></b> danh tiếng\n" +
                "<i>Bưng món ra bàn: đến gần khách đang chờ và bấm [F]</i>";

            SparkleBurst();
            SoundManager.Instance?.PlaySfx(SoundManager.Instance.coinClip, 0.8f);
        }

        // ================================================================ FX

        private Vector2 BgPoint(Vector2 n)
        {
            var r = fxLayer.rect;
            return new Vector2((n.x - 0.5f) * r.width, (n.y - 0.5f) * r.height);
        }

        private Fx SpawnFx(RectTransform layer, Sprite sprite, Color color, Vector2 pos, Vector2 size, Vector2 vel, float life)
        {
            var img = Img(layer, "Fx", sprite, color);
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            var fx = new Fx { rt = img.rectTransform, g = img, vel = vel, maxLife = life, baseAlpha = color.a };
            fxList.Add(fx);
            return fx;
        }

        private void Popup(string text, Color color, int size, Vector2 pos)
        {
            var t = Txt(popupLayer, "Popup", text, size, color, TextAnchor.MiddleCenter, true);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.GetComponent<Outline>().effectDistance = new Vector2(3, -3);
            t.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(900, size + 20));
            fxList.Add(new Fx { rt = t.rectTransform, g = t, vel = new Vector2(0, 70), maxLife = 1.1f, startScale = 1.7f, scaleTime = 0.18f });
        }

        private void Punch(RectTransform rt, float scale)
        {
            if (rt == null) return;
            fxList.Add(new Fx { rt = rt, g = null, maxLife = 0.2f, startScale = scale, scaleTime = 0.2f });
        }

        private void Shake(float time, float magnitude)
        {
            shakeTime = Mathf.Max(shakeTime, time);
            shakeMag = Mathf.Max(shakeMag, magnitude);
        }

        private IEnumerator DropIngredients()
        {
            var pan = BgPoint(PanPoint);
            foreach (var req in cookRecipe.ingredients)
            {
                for (int i = 0; i < req.count; i++)
                {
                    var start = BgPoint(new Vector2(0.5f + Random.Range(-0.08f, 0.08f), 0.72f));
                    var fx = SpawnFx(fxLayer, IngredientIcon(req.type), Color.white, start, new Vector2(110, 110),
                        new Vector2(Random.Range(-60f, 60f), Random.Range(0f, 120f)), 3f);
                    fx.gravity = -1900f;
                    fx.spin = Random.Range(-240f, 240f);
                    fx.killBelowY = pan.y + Random.Range(-10f, 20f);
                    fx.onKill = () =>
                    {
                        SoundManager.Instance?.PlaySfx(SoundManager.Instance.pickupClip, 0.6f);
                        SteamBurst(4, false);
                        Shake(0.08f, 5f);
                    };
                    yield return Wait(0.16f);
                }
            }
            yield return Wait(0.6f);
        }

        private void ChopFx()
        {
            var pan = BgPoint(PanPoint);
            var icon = IngredientIcon(cookRecipe.ingredients[0].type);
            for (int i = 0; i < 2; i++)
            {
                var fx = SpawnFx(fxLayer, icon, Color.white, pan + new Vector2(Random.Range(-90f, 90f), 60f), new Vector2(34, 34),
                    new Vector2(Random.Range(-260f, 260f), Random.Range(260f, 480f)), 0.8f);
                fx.gravity = -1500f;
                fx.spin = Random.Range(-720f, 720f);
                fx.killBelowY = pan.y;
            }
        }

        private void SpiceFx()
        {
            var pan = BgPoint(PanPoint);
            Color[] spice = { new Color(0.9f, 0.25f, 0.15f), new Color(0.95f, 0.85f, 0.4f), new Color(0.4f, 0.75f, 0.3f), Color.white };
            for (int i = 0; i < 10; i++)
            {
                var fx = SpawnFx(fxLayer, White, spice[Random.Range(0, spice.Length)], pan + new Vector2(Random.Range(-70f, 70f), Random.Range(170f, 240f)),
                    Vector2.one * Random.Range(6f, 11f), new Vector2(Random.Range(-40f, 40f), Random.Range(-80f, 0f)), 1f);
                fx.gravity = -700f;
                fx.killBelowY = pan.y;
            }
        }

        private void SteamBurst(int count, bool smoky)
        {
            var pan = BgPoint(PanPoint);
            for (int i = 0; i < count; i++)
            {
                Color c = smoky ? new Color(0.12f, 0.1f, 0.1f, 0.55f) : new Color(1f, 1f, 1f, 0.3f);
                var fx = SpawnFx(fxLayer, SoftCircle, c, pan + new Vector2(Random.Range(-140f, 140f), Random.Range(0f, 40f)),
                    Vector2.one * Random.Range(70f, 140f), new Vector2(Random.Range(-40f, 40f), Random.Range(120f, 220f)), Random.Range(1f, 1.8f));
                fx.grow = 0.9f;
            }
        }

        private void SparkleBurst()
        {
            for (int i = 0; i < 16; i++)
            {
                float a = Random.Range(0f, Mathf.PI * 2f);
                float sp = Random.Range(260f, 620f);
                var fx = SpawnFx(popupLayer, Star, GoldBright, new Vector2(-40, -20), Vector2.one * Random.Range(18f, 34f),
                    new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * sp, Random.Range(0.6f, 1f));
                fx.gravity = -500f;
                fx.spin = Random.Range(-360f, 360f);
            }
        }

        private void UpdateAmbient(float dt)
        {
            float flick = Mathf.PerlinNoise(Time.time * 3.1f, 0.3f);
            fireGlow.color = new Color(1f, 0.55f, 0.15f, 0.2f + flick * 0.18f);
            float stove = Mathf.PerlinNoise(Time.time * 2.3f, 7.7f);
            stoveGlow.color = new Color(1f, 0.55f + heatIntensity * 0.1f, 0.2f, 0.16f + stove * 0.14f + heatIntensity * 0.25f);

            steamTimer -= dt;
            if (steamTimer <= 0f)
            {
                steamTimer = Mathf.Lerp(0.28f, 0.06f, heatIntensity);
                var pan = BgPoint(PanPoint);
                var fx = SpawnFx(fxLayer, SoftCircle, new Color(1f, 1f, 1f, 0.16f + heatIntensity * 0.12f),
                    pan + new Vector2(Random.Range(-150f, 150f), Random.Range(0f, 30f)), Vector2.one * Random.Range(60f, 120f),
                    new Vector2(Random.Range(-18f, 18f), Random.Range(50f, 100f)), Random.Range(1.8f, 2.6f));
                fx.grow = 0.6f;
            }

            emberTimer -= dt;
            if (emberTimer <= 0f)
            {
                emberTimer = 0.09f;
                Vector2 origin = Random.value < 0.6f ? FirePoint : StovePoint;
                var p = BgPoint(origin) + new Vector2(Random.Range(-70f, 70f), Random.Range(-20f, 30f));
                var fx = SpawnFx(fxLayer, White, Color.Lerp(Ember, GoldBright, Random.value), p, Vector2.one * Random.Range(4f, 8f),
                    new Vector2(Random.Range(-25f, 25f), Random.Range(70f, 160f)), Random.Range(0.9f, 1.6f));
                fx.gravity = 20f;
            }
        }

        private void UpdateFx(float dt)
        {
            for (int i = fxList.Count - 1; i >= 0; i--)
            {
                var fx = fxList[i];
                if (fx.rt == null) { fxList.RemoveAt(i); continue; }

                fx.life += dt;
                float k = Mathf.Clamp01(fx.life / fx.maxLife);

                if (fx.scaleTime > 0f)
                {
                    float s = Mathf.Lerp(fx.startScale, 1f, Mathf.Clamp01(fx.life / fx.scaleTime));
                    fx.rt.localScale = Vector3.one * s;
                }

                if (fx.g == null)
                {
                    // Hiệu ứng "nảy" cho phần tử UI có sẵn
                    if (fx.life >= fx.maxLife)
                    {
                        fx.rt.localScale = Vector3.one;
                        fxList.RemoveAt(i);
                    }
                    continue;
                }

                fx.vel.y += fx.gravity * dt;
                fx.rt.anchoredPosition += fx.vel * dt;
                if (fx.spin != 0f) fx.rt.localRotation = Quaternion.Euler(0, 0, fx.rt.localEulerAngles.z + fx.spin * dt);
                if (fx.grow != 0f) fx.rt.localScale = Vector3.one * (1f + fx.grow * fx.life);

                var c = fx.g.color;
                c.a = fx.baseAlpha * (k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f);
                fx.g.color = c;

                bool landed = fx.vel.y < 0f && fx.rt.anchoredPosition.y <= fx.killBelowY;
                if (fx.life >= fx.maxLife || landed)
                {
                    if (landed) fx.onKill?.Invoke();
                    Destroy(fx.rt.gameObject);
                    fxList.RemoveAt(i);
                }
            }
        }

        private void ClearFx()
        {
            foreach (var fx in fxList)
            {
                if (fx.rt == null) continue;
                if (fx.g != null) Destroy(fx.rt.gameObject);
                else fx.rt.localScale = Vector3.one;
            }
            fxList.Clear();
            comboTimer = 0f;
            if (comboGroup != null) comboGroup.alpha = 0f;
        }

        private void UpdateShake(float dt)
        {
            if (shakeTime > 0f)
            {
                shakeTime -= dt;
                content.anchoredPosition = Random.insideUnitCircle * shakeMag;
                if (shakeTime <= 0f)
                {
                    shakeMag = 0f;
                    content.anchoredPosition = Vector2.zero;
                }
            }
        }

        private static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }
    }
}
