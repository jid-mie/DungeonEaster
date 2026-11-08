using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using NguyenMinhHiep.Data;
using NguyenMinhHiep.Bosses;
using NguyenMinhHiep.Combat;

namespace NguyenMinhHiep.Core
{
    /// <summary>
    /// Ô chứa vật phẩm dùng chung cho cả Balo (Bag) và Thanh dùng nhanh (Quick Bar).
    /// </summary>
    [Serializable]
    public class SharedInventorySlot
    {
        public string itemId = "";
        public int count = 0;

        public bool IsEmpty => string.IsNullOrEmpty(itemId) || count <= 0;

        public ItemDefinitionDto Definition => IsEmpty ? null : GameItemDatabase.GetItem(itemId);

        public int MaxStack
        {
            get
            {
                var def = Definition;
                return def != null && def.maxStack > 0 ? def.maxStack : 10;
            }
        }

        public string DisplayName
        {
            get
            {
                var def = Definition;
                return def != null ? def.displayName : itemId;
            }
        }

        public Sprite Icon => Definition != null ? Definition.GetIcon() : null;

        public void Clear()
        {
            itemId = "";
            count = 0;
        }

        public void Set(string id, int amount)
        {
            if (string.IsNullOrEmpty(id) || amount <= 0)
            {
                Clear();
                return;
            }
            itemId = GameItemDatabase.NormalizeItemId(id);
            count = amount;
        }
    }

    /// <summary>
    /// Cấu trúc dữ liệu tuần tự hóa (Serializable) lưu toàn bộ trạng thái Người chơi xuyên suốt 3 Scene:
    /// - Tiền (Gold), Danh tiếng (Reputation)
    /// - Máu (HP), Thể lực (Stamina)
    /// - Vũ khí (Kiếm & Cung, cấp độ, vũ khí đang cầm)
    /// - Hòm đồ (BagSlots + QuickSlots)
    /// </summary>
    [Serializable]
    public class SharedPlayerStateData
    {
        public int gold = 500;
        public int reputation = 20;

        public float currentHP = 100f;
        public float maxHP = 100f;
        public float currentStamina = 100f;
        public float maxStamina = 100f;

        // 0 = Sword (Kiếm), 1 = Bow (Cung)
        public int activeWeaponIndex = 0;
        public int swordLevel = 1;
        public int bowLevel = 1;

        public int maxBagSlots = 10;
        public int selectedQuickSlot = 0;

        public List<SharedInventorySlot> bagSlots = new List<SharedInventorySlot>();
        public List<SharedInventorySlot> quickSlots = new List<SharedInventorySlot>();
        public List<int> defeatedBossFloors = new List<int>();
    }

