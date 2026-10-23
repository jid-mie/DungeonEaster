using UnityEngine;
using UnityEngine.UI;
using DungeonEater.TranHuuHau.Common;
using DungeonEater.TranHuuHau.Inventory;
using DungeonEater.TranHuuHau.NPC;

namespace DungeonEater.TranHuuHau.UI
{
    /// <summary>
    /// HUD hiển thị trên màn hình làng:
    /// - Góc trên bên trái: Tên làng, Vàng (10,000), Danh tiếng (1,000), Máu (100).
    /// - Góc trên bên phải: Nút mở Túi đồ [I], Tiệm trưởng làng [Shop].
    /// - Chính giữa phía dưới: Thanh gợi ý tương tác phím [E] khi lại gần NPC hoặc Nhà Trưởng Làng.
    /// - Hướng dẫn điều khiển trực quan.
    /// </summary>
    public class VillageHUD : MonoBehaviour
    {
        public static VillageHUD Instance { get; private set; }

        private Text goldText;
        private Text repText;
        private RectTransform interactPromptBox;
        private Text interactPromptText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildHUD();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (VillageWallet.Instance != null)
            {
                VillageWallet.Instance.OnChanged += UpdateWalletDisplay;
            }
            UpdateWalletDisplay();
        }

        private void BuildHUD()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // Góc trên bên trái: Khung thông tin Người chơi & Làng
            var topLeftPanel = HauUITheme.CreatePanel(transform, "TopLeftInfo", HauUITheme.WoodDarkest, false);
            HauUITheme.Anchor(topLeftPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(25, -25), new Vector2(420, 115));

            var villageTitle = HauUITheme.CreateLabel(topLeftPanel.transform, "VillageTitle", "🏘️ NGÔI LÀNG LONDO (VILLAGE)", 21, HauUITheme.TextCream, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(villageTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(16, -10), new Vector2(-32, 28));

            goldText = HauUITheme.CreateLabel(topLeftPanel.transform, "GoldDisplay", "💰 VÀNG: 10,000", 19, HauUITheme.GoldYellow, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(goldText.rectTransform, new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(16, 42), new Vector2(0, 30));

            repText = HauUITheme.CreateLabel(topLeftPanel.transform, "RepDisplay", "⭐ DANH TIẾNG: 1,000", 18, HauUITheme.RepCyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(repText.rectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(10, 42), new Vector2(-16, 30));

            var hpText = HauUITheme.CreateLabel(topLeftPanel.transform, "HpDisplay", "❤️ MÁU: 100 / 100 (Cố định)", 16, HauUITheme.HealthRed, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(hpText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(16, 12), new Vector2(-32, 24));

            // Góc trên bên phải: Các nút tiện ích
            var topRightPanel = HauUITheme.CreatePanel(transform, "TopRightActions", HauUITheme.WoodDarkest, false);
            HauUITheme.Anchor(topRightPanel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-25, -25), new Vector2(360, 56));

            var invBtn = HauUITheme.CreateButton(topRightPanel.transform, "InventoryBtn", "🎒 HÀNH TRANG [I]", 16, HauUITheme.BtnBrown, () =>
            {
                if (VillageInventoryUI.Instance != null) VillageInventoryUI.Instance.ToggleInventory();
            });
            HauUITheme.Anchor(invBtn.image.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(85, 0), new Vector2(160, 40));

            var shopBtn = HauUITheme.CreateButton(topRightPanel.transform, "ShopBtn", "🏪 TIỆM TRƯỞNG LÀNG", 15, HauUITheme.BtnGreen, () =>
            {
                if (VillageShopUI.Instance != null) VillageShopUI.Instance.OpenShop();
            });
            HauUITheme.Anchor(shopBtn.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(85, 0), new Vector2(160, 40));

            // Góc dưới bên trái: Hướng dẫn điều khiển
            var guidePanel = HauUITheme.CreatePanel(transform, "ControlsGuide", new Color32(0x1e, 0x14, 0x0c, 0xc8), false);
            HauUITheme.Anchor(guidePanel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(25, 25), new Vector2(460, 52));

            var guideText = HauUITheme.CreateLabel(guidePanel.transform, "GuideLabel", "<b>[WASD]</b> Di chuyển | <b>[E]</b> Tương tác | <b>[I]</b> Hòm đồ", 17, HauUITheme.TextCream, TextAnchor.MiddleCenter);
            HauUITheme.FillParent(guideText.rectTransform);

            // Chính giữa phía dưới: Thanh gợi ý tương tác phím [E]
            interactPromptBox = HauUITheme.CreatePanel(transform, "InteractPromptBox", HauUITheme.WoodDark, false).rectTransform;
            HauUITheme.Anchor(interactPromptBox, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 110), new Vector2(520, 50));

            interactPromptText = HauUITheme.CreateLabel(interactPromptBox.transform, "PromptText", "[E] Nói chuyện", 20, HauUITheme.GoldYellow, TextAnchor.MiddleCenter, FontStyle.Bold);
            HauUITheme.FillParent(interactPromptText.rectTransform);

            interactPromptBox.gameObject.SetActive(false);
        }

        private void Update()
        {
            UpdateInteractionPrompt();
        }

        private void UpdateInteractionPrompt()
        {
            if (interactPromptBox == null) return;

            // Nếu đang mở Shop hoặc Dialogue thì ẩn prompt
            if ((VillageShopUI.Instance != null && VillageShopUI.Instance.IsOpen) ||
                (VillageDialogueUI.Instance != null && VillageDialogueUI.Instance.IsOpen) ||
                (VillageInventoryUI.Instance != null && VillageInventoryUI.Instance.IsOpen))
            {
                interactPromptBox.gameObject.SetActive(false);
                return;
            }

            // Kiểm tra xem người chơi có đang ở gần ElderHouse không
            var elderTrigger = FindObjectOfType<ElderHouseTrigger>();
            if (elderTrigger != null && elderTrigger.IsPlayerInRange)
            {
                interactPromptText.text = $"<b>[E]</b> {elderTrigger.PromptText}";
                interactPromptBox.gameObject.SetActive(true);
                return;
            }

            // Kiểm tra các NPC
            var npcs = FindObjectsOfType<VillageNPC>();
            VillageNPC nearest = null;
            float minDist = float.MaxValue;
            var player = GameObject.FindGameObjectWithTag("Player");
            Vector3 pPos = player != null ? player.transform.position : Vector3.zero;

            foreach (var npc in npcs)
            {
                if (npc.IsPlayerInRange)
                {
                    float d = Vector2.Distance(npc.transform.position, pPos);
                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = npc;
                    }
                }
            }

            if (nearest != null)
            {
                interactPromptText.text = $"<b>[E]</b> Nói chuyện với <b>{nearest.NpcName}</b> <color=#94a3b8>({nearest.RoleTitle})</color>";
                interactPromptBox.gameObject.SetActive(true);
            }
            else
            {
                interactPromptBox.gameObject.SetActive(false);
            }
        }

        public void UpdateWalletDisplay()
        {
            var wallet = VillageWallet.Instance;
            if (wallet != null)
            {
                if (goldText != null) goldText.text = $"💰 VÀNG: {wallet.Gold:N0}";
                if (repText != null) repText.text = $"⭐ DANH TIẾNG: {wallet.Reputation:N0}";
            }
        }
    }
}
