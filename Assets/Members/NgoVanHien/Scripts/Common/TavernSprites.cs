using UnityEngine;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Sprite sinh bằng code (ô vuông trắng, khung bo góc 9-slice kiểu pixel) để UI/bubble không phụ thuộc asset.
    /// Màu thật được tô qua Image.color / SpriteRenderer.color.
    /// </summary>
    public static class TavernSprites
    {
        private static Sprite white;
        private static Sprite frame;
        private static Font font;

        public static Sprite White
        {
            get
            {
                if (white == null)
                {
                    var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "Tavern_White" };
                    var pixels = new Color32[16];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
                    tex.SetPixels32(pixels);
                    tex.Apply();
                    white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
                    white.name = "Tavern_White";
                }
                return white;
            }
        }

        /// <summary>Khung 12x12: viền tối 1px, gờ sáng 1px, nền trắng; góc vát 1px. Border 9-slice = 3px.</summary>
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
                        name = "Tavern_Frame"
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
                    frame.name = "Tavern_Frame";
                }
                return frame;
            }
        }

        /// <summary>Font hệ thống của Unity (dynamic, fallback font OS nên hiển thị được tiếng Việt).</summary>
        public static Font Font
        {
            get
            {
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        public static int SortingLayerOrDefault(string layerName)
        {
            int id = SortingLayer.NameToID(layerName);
            return SortingLayer.IsValid(id) ? id : 0;
        }
    }
}
