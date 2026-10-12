using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace DungeonEater.Core
{
    /// <summary>
    /// Quản lý hiệu ứng chuyển cảnh mượt mà (Fade To Black -> Load Scene -> Fade In).
    /// Tự động hoạt động toàn cục qua DontDestroyOnLoad, không yêu cầu thiết lập Canvas thủ công.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Transition Settings")]
        [SerializeField] private float defaultFadeDuration = 0.35f;
        [SerializeField] private Color fadeColor = Color.black;

        private float currentAlpha = 0f;
        private bool isTransitioning = false;
        private Texture2D fadeTexture;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Tạo 1 texture đen 1x1 pixel để vẽ fade overlay
            fadeTexture = new Texture2D(1, 1);
            fadeTexture.SetPixel(0, 0, Color.white);
            fadeTexture.Apply();
        }

        private void Start()
        {
            // Bắt đầu scene với fade-in nhẹ
            StartCoroutine(FadeCoroutine(1f, 0f, defaultFadeDuration, null));
        }

        /// <summary>
        /// Chuyển scene kèm hiệu ứng Fade
        /// </summary>
        public void LoadScene(string sceneName, float duration = -1f)
        {
            if (isTransitioning) return;
            float dur = duration > 0 ? duration : defaultFadeDuration;
            StartCoroutine(TransitionRoutine(sceneName, dur));
        }

        private IEnumerator TransitionRoutine(string sceneName, float duration)
        {
            isTransitioning = true;

            // 1. Fade out to Black
            yield return StartCoroutine(FadeCoroutine(0f, 1f, duration, null));

            // 2. Load Scene bất đồng bộ
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            while (asyncOp != null && !asyncOp.isDone)
            {
                yield return null;
            }

            // 3. Fade in from Black
            yield return StartCoroutine(FadeCoroutine(1f, 0f, duration, null));

            isTransitioning = false;
        }

        private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            currentAlpha = startAlpha;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            currentAlpha = targetAlpha;
            onComplete?.Invoke();
        }

        private void OnGUI()
        {
            if (currentAlpha > 0.001f && fadeTexture != null)
            {
                // GUI.depth = -1000 đảm bảo lớp đen luôn nằm trên cùng mọi UI/Sprites
                GUI.depth = -1000;
                Color col = fadeColor;
                col.a = currentAlpha;
                GUI.color = col;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTexture);
            }
        }
    }
}
