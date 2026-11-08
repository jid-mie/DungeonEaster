using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NguyenMinhHiep.Core;

namespace NguyenMinhHiep.UI
{
    /// <summary>
    /// Canvas Hub Duy Nhất Toàn Game (UnifiedGameHubCanvas - DontDestroyOnLoad)
    /// tích hợp xuyên suốt 3 Scene (Dungeon, Village, Restaurant).
    /// - Lấy chuẩn format giao diện của NgoVanHien (TavernUI.cs + TavernHUD.cs + InventoryPanel.cs + QuickBar.cs).
    /// - Bao gồm:
    ///   1. Máu (HP 100/100) & Thể lực (Stamina 100/100)
    ///   2. Vũ khí (Kiếm & Cung Lv.1-4, phím [Q] chuyển đổi)
    ///   3. Tiền (Gold) & Danh tiếng (Reputation)
    ///   4. Inventory chung (10+ ô Balo + 4 ô Quick Bar + Tra cứu danh mục JSON)
    /// - Tự động ẩn các HUD rời rạc cũ trong từng Scene để toàn bộ game chỉ dùng chung 1 Canvas Hub duy nhất.
    /// </summary>
    public class UnifiedGameHubCanvas : MonoBehaviour
    {
        private static UnifiedGameHubCanvas instance;
        public static UnifiedGameHubCanvas Instance => instance;

        [Header("--- Cài đặt Tích hợp 3 Scene ---")]
        [Tooltip("Tự động ẩn các HUD riêng lẻ của từng Scene (DungeonHUD, TavernHUD, VillageHUD) để dùng duy nhất Hub này")]
        [SerializeField] private bool replaceSceneLocalHUDs = true;

        [Tooltip("Áp dụng quy tắc GDD 3.2.1: Chặn mở túi đồ khi quái trong Dungeon đang áp sát truy đuổi")]
        [SerializeField] private bool blockInventoryWhenEnemyNear = true;

        private Canvas hubCanvas;
        private UnifiedHubHUD hudPanel;
        private UnifiedInventoryPanel inventoryPanel;
        private BossHealthBarUI bossHealthBar;

        public UnifiedHubHUD HUD => hudPanel;
        public UnifiedInventoryPanel Inventory => inventoryPanel;
        public bool IsInventoryOpen => inventoryPanel != null && inventoryPanel.IsOpen;

        /// <summary>
        /// Tự động khởi tạo Unified Canvas Hub ngay khi bất kỳ Scene nào trong 3 Scene chạy Play Mode.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrapUnifiedHub()
        {
            EnsureHubExists();
        }

        public static UnifiedGameHubCanvas EnsureHubExists()
        {
            // Đảm bảo SharedGameDataManager luôn được khởi tạo trước
            var _ = SharedGameDataManager.Instance;

            if (instance != null) return instance;
            instance = FindObjectOfType<UnifiedGameHubCanvas>();
            if (instance != null) return instance;

            var go = new GameObject("UnifiedGameHubCanvas_Hiep");
            instance = go.AddComponent<UnifiedGameHubCanvas>();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Đảm bảo dữ liệu chung đã nạp JSON và khởi tạo đủ 10 ô Bag + 4 ô QuickBar trước khi dựng UI
            var dataMgr = SharedGameDataManager.Instance;
            dataMgr.InitializeStarterStateIfNeeded();

            EnsureEventSystem();
            BuildMasterCanvas();

            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            if (replaceSceneLocalHUDs)
            {
                SuppressLegacySceneHUDs();
            }
            hudPanel?.RefreshAll();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                instance = null;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureEventSystem();
            if (replaceSceneLocalHUDs)
            {
                SuppressLegacySceneHUDs();
            }
            if (inventoryPanel != null && inventoryPanel.IsOpen)
            {
                inventoryPanel.gameObject.SetActive(false);
            }
            hudPanel?.RefreshAll();
        }

        private void Update()
        {
            HandleGlobalHotkeys();
        }

