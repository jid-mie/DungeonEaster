using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Gốc UI của quán: tự dựng Canvas, HUD, hòm đồ, bảng nấu ăn, thông báo và nhắc phím tương tác.
    /// Khi mở bảng (modal) thì khóa di chuyển của ChiefPlayer, đóng bằng Esc.
    /// </summary>
    public class TavernUI : MonoBehaviour
    {
        public static TavernUI Instance { get; private set; }

        public static Color ColorInfo => UIKit.Text;
        public static Color ColorGood => UIKit.Good;
        public static Color ColorBad => UIKit.Bad;

        [SerializeField] private KeyCode inventoryKey = KeyCode.I;
        [SerializeField] private KeyCode closeKey = KeyCode.Escape;
        [SerializeField] private Sprite goldIcon;
        [SerializeField] private Sprite reputationIcon;

        private RectTransform root;
        private TavernHUD hud;
        private InventoryPanel inventory;
        private CookingPanel cooking;
        private ToastView toast;
        private InteractPrompt prompt;
        private ChiefPlayer player;

        public Sprite GoldIcon => goldIcon;
        public Sprite ReputationIcon => reputationIcon;
        public bool IsModalOpen => (inventory != null && inventory.IsOpen) || (cooking != null && cooking.IsOpen);
        public ChiefPlayer Player
        {
            get
            {
                if (player == null) player = FindObjectOfType<ChiefPlayer>();
                return player;
            }
        }

        private void Awake()
        {
            Instance = this;
            BuildCanvas();
            EnsureEventSystem();

            hud = CreatePart<TavernHUD>("HUD");
            prompt = CreatePart<InteractPrompt>("InteractPrompt");
            inventory = CreatePart<InventoryPanel>("InventoryPanel");
            cooking = CreatePart<CookingPanel>("CookingPanel");
            toast = CreatePart<ToastView>("Toasts");

            hud.Build(this);
            prompt.Build(this);
            inventory.Build(this);
            cooking.Build(this);
            toast.Build(this);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(closeKey) && IsModalOpen)
            {
                CloseAll();
            }
            else if (Input.GetKeyDown(inventoryKey) && !cooking.IsOpen)
            {
                if (inventory.IsOpen) inventory.Close();
                else inventory.Open();
            }
        }

        public void OpenCooking()
        {
            CloseAll();
            cooking.Open();
        }

        public void OpenInventory()
        {
            CloseAll();
            inventory.Open();
        }

        public void OpenServe(TavernCustomer customer)
        {
            CloseAll();
            inventory.OpenForServing(customer);
        }

        public void CloseAll()
        {
            if (inventory.IsOpen) inventory.Close();
            if (cooking.IsOpen) cooking.Close();
        }

        public void ShowToast(string message, Color color, float duration = 3f)
        {
            if (toast != null) toast.Show(message, color, duration);
        }

        /// <summary>Các panel gọi khi mở/đóng để khóa/mở di chuyển người chơi.</summary>
        public void NotifyModalChanged()
        {
            if (Player != null) Player.CanMove = !IsModalOpen;
        }

        private T CreatePart<T>(string partName) where T : Component
        {
            var rt = UIKit.Rect(partName, root);
            UIKit.Stretch(rt, 0, 0, 0, 0);
            return rt.gameObject.AddComponent<T>();
        }

        public RectTransform Root => root;

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("TavernCanvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            root = (RectTransform)canvasGo.transform;
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}
