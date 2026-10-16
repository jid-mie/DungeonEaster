using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DungeonEater.Core;

namespace DungeonEater.UI
{
    /// <summary>
    /// Bộ công cụ dựng UI bằng code cho màn Bếp: nạp sprite trong Resources/Kitchen,
    /// sinh khung gỗ/giấy da kiểu pixel-art và các hàm tạo Image/Text/Button gọn.
    /// </summary>
    public static class KitchenUIKit
    {
        // Bảng màu "quán gỗ ấm" theo GDD (nhà hàng: tông nâu gỗ cổ điển)
        public static readonly Color Wood = new Color32(74, 44, 26, 255);
        public static readonly Color WoodDark = new Color32(38, 22, 14, 255);
        public static readonly Color Gold = new Color32(232, 184, 82, 255);
        public static readonly Color GoldBright = new Color32(255, 222, 120, 255);
        public static readonly Color Parchment = new Color32(236, 214, 170, 255);
        public static readonly Color Ink = new Color32(58, 34, 20, 255);
        public static readonly Color PerfectGreen = new Color32(82, 196, 72, 255);
        public static readonly Color GoodYellow = new Color32(240, 196, 52, 255);
        public static readonly Color BurntRed = new Color32(196, 52, 40, 255);
        public static readonly Color Ember = new Color32(255, 140, 40, 255);

        private static Font font;
        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        public static Font Font
        {
            get
            {
                if (font == null)
                {
                    font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return font;
            }
        }

        // ---------------------------------------------------------------- Sprites

        public static Sprite Load(string name)
        {
            string key = "res:" + name;
            if (spriteCache.TryGetValue(key, out var s) && s != null) return s;
            s = Resources.Load<Sprite>("Kitchen/" + name);
            if (s == null)
            {
                // Phòng khi texture chưa được import dạng Sprite
                var tex = Resources.Load<Texture2D>("Kitchen/" + name);
                if (tex != null) s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            spriteCache[key] = s;
            return s;
        }

        public static Sprite IngredientIcon(IngredientType type)
        {
            switch (type)
            {
                case IngredientType.MonsterMeat: return Load("Loot_Meat");
                case IngredientType.CaveMushroom: return Load("Loot_Mushroom");
                case IngredientType.BombCore: return Load("Loot_Bomb");
                case IngredientType.BossRib: return Load("Loot_Rib");
                default: return null;
            }
        }

        public static string IngredientShortName(IngredientType type)
        {
            switch (type)
            {
                case IngredientType.MonsterMeat: return "Thịt quái";
                case IngredientType.CaveMushroom: return "Nấm hang";
                case IngredientType.BombCore: return "Lõi bọ nổ";
                case IngredientType.BossRib: return "Sườn Vua Thú";
                default: return type.ToString();
            }
        }

        public static Sprite DishSprite(string recipeId)
        {
            switch (recipeId)
            {
                case "skewer": return Load("Dish_Skewer");
                case "soup": return Load("Dish_Soup");
                case "steak": return Load("Dish_Steak");
                case "feast": return Load("Dish_Feast");
                default: return null;
            }
        }

        public static Sprite White => Procedural("white", 4, 4, 0, (x, y) => Color.white);

        /// <summary>Khung bo góc kiểu pixel: viền đậm + chỉ vàng bên trong + nền. Dùng 9-slice.</summary>
        public static Sprite Frame(Color fill, Color border, Color trim)
        {
            string key = $"frame:{ColorUtility.ToHtmlStringRGBA(fill)}:{ColorUtility.ToHtmlStringRGBA(border)}:{ColorUtility.ToHtmlStringRGBA(trim)}";
            return Procedural(key, 32, 32, 10, (x, y) =>
            {
                int dx = Mathf.Min(x, 31 - x);
                int dy = Mathf.Min(y, 31 - y);
                // Bo góc bậc thang
                if (dx + dy < 3) return Color.clear;
                if (dx < 3 || dy < 3 || dx + dy < 5) return border;
                if (dx == 4 || dy == 4) return trim;
                return fill;
            });
        }

        public static Sprite SoftCircle => Procedural("softcircle", 64, 64, 0, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(32, 32)) / 32f;
            float a = Mathf.Clamp01(1f - d);
            return new Color(1, 1, 1, a * a);
        });

        public static Sprite Star => Procedural("star", 64, 64, 0, (x, y) =>
        {
            Vector2 p = new Vector2(x + 0.5f - 32f, y + 0.5f - 30f);
            return InsideStar(p, 31f, 13f) ? Color.white : Color.clear;
        });

        public static Sprite TriangleDown => Procedural("tri", 32, 32, 0, (x, y) =>
        {
            float half = (y / 31f) * 16f;
            return Mathf.Abs(x + 0.5f - 16f) <= half ? Color.white : Color.clear;
        });

        private static bool InsideStar(Vector2 p, float outer, float inner)
        {
            float ang = Mathf.Atan2(p.x, p.y);
            float r = p.magnitude;
            float sector = Mathf.PI * 2f / 5f;
            float a = Mathf.Repeat(ang, sector) / sector; // 0..1 trong một cánh
            float t = Mathf.Abs(a - 0.5f) * 2f;           // 1 ở đỉnh cánh, 0 ở khe
            float limit = Mathf.Lerp(inner, outer, t);
            return r <= limit;
        }

        private static Sprite Procedural(string key, int w, int h, int border, System.Func<int, int, Color> pixel)
        {
            if (spriteCache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = key == "softcircle" || key == "star" ? FilterMode.Bilinear : FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = pixel(x, y);
            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
            spriteCache[key] = sprite;
            return sprite;
        }

        // ---------------------------------------------------------------- Builders

        public static RectTransform Node(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static RectTransform Place(this RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Stretch(this RectTransform rt, float left = 0, float right = 0, float top = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static Image Img(Transform parent, string name, Sprite sprite, Color color, bool raycast = false)
        {
            var rt = Node(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            if (sprite != null && sprite.border.sqrMagnitude > 0f)
            {
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 0.5f; // viền pixel phóng x2 cho chắc nét
            }
            return img;
        }

        public static Image Icon(Transform parent, string name, Sprite sprite, Vector2 size)
        {
            var img = Img(parent, name, sprite, Color.white);
            img.preserveAspect = true;
            img.rectTransform.sizeDelta = size;
            return img;
        }

        public static Text Txt(Transform parent, string name, string text, int size, Color color,
            TextAnchor align = TextAnchor.MiddleCenter, bool bold = false, bool outline = true)
        {
            var rt = Node(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.supportRichText = true;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            if (outline)
            {
                var o = rt.gameObject.AddComponent<Outline>();
                o.effectColor = new Color(0.08f, 0.04f, 0.02f, 0.9f);
                o.effectDistance = new Vector2(2f, -2f);
            }
            return t;
        }

        public static Button Btn(Transform parent, string name, string label, Color fill, Color textColor, int fontSize, System.Action onClick)
        {
            var img = Img(parent, name, Frame(fill, WoodDark, Color.Lerp(fill, Color.white, 0.35f)), Color.white, true);
            var btn = img.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.1f, 1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.75f, 0.7f, 1f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.8f);
            colors.colorMultiplier = 1.2f;
            btn.colors = colors;
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            var t = Txt(img.transform, "Label", label, fontSize, textColor, TextAnchor.MiddleCenter, true);
            t.rectTransform.Stretch(8, 8, 4, 4);
            return btn;
        }
    }
}
