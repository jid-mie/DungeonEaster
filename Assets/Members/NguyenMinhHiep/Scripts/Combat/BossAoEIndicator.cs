using System.Collections;
using UnityEngine;

namespace NguyenMinhHiep.Combat
{
    /// <summary>
    /// Hiển thị vạch báo hiệu đòn đánh (Telegraph Indicator) dạng Hình tròn, Hình nón hoặc Hình chữ nhật.
    /// Giúp người chơi quan sát thấy vùng nguy hiểm trước khi đòn đánh của Boss giáng xuống.
    /// </summary>
    public class BossAoEIndicator : MonoBehaviour
    {
        public enum IndicatorType { Circle, Line, Cone }

        private LineRenderer lr;
        private SpriteRenderer sr;
        private IndicatorType currentType = IndicatorType.Circle;
        private float targetSize = 3.0f;
        private float duration = 0.5f;
        private Color baseColor = new Color(1f, 0.2f, 0.2f, 0.6f);

        private void Awake()
        {
            lr = gameObject.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.widthMultiplier = 0.08f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = baseColor;
            lr.endColor = baseColor;
            lr.sortingOrder = 5500;
        }

        public static BossAoEIndicator CreateCircle(Vector3 position, float radius, float duration, Color? color = null)
        {
            var go = new GameObject("Telegraph_Circle");
            go.transform.position = position;
            var ind = go.AddComponent<BossAoEIndicator>();
            ind.currentType = IndicatorType.Circle;
            ind.targetSize = radius;
            ind.duration = duration;
            if (color.HasValue) ind.baseColor = color.Value;
            ind.StartIndicator();
            return ind;
        }

        public static BossAoEIndicator CreateLine(Vector3 position, Vector2 direction, float length, float width, float duration, Color? color = null)
        {
            var go = new GameObject("Telegraph_Line");
            go.transform.position = position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0, 0, angle);

            var ind = go.AddComponent<BossAoEIndicator>();
            ind.currentType = IndicatorType.Line;
            ind.targetSize = length;
            ind.duration = duration;
            if (color.HasValue) ind.baseColor = color.Value;
            ind.DrawRectangle(length, width);
            ind.StartIndicator();
            return ind;
        }

        public static BossAoEIndicator CreateCone(Vector3 position, Vector2 direction, float radius, float angleDegree, float duration, Color? color = null)
        {
            var go = new GameObject("Telegraph_Cone");
            go.transform.position = position;
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0, 0, baseAngle);

            var ind = go.AddComponent<BossAoEIndicator>();
            ind.currentType = IndicatorType.Cone;
            ind.targetSize = radius;
            ind.duration = duration;
            if (color.HasValue) ind.baseColor = color.Value;
            ind.DrawCone(radius, angleDegree);
            ind.StartIndicator();
            return ind;
        }

        private void StartIndicator()
        {
            if (currentType == IndicatorType.Circle)
            {
                DrawCircle(targetSize);
            }
            StartCoroutine(IndicatorRoutine());
        }

        private void DrawCircle(float radius)
        {
            int segments = 36;
            lr.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float rad = Mathf.Deg2Rad * (i * 360f / segments);
                lr.SetPosition(i, new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0));
            }
        }

        private void DrawRectangle(float length, float width)
        {
            lr.positionCount = 4;
            float halfW = width * 0.5f;
            lr.SetPosition(0, new Vector3(0, -halfW, 0));
            lr.SetPosition(1, new Vector3(length, -halfW, 0));
            lr.SetPosition(2, new Vector3(length, halfW, 0));
            lr.SetPosition(3, new Vector3(0, halfW, 0));
        }

        private void DrawCone(float radius, float angleDegree)
        {
            int segments = 20;
            lr.positionCount = segments + 2;
            lr.SetPosition(0, Vector3.zero);

            float halfAngle = angleDegree * 0.5f;
            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = -halfAngle + (i * angleDegree / segments);
                float rad = Mathf.Deg2Rad * currentAngle;
                lr.SetPosition(i + 1, new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0));
            }
        }

        private IEnumerator IndicatorRoutine()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Tăng độ sáng và độ đậm của vạch cảnh báo theo thời gian
                Color c = baseColor;
                c.a = Mathf.Lerp(0.3f, 1.0f, progress);
                lr.startColor = c;
                lr.endColor = c;
                lr.widthMultiplier = Mathf.Lerp(0.06f, 0.12f, progress);

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}