        private void HandleGlobalHotkeys()
        {
            bool otherSceneModalOpen =
                (DungeonEater.Tavern.TavernUI.Instance != null && DungeonEater.Tavern.TavernUI.Instance.IsModalOpen) ||
                (DungeonEater.TranHuuHau.UI.VillageShopUI.Instance != null && DungeonEater.TranHuuHau.UI.VillageShopUI.Instance.IsOpen) ||
                (DungeonEater.TranHuuHau.UI.VillageDialogueUI.Instance != null && DungeonEater.TranHuuHau.UI.VillageDialogueUI.Instance.IsOpen);

            // 1. Phím [I]: Bật / Tắt Túi đồ chung
            if (Input.GetKeyDown(KeyCode.I))
            {
                // Nếu Tavern.InventoryPanel vừa tự mở do bấm [I] trong Nhà hàng (không phải chế độ giao món), đóng nó lại để nhường cho UnifiedInventoryPanel
                CloseTavernNormalInventoryIfOpen();
                if (!otherSceneModalOpen || IsInventoryOpen)
                {
                    ToggleInventory();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && IsInventoryOpen)
            {
                inventoryPanel.gameObject.SetActive(false);
            }

            if (otherSceneModalOpen) return;

            // 2. Phím [Q]: Đổi Vũ khí (nếu trong scene chưa có HunterPlayer tự xử lý phím Q)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                var hunter = FindObjectOfType<HunterPlayer>();
                if (hunter == null)
                {
                    SharedGameDataManager.Instance?.SwapWeapon();
                }
            }

            // 3. Phím [R]: Dùng nhanh Bình máu (+20 HP)
            if (Input.GetKeyDown(KeyCode.R))
            {
                SharedGameDataManager.Instance?.UseHealingPotionShortcut();
            }

            // 4. Phím [Tab]: Chuyển ô Quick Slot tiếp theo (chuẩn NgoVanHien)
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                var data = SharedGameDataManager.Instance;
                if (data != null && data.QuickSlots.Count > 0)
                {
                    int next = (data.SelectedQuickSlot + 1) % data.QuickSlots.Count;
                    data.SelectQuickSlot(next);
                }
            }

