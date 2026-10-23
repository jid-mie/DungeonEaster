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
        [Tooltip("Bán kính quét vật trước mặt, giống ChiefPlayer.interactionRadius")]
        [SerializeField] private float facingCheckRadius = 0.35f;
        [SerializeField] private Sprite goldIcon;
        [SerializeField] private Sprite reputationIcon;

        private RectTransform root;
        private TavernHUD hud;
        private InventoryPanel inventory;
        private CookingPanel cooking;
        private ToastView toast;
        private InteractPrompt prompt;
        private QuickBar quickBar;
        private ChiefPlayer player;
        private int interactableMask;

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
            interactableMask = LayerMask.GetMask("Interactable");
            BuildCanvas();
            EnsureEventSystem();

            hud = CreatePart<TavernHUD>("HUD");
            prompt = CreatePart<InteractPrompt>("InteractPrompt");
            quickBar = CreatePart<QuickBar>("QuickBar");
            inventory = CreatePart<InventoryPanel>("InventoryPanel");
            cooking = CreatePart<CookingPanel>("CookingPanel");
            toast = CreatePart<ToastView>("Toasts");

            hud.Build(this);
            prompt.Build(this);
            quickBar.Build(this);
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

            if (cooking.IsOpen || PlayerInventory.Instance == null) return;
            for (int k = 0; k < PlayerInventory.Instance.QuickSlotCount && k < 9; k++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + k) || Input.GetKeyDown(KeyCode.Keypad1 + k))
                {
                    UseQuickSlot(k);
                }
            }
        }

        /// <summary>Phím 1-5: giao món ở ô quick item cho khách ở bàn trước mặt (hoặc bàn đang mở bảng phục vụ).</summary>
        private void UseQuickSlot(int k)
        {
            var inv = PlayerInventory.Instance;
            int index = inv.QuickSlotIndex(k);
            var slot = inv.Slots[index];
            quickBar.Flash(k);

            if (slot.IsEmpty)
            {
                ShowToast($"Ô quick item {k + 1} đang trống.", ColorInfo, 1.5f);
                return;
            }

            TavernCustomer customer = inventory.IsOpen ? inventory.ServeTarget : null;
            if (customer == null && !inventory.IsOpen)
            {
                var table = FindFacingInteractable(out _) as TavernTable;
                if (table != null) customer = table.Customer;
            }
            if (customer == null || customer.State != CustomerState.Waiting)
            {
                ShowToast($"Đứng trước bàn có khách đang chờ rồi bấm {k + 1} để giao {slot.item.displayName}.", ColorInfo, 2f);
                return;
            }

            var item = slot.item;
            var outcome = customer.TryServe(item, index);
            ReportServe(outcome, customer, item);
            if (outcome.result == ServeResult.Served && inventory.IsOpen) inventory.Close();
        }

        /// <summary>Thông báo kết quả giao món (dùng chung cho bấm ô trong hòm đồ và phím quick item).</summary>
        public void ReportServe(ServeOutcome outcome, TavernCustomer customer, ItemData item)
        {
            switch (outcome.result)
            {
                case ServeResult.Served:
                    string tip = outcome.tip > 0 ? $" (gồm {outcome.tip} tiền boa)" : "";
                    ShowToast($"Đã phục vụ {customer.Order.DisplayName} cho bàn {customer.Table.TableNumber}: +{outcome.gold + outcome.tip} vàng{tip}, +{outcome.reputation} danh tiếng.", ColorGood);
                    break;
                case ServeResult.WrongDish:
                    ShowToast($"Khách không gọi {item.displayName}! Khách muốn {customer.Order.DisplayName}.", ColorBad);
                    break;
                case ServeResult.DishNotInInventory:
                    ShowToast("Không còn món này trong hòm đồ.", ColorBad);
                    break;
            }
        }

        /// <summary>Vật tương tác ngay trước mặt ChiefPlayer (cùng vùng quét với ChiefPlayer.TryInteract).</summary>
        public IInteractable FindFacingInteractable(out Collider2D hit)
        {
            hit = null;
            if (Player == null) return null;
            hit = Physics2D.OverlapCircle(Player.GetInteractionCheckPosition(), facingCheckRadius, interactableMask);
            return hit != null ? hit.GetComponent<IInteractable>() : null;
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
