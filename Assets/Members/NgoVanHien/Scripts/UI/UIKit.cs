using System;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Bảng màu + hàm dựng uGUI bằng code. Tông nâu gỗ, chữ kem, theo guideline "Nhà hàng: nâu gỗ, màu ấm".
    /// </summary>
    public static class UIKit
    {
        public static readonly Color Wood = new Color32(0x5a, 0x3a, 0x22, 0xf5);
        public static readonly Color WoodDark = new Color32(0x3a, 0x24, 0x14, 0xff);
        public static readonly Color WoodLight = new Color32(0x8a, 0x5a, 0x34, 0xff);
        public static readonly Color Slot = new Color32(0x4a, 0x2e, 0x1a, 0xff);
        public static readonly Color Text = new Color32(0xf3, 0xe2, 0xc0, 0xff);
        public static readonly Color TextDim = new Color32(0xb8, 0x9f, 0x80, 0xff);
        public static readonly Color Gold = new Color32(0xff, 0xd2, 0x4a, 0xff);
        public static readonly Color Rep = new Color32(0x7f, 0xd8, 0xff, 0xff);
        public static readonly Color Good = new Color32(0x86, 0xe0, 0x5e, 0xff);
        public static readonly Color Bad = new Color32(0xf0, 0x6a, 0x58, 0xff);
        public static readonly Color Highlight = new Color32(0xff, 0xc8, 0x3d, 0xff);
        public static readonly Color ButtonGreen = new Color32(0x4f, 0x8a, 0x34, 0xff);
        public static readonly Color ButtonBrown = new Color32(0x7a, 0x4e, 0x2c, 0xff);
        public static readonly Color ButtonRed = new Color32(0x9a, 0x3a, 0x2c, 0xff);

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Đặt theo 1 điểm neo (anchor = pivot), pos tính từ điểm neo.</summary>
        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>Kéo giãn theo cha với lề left/bottom/right/top.</summary>
        public static RectTransform Stretch(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        /// <summary>Khung bo góc kiểu pixel (9-slice).</summary>
        public static Image Panel(Transform parent, string name, Color color)
        {
            var img = Rect(name, parent).gameObject.AddComponent<Image>();
            img.sprite = TavernSprites.Frame;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.75f;
            img.color = color;
            return img;
        }

        public static Image Box(Transform parent, string name, Color color)
        {
            var img = Rect(name, parent).gameObject.AddComponent<Image>();
            img.sprite = TavernSprites.White;
            img.color = color;
            return img;
        }

        public static Image Icon(Transform parent, string name, Sprite sprite)
        {
            var img = Rect(name, parent).gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.enabled = sprite != null;
            return img;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color,
            TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var t = Rect(name, parent).gameObject.AddComponent<Text>();
            t.font = TavernSprites.Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.fontStyle = style;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var shadow = t.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2f, -2f);
            return t;
        }

        public static Button Button(Transform parent, string name, string label, int fontSize, Color color, Action onClick, out Text text)
        {
            var img = Panel(parent, name, color);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.8f);
            colors.colorMultiplier = 1.2f;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            text = Label(img.transform, "Label", label, fontSize, Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(text.rectTransform, 4, 2, 4, 2);
            return btn;
        }

        public static Button Button(Transform parent, string name, string label, int fontSize, Color color, Action onClick)
        {
            return Button(parent, name, label, fontSize, color, onClick, out _);
        }

        public static string FormatNumber(int value)
        {
            return value.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', '.');
        }

        public static string Hex(Color c)
        {
            return ColorUtility.ToHtmlStringRGB(c);
        }
    }
}