            // 5. Phím số [1] - [4]: Dùng vật phẩm trên thanh Quick Bar (nếu đang ở Nhà hàng và ô đó là Món ăn, nhường cho TavernUI giao món cho khách)
            for (int i = 0; i < 4; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    var data = SharedGameDataManager.Instance;
                    if (data != null && i < data.QuickSlots.Count)
                    {
                        var slot = data.QuickSlots[i];
                        if (DungeonEater.Tavern.TavernUI.Instance != null && !slot.IsEmpty && slot.Definition != null && slot.Definition.IsDish)
                        {
                            data.SelectQuickSlot(i);
                            continue;
                        }
                    }
                    SharedGameDataManager.Instance?.UseQuickSlotItem(i);
                }
            }
        }

        private void CloseTavernNormalInventoryIfOpen()
        {
            var tavernInvs = FindObjectsOfType<DungeonEater.Tavern.InventoryPanel>(true);
            foreach (var inv in tavernInvs)
            {
                if (inv != null && inv.IsOpen && inv.ServeTarget == null)
                {
                    inv.Close();
                }
            }
        }

        public void ToggleInventory()
        {
            if (inventoryPanel == null) return;

            if (inventoryPanel.IsOpen)
            {
                inventoryPanel.gameObject.SetActive(false);
                return;
            }

            // Kiểm tra quy tắc GDD 3.2.1: Không thể mở Inventory khi đang bị quái vật truy đuổi trong Dungeon
            if (blockInventoryWhenEnemyNear && IsPlayerTargetedByDungeonEnemy())
            {
                hudPanel?.ShowToast("⚠️ Không thể mở Túi đồ khi đang bị quái vật truy đuổi!");
                return;
            }

            inventoryPanel.gameObject.SetActive(true);
            inventoryPanel.Refresh();
        }

        private bool IsPlayerTargetedByDungeonEnemy()
        {
            var hunter = FindObjectOfType<HunterPlayer>();
            if (hunter == null) return false;

            var enemies = FindObjectsOfType<NguyenVanHai.Enemies.EnemyBase>();
            foreach (var e in enemies)
            {
                if (e != null && e.IsAlive && Vector2.Distance(e.transform.position, hunter.transform.position) <= 6.5f)
                {
                    return true;
                }
            }

            var bosses = FindObjectsOfType<NguyenMinhHiep.Bosses.BossBase>();
            foreach (var b in bosses)
            {
                if (b != null && b.IsAlive && Vector2.Distance(b.transform.position, hunter.transform.position) <= 8.5f)
                {
                    return true;
                }
            }
            return false;
        }

        private void BuildMasterCanvas()
        {
            hubCanvas = gameObject.GetComponent<Canvas>();
            if (hubCanvas == null) hubCanvas = gameObject.AddComponent<Canvas>();
            hubCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hubCanvas.sortingOrder = 500; // Hiển thị trên mọi thành phần cảnh

            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            // 1. Tạo lớp HUD cố định (Máu, Lực, Tiền, Reputation, Vũ khí, QuickBar)
            var hudGo = HiepUIKit.CreateRect(transform, "UnifiedHub_HUD");
            HiepUIKit.Stretch(hudGo);
            hudPanel = hudGo.gameObject.AddComponent<UnifiedHubHUD>();
            hudPanel.Initialize(this);

            // 2. Tích hợp luôn Thanh máu Boss (BossHealthBarUI của Nguyễn Minh Hiệp) vào chung Canvas Hub
            var bossBarGo = HiepUIKit.CreateRect(transform, "UnifiedHub_BossHealthBar");
            HiepUIKit.Stretch(bossBarGo);
            bossHealthBar = bossBarGo.gameObject.AddComponent<BossHealthBarUI>();

            // 3. Tạo cửa sổ Inventory chung (Mặc định đóng, mở bằng phím [I])
            var invGo = HiepUIKit.CreateRect(transform, "UnifiedHub_InventoryModal");
            HiepUIKit.Stretch(invGo);
            inventoryPanel = invGo.gameObject.AddComponent<UnifiedInventoryPanel>();
            inventoryPanel.Initialize();
            inventoryPanel.gameObject.SetActive(false);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        /// <summary>
        /// Vô hiệu hóa các HUD và Inventory rời rạc của từng Scene riêng lẻ (Dungeon, Village, Restaurant)
        /// để tránh chồng lấp giao diện, giữ lại các bảng tương tác đặc thù (Bếp nấu ăn, Shop Trưởng làng, Hội thoại).
        /// </summary>
        public void SuppressLegacySceneHUDs()
        {
            // 1. Scene Dungeon (NguyenVanHai)
            var dungeonHuds = FindObjectsOfType<NguyenVanHai.UI.DungeonHUD>(true);
            foreach (var h in dungeonHuds)
            {
                if (h != null) h.gameObject.SetActive(false);
            }

            var dungeonInvs = FindObjectsOfType<NguyenVanHai.UI.DungeonInventoryUI>(true);
            foreach (var inv in dungeonInvs)
            {
                if (inv == null) continue;
                // Ẩn Canvas con nhưng giữ script để LootDrop vẫn gọi được AddItem, sau đó SharedGameDataManager hút sang kho chung
                var childCanvas = inv.transform.Find("Dungeon_Inventory_Canvas");
                if (childCanvas != null) childCanvas.gameObject.SetActive(false);
                inv.enabled = false;
            }

            // 2. Scene Restaurant (NgoVanHien) - chỉ ẩn Wallet & Hint trùng lặp trên TavernHUD, giữ lại Shift (Đồng hồ ca) & Orders (Khách gọi món)
            var tavernHuds = FindObjectsOfType<DungeonEater.Tavern.TavernHUD>(true);
            foreach (var h in tavernHuds)
            {
                if (h == null) continue;
                var wallet = h.transform.Find("Wallet");
                if (wallet != null) wallet.gameObject.SetActive(false);
                var hint = h.transform.Find("Hint");
                if (hint != null) hint.gameObject.SetActive(false);
                var orders = h.transform.Find("Orders") as RectTransform;
                if (orders != null) orders.anchoredPosition = new Vector2(20f, -246f);
            }

            var tavernQuickBars = FindObjectsOfType<DungeonEater.Tavern.QuickBar>(true);
            foreach (var qb in tavernQuickBars)
            {
                if (qb != null) qb.gameObject.SetActive(false);
            }
            // Không tắt gameObject của Tavern.InventoryPanel để TavernUI.OpenServe(customer) vẫn hoạt động khi đứng trước bàn khách bấm [F]

            // 3. Scene Village (TranHuuHau) - ẩn TopLeftInfo, TopRightActions, ControlsGuide và VillageInventoryUI, giữ lại InteractPromptBox, Shop, Dialogue
            var villageHuds = FindObjectsOfType<DungeonEater.TranHuuHau.UI.VillageHUD>(true);
            foreach (var vh in villageHuds)
            {
                if (vh == null) continue;
                var topLeft = vh.transform.Find("TopLeftInfo");
                if (topLeft != null) topLeft.gameObject.SetActive(false);
                var topRight = vh.transform.Find("TopRightActions");
                if (topRight != null) topRight.gameObject.SetActive(false);
                var guide = vh.transform.Find("ControlsGuide");
                if (guide != null) guide.gameObject.SetActive(false);
            }

            var villageInvs = FindObjectsOfType<DungeonEater.TranHuuHau.UI.VillageInventoryUI>(true);
            foreach (var vinv in villageInvs)
            {
                if (vinv != null) vinv.gameObject.SetActive(false);
            }
        }
    }
}
