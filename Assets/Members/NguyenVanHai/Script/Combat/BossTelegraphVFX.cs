using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NguyenVanHai.Combat
{
    /// <summary>
    /// Hệ thống vẽ vùng cảnh báo nguy hiểm màu đỏ mờ dưới mặt đất (Ground Telegraph VFX) cho các Boss:
    /// - Hỗ trợ 3 hình dạng: Vòng tròn (Circle), Đường thẳng lao/phi giáo (Line), Hình bán nguyệt/quạt (SemiCircle).
    /// - Hiển thị 2 lớp: Lớp nền đỏ mờ khóa vùng nguy hiểm + Lớp lõi đỏ đậm lan dần từ 0 -> 100% theo thời gian gồng chiêu.
    /// - Đặt trên Sorting Layer "FloorDecal" để luôn nổi trên sàn gạch nhưng nằm dưới chân Nhân vật và Boss.
    /// </summary>
    public class BossTelegraphVFX : MonoBehaviour
    {
        private const string TELEGRAPH_SORTING_LAYER = "FloorDecal";
        private const int BASE_SORTING_ORDER = 5000;

        private static Sprite cachedCircleSprite;
        private static Sprite cachedBoxSprite;
        private static readonly Dictionary<int, Sprite> cachedArcSprites = new Dictionary<int, Sprite>();

        private SpriteRenderer baseRenderer;
        private Transform fillTransform;
        private SpriteRenderer fillRenderer;

        /// <summary>
        /// Tạo vùng cảnh báo VÒNG TRÒN đỏ mờ dưới mặt đất (dùng cho Nhảy dậm, Chém 360 độ, Kiếm rơi, Mưa đạn).
        /// </summary>
        public static BossTelegraphVFX SpawnCircle(Vector2 center, float radius, float duration, Color? tint = null)
        {
            Color baseCol = tint ?? new Color(1f, 0.15f, 0.15f, 0.28f);
            Color fillCol = new Color(baseCol.r, baseCol.g, baseCol.b, Mathf.Clamp01(baseCol.a + 0.25f));

            var rootGo = new GameObject("BossTelegraph_Circle");
            rootGo.transform.position = new Vector3(center.x, center.y, 0f);

            var vfx = rootGo.AddComponent<BossTelegraphVFX>();
            Sprite circleSp = GetOrCreateCircleSprite();

            // Lớp nền cố định kích thước
            float diameter = radius * 2f;
            vfx.baseRenderer = rootGo.AddComponent<SpriteRenderer>();
            vfx.baseRenderer.sprite = circleSp;
            vfx.baseRenderer.color = baseCol;
            vfx.baseRenderer.sortingLayerName = TELEGRAPH_SORTING_LAYER;
            vfx.baseRenderer.sortingOrder = BASE_SORTING_ORDER;
            rootGo.transform.localScale = new Vector3(diameter, diameter, 1f);

            // Lớp lõi lan dần từ tâm ra viền (0 -> 1)
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(rootGo.transform, false);
            fillGo.transform.localScale = Vector3.zero;
            vfx.fillTransform = fillGo.transform;

            vfx.fillRenderer = fillGo.AddComponent<SpriteRenderer>();
            vfx.fillRenderer.sprite = circleSp;
            vfx.fillRenderer.color = fillCol;
            vfx.fillRenderer.sortingLayerName = TELEGRAPH_SORTING_LAYER;
            vfx.fillRenderer.sortingOrder = BASE_SORTING_ORDER + 1;

            vfx.StartCoroutine(vfx.AnimateRadialFillRoutine(duration));
            return vfx;
        }

        /// <summary>
        /// Tạo vùng cảnh báo ĐƯỜNG THẲNG đỏ mờ dưới mặt đất (dùng cho Lao húc, Phi giáo, Lướt thần tốc).
        /// </summary>
        public static BossTelegraphVFX SpawnLine(Vector2 origin, Vector2 direction, float length, float width, float duration, Color? tint = null)
        {
            if (direction.sqrMagnitude < 0.001f) direction = Vector2.down;
            direction.Normalize();

            Color baseCol = tint ?? new Color(1f, 0.15f, 0.15f, 0.28f);
            Color fillCol = new Color(baseCol.r, baseCol.g, baseCol.b, Mathf.Clamp01(baseCol.a + 0.25f));

            var rootGo = new GameObject("BossTelegraph_Line");
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rootGo.transform.position = new Vector3(origin.x, origin.y, 0f);
            rootGo.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            var vfx = rootGo.AddComponent<BossTelegraphVFX>();
            Sprite boxSp = GetOrCreateBoxSprite();

            // Lớp nền trải dài từ origin theo hướng direction
            var baseGo = new GameObject("BaseBox");
            baseGo.transform.SetParent(rootGo.transform, false);
            baseGo.transform.localPosition = new Vector3(length * 0.5f, 0f, 0f);
            baseGo.transform.localScale = new Vector3(length, width, 1f);

            vfx.baseRenderer = baseGo.AddComponent<SpriteRenderer>();
            vfx.baseRenderer.sprite = boxSp;
            vfx.baseRenderer.color = baseCol;
            vfx.baseRenderer.sortingLayerName = TELEGRAPH_SORTING_LAYER;
            vfx.baseRenderer.sortingOrder = BASE_SORTING_ORDER;

            // Lớp lõi chạy dọc từ gốc (0) tới ngọn (length)
            var fillGo = new GameObject("FillBox");
            fillGo.transform.SetParent(rootGo.transform, false);
            fillGo.transform.localPosition = Vector3.zero;
            fillGo.transform.localScale = new Vector3(0f, width * 0.92f, 1f);
            vfx.fillTransform = fillGo.transform;

            vfx.fillRenderer = fillGo.AddComponent<SpriteRenderer>();
            vfx.fillRenderer.sprite = boxSp;
            vfx.fillRenderer.color = fillCol;
            vfx.fillRenderer.sortingLayerName = TELEGRAPH_SORTING_LAYER;
            vfx.fillRenderer.sortingOrder = BASE_SORTING_ORDER + 1;

            vfx.StartCoroutine(vfx.AnimateLinearFillRoutine(length, width * 0.92f, duration));
            return vfx;
        }

        /// <summary>
        /// Tạo vùng cảnh báo HÌNH BÁN NGUYỆT / HÌNH QUẠT đỏ mờ dưới mặt đất (dùng cho Đập tay cận chiến).
        /// </summary>
        public static BossTelegraphVFX SpawnSemiCircle(Vector2 origin, Vector2 direction, float radius, float arcAngle, float duration, Color? tint = null)
        {
            if (direction.sqrMagnitude < 0.001f) direction = Vector2.down;
            direction.Normalize();

            Color baseCol = tint ?? new Color(1f, 0.15f, 0.15f, 0.28f);
            Color fillCol = new Color(baseCol.r, baseCol.g, baseCol.b, Mathf.Clamp01(baseCol.a + 0.25f));

            var rootGo = new GameObject("BossTelegraph_Arc");
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rootGo.transform.position = new Vector3(origin.x, origin.y, 0f);
            rootGo.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            var vfx = rootGo.AddComponent<BossTelegraphVFX>();
            int roundedAngle = Mathf.Clamp(Mathf.RoundToInt(arcAngle), 30, 360);
            Sprite arcSp = GetOrCreateArcSprite(roundedAngle);

            float diameter = radius * 2f;
            vfx.baseRenderer = rootGo.AddComponent<SpriteRenderer>();
            vfx.baseRenderer.sprite = arcSp;
            vfx.baseRenderer.color = baseCol;
            vfx.baseRenderer.sortingLayerName = TELEGRAPH_SORTING_LAYER;
            vfx.baseRenderer.sortingOrder = BASE_SORTING_ORDER;
            rootGo.transform.localScale = new Vector3(diameter, diameter, 1f);

            var fillGo = new GameObject("FillArc");
            fillGo.transform.SetParent(rootGo.transform, false);
            fillGo.transform.localScale = Vector3.zero;
            vfx.fillTransform = fillGo.transform;

            vfx.fillRenderer = fillGo.AddComponent<SpriteRenderer>();
            vfx.fillRenderer.sprite = arcSp;
            vfx.fillRenderer.color = fillCol;
            vfx.fillRenderer.sortingLayerName = TELEGRAPH_SORTING_LAYER;
            vfx.fillRenderer.sortingOrder = BASE_SORTING_ORDER + 1;

            vfx.StartCoroutine(vfx.AnimateRadialFillRoutine(duration));
            return vfx;
        }

        private IEnumerator AnimateRadialFillRoutine(float duration)
        {
            float elapsed = 0f;
            float safeDur = Mathf.Max(0.05f, duration);

            while (elapsed < safeDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / safeDur);
                if (fillTransform != null)
                {
                    fillTransform.localScale = new Vector3(t, t, 1f);
                }
                yield return null;
            }

            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator AnimateLinearFillRoutine(float fullLength, float width, float duration)
        {
            float elapsed = 0f;
            float safeDur = Mathf.Max(0.05f, duration);

            while (elapsed < safeDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / safeDur);
                float curLen = fullLength * t;
                if (fillTransform != null)
                {
                    fillTransform.localPosition = new Vector3(curLen * 0.5f, 0f, 0f);
                    fillTransform.localScale = new Vector3(curLen, width, 1f);
                }
                yield return null;
            }

            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        private static Sprite GetOrCreateCircleSprite()
        {
            if (cachedCircleSprite != null) return cachedCircleSprite;

            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = (size - 1) * 0.5f;
            float maxR = center;

            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float norm = dist / maxR;

                    if (norm > 1f)
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                    else if (norm >= 0.90f)
                    {
                        // Viền sáng rõ giúp dễ nhìn ranh giới né đòn
                        pixels[y * size + x] = new Color(1f, 1f, 1f, 1f);
                    }
                    else
                    {
                        pixels[y * size + x] = new Color(1f, 1f, 1f, 0.72f);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            cachedCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return cachedCircleSprite;
        }

        private static Sprite GetOrCreateBoxSprite()
        {
            if (cachedBoxSprite != null) return cachedBoxSprite;

            const int w = 64;
            const int h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isBorder = (x <= 2 || x >= w - 3 || y <= 3 || y >= h - 4);
                    pixels[y * w + x] = isBorder ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.72f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            cachedBoxSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), w);
            return cachedBoxSprite;
        }

        private static Sprite GetOrCreateArcSprite(int arcDegrees)
        {
            if (cachedArcSprites.TryGetValue(arcDegrees, out var existing) && existing != null)
            {
                return existing;
            }

            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = (size - 1) * 0.5f;
            float maxR = center;
            float halfArc = arcDegrees * 0.5f;

            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float norm = dist / maxR;

                    if (norm > 1f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    // Hướng chuẩn là trục +X (0 độ)
                    float angle = Mathf.Abs(Mathf.Atan2(dy, dx) * Mathf.Rad2Deg);
                    if (angle > halfArc)
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                    else if (norm >= 0.90f || Mathf.Abs(angle - halfArc) <= 3.5f)
                    {
                        pixels[y * size + x] = new Color(1f, 1f, 1f, 1f);
                    }
                    else
                    {
                        pixels[y * size + x] = new Color(1f, 1f, 1f, 0.72f);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            var sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedArcSprites[arcDegrees] = sp;
            return sp;
        }
    }
}
