using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>Thông báo ngắn ở giữa dưới màn hình, tự mờ dần. Tối đa vài dòng cùng lúc.</summary>
    public class ToastView : MonoBehaviour
    {
        private const int MaxToasts = 4;
        private const float RowHeight = 52f;

        private RectTransform container;
        private readonly List<Entry> entries = new List<Entry>();

        private class Entry
        {
            public RectTransform rt;
            public Image bg;
            public Text text;
            public Color color;
            public float timeLeft;
            public float duration;
        }

        public void Build(TavernUI ui)
        {
            container = UIKit.Rect("Container", transform);
            UIKit.Place(container, new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(980f, RowHeight * MaxToasts));
        }

        public void Show(string message, Color color, float duration)
        {
            if (entries.Count >= MaxToasts)
            {
                Destroy(entries[0].rt.gameObject);
                entries.RemoveAt(0);
            }

            var bg = UIKit.Panel(container, "Toast", new Color(0.16f, 0.1f, 0.06f, 0.92f));
            var text = UIKit.Label(bg.transform, "Text", message, 22, color, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIKit.Stretch(text.rectTransform, 16, 4, 16, 4);
            entries.Add(new Entry { rt = bg.rectTransform, bg = bg, text = text, color = color, timeLeft = duration, duration = duration });
            Layout();
        }

        private void Update()
        {
            bool changed = false;
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var e = entries[i];
                e.timeLeft -= Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(e.timeLeft / 0.5f);
                var c = e.color;
                c.a = alpha;
                e.text.color = c;
                var bgc = e.bg.color;
                bgc.a = 0.92f * alpha;
                e.bg.color = bgc;
                if (e.timeLeft <= 0f)
                {
                    Destroy(e.rt.gameObject);
                    entries.RemoveAt(i);
                    changed = true;
                }
            }
            if (changed) Layout();
        }

        private void Layout()
        {
            // mới nhất ở dưới cùng
            for (int i = 0; i < entries.Count; i++)
            {
                int fromBottom = entries.Count - 1 - i;
                float width = Mathf.Min(980f, entries[i].text.preferredWidth + 48f);
                UIKit.Place(entries[i].rt, new Vector2(0.5f, 0f), new Vector2(0f, fromBottom * (RowHeight + 6f)), new Vector2(width, RowHeight));
            }
        }
    }
}
