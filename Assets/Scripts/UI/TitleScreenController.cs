using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonEater.Core;

namespace DungeonEater.UI
{
    public class TitleScreenController : MonoBehaviour
    {
        [Header("Scene Navigation")]
        [SerializeField] private string startSceneName = "DungeonLevel";
        [SerializeField] private string villageSceneName = "RestaurantLevel";

        [Header("Controls Modal")]
        private bool showControls = false;
        private bool showCredits = false;

        private void Update()
        {
            // Press Any Key to Start
            if (Input.anyKeyDown && !showControls && !showCredits)
            {
                // If it's not a mouse movement or ESC
                if (!Input.GetKeyDown(KeyCode.Escape))
                {
                    StartGame();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                showControls = false;
                showCredits = false;
            }
        }

        public void StartGame()
        {
            SoundManager.Instance?.PlayCook();
            Debug.Log($"[TitleScreen] Khởi hành vào: {startSceneName}");
            SceneManager.LoadScene(startSceneName);
        }

        public void OpenVillage()
        {
            SoundManager.Instance?.PlayCoin();
            SceneManager.LoadScene(villageSceneName);
        }

        public void ToggleControls()
        {
            showControls = !showControls;
            showCredits = false;
            SoundManager.Instance?.PlaySwordSlash();
        }

        public void ToggleCredits()
        {
            showCredits = !showCredits;
            showControls = false;
            SoundManager.Instance?.PlayCoin();
        }

        public void QuitGame()
        {
            Debug.Log("[TitleScreen] Thoát game.");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void OnGUI()
        {
            // Custom high-contrast pixel art styled GUI
            GUI.skin.box.fontSize = 14;
            GUI.skin.button.fontSize = 15;

            // Flash effect for prompt
            float alpha = 0.6f + Mathf.PingPong(Time.time * 1.5f, 0.4f);
            Color origColor = GUI.color;

            // Bottom Prompt
            if (!showControls && !showCredits)
            {
                GUI.color = new Color(1f, 0.85f, 0.3f, alpha);
                GUIStyle promptStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                float promptW = 400;
                float promptH = 40;
                float promptX = (Screen.width - promptW) / 2f;
                float promptY = Screen.height - 110;

                GUI.Label(new Rect(promptX, promptY, promptW, promptH), "★ BẤM PHÍM BẤT KỲ ĐỂ VÀO HẦM NGỤC ★", promptStyle);
                GUI.color = origColor;

                // Quick Navigation Buttons bar at bottom
                GUILayout.BeginArea(new Rect((Screen.width - 640) / 2f, Screen.height - 65, 640, 45));
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("⚔ Bắt Đầu (Dungeon)", GUILayout.Height(40)))
                {
                    StartGame();
                }

                if (GUILayout.Button("🏪 Vào Quán Ăn", GUILayout.Height(40)))
                {
                    OpenVillage();
                }

                if (GUILayout.Button("📖 Hướng Dẫn", GUILayout.Height(40)))
                {
                    ToggleControls();
                }

                if (GUILayout.Button("👥 Nhóm Tác Giả", GUILayout.Height(40)))
                {
                    ToggleCredits();
                }

                if (GUILayout.Button("❌ Thoát", GUILayout.Height(40)))
                {
                    QuitGame();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            // Controls Modal Popup
            if (showControls)
            {
                float mw = 520;
                float mh = 360;
                float mx = (Screen.width - mw) / 2f;
                float my = (Screen.height - mh) / 2f;

                GUI.Box(new Rect(mx, my, mw, mh), "📖 HƯỚNG DẪN ĐIỀU KHIỂN & LỐI CHƠI");

                GUILayout.BeginArea(new Rect(mx + 20, my + 40, mw - 40, mh - 50));
                
                GUILayout.Label("🎮 <b>PHÍM DI CHUYỂN:</b> [W, A, S, D]");
                GUILayout.Label("⚔ <b>TẤN CÔNG NHẸ:</b> Chuột Trái [LMB] hoặc [J]");
                GUILayout.Label("💥 <b>TẤN CÔNG NẶNG:</b> Chuột Phải [RMB] hoặc [K]");
                GUILayout.Label("🌀 <b>KỸ NĂNG VŨ KHÍ:</b> [Q] (Lốc xoáy Kiếm / Tên xuyên thấu)");
                GUILayout.Label("💨 <b>LƯỚT NÉ ĐÒN (Dash):</b> [Space]");
                GUILayout.Label("🔄 <b>ĐỔI VŨ KHÍ:</b> [Tab] (Kiếm cận chiến ⟷ Cung tầm xa)");
                GUILayout.Label("💬 <b>TƯƠNG TÁC / NHẬT ĐỒ:</b> [F]");
                GUILayout.Space(10);
                GUILayout.Label("🍲 <b>VÒNG LẶP CỐT LÕI:</b>\nSăn quái → Thu thịt & nấm → Nấu ăn bán cho khách → Kiếm vàng nâng cấp!");

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("ĐÓNG [ESC]", GUILayout.Height(35)))
                {
                    showControls = false;
                }
                GUILayout.EndArea();
            }

            // Credits Modal Popup
            if (showCredits)
            {
                float mw = 480;
                float mh = 320;
                float mx = (Screen.width - mw) / 2f;
                float my = (Screen.height - mh) / 2f;

                GUI.Box(new Rect(mx, my, mw, mh), "👥 NHÓM PHÁT TRIỂN DUNGEON EATER");

                GUILayout.BeginArea(new Rect(mx + 20, my + 40, mw - 40, mh - 50));
                GUILayout.Label("🏛 <b>TRƯỜNG ĐẠI HỌC THỦY LỢI - KHOA CNTT</b>");
                GUILayout.Label("📚 <b>Môn học:</b> Thiết kế & Phát triển Game (65CNTT)");
                GUILayout.Label("👨‍🏫 <b>GVHD:</b> Trương Xuân Nam");
                GUILayout.Space(8);
                GUILayout.Label("• <b>Nguyễn Văn Hải</b> (2351060441) - Trưởng nhóm");
                GUILayout.Label("• <b>Nguyễn Minh Hiệp</b> (2351060445) - Thành viên");
                GUILayout.Label("• <b>Ngô Văn Hiến</b> (2351060444) - Thành viên");
                GUILayout.Label("• <b>Trần Hữu Hậu</b> (2351060443) - Thành viên");

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("ĐÓNG [ESC]", GUILayout.Height(35)))
                {
                    showCredits = false;
                }
                GUILayout.EndArea();
            }
        }
    }
}

