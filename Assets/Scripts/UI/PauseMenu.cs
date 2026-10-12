using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonEater.Core;

namespace DungeonEater.UI
{
    /// <summary>
    /// Quản lý menu tạm dừng (Pause Menu) khi nhấn phím [ESC] trong các màn chơi
    /// theo thiết kế tài liệu GDD (Mục 3.2.1 & Mục 6.1).
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        [SerializeField] private bool isPaused = false;
        public bool IsPaused => isPaused;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            // Bấm ESC để bật/tắt Tạm Dừng nếu không ở TitleScreen
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                string activeScene = SceneManager.GetActiveScene().name;
                if (activeScene != "TitleScreen")
                {
                    if (isPaused)
                        ResumeGame();
                    else
                        PauseGame();
                }
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
            SoundManager.Instance?.PlayPickup();
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            SoundManager.Instance?.PlayPickup();
        }

        public void RestartCurrentScene()
        {
            Time.timeScale = 1f;
            isPaused = false;
            string currentScene = SceneManager.GetActiveScene().name;
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(currentScene);
            else
                SceneManager.LoadScene(currentScene);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            isPaused = false;
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene("TitleScreen");
            else
                SceneManager.LoadScene("TitleScreen");
        }

        private void OnGUI()
        {
            if (!isPaused) return;

            // Đặt trên cùng
            GUI.depth = -500;

            // Làm mờ nền
            Color dark = new Color(0, 0, 0, 0.75f);
            GUI.color = dark;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            // Khung Pause Menu trung tâm
            float w = 380;
            float h = 320;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;

            GUI.color = Color.white;
            GUI.skin.box.fontSize = 16;
            GUI.skin.button.fontSize = 15;

            GUI.Box(new Rect(x, y, w, h), "⏸️ TẠM DỪNG TRÒ CHƠI (PAUSED)");

            GUILayout.BeginArea(new Rect(x + 30, y + 45, w - 60, h - 60));

            GUILayout.Label("<color=#ffd700>Trò chơi đang được tạm dừng.</color>", GUILayout.Height(25));
            GUILayout.Space(10);

            GUI.color = Color.green;
            if (GUILayout.Button("▶️ TIẾP TỤC [ESC]", GUILayout.Height(42)))
            {
                ResumeGame();
            }

            GUI.color = new Color(0.3f, 0.7f, 1f);
            if (GUILayout.Button("🔄 CHƠI LẠI MÀN NÀY", GUILayout.Height(42)))
            {
                RestartCurrentScene();
            }

            GUI.color = new Color(1f, 0.4f, 0.3f);
            if (GUILayout.Button("🏠 VỀ MENU CHÍNH", GUILayout.Height(42)))
            {
                ReturnToMainMenu();
            }

            GUI.color = Color.white;
            GUILayout.EndArea();
        }
    }
}
