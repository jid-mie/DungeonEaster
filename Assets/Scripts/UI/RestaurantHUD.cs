using UnityEngine;
using DungeonEater.Core;
using DungeonEater.Restaurant;

namespace DungeonEater.UI
{
    public class RestaurantHUD : MonoBehaviour
    {
        private CookingSystem cookingSystem;
        private RestaurantManager restaurantManager;

        private void Start()
        {
            cookingSystem = FindObjectOfType<CookingSystem>();
            restaurantManager = FindObjectOfType<RestaurantManager>();
        }

        private void OnGUI()
        {
            // --- TOP LEFT: RESTAURANT & REPUTATION STATUS ---
            GUILayout.BeginArea(new Rect(15, 15, 300, 130), GUI.skin.box);
            GUILayout.Label("<b>🍳 QUÁN ĂN LÀNG DUN (RESTAURANT)</b>");

            int gold = GameManager.Instance != null ? GameManager.Instance.Gold : 50;
            int rep = GameManager.Instance != null ? GameManager.Instance.Reputation : 0;
            GUILayout.Label($"💰 Vàng kinh doanh: <color=yellow><b>{gold}</b></color>");
            GUILayout.Label($"⭐ Danh tiếng quán: <color=cyan><b>{rep}</b></color>");
            GUILayout.Label("<color=#cccccc>Di chuyển: WASD | Phục vụ bàn ăn: Đến gần bàn</color>");
            GUILayout.EndArea();

            // --- TOP RIGHT: INVENTORY / INGREDIENTS ---
            GUILayout.BeginArea(new Rect(Screen.width - 260, 15, 245, 150), GUI.skin.box);
            GUILayout.Label("<b>🥩 NGUYÊN LIỆU THU THẬP</b>");

            int meat = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.MonsterMeat) : 0;
            int mush = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.CaveMushroom) : 0;
            int bomb = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.BombCore) : 0;
            int rib = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.BossRib) : 0;

            GUILayout.Label($"Thịt quái dã thú: <b>{meat}</b>");
            GUILayout.Label($"Nấm hang sâu: <b>{mush}</b>");
            GUILayout.Label($"Lõi bọ phát nổ: <b>{bomb}</b>");
            GUILayout.Label($"Sườn Vua Lợn Rừng: <b>{rib}</b>");
            GUILayout.EndArea();

            // --- BOTTOM BAR: COOKING & DUNGEON EXPEDITION ---
            GUILayout.BeginArea(new Rect(Screen.width / 2f - 260, Screen.height - 90, 520, 80), GUI.skin.box);
            GUILayout.BeginHorizontal();

            GUI.color = Color.green;
            if (GUILayout.Button("🍖 NẤU XIÊN THỊT\n(1 Thịt quái)", GUILayout.Height(50)))
            {
                if (cookingSystem != null)
                {
                    cookingSystem.Cook("skewer");
                }
            }

            GUI.color = Color.cyan;
            if (GUILayout.Button("🍲 NẤU SÚP NẤM\n(2 Nấm hang)", GUILayout.Height(50)))
            {
                if (cookingSystem != null)
                {
                    cookingSystem.Cook("soup");
                }
            }

            GUI.color = new Color(0.3f, 0.8f, 0.4f);
            if (GUILayout.Button("🏘 RA NGOÀI LÀNG\n(Làng Dun)", GUILayout.Height(50)))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("VillageLevel");
            }

            GUI.color = new Color(1f, 0.4f, 0.2f);
            if (GUILayout.Button("⚔ VÀO HẦM NGỤC\n(Săn Quái Lấy Thịt)", GUILayout.Height(50)))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("DungeonLevel");
            }

            GUI.color = Color.white;
            if (GUILayout.Button("🏠 MENU CHÍNH\n(Thoát)", GUILayout.Height(50)))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
            }

            GUILayout.EndHorizontal();
            GUI.color = Color.white;
            GUILayout.EndArea();
        }
    }
}

