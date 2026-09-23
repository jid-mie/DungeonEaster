using UnityEngine;
using UnityEngine.UI;
using DungeonEater.Core;
using DungeonEater.Dungeon;

namespace DungeonEater.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD Bars")]
        [SerializeField] private Image hpBarFill;
        [SerializeField] private Text hpBarText;
        [SerializeField] private Image staminaBarFill;
        [SerializeField] private Text staminaBarText;
        [SerializeField] private Text weaponBadgeText;

        [Header("Economy & Inventory Display")]
        [SerializeField] private Text goldText;
        [SerializeField] private Text reputationText;
        [SerializeField] private Text bagCountText;

        [Header("Modal Panels")]
        [SerializeField] private GameObject cookingPanel;
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private GameObject dungeonHUD;
        [SerializeField] private GameObject restaurantHUD;

        private PlayerController player;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            player = FindObjectOfType<PlayerController>();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
                GameManager.Instance.OnGoldChanged += UpdateGoldUI;
                GameManager.Instance.OnReputationChanged += UpdateReputationUI;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged += UpdateBagUI;
            }

            UpdateAllUI();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
                GameManager.Instance.OnGoldChanged -= UpdateGoldUI;
                GameManager.Instance.OnReputationChanged -= UpdateReputationUI;
            }
        }

        private void Update()
        {
            UpdatePlayerBars();

            // Phím ESC đóng modal
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseAllModals();
            }
        }

        private void UpdatePlayerBars()
        {
            if (player == null) player = FindObjectOfType<PlayerController>();
            if (player == null) return;

            // HP
            if (hpBarFill != null) hpBarFill.fillAmount = player.CurrentHealth / player.MaxHealth;
            if (hpBarText != null) hpBarText.text = $"{Mathf.CeilToInt(player.CurrentHealth)} / {player.MaxHealth}";

            // Stamina
            if (staminaBarFill != null) staminaBarFill.fillAmount = player.CurrentStamina / player.MaxStamina;
            if (staminaBarText != null) staminaBarText.text = $"STA {Mathf.CeilToInt(player.CurrentStamina)}%";

            // Weapon
            if (weaponBadgeText != null)
            {
                weaponBadgeText.text = player.CurrentWeapon == WeaponType.Sword ? "🗡 KIẾM" : "🏹 CUNG";
            }
        }

        private void UpdateGoldUI(int gold)
        {
            if (goldText != null) goldText.text = gold.ToString();
        }

        private void UpdateReputationUI(int rep)
        {
            if (reputationText != null) reputationText.text = rep.ToString();
        }

        private void UpdateBagUI()
        {
            if (bagCountText != null && InventoryManager.Instance != null)
            {
                bagCountText.text = $"{InventoryManager.Instance.GetTotalItemCount()} / {InventoryManager.Instance.MaxCapacity}";
            }
        }

        public void UpdateAllUI()
        {
            if (GameManager.Instance != null)
            {
                UpdateGoldUI(GameManager.Instance.Gold);
                UpdateReputationUI(GameManager.Instance.Reputation);
            }
            UpdateBagUI();
        }

        private void HandleStateChanged(GameState state)
        {
            bool isDungeon = state == GameState.Dungeon;
            if (dungeonHUD != null) dungeonHUD.SetActive(isDungeon);
            if (restaurantHUD != null) restaurantHUD.SetActive(!isDungeon);
        }

        public void OpenCookingModal()
        {
            if (cookingPanel != null) cookingPanel.SetActive(true);
        }

        public void CloseCookingModal()
        {
            if (cookingPanel != null) cookingPanel.SetActive(false);
        }

        public void OpenUpgradeModal()
        {
            if (upgradePanel != null) upgradePanel.SetActive(true);
        }

        public void CloseUpgradeModal()
        {
            if (upgradePanel != null) upgradePanel.SetActive(false);
        }

        public void CloseAllModals()
        {
            CloseCookingModal();
            CloseUpgradeModal();
        }
    }
}

