using System;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.TranHuuHau.Common
{
    /// <summary>
    /// Hệ thống theme UI và sinh texture / sprite pixel-art procedural
    /// cho màn hình Làng của Trần Hữu Hậu.
    /// Giúp UI luôn hiển thị chuẩn đẹp mà không lo thiếu asset hay missing reference.
    /// </summary>
    public static class HauUITheme
    {
        // Bảng màu chính theo GDD & tông màu làng cổ điển
        public static readonly Color WoodDarkest = new Color32(0x1e, 0x14, 0x0c, 0xf8);
        public static readonly Color WoodDark    = new Color32(0x32, 0x20, 0x14, 0xf6);
        public static readonly Color WoodNormal  = new Color32(0x4a, 0x2e, 0x1b, 0xf5);
        public static readonly Color WoodLight   = new Color32(0x6b, 0x44, 0x27, 0xff);
        public static readonly Color WoodBorder  = new Color32(0x8a, 0x5a, 0x33, 0xff);
        public static readonly Color SlotBg      = new Color32(0x27, 0x18, 0x0f, 0xff);
        public static readonly Color SlotHover   = new Color32(0x56, 0x39, 0x23, 0xff);
        public static readonly Color SlotSelect  = new Color32(0xd6, 0x9b, 0x34, 0xff);

        public static readonly Color TextWhite   = new Color32(0xff, 0xff, 0xff, 0xff);
        public static readonly Color TextCream   = new Color32(0xf5, 0xeb, 0xd5, 0xff);
        public static readonly Color TextDim     = new Color32(0xb8, 0xa4, 0x8a, 0xff);
        public static readonly Color GoldYellow  = new Color32(0xff, 0xd0, 0x36, 0xff);
        public static readonly Color RepCyan     = new Color32(0x58, 0xd5, 0xff, 0xff);
        public static readonly Color HealthRed   = new Color32(0xef, 0x44, 0x44, 0xff);

        public static readonly Color BtnGreen    = new Color32(0x2e, 0x7d, 0x32, 0xff);
        public static readonly Color BtnGreenHover = new Color32(0x38, 0x8e, 0x3c, 0xff);
        public static readonly Color BtnBlue     = new Color32(0x19, 0x76, 0xd2, 0xff);
        public static readonly Color BtnRed      = new Color32(0xc6, 0x28, 0x28, 0xff);
        public static readonly Color BtnBrown    = new Color32(0x5d, 0x40, 0x37, 0xff);

        private static Sprite white;
        private static Sprite frame;
        private static Font font;

        public static Sprite White
        {
            get
            {
                if (white == null)
                {
                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    var cols = new Color32[] { Color.white, Color.white, Color.white, Color.white };
                    tex.SetPixels32(cols);
                    tex.Apply();
                    white = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f);
                    white.name = "Hau_White";
                }
                return white;
            }
        }

        /// <summary>
        /// Sprite khung 16x16 pixel có viền 9-slice để làm bảng điều khiển, nút bấm, ô đồ.
        /// </summary>
        public static Sprite Frame
        {
            get
            {
                if (frame == null)
                {
                    const int s = 16;
                    var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                        name = "Hau_Frame_Texture"
                    };

                    Color32 outer = new Color32(28, 17, 10, 255);
                    Color32 highlight = new Color32(195, 142, 85, 255);
                    Color32 shadow = new Color32(45, 27, 15, 255);
                    Color32 fill = new Color32(230, 230, 230, 255);
                    Color32 transparent = new Color32(0, 0, 0, 0);

                    Color32[] px = new Color32[s * s];
                    for (int y = 0; y < s; y++)
                    {
                        for (int x = 0; x < s; x++)
                        {
                            bool corner = (x == 0 && y == 0) || (x == 0 && y == s - 1) || (x == s - 1 && y == 0) || (x == s - 1 && y == s - 1);
                            if (corner)
                            {
                                px[y * s + x] = transparent;
                                continue;
                            }

                            if (x == 0 || x == s - 1 || y == 0 || y == s - 1)
                            {
                                px[y * s + x] = outer;
                            }
                            else if (y == s - 2 || x == 1)
                            {
                                px[y * s + x] = highlight;
                            }
                            else if (y == 1 || x == s - 2)
                            {
                                px[y * s + x] = shadow;
                            }
                            else
                            {
                                px[y * s + x] = fill;
                            }
                        }
                    }
                    tex.SetPixels32(px);
                    tex.Apply();
                    frame = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 16f, 0,
                        SpriteMeshType.FullRect, new Vector4(3, 3, 3, 3));
                    frame.name = "Hau_Frame_Sprite";
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

        // ========================== TIỆN ÍCH DỰNG UGUI NHANH ==========================

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static RectTransform Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform FillParent(RectTransform rt, float padLeft = 0, float padBottom = 0, float padRight = 0, float padTop = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(padLeft, padBottom);
            rt.offsetMax = new Vector2(-padRight, -padTop);
            return rt;
        }

        public static Image CreatePanel(Transform parent, string name, Color color, bool raycast = true)
        {
            var rt = CreateRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = Frame;
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        public static Image CreateBox(Transform parent, string name, Color color, bool raycast = false)
        {
            var rt = CreateRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = White;
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        public static Text CreateLabel(Transform parent, string name, string text, int fontSize, Color color,
            TextAnchor align = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var rt = CreateRect(name, parent);
            var txt = rt.gameObject.AddComponent<Text>();
            txt.font = Font;
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.fontStyle = style;
            txt.supportRichText = true;
            txt.raycastTarget = false;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;

            var shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.7f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return txt;
        }

        public static Button CreateButton(Transform parent, string name, string labelText, int fontSize, Color btnColor, Action onClick)
        {
            var panel = CreatePanel(parent, name, btnColor, true);
            var btn = panel.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = btnColor;
            colors.highlightedColor = Color.Lerp(btnColor, Color.white, 0.25f);
            colors.pressedColor = Color.Lerp(btnColor, Color.black, 0.25f);
            btn.colors = colors;

            if (onClick != null)
            {
                btn.onClick.AddListener(() => onClick());
            }

            if (!string.IsNullOrEmpty(labelText))
            {
                var lbl = CreateLabel(panel.transform, "Label", labelText, fontSize, TextCream, TextAnchor.MiddleCenter, FontStyle.Bold);
                FillParent(lbl.rectTransform);
            }

            return btn;
        }

        public static Image CreateIcon(Transform parent, string name, Sprite sprite)
        {
            var rt = CreateRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite != null ? sprite : White;
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        // ========================== PROCEDURAL PIXEL ICONS ==========================

        private static Sprite potionIcon;
        private static Sprite bombIcon;
        private static Sprite iceIcon;
        private static Sprite poisonIcon;
        private static Sprite mushroomIcon;
        private static Sprite boneIcon;
        private static Sprite goldIcon;
        private static Sprite repIcon;

        public static Sprite PotionIcon
        {
            get
            {
                if (potionIcon == null)
                    potionIcon = GeneratePixelIcon("Potion", (x, y) =>
                    {
                        if (y >= 12 && x >= 6 && x <= 9) return new Color32(160, 100, 60, 255); // Cork
                        if (y == 11 && x >= 5 && x <= 10) return new Color32(200, 200, 200, 255); // Rim
                        if (y <= 10 && y >= 2 && x >= 3 && x <= 12)
                        {
                            if (x == 3 || x == 12 || y == 2) return new Color32(60, 10, 10, 255); // Bottle edge
                            if (y >= 8 && x <= 5) return new Color32(255, 180, 180, 255); // Glass highlight
                            return new Color32(230, 40, 40, 255); // Red liquid
                        }
                        return new Color32(0, 0, 0, 0);
                    });
                return potionIcon;
            }
        }

        public static Sprite BombIcon
        {
            get
            {
                if (bombIcon == null)
                    bombIcon = GeneratePixelIcon("Bomb", (x, y) =>
                    {
                        if (x == 12 && y == 14) return new Color32(255, 230, 50, 255); // Spark
                        if (x >= 9 && y >= 11 && x <= 11 && y <= 13) return new Color32(180, 140, 90, 255); // Fuse
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(7, 6));
                        if (dist <= 5.2f)
                        {
                            if (dist > 4.5f) return new Color32(15, 15, 15, 255); // Outer
                            if (x <= 5 && y >= 8) return new Color32(120, 120, 120, 255); // Highlight
                            return new Color32(45, 45, 45, 255); // Body
                        }
                        return new Color32(0, 0, 0, 0);
                    });
                return bombIcon;
            }
        }

        public static Sprite IceIcon
        {
            get
            {
                if (iceIcon == null)
                    iceIcon = GeneratePixelIcon("Ice", (x, y) =>
                    {
                        float dist = Mathf.Abs(x - 7.5f) + Mathf.Abs(y - 7.5f);
                        if (dist <= 6.5f)
                        {
                            if (dist > 5.5f) return new Color32(40, 100, 160, 255);
                            if (x <= 6 && y >= 8) return new Color32(240, 255, 255, 255);
                            return new Color32(100, 210, 255, 255);
                        }
                        return new Color32(0, 0, 0, 0);
                    });
                return iceIcon;
            }
        }

        public static Sprite PoisonIcon
        {
            get
            {
                if (poisonIcon == null)
                    poisonIcon = GeneratePixelIcon("Poison", (x, y) =>
                    {
                        if (y >= 12 && x >= 6 && x <= 9) return new Color32(120, 120, 120, 255);
                        if (y <= 11 && y >= 2 && x >= 3 && x <= 12)
                        {
                            if (x == 3 || x == 12 || y == 2) return new Color32(20, 50, 20, 255);
                            if (x == 6 && y == 7) return new Color32(180, 255, 100, 255); // Bubble
                            return new Color32(80, 200, 40, 255); // Toxic green
                        }
                        return new Color32(0, 0, 0, 0);
                    });
                return poisonIcon;
            }
        }

        public static Sprite MushroomIcon
        {
            get
            {
                if (mushroomIcon == null)
                    mushroomIcon = GeneratePixelIcon("Mushroom", (x, y) =>
                    {
                        // Stem
                        if (y <= 6 && y >= 2 && x >= 6 && x <= 9) return new Color32(220, 220, 200, 255);
                        // Cap
                        if (y >= 6 && y <= 13 && x >= 2 && x <= 13)
                        {
                            if ((x - 7.5f) * (x - 7.5f) / 30f + (y - 7f) * (y - 7f) / 25f <= 1f)
                            {
                                if ((x == 5 && y == 10) || (x == 10 && y == 9)) return new Color32(230, 255, 255, 255); // Spot
                                return new Color32(40, 180, 220, 255); // Cyan glow
                            }
                        }
                        return new Color32(0, 0, 0, 0);
                    });
                return mushroomIcon;
            }
        }

        public static Sprite BoneIcon
        {
            get
            {
                if (boneIcon == null)
                    boneIcon = GeneratePixelIcon("Bone", (x, y) =>
                    {
                        if (Mathf.Abs(x - y) <= 1 && x >= 4 && x <= 11) return new Color32(240, 235, 225, 255);
                        if ((x <= 4 && y <= 4) || (x >= 11 && y >= 11)) return new Color32(220, 215, 200, 255);
                        return new Color32(0, 0, 0, 0);
                    });
                return boneIcon;
            }
        }

        public static Sprite GoldIcon
        {
            get
            {
                if (goldIcon == null)
                    goldIcon = GeneratePixelIcon("Gold", (x, y) =>
                    {
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                        if (d <= 6.5f)
                        {
                            if (d > 5.5f) return new Color32(180, 120, 10, 255);
                            if (d <= 3.5f && (x == 7 || x == 8)) return new Color32(255, 240, 150, 255);
                            return new Color32(255, 200, 30, 255);
                        }
                        return new Color32(0, 0, 0, 0);
                    });
                return goldIcon;
            }
        }

        public static Sprite RepIcon
        {
            get
            {
                if (repIcon == null)
                    repIcon = GeneratePixelIcon("Reputation", (x, y) =>
                    {
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                        if (d <= 6.2f)
                        {
                            if (d > 5.2f) return new Color32(20, 90, 150, 255);
                            return new Color32(70, 190, 255, 255);
                        }
                        return new Color32(0, 0, 0, 0);
                    });
                return repIcon;
            }
        }

        private static Sprite GeneratePixelIcon(string name, Func<int, int, Color32> pixelFunc)
        {
            const int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "Hau_Icon_" + name
            };
            Color32[] cols = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    cols[y * size + x] = pixelFunc(x, y);
                }
            }
            tex.SetPixels32(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            spr.name = "Hau_Icon_" + name;
            return spr;
        }
    }
}
