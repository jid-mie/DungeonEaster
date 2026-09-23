using UnityEngine;
using DungeonEater.Core;
using DungeonEater.Dungeon;

namespace DungeonEater.UI
{
    public class DungeonHUD : MonoBehaviour
    {
        private PlayerController player;
        private DungeonManager dungeonManager;

        private void Start()
        {
            player = FindObjectOfType<PlayerController>();
            dungeonManager = DungeonManager.Instance;
        }

        private void Update()
        {
            if (player == null) player = FindObjectOfType<PlayerController>();
            if (dungeonManager == null) dungeonManager = DungeonManager.Instance;
        }

        private void OnGUI()
        {
            // --- TOP LEFT: PLAYER HEALTH & STAMINA ---
            GUILayout.BeginArea(new Rect(15, 15, 280, 130), GUI.skin.box);
            GUILayout.Label("<b>👤 THỢ SĂN (HUNTER)</b>");

            float hp = player != null ? player.CurrentHealth : 100f;
            float maxHp = player != null ? player.MaxHealth : 100f;
            float hpPct = Mathf.Clamp01(hp / maxHp);

            // HP Bar
            GUI.color = hpPct > 0.5f ? Color.green : (hpPct > 0.25f ? Color.yellow : Color.red);
            GUILayout.Label($"<b>HP:</b> {Mathf.CeilToInt(hp)}/{maxHp} [{new string('|', Mathf.RoundToInt(hpPct * 20))}]");

            // Stamina Bar
            float sta = player != null ? player.CurrentStamina : 100f;
            float maxSta = player != null ? player.MaxStamina : 100f;
            float staPct = Mathf.Clamp01(sta / maxSta);
            GUI.color = Color.cyan;
            GUILayout.Label($"<b>STA:</b> {Mathf.CeilToInt(sta)}% [{new string('|', Mathf.RoundToInt(staPct * 20))}]");

            // Weapon & Controls
            GUI.color = Color.white;
            string weaponName = player != null && player.CurrentWeapon == WeaponType.Sword ? "🗡 KIẾM SẮT (3-Hit Combo)" : "🏹 CUNG GỖ (Tầm xa)";
            GUILayout.Label($"<b>Vũ khí [Q]:</b> <color=yellow>{weaponName}</color>");
            GUILayout.EndArea();

            // --- TOP CENTER: WAVE STATUS ---
            int wave = dungeonManager != null ? dungeonManager.CurrentWave : 1;
            string waveTitle = wave == 3 ? "👑 ĐỢT 3/3: BOSS VUA LỢN RỪNG (KING BOAR)!" : $"⚔ ĐỢT {wave}/3 - QUÁI VẬT HẦM NGỤC";
            GUI.color = wave == 3 ? new Color(1f, 0.3f, 0.3f) : Color.white;
            GUILayout.BeginArea(new Rect(Screen.width / 2f - 180, 15, 360, 65), GUI.skin.box);
            GUILayout.Label($"<size=15><b>{waveTitle}</b></size>");
            GUILayout.Label("<color=#cccccc>LMB: Tấn công | Space: Lướt né đòn | Q: Đổi vũ khí</color>");
            GUILayout.EndArea();

            // --- TOP RIGHT: INVENTORY & CURRENCY ---
            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(Screen.width - 250, 15, 235, 140), GUI.skin.box);
            GUILayout.Label("<b>🎒 TÚI ĐỒ & CHIẾN LỢI PHẨM</b>");

            int meat = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.MonsterMeat) : 0;
            int mush = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.CaveMushroom) : 0;
            int bomb = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.BombCore) : 0;
            int rib = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.BossRib) : 0;

            GUILayout.Label($"🥩 Thịt quái: <b>{meat}</b> | 🍄 Nấm hang: <b>{mush}</b>");
            GUILayout.Label($"💣 Lõi bọ nổ: <b>{bomb}</b> | 🍖 Sườn Boss: <b>{rib}</b>");

            int gold = GameManager.Instance != null ? GameManager.Instance.Gold : 50;
            int rep = GameManager.Instance != null ? GameManager.Instance.Reputation : 0;
            GUILayout.Space(4);
            GUILayout.Label($"💰 Vàng: <color=yellow><b>{gold}</b></color> | ⭐ Danh tiếng: <color=cyan><b>{rep}</b></color>");

            if (GUILayout.Button("🏠 Về Menu Chính", GUILayout.Height(22)))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
            }

            GUILayout.EndArea();
        }
    }
}

