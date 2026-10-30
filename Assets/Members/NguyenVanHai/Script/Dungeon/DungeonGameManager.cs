using UnityEngine;
using NguyenVanHai.UI;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Điều phối viên trung tâm cho Scene Dungeon:
    /// Quản lý hệ thống UI (HUD, Hòm đồ balo 10 ô + 4 quick slot, Màn hình Thắng/Thua).
    /// </summary>
    public class DungeonGameManager : MonoBehaviour
    {
        public static DungeonGameManager Instance { get; private set; }

        [Header("Tham chiếu Hệ thống UI (Gắn trên cùng GameObject hoặc con)")]
        [SerializeField] private DungeonHUD hud;
        [SerializeField] private DungeonInventoryUI inventoryUI;
        [SerializeField] private DungeonEndScreens endScreens;

        public DungeonHUD HUD => hud;
        public DungeonInventoryUI InventoryUI => inventoryUI;
        public DungeonEndScreens EndScreens => endScreens;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureUISystems();
        }

        private void EnsureUISystems()
        {
            if (hud == null) hud = GetComponentInChildren<DungeonHUD>();
            if (hud == null) hud = gameObject.AddComponent<DungeonHUD>();

            if (inventoryUI == null) inventoryUI = GetComponentInChildren<DungeonInventoryUI>();
            if (inventoryUI == null) inventoryUI = gameObject.AddComponent<DungeonInventoryUI>();

            if (endScreens == null) endScreens = GetComponentInChildren<DungeonEndScreens>();
            if (endScreens == null) endScreens = gameObject.AddComponent<DungeonEndScreens>();
        }
    }
}
