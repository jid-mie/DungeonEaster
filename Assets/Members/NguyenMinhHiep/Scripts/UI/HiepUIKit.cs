using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NguyenMinhHiep.UI
{
    /// <summary>
    /// Sprite sinh bằng code (ô vuông trắng, khung bo góc 9-slice kiểu pixel, và icon fallback)
    /// lấy đúng format từ NgoVanHien (TavernSprites.cs + UIKit.cs) để Canvas Hub không phụ thuộc prefab ngoài.
    /// </summary>
    public static class HiepSprites
    {
        private static Sprite white;
        private static Sprite frame;
        private static Font font;
        private static readonly Dictionary<string, Sprite> badgeSprites = new Dictionary<string, Sprite>();

        public static Sprite White
        {
            get
            {
                if (white == null)
                {
                    var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false)
                    {
                        filterMode = FilterMode.Point,
                        name = "HiepHub_White"
                    };
                    var pixels = new Color32[16];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
                    tex.SetPixels32(pixels);
                    tex.Apply();
                    white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
                    white.name = "HiepHub_White";
                }
                return white;
            }
        }

        /// <summary>Khung 12x12: viền tối 1px, gờ sáng 1px, nền trắng; góc vát 1px. Border 9-slice = 3px (Chuẩn NgoVanHien).</summary>
        public static Sprite Frame
        {
            get
            {
                if (frame == null)
                {
                    const int size = 12;
                    var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                        name = "HiepHub_Frame"
                    };
                    var outline = new Color32(40, 40, 40, 255);
                    var bevel = new Color32(255, 255, 255, 255);
                    var fill = new Color32(220, 220, 220, 255);
                    var clear = new Color32(0, 0, 0, 0);
                    var pixels = new Color32[size * size];
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            int edge = Mathf.Min(Mathf.Min(x, y), Mathf.Min(size - 1 - x, size - 1 - y));
                            bool corner = (x == 0 || x == size - 1) && (y == 0 || y == size - 1);
                            Color32 c;
                            if (corner) c = clear;
                            else if (edge == 0) c = outline;
                            else if (edge == 1) c = bevel;
                            else c = fill;
                            pixels[y * size + x] = c;
                        }
                    }
                    tex.SetPixels32(pixels);
                    tex.Apply();
                    frame = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f, 0,
                        SpriteMeshType.FullRect, new Vector4(3, 3, 3, 3));
                    frame.name = "HiepHub_Frame";
                }
                return frame;
            }
        }

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

        /// <summary>
        /// Tạo icon pixel 16x16 theo mã màu hex và hình dáng đặc trưng cho vật phẩm chưa có file ảnh ngoài.
        /// </summary>
        public static Sprite GetOrCreateIconBadge(string key, string hexColor, string category = "Ingredient")
        {
            string cacheKey = $"{key}_{hexColor}_{category}";
            if (badgeSprites.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            if (!ColorUtility.TryParseHtmlString(hexColor, out Color baseCol))
            {
                baseCol = new Color(0.9f, 0.75f, 0.3f, 1f);
            }

            const int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = $"Badge_{key}"
            };

            Color darkBorder = new Color(0.12f, 0.08f, 0.06f, 1f);
            Color highlight = Color.Lerp(baseCol, Color.white, 0.45f);
            Color shadow = Color.Lerp(baseCol, Color.black, 0.35f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - 7.5f;
                    float dy = y - 7.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > 6.8f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (dist > 5.6f)
                    {
                        tex.SetPixel(x, y, darkBorder);
                    }
                    else if (x + y > 16 && dist < 4.5f)
                    {
                        tex.SetPixel(x, y, highlight);
                    }
                    else if (x + y < 11)
                    {
                        tex.SetPixel(x, y, shadow);
                    }
                    else
                    {
                        tex.SetPixel(x, y, baseCol);
                    }
                }
            }

            tex.Apply();
            var sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            sp.name = $"Badge_{key}";
            badgeSprites[cacheKey] = sp;
            return sp;
        }
    }

    /// <summary>
    /// Bộ công cụ dựng uGUI bằng code theo đúng mẫu UIKit.cs của NgoVanHien:
    /// Bảng màu gỗ ấm (WoodDark, WoodMid, WoodLight), giấy da (Parchment), viền 9-slice pixel.
    /// </summary>
    public static class HiepUIKit
    {
        // Bảng màu chuẩn NgoVanHien (UIKit.cs)
        public static readonly Color WoodDark       = new Color32(0x2a, 0x1a, 0x10, 0xf2);
        public static readonly Color WoodMid        = new Color32(0x4a, 0x2f, 0x1c, 0xff);
        public static readonly Color WoodLight      = new Color32(0x6e, 0x47, 0x2b, 0xff);
        public static readonly Color Parchment      = new Color32(0xf3, 0xe3, 0xc3, 0xff);
        public static readonly Color ParchmentDark  = new Color32(0xd8, 0xc3, 0x9c, 0xff);
        public static readonly Color SlotIdle       = new Color32(0x3a, 0x25, 0x16, 0xff);
        public static readonly Color SlotSelected   = new Color32(0xf5, 0xc4, 0x51, 0xff);
        public static readonly Color SlotHover      = new Color32(0x7c, 0x52, 0x32, 0xff);

        public static readonly Color AccentGold     = new Color32(0xff, 0xd2, 0x4a, 0xff);
        public static readonly Color AccentGreen    = new Color32(0x58, 0xc2, 0x67, 0xff);
        public static readonly Color AccentRed      = new Color32(0xe2, 0x57, 0x4c, 0xff);
        public static readonly Color AccentBlue     = new Color32(0x63, 0xb4, 0xf6, 0xff);
        public static readonly Color AccentOrange   = new Color32(0xf3, 0x9c, 0x3d, 0xff);

        // Màu thanh Máu & Thể lực
        public static readonly Color HpBarBg        = new Color32(0x28, 0x0c, 0x0c, 0xff);
        public static readonly Color HpBarFill      = new Color32(0xdc, 0x26, 0x26, 0xff);
        public static readonly Color StaminaBarBg   = new Color32(0x0c, 0x24, 0x18, 0xff);
        public static readonly Color StaminaBarFill = new Color32(0x22, 0xc5, 0x5e, 0xff);

        public static readonly Color TextLight      = new Color32(0xfb, 0xf2, 0xdc, 0xff);
        public static readonly Color TextDark       = new Color32(0x2a, 0x1a, 0x10, 0xff);
        public static readonly Color TextMuted      = new Color32(0xb8, 0xa2, 0x84, 0xff);

        public static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static Image CreatePanel(Transform parent, string name, Color color, bool framed = true)
        {
            var rt = CreateRect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = framed ? HiepSprites.Frame : HiepSprites.White;
            img.type = framed ? Image.Type.Sliced : Image.Type.Simple;
            if (framed) img.pixelsPerUnitMultiplier = 0.75f;
            img.color = color;
            return img;
        }

        public static Image CreateFillBar(Transform parent, string name, Color color)
        {
            var rt = CreateRect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = HiepSprites.White;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Left;
            img.fillAmount = 1f;
            img.color = color;
            return img;
        }

        public static Text CreateLabel(Transform parent, string name, string content, int fontSize,
            Color color, TextAnchor align = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var rt = CreateRect(parent, name);
            var txt = rt.gameObject.AddComponent<Text>();
            txt.font = HiepSprites.Font;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.fontStyle = style;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.supportRichText = true;
            txt.raycastTarget = false;

            var shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return txt;
        }

        public static Button CreateButton(Transform parent, string name, string label, int fontSize,
            Color bg, Color fg, UnityAction onClick)
        {
            var img = CreatePanel(parent, name, bg, true);
            var btn = img.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            btn.colors = colors;
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);

            var txt = CreateLabel(img.transform, "Label", label, fontSize, fg, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(txt.rectTransform, 4, 4, 4, 4);
            return btn;
        }

        public static void Stretch(RectTransform rt, float left = 0, float right = 0, float top = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        public static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }
    }
}
