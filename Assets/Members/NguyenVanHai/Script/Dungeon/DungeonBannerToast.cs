using UnityEngine;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Tiện ích hiển thị thông báo ngắn (Toast Banner) và hiệu ứng chớp màn hình khi dịch chuyển.
    /// Tự khởi tạo runtime khi có script gọi — không cần gắn thủ công trong Scene.
    /// </summary>
    public class DungeonBannerToast : MonoBehaviour
    {
        private static DungeonBannerToast instance;

        private string currentMessage = "";
        private Color currentColor = new Color(1f, 0.9f, 0.55f, 1f);
        private float messageTimer = 0f;
        private float messageDuration = 2.6f;

        private float flashTimer = 0f;
        private float flashDuration = 0.28f;
        private Color flashColor = new Color(0.65f, 0.85f, 1f, 0.75f);

        private GUIStyle boxStyle;
        private GUIStyle textStyle;
        private Texture2D whiteTex;

        private static DungeonBannerToast EnsureInstance()
        {
            if (instance == null)
            {
                var go = new GameObject("[DungeonBannerToast]");
                instance = go.AddComponent<DungeonBannerToast>();
            }
            return instance;
        }

        public static void Show(string message, float duration = 2.8f)
        {
            Show(message, new Color(1f, 0.9f, 0.5f, 1f), duration);
        }

        public static void Show(string message, Color textColor, float duration = 2.8f)
        {
            if (string.IsNullOrEmpty(message)) return;
            var inst = EnsureInstance();
            inst.currentMessage = message;
            inst.currentColor = textColor;
            inst.messageDuration = Mathf.Max(0.5f, duration);
            inst.messageTimer = inst.messageDuration;
        }

        public static void FlashTeleport(Color? color = null, float duration = 0.28f)
        {
            var inst = EnsureInstance();
            inst.flashColor = color ?? new Color(0.55f, 0.85f, 1f, 0.75f);
            inst.flashDuration = Mathf.Max(0.1f, duration);
            inst.flashTimer = inst.flashDuration;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            whiteTex = new Texture2D(1, 1);
            whiteTex.SetPixel(0, 0, Color.white);
            whiteTex.Apply();
        }

        private void Update()
        {
            if (messageTimer > 0f)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer < 0f) messageTimer = 0f;
            }

            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer < 0f) flashTimer = 0f;
            }
        }

        private void OnGUI()
        {
            // 1. Hiệu ứng chớp màn hình dịch chuyển
            if (flashTimer > 0f && whiteTex != null)
            {
                float alpha = (flashTimer / flashDuration) * flashColor.a;
                Color prev = GUI.color;
                GUI.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteTex);
                GUI.color = prev;
            }

            // 2. Banner thông báo ngắn ở phía trên giữa màn hình
            if (messageTimer > 0f && !string.IsNullOrEmpty(currentMessage))
            {
                if (boxStyle == null)
                {
                    boxStyle = new GUIStyle(GUI.skin.box);
                    textStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 16,
                        fontStyle = FontStyle.Bold,
                        wordWrap = true
                    };
                }

                float fade = Mathf.Clamp01(messageTimer / 0.45f);
                float width = Mathf.Min(Screen.width - 80f, 520f);
                float height = 42f;
                Rect rect = new Rect((Screen.width - width) * 0.5f, 76f, width, height);

                Color prevBg = GUI.backgroundColor;
                Color prevContent = GUI.contentColor;

                GUI.backgroundColor = new Color(0.08f, 0.06f, 0.05f, 0.88f * fade);
                GUI.Box(rect, GUIContent.none, boxStyle);

                textStyle.normal.textColor = new Color(currentColor.r, currentColor.g, currentColor.b, fade);
                GUI.Label(rect, currentMessage, textStyle);

                GUI.backgroundColor = prevBg;
                GUI.contentColor = prevContent;
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            if (whiteTex != null) Destroy(whiteTex);
        }
    }
}

