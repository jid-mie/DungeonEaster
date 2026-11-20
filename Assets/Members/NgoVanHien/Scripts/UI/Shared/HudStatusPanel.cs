using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Khung trạng thái người chơi dùng chung cho 3 scene (Restaurant / Village / Dungeon), góc trên trái:
    /// thanh Máu, thanh Thể lực và (tùy scene) Vàng + Danh tiếng. Style gốc: HUD quán của Hiến.
    /// Scene nào có nguồn số liệu thật (Dungeon) thì gọi SetHealth/SetStamina; scene không chiến đấu
    /// gọi ShowLastKnownVitals để hiện giá trị gần nhất (mặc định đầy theo dungeon_combat_config.json).
    /// </summary>
    public class HudStatusPanel : MonoBehaviour
    {
        public const float Width = 380f;
        public const float Margin = 24f;
        private const float VitalsHeight = 82f;
        private const float WalletHeight = 46f;

        // Giá trị gần nhất, giữ qua các lần chuyển scene trong cùng phiên chơi
        private static bool hasVitals;
        private static float lastHealth, lastMaxHealth, lastStamina, lastMaxStamina;

        private Image healthFill;
        private Text healthText;
        private Image staminaFill;
        private Text staminaText;
        private Text goldText;
        private Text repText;
        private int shownGold = int.MinValue;
        private int shownRep = int.MinValue;

        public float Height { get; private set; }
        /// <summary>Mép dưới của khung tính từ đỉnh màn hình (để xếp panel khác ngay bên dưới).</summary>
        public float Bottom => Margin + Height;

        public static HudStatusPanel Create(Transform parent, bool showWallet, Sprite goldIcon = null, Sprite reputationIcon = null)
        {
            var panel = UIKit.Panel(parent, "PlayerStatus", UIKit.Wood);
            panel.raycastTarget = false;
            var view = panel.gameObject.AddComponent<HudStatusPanel>();
            view.Build(showWallet, goldIcon, reputationIcon);
            return view;
        }

        private void Build(bool showWallet, Sprite goldIcon, Sprite reputationIcon)
        {
            var self = (RectTransform)transform;
            Height = VitalsHeight + (showWallet ? WalletHeight : 0f);
            UIKit.Place(self, new Vector2(0f, 1f), new Vector2(Margin, -Margin), new Vector2(Width, Height));

            healthFill = VitalRow("Health", "HP", UIKit.Health, -14f, 26f, 18, out healthText);
            staminaFill = VitalRow("Stamina", "TL", UIKit.Stamina, -48f, 20f, 15, out staminaText);

            if (showWallet)
            {
                goldText = WalletCell(goldIcon, "Gold", UIKit.Gold, 16f);
                repText = WalletCell(reputationIcon, "Reputation", UIKit.Rep, 16f + (Width - 32f) * 0.5f);
            }

            ShowLastKnownVitals();
        }

        private Image VitalRow(string objName, string caption, Color color, float y, float height, int fontSize, out Text valueText)
        {
            var cap = UIKit.Label(transform, objName + "Caption", caption, 20, color, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIKit.Place(cap.rectTransform, new Vector2(0f, 1f), new Vector2(16f, y), new Vector2(44f, height));

            var fill = UIKit.Bar(transform, objName + "Bar", color, out var back);
            UIKit.Place(back, new Vector2(0f, 1f), new Vector2(60f, y), new Vector2(Width - 76f, height));

            valueText = UIKit.Label(back, "Value", "", fontSize, UIKit.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIKit.Stretch(valueText.rectTransform, 0, 0, 0, 0);
            return fill;
        }

        private Text WalletCell(Sprite icon, string objName, Color color, float x)
        {
            var img = UIKit.Icon(transform, objName + "Icon", icon);
            UIKit.Place(img.rectTransform, new Vector2(0f, 0f), new Vector2(x, 10f), new Vector2(34f, 34f));
            if (icon == null)
            {
                // không có icon -> chấm màu thay thế
                img.sprite = TavernSprites.Frame;
                img.type = Image.Type.Sliced;
                img.color = color;
                img.enabled = true;
            }
            var value = UIKit.Label(transform, objName + "Value", "0", 26, color, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIKit.Place(value.rectTransform, new Vector2(0f, 0f), new Vector2(x + 42f, 10f), new Vector2(Width * 0.5f - 62f, 34f));
            return value;
        }

        public void SetHealth(float current, float max)
        {
            lastHealth = current;
            lastMaxHealth = max;
            hasVitals = true;
            ApplyVital(healthFill, healthText, current, max);
        }

        public void SetStamina(float current, float max)
        {
            lastStamina = current;
            lastMaxStamina = max;
            hasVitals = true;
            ApplyVital(staminaFill, staminaText, current, max);
        }

        public void SetWallet(int gold, int reputation)
        {
            if (goldText != null && gold != shownGold)
            {
                shownGold = gold;
                goldText.text = UIKit.FormatNumber(gold);
            }
            if (repText != null && reputation != shownRep)
            {
                shownRep = reputation;
                repText.text = UIKit.FormatNumber(reputation);
            }
        }

        /// <summary>
        /// Scene không có chiến đấu (Restaurant, Village): hiện máu/thể lực gần nhất của phiên chơi,
        /// chưa có thì lấy mức tối đa trong dungeon_combat_config.json.
        /// </summary>
        public void ShowLastKnownVitals()
        {
            if (!hasVitals || lastMaxHealth <= 0f || lastMaxStamina <= 0f)
            {
                var cfg = NguyenVanHai.Data.DungeonCombatDatabase.GetPlayerConfig();
                if (lastMaxHealth <= 0f) lastHealth = lastMaxHealth = cfg.maxHealth;
                if (lastMaxStamina <= 0f) lastStamina = lastMaxStamina = cfg.maxStamina;
            }
            ApplyVital(healthFill, healthText, lastHealth, lastMaxHealth);
            ApplyVital(staminaFill, staminaText, lastStamina, lastMaxStamina);
        }

        private static void ApplyVital(Image fill, Text text, float current, float max)
        {
            if (fill == null) return;
            UIKit.SetBar(fill, max > 0f ? current / max : 0f);
            text.text = $"{Mathf.CeilToInt(Mathf.Max(0f, current))} / {Mathf.CeilToInt(max)}";
        }
    }
}
