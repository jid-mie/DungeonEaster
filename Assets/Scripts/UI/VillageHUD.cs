using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonEater.Core;

namespace DungeonEater.UI
{
    public class VillageHUD : MonoBehaviour
    {
        [Header("Scene Navigation")]
        [SerializeField] private string dungeonScene = "DungeonLevel";
        [SerializeField] private string restaurantScene = "RestaurantLevel";
        [SerializeField] private string titleScene = "TitleScreen";

        private void OnGUI()
        {
            // Top Left: Village Info
            GUILayout.BeginArea(new Rect(15, 15, 300, 110), GUI.skin.box);
            GUILayout.Label("<b>🏘️ NGÔI LÀNG DUN (DUN VILLAGE)</b>");
            GUILayout.Label("<color=#ffd700>• Vùng đất nghèo sau mùa quái vật tàn phá</color>");
            GUILayout.Label("<color=#cccccc>• Di chuyển: [WASD] | Nói chuyện: Gặp Trưởng Làng bấm [F]</color>");

            int gold = GameManager.Instance != null ? GameManager.Instance.Gold : 50;
            int rep = GameManager.Instance != null ? GameManager.Instance.Reputation : 0;
            GUILayout.Label($"💰 Vàng: <b>{gold}</b> | ⭐ Danh tiếng: <b>{rep}</b>");
            GUILayout.EndArea();

            // Top Right: Inventory summary
            GUILayout.BeginArea(new Rect(Screen.width - 250, 15, 235, 120), GUI.skin.box);
            GUILayout.Label("<b>🎒 HÀNH TRANG THỢ SĂN</b>");
            int meat = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.MonsterMeat) : 0;
            int mush = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.CaveMushroom) : 0;
            int bomb = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.BombCore) : 0;
            int rib = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(IngredientType.BossRib) : 0;
            GUILayout.Label($"🥩 Thịt quái: <b>{meat}</b> | 🍄 Nấm: <b>{mush}</b>");
            GUILayout.Label($"💣 Lõi bọ: <b>{bomb}</b> | 🍖 Sườn: <b>{rib}</b>");
            GUILayout.EndArea();

            // Bottom Navigation Fast Travel Buttons
            GUILayout.BeginArea(new Rect((Screen.width - 560) / 2f, Screen.height - 70, 560, 55));
            GUILayout.BeginHorizontal();

            GUI.color = new Color(0.9f, 0.4f, 0.3f);
            if (GUILayout.Button("⚔ CỔNG HẦM NGỤC\n(Trở lại Dungeon)", GUILayout.Height(45)))
            {
                SceneManager.LoadScene(dungeonScene);
            }

            GUI.color = Color.green;
            if (GUILayout.Button("🏪 CĂN NHÀ HOANG\n(Vào Quán Ăn)", GUILayout.Height(45)))
            {
                SceneManager.LoadScene(restaurantScene);
            }

            GUI.color = Color.white;
            if (GUILayout.Button("🏠 MENU CHÍNH\n(Thoát)", GUILayout.Height(45)))
            {
                SceneManager.LoadScene(titleScene);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.color = Color.white;
        }
    }
}

