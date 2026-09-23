using UnityEngine;

namespace DungeonEater
{
    public class CharacterViewSwitcher : MonoBehaviour
    {
        public enum ViewMode
        {
            CombatHunter,
            ChefRestaurant
        }

        [Header("Character Views")]
        public GameObject combatHunterObject;
        public GameObject chefRestaurantObject;

        [Header("Current State")]
        public ViewMode currentMode = ViewMode.CombatHunter;

        private void Start()
        {
            UpdateViews();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                currentMode = (currentMode == ViewMode.CombatHunter) ? ViewMode.ChefRestaurant : ViewMode.CombatHunter;
                UpdateViews();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                currentMode = ViewMode.CombatHunter;
                UpdateViews();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentMode = ViewMode.ChefRestaurant;
                UpdateViews();
            }
        }

        public void SwitchToCombat()
        {
            currentMode = ViewMode.CombatHunter;
            UpdateViews();
        }

        public void SwitchToChef()
        {
            currentMode = ViewMode.ChefRestaurant;
            UpdateViews();
        }

        private void UpdateViews()
        {
            if (combatHunterObject != null)
            {
                combatHunterObject.SetActive(currentMode == ViewMode.CombatHunter);
                if (currentMode == ViewMode.CombatHunter)
                {
                    combatHunterObject.transform.position = Vector3.zero;
                }
            }

            if (chefRestaurantObject != null)
            {
                chefRestaurantObject.SetActive(currentMode == ViewMode.ChefRestaurant);
                if (currentMode == ViewMode.ChefRestaurant)
                {
                    chefRestaurantObject.transform.position = Vector3.zero;
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 360, 150), GUI.skin.box);
            GUILayout.Label("<b>🎮 CHỌN VIEW NHÂN VẬT (CHARACTER VIEW)</b>");
            
            GUILayout.BeginHorizontal();
            GUI.color = (currentMode == ViewMode.CombatHunter) ? Color.cyan : Color.white;
            if (GUILayout.Button("⚔️ [1] Combat Hunter", GUILayout.Height(35)))
            {
                SwitchToCombat();
            }

            GUI.color = (currentMode == ViewMode.ChefRestaurant) ? Color.yellow : Color.white;
            if (GUILayout.Button("🍳 [2] Chef Restaurant", GUILayout.Height(35)))
            {
                SwitchToChef();
            }
            GUILayout.EndHorizontal();

            GUI.color = Color.white;
            GUILayout.Space(5);
            GUILayout.Label($"<b>Đang xem:</b> {(currentMode == ViewMode.CombatHunter ? "<color=cyan>Thợ Săn Đánh Quái</color>" : "<color=yellow>Đầu Bếp Quán Ăn</color>")}");
            GUILayout.Label("<i>Phím tắt: Bấm [Tab] hoặc [1]/[2] để đổi view</i>");
            GUILayout.EndArea();
        }
    }
}

