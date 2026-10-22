using UnityEngine;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Bong bóng order trên đầu khách: icon món + thanh kiên nhẫn.
    /// Guideline UI nhà hàng: timer phải đọc nhanh, màu kết hợp hình thức/chuyển động
    /// -> thanh đổi màu xanh/vàng/đỏ và bong bóng rung + nhấp nháy khi sắp hết kiên nhẫn.
    /// </summary>
    public class OrderBubble : MonoBehaviour
    {
        private const float Width = 1.0f;
        private const float Height = 1.15f;
        private const float IconSize = 0.82f;
        private const float BarWidth = 0.8f;
        private const float BarHeight = 0.12f;

        private static readonly Color BubbleColor = new Color(0.97f, 0.9f, 0.74f);
        private static readonly Color BarBackColor = new Color(0.22f, 0.13f, 0.08f);
        private static readonly Color Good = new Color(0.49f, 0.85f, 0.34f);
        private static readonly Color Warn = new Color(1f, 0.78f, 0.24f);
        private static readonly Color Bad = new Color(0.88f, 0.3f, 0.24f);

        [SerializeField] private float urgentThreshold = 0.25f;

        private Transform visual;
        private SpriteRenderer background;
        private SpriteRenderer tail;
        private SpriteRenderer icon;
        private SpriteRenderer barBack;
        private SpriteRenderer barFill;
        private float ratio = 1f;

        public void Build(int sortingLayerId, int baseOrder)
        {
            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);

            background = CreateRenderer("Background", TavernSprites.Frame, BubbleColor, sortingLayerId, baseOrder);
            background.drawMode = SpriteDrawMode.Sliced;
            background.size = new Vector2(Width, Height);

            tail = CreateRenderer("Tail", TavernSprites.White, BubbleColor, sortingLayerId, baseOrder);
            tail.transform.localPosition = new Vector3(0f, -Height * 0.5f, 0f);
            tail.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            tail.transform.localScale = Vector3.one * 0.22f;

            icon = CreateRenderer("Icon", null, Color.white, sortingLayerId, baseOrder + 1);
            icon.transform.localPosition = new Vector3(0f, 0.08f, 0f);

            barBack = CreateRenderer("PatienceBack", TavernSprites.White, BarBackColor, sortingLayerId, baseOrder + 1);
            barBack.transform.localPosition = new Vector3(0f, -Height * 0.5f + 0.2f, 0f);
            barBack.transform.localScale = new Vector3(BarWidth + 0.06f, BarHeight + 0.06f, 1f);

            barFill = CreateRenderer("PatienceFill", TavernSprites.White, Good, sortingLayerId, baseOrder + 2);

            SetPatience(1f);
            Hide();
        }

        public void Show(Sprite dishIcon)
        {
            icon.sprite = dishIcon;
            if (dishIcon != null)
            {
                Vector2 size = dishIcon.bounds.size;
                float scale = IconSize / Mathf.Max(0.01f, Mathf.Max(size.x, size.y));
                icon.transform.localScale = Vector3.one * scale;
            }
            visual.gameObject.SetActive(true);
        }

        public void Hide()
        {
            visual.gameObject.SetActive(false);
        }

        public void SetPatience(float value)
        {
            ratio = Mathf.Clamp01(value);
            float y = -Height * 0.5f + 0.2f;
            barFill.transform.localPosition = new Vector3(-BarWidth * 0.5f + BarWidth * ratio * 0.5f, y, 0f);
            barFill.transform.localScale = new Vector3(BarWidth * ratio, BarHeight, 1f);
            barFill.color = ratio > 0.5f ? Color.Lerp(Warn, Good, (ratio - 0.5f) * 2f) : Color.Lerp(Bad, Warn, ratio * 2f);
        }

        private void Update()
        {
            if (visual == null || !visual.gameObject.activeSelf) return;

            if (ratio <= urgentThreshold)
            {
                // Rung + nhấp nháy báo khách sắp bỏ về
                float t = Time.time;
                visual.localPosition = new Vector3(Mathf.Sin(t * 40f) * 0.04f, 0f, 0f);
                float blink = Mathf.PingPong(t * 4f, 1f);
                background.color = Color.Lerp(BubbleColor, new Color(1f, 0.6f, 0.55f), blink);
                tail.color = background.color;
            }
            else
            {
                visual.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 2.5f) * 0.03f, 0f);
                background.color = BubbleColor;
                tail.color = BubbleColor;
            }
        }

        private SpriteRenderer CreateRenderer(string objName, Sprite sprite, Color color, int sortingLayerId, int order)
        {
            var go = new GameObject(objName);
            go.transform.SetParent(visual, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingLayerID = sortingLayerId;
            sr.sortingOrder = order;
            return sr;
        }
    }
}