    /// <summary>
    /// Class chung duy nhất (Singleton DontDestroyOnLoad) quản lý Tiền, Reputation, Inventory, Máu, Thể lực, Vũ khí
    /// tích hợp đồng bộ xuyên suốt 3 Scene (Dungeon, Village, Restaurant).
    /// </summary>
    public class SharedGameDataManager : MonoBehaviour
    {
        private static SharedGameDataManager instance;
        public static SharedGameDataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SharedGameDataManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("SharedGameDataManager_Hiep");
                        instance = go.AddComponent<SharedGameDataManager>();
                    }
                }
                return instance;
            }
        }

        [Header("--- Trạng thái Chung Toàn Game (3 Scene) ---")]
        [SerializeField] private SharedPlayerStateData state = new SharedPlayerStateData();

        // Events phát tới Unified Canvas Hub
        public event Action OnWalletChanged;          // Gold, Reputation
        public event Action OnVitalsChanged;          // HP, Stamina
        public event Action OnWeaponChanged;          // Active Weapon, Level
        public event Action OnInventoryChanged;       // BagSlots, QuickSlots
        public event Action<string> OnNotification;   // Thông báo nổi trên HUD

        // References tới các script thành viên trong từng Scene để đồng bộ 2 chiều
        private HunterPlayer boundHunterPlayer;
        private NguyenVanHai.UI.DungeonInventoryUI boundDungeonInventory;
        private FieldInfo dungeonBagSlotsField;

        // Reflection fields cho HunterPlayer để đồng bộ máu, bình máu và sát thương vũ khí mà không sửa code của Hải
        private FieldInfo hunterCurrentHealthField;
        private FieldInfo hunterPotionCountField;
        private FieldInfo hunterCurrentWeaponField;
        private FieldInfo hunterSwordLightField;
        private FieldInfo hunterSwordHeavyField;
        private FieldInfo hunterSwordSkillField;

        private DungeonEater.Tavern.PlayerWallet boundTavernWallet;
        private DungeonEater.Tavern.PlayerInventory boundTavernInventory;
        private readonly Dictionary<string, DungeonEater.Tavern.ItemData> tavernItemCache =
            new Dictionary<string, DungeonEater.Tavern.ItemData>(StringComparer.OrdinalIgnoreCase);

        private DungeonEater.TranHuuHau.Inventory.VillageWallet boundVillageWallet;
        private DungeonEater.TranHuuHau.Inventory.VillageInventory boundVillageInventory;

        private bool isSyncingWallet;
        private bool isSyncingInventory;
        private float fullBagWarningCooldown;

        // Properties truy cập nhanh
        public int Gold => state.gold;
        public int Reputation => state.reputation;
        public float CurrentHP => state.currentHP;
        public float MaxHP => state.maxHP;
        public float CurrentStamina => state.currentStamina;
        public float MaxStamina => state.maxStamina;
        public int ActiveWeaponIndex => state.activeWeaponIndex;
        public int SwordLevel => state.swordLevel;
        public int BowLevel => state.bowLevel;
        public int MaxBagSlots => state.maxBagSlots;
        public int SelectedQuickSlot => state.selectedQuickSlot;

        public IReadOnlyList<SharedInventorySlot> BagSlots => state.bagSlots;
        public IReadOnlyList<SharedInventorySlot> QuickSlots => state.quickSlots;
        public IReadOnlyList<int> DefeatedBossFloors => state.defeatedBossFloors;

        public WeaponDefinitionDto ActiveWeaponDef =>
            GameItemDatabase.GetWeapon(state.activeWeaponIndex == 0 ? "weapon_sword" : "weapon_bow");

        public int ActiveWeaponLevel => state.activeWeaponIndex == 0 ? state.swordLevel : state.bowLevel;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            GameItemDatabase.EnsureLoaded();
            InitializeStarterStateIfNeeded();

            SceneManager.sceneLoaded += HandleSceneLoaded;
            BossBase.OnBossDefeatedGeneric += HandleBossDefeated;
            NguyenVanHai.Enemies.BossKingBoarAI.OnBossDefeated += HandleKingBoarDefeated;
        }

        private void Start()
        {
            BindSceneSystems();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                BossBase.OnBossDefeatedGeneric -= HandleBossDefeated;
                NguyenVanHai.Enemies.BossKingBoarAI.OnBossDefeated -= HandleKingBoarDefeated;
                UnbindSceneSystems();
                instance = null;
            }
        }

        private void Update()
        {
            if (fullBagWarningCooldown > 0f)
            {
                fullBagWarningCooldown -= Time.unscaledDeltaTime;
            }

            // Đồng bộ liên tục chỉ số sinh tồn từ HunterPlayer nếu đang ở trong Dungeon
            if (boundHunterPlayer != null)
            {
                bool changed = false;
                if (Mathf.Abs(state.currentHP - boundHunterPlayer.CurrentHealth) > 0.1f)
                {
                    state.currentHP = Mathf.Clamp(boundHunterPlayer.CurrentHealth, 0f, state.maxHP);
                    changed = true;
                }
                if (Mathf.Abs(state.currentStamina - boundHunterPlayer.CurrentStamina) > 0.1f)
                {
                    state.currentStamina = Mathf.Clamp(boundHunterPlayer.CurrentStamina, 0f, state.maxStamina);
                    changed = true;
                }
                int hunterWeapon = boundHunterPlayer.CurrentWeapon == HunterPlayer.WeaponType.Sword ? 0 : 1;
                if (state.activeWeaponIndex != hunterWeapon)
                {
                    state.activeWeaponIndex = hunterWeapon;
                    OnWeaponChanged?.Invoke();
                }
                if (changed)
                {
                    OnVitalsChanged?.Invoke();
                }
            }
            else
            {
                // Ở Làng hoặc Nhà hàng: tự hồi thể lực nhẹ nhàng về 100
                if (state.currentStamina < state.maxStamina)
                {
                    state.currentStamina = Mathf.Min(state.maxStamina, state.currentStamina + 15f * Time.deltaTime);
                    OnVitalsChanged?.Invoke();
                }
            }

            // Đồng bộ vật phẩm nhặt được từ LootDrop (qua DungeonInventoryUI) vào kho chung
            PollDungeonLootBridge();
        }

        public void InitializeStarterStateIfNeeded()
        {
            var cfg = GameItemDatabase.Catalog.inventoryConfig;
            if (state.maxBagSlots <= 0) state.maxBagSlots = cfg.defaultBagSlots;
            if (state.maxHP <= 0) state.maxHP = cfg.defaultMaxHP;
            if (state.maxStamina <= 0) state.maxStamina = cfg.defaultMaxStamina;

            bool firstInit = state.bagSlots.Count == 0 && state.quickSlots.Count == 0;

            while (state.bagSlots.Count < state.maxBagSlots)
            {
                state.bagSlots.Add(new SharedInventorySlot());
            }
            while (state.quickSlots.Count < cfg.quickSlotCount)
            {
                state.quickSlots.Add(new SharedInventorySlot());
            }

            if (firstInit)
            {
                ResetToStarterKit();
            }
        }

        /// <summary>
        /// Khởi tạo bộ trang bị & nguyên liệu mẫu đầu game theo chuẩn báo cáo TaiLieu.md.
        /// </summary>
        public void ResetToStarterKit()
        {
            var cfg = GameItemDatabase.Catalog.inventoryConfig;
            state.gold = cfg.defaultStartingGold;
            state.reputation = cfg.defaultStartingReputation;
            state.maxHP = cfg.defaultMaxHP;
            state.currentHP = state.maxHP;
            state.maxStamina = cfg.defaultMaxStamina;
            state.currentStamina = state.maxStamina;
            state.activeWeaponIndex = 0;
            state.swordLevel = 1;
            state.bowLevel = 1;
            state.selectedQuickSlot = 0;

            for (int i = 0; i < state.bagSlots.Count; i++) state.bagSlots[i].Clear();
            for (int i = 0; i < state.quickSlots.Count; i++) state.quickSlots[i].Clear();

            // Quick Bar: Bình máu hồi phục + Lựu đạn nổ
            state.quickSlots[0].Set("item_healing_potion", 3);
            state.quickSlots[1].Set("item_frag_grenade", 2);

            // Bag: Nguyên liệu mẫu để test nấu ăn ở Nhà hàng và giao dịch ở Làng
            AddItem("ing_glow_mushroom", 4, false);
            AddItem("ing_slime_gel", 3, false);
            AddItem("ing_bone_shard", 3, false);
            AddItem("ing_giant_snake_meat", 1, false);
            AddItem("ing_dried_flesh", 2, false);
            AddItem("ing_boar_shoulder", 1, false);

            NotifyAllChanged();
        }

        public void NotifyAllChanged()
        {
            OnWalletChanged?.Invoke();
            OnVitalsChanged?.Invoke();
            OnWeaponChanged?.Invoke();
            OnInventoryChanged?.Invoke();
        }

        // =====================================================================
        // 1. QUẢN LÝ TIỀN (GOLD) & DANH TIẾNG (REPUTATION)
        // =====================================================================

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            state.gold += amount;
            PushWalletToScene();
            OnWalletChanged?.Invoke();
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0) return true;
            if (state.gold < amount) return false;
            state.gold -= amount;
            PushWalletToScene();
            OnWalletChanged?.Invoke();
            return true;
        }

        public void SetGold(int value)
        {
            state.gold = Mathf.Max(0, value);
            PushWalletToScene();
            OnWalletChanged?.Invoke();
        }

        public void AddReputation(int delta)
        {
            if (delta == 0) return;
            state.reputation = Mathf.Max(0, state.reputation + delta);
            PushWalletToScene();
            OnWalletChanged?.Invoke();
        }

        public void SetReputation(int value)
        {
            state.reputation = Mathf.Max(0, value);
            PushWalletToScene();
            OnWalletChanged?.Invoke();
        }

        // =====================================================================
        // 2. QUẢN LÝ MÁU (HP), THỂ LỰC (STAMINA) & VŨ KHÍ (WEAPONS)
        // =====================================================================

        public void SetHealth(float current, float max = 100f)
        {
            state.maxHP = Mathf.Max(1f, max);
            state.currentHP = Mathf.Clamp(current, 0f, state.maxHP);
            PushHealthToHunter();
            OnVitalsChanged?.Invoke();
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            state.currentHP = Mathf.Clamp(state.currentHP + amount, 0f, state.maxHP);
            PushHealthToHunter();
            OnVitalsChanged?.Invoke();
        }

        public void SetStamina(float current, float max = 100f)
        {
            state.maxStamina = Mathf.Max(1f, max);
            state.currentStamina = Mathf.Clamp(current, 0f, state.maxStamina);
            OnVitalsChanged?.Invoke();
        }

        public void SwapWeapon()
        {
            SetActiveWeapon(state.activeWeaponIndex == 0 ? 1 : 0);
        }

        public void SetActiveWeapon(int index)
        {
            state.activeWeaponIndex = Mathf.Clamp(index, 0, 1);
            if (boundHunterPlayer != null && hunterCurrentWeaponField != null)
            {
                var targetType = state.activeWeaponIndex == 0
                    ? HunterPlayer.WeaponType.Sword
                    : HunterPlayer.WeaponType.Bow;
                if (boundHunterPlayer.CurrentWeapon != targetType)
                {
                    hunterCurrentWeaponField.SetValue(boundHunterPlayer, targetType);
                }
            }
            OnWeaponChanged?.Invoke();
        }

        /// <summary>
        /// Rèn nâng cấp Vũ khí (Kiếm hoặc Cung) theo đúng Bảng 7.5 trong TaiLieu.md:
        /// Kiểm tra điều kiện hạ Boss Tầng 1-3, kiểm tra nguyên liệu nâng cấp và trừ Gold.
        /// </summary>
        public bool UpgradeWeapon(bool isSword)
        {
            var wpDef = GameItemDatabase.GetWeapon(isSword ? "weapon_sword" : "weapon_bow");
            int currentLv = isSword ? state.swordLevel : state.bowLevel;
            if (currentLv >= 4)
            {
                OnNotification?.Invoke("Vũ khí đã đạt cấp tối đa (Lv.4)!");
                return false;
            }

            int nextLv = currentLv + 1;
            var nextStats = wpDef != null ? wpDef.GetLevelStats(nextLv) : null;
            if (nextStats == null) return false;

            // 1. Kiểm tra điều kiện hạ Boss tầng tương ứng (Bảng 7.5)
            if (nextStats.requiredBossFloor > 0 && !state.defeatedBossFloors.Contains(nextStats.requiredBossFloor))
            {
                OnNotification?.Invoke($"⚠️ Cần hạ gục Boss Tầng {nextStats.requiredBossFloor} trước khi rèn lên Lv.{nextLv}!");
                return false;
            }

            // 2. Kiểm tra đủ nguyên liệu rèn vũ khí (Bảng 7.5)
            if (nextStats.upgradeMaterials != null)
            {
                foreach (var mat in nextStats.upgradeMaterials)
                {
                    if (mat == null || string.IsNullOrEmpty(mat.itemId) || mat.count <= 0) continue;
                    int have = GetItemCount(mat.itemId);
                    if (have < mat.count)
                    {
                        var matDef = GameItemDatabase.GetItem(mat.itemId);
                        string matName = matDef != null ? matDef.displayName : mat.itemId;
                        OnNotification?.Invoke($"⚠️ Thiếu nguyên liệu rèn Lv.{nextLv}: Cần {mat.count}x {matName} (Đang có: {have})!");
                        return false;
                    }
                }
            }

            // 3. Kiểm tra đủ Vàng
            int cost = nextStats.upgradeCostGold;
            if (state.gold < cost)
            {
                OnNotification?.Invoke($"⚠️ Không đủ Vàng để rèn Lv.{nextLv} (Cần {cost} Gold, đang có {state.gold} Gold)!");
                return false;
            }

            // 4. Trừ Vàng và Nguyên liệu
            SpendGold(cost);
            if (nextStats.upgradeMaterials != null)
            {
                foreach (var mat in nextStats.upgradeMaterials)
                {
                    if (mat == null || string.IsNullOrEmpty(mat.itemId) || mat.count <= 0) continue;
                    RemoveItem(mat.itemId, mat.count);
                }
            }

            if (isSword) state.swordLevel = nextLv;
            else state.bowLevel = nextLv;

            PushWeaponStatsToHunter();
            OnWeaponChanged?.Invoke();
            OnNotification?.Invoke($"⚒️ Đã rèn thành công {(isSword ? "Kiếm Thợ Săn" : "Cung Tầm Xa")} lên Lv.{nextLv}!");
            return true;
        }

        // =====================================================================
        // 3. QUẢN LÝ HÒM ĐỒ (BAG SLOTS) & THANH QUICK BAR (4 Ô)
        // =====================================================================

        public void SelectQuickSlot(int index)
        {
            if (index < 0 || index >= state.quickSlots.Count) return;
            state.selectedQuickSlot = index;
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Mở rộng thêm +5 ô hành trang khi hạ gục Boss tầng mới (theo GDD mục 3.2.1).
        /// </summary>
        public void UnlockBossBagExpansion(int bossFloor)
        {
            if (bossFloor <= 0 || state.defeatedBossFloors.Contains(bossFloor)) return;
            state.defeatedBossFloors.Add(bossFloor);

            int bonus = GameItemDatabase.Catalog.inventoryConfig.bonusSlotsPerBoss;
            state.maxBagSlots += bonus;
            while (state.bagSlots.Count < state.maxBagSlots)
            {
                state.bagSlots.Add(new SharedInventorySlot());
            }

            OnInventoryChanged?.Invoke();
            OnNotification?.Invoke($"🎉 Hạ gục Boss Tầng {bossFloor}! Mở rộng thêm +{bonus} ô Balo (Tổng: {state.maxBagSlots} ô)!");
        }

        /// <summary>
        /// Thêm vật phẩm vào Inventory chung. Trả về số lượng thực tế đã thêm.
        /// </summary>
        public int AddItem(string idOrAlias, int count = 1, bool preferQuickBar = false)
        {
            if (string.IsNullOrEmpty(idOrAlias) || count <= 0) return 0;

            string canonicalId = GameItemDatabase.NormalizeItemId(idOrAlias);
            var def = GameItemDatabase.GetItem(canonicalId);
            int maxStack = def != null && def.maxStack > 0 ? def.maxStack : 10;

            int remaining = count;

            var primaryList = preferQuickBar ? state.quickSlots : state.bagSlots;
            var secondaryList = preferQuickBar ? state.bagSlots : state.quickSlots;

            // Nếu là vật phẩm tiêu hao (Bình máu / Lựu đạn) và đã có trên QuickBar thì ưu tiên cộng dồn vào QuickBar
            if (def != null && def.IsConsumable)
            {
                remaining = FillExistingStacks(state.quickSlots, canonicalId, maxStack, remaining);
            }

            remaining = FillExistingStacks(primaryList, canonicalId, maxStack, remaining);
            remaining = FillExistingStacks(secondaryList, canonicalId, maxStack, remaining);
            remaining = FillEmptySlots(primaryList, canonicalId, maxStack, remaining);
            remaining = FillEmptySlots(secondaryList, canonicalId, maxStack, remaining);

            int added = count - remaining;
            if (added > 0)
            {
                PushInventoryToScene();
                OnInventoryChanged?.Invoke();
            }
            return added;
        }

        private int FillExistingStacks(List<SharedInventorySlot> list, string canonicalId, int maxStack, int remaining)
        {
            for (int i = 0; i < list.Count && remaining > 0; i++)
            {
                var s = list[i];
                if (!s.IsEmpty && string.Equals(s.itemId, canonicalId, StringComparison.OrdinalIgnoreCase) && s.count < maxStack)
                {
                    int add = Mathf.Min(maxStack - s.count, remaining);
                    s.count += add;
                    remaining -= add;
                }
            }
            return remaining;
        }

        private int FillEmptySlots(List<SharedInventorySlot> list, string canonicalId, int maxStack, int remaining)
        {
            for (int i = 0; i < list.Count && remaining > 0; i++)
            {
                var s = list[i];
                if (s.IsEmpty)
                {
                    int add = Mathf.Min(maxStack, remaining);
                    s.Set(canonicalId, add);
                    remaining -= add;
                }
            }
            return remaining;
        }

        public bool RemoveItem(string idOrAlias, int count = 1)
        {
            if (string.IsNullOrEmpty(idOrAlias) || count <= 0) return false;
            string canonicalId = GameItemDatabase.NormalizeItemId(idOrAlias);
            if (GetItemCount(canonicalId) < count) return false;

            int remaining = count;
            remaining = RemoveFromList(state.bagSlots, canonicalId, remaining);
            if (remaining > 0)
            {
                remaining = RemoveFromList(state.quickSlots, canonicalId, remaining);
            }

            PushInventoryToScene();
            OnInventoryChanged?.Invoke();
            return true;
        }

        private int RemoveFromList(List<SharedInventorySlot> list, string canonicalId, int remaining)
        {
            for (int i = 0; i < list.Count && remaining > 0; i++)
            {
                var s = list[i];
                if (!s.IsEmpty && string.Equals(s.itemId, canonicalId, StringComparison.OrdinalIgnoreCase))
                {
                    int take = Mathf.Min(s.count, remaining);
                    s.count -= take;
                    remaining -= take;
                    if (s.count <= 0) s.Clear();
                }
            }
            return remaining;
        }

        public int GetItemCount(string idOrAlias)
        {
            if (string.IsNullOrEmpty(idOrAlias)) return 0;
            string canonicalId = GameItemDatabase.NormalizeItemId(idOrAlias);
            int total = 0;

            foreach (var s in state.bagSlots)
            {
                if (!s.IsEmpty && string.Equals(s.itemId, canonicalId, StringComparison.OrdinalIgnoreCase))
                    total += s.count;
            }
            foreach (var s in state.quickSlots)
            {
                if (!s.IsEmpty && string.Equals(s.itemId, canonicalId, StringComparison.OrdinalIgnoreCase))
                    total += s.count;
            }
            return total;
        }

        /// <summary>
        /// Di chuyển, gộp stack hoặc hoán đổi vật phẩm giữa 2 ô (Balo <-> QuickBar), chuẩn format NgoVanHien.
        /// </summary>
        public void MoveOrSwapSlots(bool fromQuick, int fromIdx, bool toQuick, int toIdx)
        {
            var srcList = fromQuick ? state.quickSlots : state.bagSlots;
            var dstList = toQuick ? state.quickSlots : state.bagSlots;

            if (fromIdx < 0 || fromIdx >= srcList.Count) return;
            if (toIdx < 0 || toIdx >= dstList.Count) return;
            if (fromQuick == toQuick && fromIdx == toIdx) return;

            var src = srcList[fromIdx];
            var dst = dstList[toIdx];
            if (src.IsEmpty) return;

            if (dst.IsEmpty)
            {
                dst.Set(src.itemId, src.count);
                src.Clear();
            }
            else if (string.Equals(src.itemId, dst.itemId, StringComparison.OrdinalIgnoreCase) && dst.count < dst.MaxStack)
            {
                int move = Mathf.Min(dst.MaxStack - dst.count, src.count);
                dst.count += move;
                src.count -= move;
                if (src.count <= 0) src.Clear();
            }
            else
            {
                string tmpId = dst.itemId;
                int tmpCount = dst.count;
                dst.Set(src.itemId, src.count);
                src.Set(tmpId, tmpCount);
            }

            PushInventoryToScene();
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Chuột phải (RMB) để trang bị nhanh từ Balo xuống QuickBar hoặc tháo từ QuickBar về Balo (theo GDD 3.2.1).
        /// </summary>
        public void QuickTransferSlot(bool fromQuick, int index)
        {
            if (fromQuick)
            {
                if (index < 0 || index >= state.quickSlots.Count) return;
                var qSlot = state.quickSlots[index];
                if (qSlot.IsEmpty) return;

                int remaining = FillExistingStacks(state.bagSlots, qSlot.itemId, qSlot.MaxStack, qSlot.count);
                remaining = FillEmptySlots(state.bagSlots, qSlot.itemId, qSlot.MaxStack, remaining);
                qSlot.count = remaining;
                if (qSlot.count <= 0) qSlot.Clear();
            }
            else
            {
                if (index < 0 || index >= state.bagSlots.Count) return;
                var bSlot = state.bagSlots[index];
                if (bSlot.IsEmpty) return;

                int remaining = FillExistingStacks(state.quickSlots, bSlot.itemId, bSlot.MaxStack, bSlot.count);
                remaining = FillEmptySlots(state.quickSlots, bSlot.itemId, bSlot.MaxStack, remaining);
                bSlot.count = remaining;
                if (bSlot.count <= 0) bSlot.Clear();
            }

            PushInventoryToScene();
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Sử dụng vật phẩm tại ô QuickSlot (phím 1-4 hoặc phím R).
        /// </summary>
        public bool UseQuickSlotItem(int quickIndex)
        {
            if (quickIndex < 0 || quickIndex >= state.quickSlots.Count) return false;
            state.selectedQuickSlot = quickIndex;
            var slot = state.quickSlots[quickIndex];
            if (slot.IsEmpty)
            {
                OnInventoryChanged?.Invoke();
                return false;
            }

            var def = slot.Definition;
            if (def == null) return false;

            if (def.IsDish)
            {
                OnNotification?.Invoke($"🍽️ [{def.displayName}] là món ăn phục vụ khách tại Nhà Hàng (đứng trước bàn khách bấm [F] hoặc [1-4])!");
                return false;
            }

            if (def.IsConsumable && def.healAmount > 0)
            {
                if (state.currentHP >= state.maxHP - 0.1f)
                {
                    OnNotification?.Invoke("Máu đã đầy (100/100)!");
                    return false;
                }
                Heal(def.healAmount);
                slot.count--;
                if (slot.count <= 0) slot.Clear();
                PushInventoryToScene();
                OnInventoryChanged?.Invoke();
                OnNotification?.Invoke($"🧪 Đã dùng {def.displayName}: +{def.healAmount} HP!");
                return true;
            }

            if (def.IsConsumable && def.damageAmount > 0)
            {
                TriggerCombatGrenadeEffect(def);
                slot.count--;
                if (slot.count <= 0) slot.Clear();
                PushInventoryToScene();
                OnInventoryChanged?.Invoke();
                return true;
            }

            OnNotification?.Invoke($"📦 [{def.displayName}] là nguyên liệu dùng để nấu ăn hoặc rèn vũ khí!");
            return false;
        }

        /// <summary>
        /// Phím R dùng nhanh Bình Máu Hồi Phục (ưu tiên tìm trong QuickBar rồi đến Bag).
        /// </summary>
        public bool UseHealingPotionShortcut()
        {
            for (int i = 0; i < state.quickSlots.Count; i++)
            {
                if (!state.quickSlots[i].IsEmpty && state.quickSlots[i].itemId == "item_healing_potion")
                {
                    return UseQuickSlotItem(i);
                }
            }
            for (int i = 0; i < state.bagSlots.Count; i++)
            {
                if (!state.bagSlots[i].IsEmpty && state.bagSlots[i].itemId == "item_healing_potion")
                {
                    if (state.currentHP >= state.maxHP - 0.1f)
                    {
                        OnNotification?.Invoke("Máu đã đầy (100/100)!");
                        return false;
                    }
                    Heal(20f);
                    state.bagSlots[i].count--;
                    if (state.bagSlots[i].count <= 0) state.bagSlots[i].Clear();
                    PushInventoryToScene();
                    OnInventoryChanged?.Invoke();
                    OnNotification?.Invoke("🧪 Đã dùng Bình Máu Hồi Phục (+20 HP)!");
                    return true;
                }
            }
            OnNotification?.Invoke("Không còn Bình Máu Hồi Phục trong hành trang!");
            return false;
        }

        // =====================================================================
        // 4. HIỆU ỨNG CHIẾN ĐẤU CỦA 3 LOẠI LỰU ĐẠN (BẢNG 7.7 GDD)
        // =====================================================================

        private void TriggerCombatGrenadeEffect(ItemDefinitionDto def)
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            Vector3 center = playerGo != null ? playerGo.transform.position : Vector3.zero;
            if (boundHunterPlayer != null)
            {
                center = boundHunterPlayer.transform.position + (Vector3)(boundHunterPlayer.FacingDirection * 2.5f);
            }

            float radius = def.effectRadius > 0f ? def.effectRadius : 2.5f;

            if (def.id == "item_ice_grenade")
            {
                // Lựu đạn Băng: 15 DMG + đóng băng quái thường 2.0s (Boss bị trói chân 1.0s)
                BossAoEIndicator.CreateCircle(center, radius, 0.4f, new Color(0.2f, 0.8f, 1f, 0.8f));
                StartCoroutine(FreezeTargetsRoutine(center, radius, 15f, 2.0f, 1.0f));
                OnNotification?.Invoke("❄️ Đã ném Lựu Đạn Băng: 15 DMG + Đóng băng 2.0s (Boss 1.0s)!");
            }
            else if (def.id == "item_poison_grenade")
            {
                // Lựu đạn Độc: Vùng độc 3.0m trong 4.0s, làm chậm 40% và gây 8 DMG mỗi 0.5s (tổng 64 DMG)
                StartCoroutine(PoisonCloudRoutine(center, radius, 4.0f, 0.5f, 8f));
                OnNotification?.Invoke("☠️ Đã ném Lựu Đạn Độc: Khói độc 4.0s (8 DMG/0.5s + Chậm 40%)!");
            }
            else
            {
                // Lựu đạn Nổ: 50 DMG vật lý diện rộng bán kính 2.5m + đẩy lùi
                BossAoEIndicator.CreateCircle(center, radius, 0.25f, new Color(1f, 0.45f, 0.1f, 0.85f));
                Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
                foreach (var hit in hits)
                {
                    if (hit == null) continue;
                    var enemy = hit.GetComponentInParent<NguyenVanHai.Enemies.EnemyBase>();
                    if (enemy != null && enemy.IsAlive)
                    {
                        Vector2 kb = (enemy.transform.position - center).normalized * 3.5f;
                        enemy.TakeDamage(def.damageAmount, kb);
                        continue;
                    }
                    var boss = hit.GetComponentInParent<BossBase>();
                    if (boss != null && boss.IsAlive)
                    {
                        boss.TakeDamage(def.damageAmount);
                    }
                }
                OnNotification?.Invoke($"💣 Đã ném {def.displayName}: {def.damageAmount} DMG AoE ({radius}m)!");
            }
        }

        private IEnumerator FreezeTargetsRoutine(Vector3 center, float radius, float initialDmg, float enemyFreezeSec, float bossRootSec)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
            var frozenEnemies = new List<NguyenVanHai.Enemies.EnemyBase>();
            var rootedBosses = new List<BossBase>();

            foreach (var hit in hits)
            {
                if (hit == null) continue;
                var enemy = hit.GetComponentInParent<NguyenVanHai.Enemies.EnemyBase>();
                if (enemy != null && enemy.IsAlive && !frozenEnemies.Contains(enemy))
                {
                    enemy.TakeDamage(initialDmg, Vector2.zero);
                    if (enemy.IsAlive)
                    {
                        frozenEnemies.Add(enemy);
                        var rb = enemy.GetComponent<Rigidbody2D>();
                        if (rb != null) rb.velocity = Vector2.zero;
                        enemy.enabled = false;
                        var sr = enemy.GetComponent<SpriteRenderer>();
                        if (sr != null) sr.color = new Color(0.4f, 0.85f, 1f, 1f);
                    }
                    continue;
                }

                var boss = hit.GetComponentInParent<BossBase>();
                if (boss != null && boss.IsAlive && !rootedBosses.Contains(boss))
                {
                    boss.TakeDamage(initialDmg);
                    if (boss.IsAlive)
                    {
                        rootedBosses.Add(boss);
                        var rb = boss.GetComponent<Rigidbody2D>();
                        if (rb != null) rb.velocity = Vector2.zero;
                        boss.enabled = false;
                    }
                }
            }

            yield return new WaitForSeconds(bossRootSec);
            foreach (var b in rootedBosses)
            {
                if (b != null && b.IsAlive) b.enabled = true;
            }

            yield return new WaitForSeconds(Mathf.Max(0f, enemyFreezeSec - bossRootSec));
            foreach (var e in frozenEnemies)
            {
                if (e != null)
                {
                    if (e.IsAlive) e.enabled = true;
                    var sr = e.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = Color.white;
                }
            }
        }

        private IEnumerator PoisonCloudRoutine(Vector3 center, float radius, float duration, float tickInterval, float dmgPerTick)
        {
            int ticks = Mathf.Max(1, Mathf.RoundToInt(duration / tickInterval));
            for (int t = 0; t < ticks; t++)
            {
                BossAoEIndicator.CreateCircle(center, radius, tickInterval * 0.9f, new Color(0.65f, 0.25f, 0.95f, 0.55f));
                Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
                foreach (var hit in hits)
                {
                    if (hit == null) continue;
                    var enemy = hit.GetComponentInParent<NguyenVanHai.Enemies.EnemyBase>();
                    if (enemy != null && enemy.IsAlive)
                    {
                        enemy.TakeDamage(dmgPerTick, Vector2.zero);
                        var rb = enemy.GetComponent<Rigidbody2D>();
                        if (rb != null) rb.velocity *= 0.6f; // Làm chậm 40%
                        continue;
                    }
                    var boss = hit.GetComponentInParent<BossBase>();
                    if (boss != null && boss.IsAlive)
                    {
                        boss.TakeDamage(dmgPerTick);
                        var rb = boss.GetComponent<Rigidbody2D>();
                        if (rb != null) rb.velocity *= 0.6f;
                    }
                }
                yield return new WaitForSeconds(tickInterval);
            }
        }

        // =====================================================================
        // 5. ĐỒNG BỘ 2 CHIỀU VỚI CÁC SCENE (DUNGEON, VILLAGE, RESTAURANT)
        // =====================================================================

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindSceneSystems();
            NotifyAllChanged();
        }

        private void HandleBossDefeated(BossBase boss)
        {
            if (boss != null)
            {
                UnlockBossBagExpansion(boss.DungeonFloor);
            }
        }

        private void HandleKingBoarDefeated()
        {
            UnlockBossBagExpansion(2);
        }

        public void BindSceneSystems()
        {
            UnbindSceneSystems();

            // 1. Scene Dungeon (NguyenVanHai)
            boundHunterPlayer = FindObjectOfType<HunterPlayer>();
            if (boundHunterPlayer != null)
            {
                CacheHunterReflectionFields();
                boundHunterPlayer.OnHealthChanged += HandleHunterHealthChanged;
                boundHunterPlayer.OnStaminaChanged += HandleHunterStaminaChanged;
                boundHunterPlayer.OnWeaponSwapped += HandleHunterWeaponSwapped;

                // Đẩy máu, bình máu và sát thương vũ khí từ SharedGameDataManager sang HunterPlayer
                PushHealthToHunter();
                PushWeaponStatsToHunter();
                SetActiveWeapon(state.activeWeaponIndex);
            }

            boundDungeonInventory = FindObjectOfType<NguyenVanHai.UI.DungeonInventoryUI>(true);
            if (boundDungeonInventory != null)
            {
                dungeonBagSlotsField = typeof(NguyenVanHai.UI.DungeonInventoryUI).GetField(
                    "bagSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            // 2. Scene Restaurant (NgoVanHien)
            boundTavernWallet = FindObjectOfType<DungeonEater.Tavern.PlayerWallet>();
            if (boundTavernWallet != null)
            {
                PushWalletToScene();
                boundTavernWallet.OnChanged += PullFromTavernWallet;
            }

            boundTavernInventory = FindObjectOfType<DungeonEater.Tavern.PlayerInventory>();
            if (boundTavernInventory != null)
            {
                BuildTavernItemLookupCache();
                PushInventoryToScene();
                boundTavernInventory.OnChanged += PullFromTavernInventory;
            }

            // 3. Scene Village (TranHuuHau)
            boundVillageWallet = FindObjectOfType<DungeonEater.TranHuuHau.Inventory.VillageWallet>();
            if (boundVillageWallet != null)
            {
                PushWalletToScene();
                boundVillageWallet.OnChanged += PullFromVillageWallet;
            }

            boundVillageInventory = FindObjectOfType<DungeonEater.TranHuuHau.Inventory.VillageInventory>();
            if (boundVillageInventory != null)
            {
                PushInventoryToScene();
                boundVillageInventory.OnInventoryChanged += PullFromVillageInventory;
            }
        }

        private void UnbindSceneSystems()
        {
            if (boundHunterPlayer != null)
            {
                boundHunterPlayer.OnHealthChanged -= HandleHunterHealthChanged;
                boundHunterPlayer.OnStaminaChanged -= HandleHunterStaminaChanged;
                boundHunterPlayer.OnWeaponSwapped -= HandleHunterWeaponSwapped;
                boundHunterPlayer = null;
            }
            if (boundTavernWallet != null)
            {
                boundTavernWallet.OnChanged -= PullFromTavernWallet;
                boundTavernWallet = null;
            }
            if (boundTavernInventory != null)
            {
                boundTavernInventory.OnChanged -= PullFromTavernInventory;
                boundTavernInventory = null;
            }
            if (boundVillageWallet != null)
            {
                boundVillageWallet.OnChanged -= PullFromVillageWallet;
                boundVillageWallet = null;
            }
            if (boundVillageInventory != null)
            {
                boundVillageInventory.OnInventoryChanged -= PullFromVillageInventory;
                boundVillageInventory = null;
            }
        }

        private void CacheHunterReflectionFields()
        {
            var t = typeof(HunterPlayer);
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            hunterCurrentHealthField = t.GetField("currentHealth", flags);
            hunterPotionCountField = t.GetField("potionCount", flags);
            hunterCurrentWeaponField = t.GetField("currentWeapon", flags);
            hunterSwordLightField = t.GetField("swordLightDamage", flags);
            hunterSwordHeavyField = t.GetField("swordHeavyDamage", flags);
            hunterSwordSkillField = t.GetField("swordSkillDamage", flags);
        }

        private void PushHealthToHunter()
        {
            if (boundHunterPlayer == null) return;
            if (hunterCurrentHealthField != null)
            {
                hunterCurrentHealthField.SetValue(boundHunterPlayer, Mathf.Clamp(state.currentHP, 1f, state.maxHP));
            }
            if (hunterPotionCountField != null)
            {
                // Đặt potionCount nội bộ của HunterPlayer = 0 để phím R chỉ đi qua một nguồn duy nhất là SharedGameDataManager
                hunterPotionCountField.SetValue(boundHunterPlayer, 0);
            }
        }

        private void PushWeaponStatsToHunter()
        {
            if (boundHunterPlayer == null) return;
            var swordDef = GameItemDatabase.GetWeapon("weapon_sword");
            if (swordDef != null)
            {
                var st = swordDef.GetLevelStats(state.swordLevel);
                if (hunterSwordLightField != null) hunterSwordLightField.SetValue(boundHunterPlayer, (float)st.lightDamage);
                if (hunterSwordHeavyField != null) hunterSwordHeavyField.SetValue(boundHunterPlayer, (float)st.heavyDamage);
                if (hunterSwordSkillField != null) hunterSwordSkillField.SetValue(boundHunterPlayer, (float)st.skillDamage);
            }
        }

        private void HandleHunterHealthChanged(float cur, float max)
        {
            state.maxHP = max > 0 ? max : 100f;
            state.currentHP = Mathf.Clamp(cur, 0f, state.maxHP);
            OnVitalsChanged?.Invoke();
        }

        private void HandleHunterStaminaChanged(float cur, float max)
        {
            state.maxStamina = max > 0 ? max : 100f;
            state.currentStamina = Mathf.Clamp(cur, 0f, state.maxStamina);
            OnVitalsChanged?.Invoke();
        }

        private void HandleHunterWeaponSwapped(HunterPlayer.WeaponType wp)
        {
            state.activeWeaponIndex = wp == HunterPlayer.WeaponType.Sword ? 0 : 1;
            OnWeaponChanged?.Invoke();
        }

        private void PollDungeonLootBridge()
        {
            if (boundDungeonInventory == null || dungeonBagSlotsField == null) return;
            var entries = dungeonBagSlotsField.GetValue(boundDungeonInventory) as NguyenVanHai.UI.DungeonInventoryUI.InventoryItemEntry[];
            if (entries == null) return;

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry != null && !entry.IsEmpty)
                {
                    string pickedId = entry.id;
                    int pickedCount = entry.count;
                    string pickedName = entry.name;

                    int added = AddItem(pickedId, pickedCount, false);
                    if (added >= pickedCount)
                    {
                        entry.Clear();
                        OnNotification?.Invoke($"🎒 Đã nhặt: +{added} {pickedName}");
                    }
                    else if (added > 0)
                    {
                        entry.count -= added;
                        OnNotification?.Invoke($"🎒 Đã nhặt: +{added} {pickedName} (Balo đã đầy!)");
                    }
                    else
                    {
                        if (fullBagWarningCooldown <= 0f)
                        {
                            fullBagWarningCooldown = 2.5f;
                            OnNotification?.Invoke("⚠️ Balo đã đầy! Không thể nhặt thêm vật phẩm.");
                        }
                    }
                }
            }
        }

        private void PushWalletToScene()
        {
            if (isSyncingWallet) return;
            isSyncingWallet = true;
            try
            {
                if (boundTavernWallet != null)
                {
                    int diffGold = state.gold - boundTavernWallet.Gold;
                    if (diffGold > 0) boundTavernWallet.AddGold(diffGold);
                    else if (diffGold < 0) boundTavernWallet.TrySpendGold(-diffGold);

                    int diffRep = state.reputation - boundTavernWallet.Reputation;
                    if (diffRep != 0) boundTavernWallet.AddReputation(diffRep);
                }

                if (boundVillageWallet != null)
                {
                    int diffGold = state.gold - boundVillageWallet.Gold;
                    if (diffGold > 0) boundVillageWallet.AddGold(diffGold);
                    else if (diffGold < 0) boundVillageWallet.SpendGold(-diffGold);

                    int diffRep = state.reputation - boundVillageWallet.Reputation;
                    if (diffRep != 0) boundVillageWallet.AddReputation(diffRep);
                }
            }
            finally
            {
                isSyncingWallet = false;
            }
        }

        private void PullFromTavernWallet()
        {
            if (isSyncingWallet || boundTavernWallet == null) return;
            state.gold = boundTavernWallet.Gold;
            state.reputation = boundTavernWallet.Reputation;
            PushWalletToScene();
            OnWalletChanged?.Invoke();
        }

        private void PullFromVillageWallet()
        {
            if (isSyncingWallet || boundVillageWallet == null) return;
            state.gold = boundVillageWallet.Gold;
            state.reputation = boundVillageWallet.Reputation;
            PushWalletToScene();
            OnWalletChanged?.Invoke();
        }

        private void BuildTavernItemLookupCache()
        {
            // Thu thập toàn bộ ItemData sẵn có trong Kitchen.Recipes và bộ nhớ để trùng khớp Reference 100% khi nấu ăn / giao món
            if (DungeonEater.Tavern.Kitchen.Instance != null && DungeonEater.Tavern.Kitchen.Instance.Recipes != null)
            {
                foreach (var r in DungeonEater.Tavern.Kitchen.Instance.Recipes)
                {
                    if (r == null) continue;
                    RegisterTavernItemData(r.dish);
                    if (r.ingredients != null)
                    {
                        foreach (var ing in r.ingredients)
                        {
                            RegisterTavernItemData(ing.item);
                        }
                    }
                }
            }

            var allLoaded = Resources.FindObjectsOfTypeAll<DungeonEater.Tavern.ItemData>();
            foreach (var itemData in allLoaded)
            {
                RegisterTavernItemData(itemData);
            }
        }

        private void RegisterTavernItemData(DungeonEater.Tavern.ItemData itemData)
        {
            if (itemData == null || string.IsNullOrEmpty(itemData.id)) return;
            string canonical = GameItemDatabase.NormalizeItemId(itemData.id);
            tavernItemCache[canonical] = itemData;
            tavernItemCache[itemData.id] = itemData;
        }

        private DungeonEater.Tavern.ItemData ResolveOrCreateTavernItemData(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            string canonical = GameItemDatabase.NormalizeItemId(itemId);
            if (tavernItemCache.TryGetValue(canonical, out var existing) && existing != null)
            {
                return existing;
            }

            var def = GameItemDatabase.GetItem(canonical);
            if (def == null) return null;

            var runtimeItem = ScriptableObject.CreateInstance<DungeonEater.Tavern.ItemData>();
            runtimeItem.name = def.id;
            runtimeItem.id = def.id;
            runtimeItem.displayName = def.displayName;
            runtimeItem.kind = def.IsDish ? DungeonEater.Tavern.ItemKind.Dish : DungeonEater.Tavern.ItemKind.Ingredient;
            runtimeItem.icon = def.GetIcon();
            runtimeItem.maxStack = Mathf.Max(1, def.maxStack);
            runtimeItem.sellPrice = def.sellPrice;
            runtimeItem.description = def.description;

            tavernItemCache[canonical] = runtimeItem;
            return runtimeItem;
        }

        private void PushInventoryToScene()
        {
            if (isSyncingInventory) return;
            isSyncingInventory = true;
            try
            {
                // 1. Đồng bộ sang PlayerInventory (Scene Restaurant của NgoVanHien)
                if (boundTavernInventory != null && boundTavernInventory.Slots != null)
                {
                    BuildTavernItemLookupCache();
                    var tSlots = boundTavernInventory.Slots;
                    int bagLimit = Mathf.Min(boundTavernInventory.BagSlotCount, state.bagSlots.Count);

                    for (int i = 0; i < boundTavernInventory.BagSlotCount && i < tSlots.Count; i++)
                    {
                        tSlots[i].Clear();
                        if (i < bagLimit && !state.bagSlots[i].IsEmpty)
                        {
                            var itemAsset = ResolveOrCreateTavernItemData(state.bagSlots[i].itemId);
                            if (itemAsset != null)
                            {
                                tSlots[i].item = itemAsset;
                                tSlots[i].count = state.bagSlots[i].count;
                            }
                        }
                    }

                    for (int q = 0; q < boundTavernInventory.QuickSlotCount; q++)
                    {
                        int slotIdx = boundTavernInventory.QuickSlotIndex(q);
                        if (slotIdx < 0 || slotIdx >= tSlots.Count) continue;
                        tSlots[slotIdx].Clear();
                        if (q < state.quickSlots.Count && !state.quickSlots[q].IsEmpty)
                        {
                            var itemAsset = ResolveOrCreateTavernItemData(state.quickSlots[q].itemId);
                            if (itemAsset != null)
                            {
                                tSlots[slotIdx].item = itemAsset;
                                tSlots[slotIdx].count = state.quickSlots[q].count;
                            }
                        }
                    }
                }

                // 2. Đồng bộ sang VillageInventory (Scene Village của TranHuuHau)
                if (boundVillageInventory != null && boundVillageInventory.Slots != null)
                {
                    var vSlots = boundVillageInventory.Slots;
                    for (int i = 0; i < vSlots.Count; i++)
                    {
                        vSlots[i].Clear();
                        if (i < state.bagSlots.Count && !state.bagSlots[i].IsEmpty)
                        {
                            var def = state.bagSlots[i].Definition;
                            if (def != null)
                            {
                                vSlots[i].item = new DungeonEater.TranHuuHau.Data.VillageItemData(
                                    def.id, def.displayName, def.subCategory, def.description,
                                    def.buyPrice, def.sellPrice, def.maxStack, def.GetIcon());
                                vSlots[i].count = state.bagSlots[i].count;
                            }
                        }
                    }
                }
            }
            finally
            {
                isSyncingInventory = false;
            }
        }

        private void PullFromTavernInventory()
        {
            if (isSyncingInventory || boundTavernInventory == null || boundTavernInventory.Slots == null) return;
            isSyncingInventory = true;
            try
            {
                var tSlots = boundTavernInventory.Slots;
                int bagLimit = Mathf.Min(boundTavernInventory.BagSlotCount, state.bagSlots.Count);

                for (int i = 0; i < bagLimit && i < tSlots.Count; i++)
                {
                    var slot = tSlots[i];
                    if (slot != null && !slot.IsEmpty)
                    {
                        RegisterTavernItemData(slot.item);
                        state.bagSlots[i].Set(slot.item.id, slot.count);
                    }
                    else
                    {
                        state.bagSlots[i].Clear();
                    }
                }

                for (int q = 0; q < state.quickSlots.Count && q < boundTavernInventory.QuickSlotCount; q++)
                {
                    int slotIdx = boundTavernInventory.QuickSlotIndex(q);
                    if (slotIdx < 0 || slotIdx >= tSlots.Count) continue;
                    var slot = tSlots[slotIdx];
                    if (slot != null && !slot.IsEmpty)
                    {
                        RegisterTavernItemData(slot.item);
                        state.quickSlots[q].Set(slot.item.id, slot.count);
                    }
                    else
                    {
                        state.quickSlots[q].Clear();
                    }
                }

                OnInventoryChanged?.Invoke();
            }
            finally
            {
                isSyncingInventory = false;
            }
        }

        private void PullFromVillageInventory()
        {
            if (isSyncingInventory || boundVillageInventory == null || boundVillageInventory.Slots == null) return;
            isSyncingInventory = true;
            try
            {
                for (int i = 0; i < boundVillageInventory.Slots.Count && i < state.bagSlots.Count; i++)
                {
                    var slot = boundVillageInventory.Slots[i];
                    if (slot != null && !slot.IsEmpty)
                    {
                        state.bagSlots[i].Set(slot.item.id, slot.count);
                    }
                    else
                    {
                        state.bagSlots[i].Clear();
                    }
                }
                OnInventoryChanged?.Invoke();
            }
            finally
            {
                isSyncingInventory = false;
            }
        }

        // =====================================================================
        // 6. LƯU / TẢI TRẠNG THÁI BẰNG JSON
        // =====================================================================

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "dungeon_eater_shared_state.json");

        public void SaveStateToJson()
        {
            try
            {
                string json = JsonUtility.ToJson(state, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[SharedGameDataManager] Đã lưu trạng thái chung tại: {SaveFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SharedGameDataManager] Không thể ghi file save JSON: {ex.Message}");
            }
        }

        public bool LoadStateFromJson()
        {
            try
            {
                if (!File.Exists(SaveFilePath)) return false;
                string json = File.ReadAllText(SaveFilePath);
                var loaded = JsonUtility.FromJson<SharedPlayerStateData>(json);
                if (loaded != null)
                {
                    state = loaded;
                    InitializeStarterStateIfNeeded();
                    NotifyAllChanged();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SharedGameDataManager] Lỗi đọc file save JSON: {ex.Message}");
            }
            return false;
        }
    }
}
