#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace DungeonEater.Setup
{
    /// <summary>
    /// Tự động slice tất cả sprite sheet thành Multiple sprites.
    /// Chạy từ menu: DungeonEater > Setup > Slice Sprite Sheets
    /// (Phải chạy TRƯỚC khi Create Animator Controllers)
    /// </summary>
    public static class SpriteSliceTool
    {
        // Cấu hình sheet size cho từng file
        private static readonly (string path, int frameW, int frameH, string prefix)[] Sheets =
        {
            // Hunter: 1024x1024
            ("Assets/Sprites/Detailed/Hunter_Combat_4Dir_Sheet.png",  170, 170, "hunter"),
            // Chef: 1024x1024
            ("Assets/Sprites/Detailed/Hero_Chef_4Dir_Sheet.png",      170, 170, "chef"),
            // Boss: nếu có
            ("Assets/Sprites/Detailed/Boss_KingBoar_4Dir_Sheet.png",  128, 128, "boss"),
            // Monsters
            ("Assets/Sprites/Detailed/Monsters_4Dir_Sheet.png",       128, 128, "monster"),
            // NPC + Customers
            ("Assets/Sprites/Detailed/NPC_Elder_Customers_4Dir_Sheet.png", 128, 128, "npc"),
        };

        [MenuItem("DungeonEater/Setup/Slice Sprite Sheets")]
        public static void SliceAll()
        {
            int total = 0;
            foreach (var (path, fw, fh, prefix) in Sheets)
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[SliceTool] Bỏ qua (không tồn tại): {path}");
                    continue;
                }
                int count = SliceSheet(path, fw, fh, prefix);
                total += count;
                Debug.Log($"[SliceTool] ✅ {Path.GetFileName(path)}: sliced {count} frames ({fw}x{fh} mỗi frame)");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SliceTool] ✅ Hoàn tất! Tổng {total} sprite frames đã slice.");
            EditorUtility.DisplayDialog("Slice Xong!",
                $"Đã slice {total} sprite frames.\nBây giờ chạy: DungeonEater > Setup > Create Animator Controllers",
                "OK");
        }

        private static int SliceSheet(string assetPath, int frameW, int frameH, string prefix)
        {
            var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (ti == null) return 0;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex == null) return 0;

            int texW = tex.width, texH = tex.height;
            int cols = texW / frameW;
            int rows = texH / frameH;

            // Cấu hình TextureImporter → Multiple Sprite mode
            ti.textureType          = TextureImporterType.Sprite;
            ti.spriteImportMode     = SpriteImportMode.Multiple;
            ti.filterMode           = FilterMode.Point;
            ti.textureCompression   = TextureImporterCompression.Uncompressed;
            ti.alphaIsTransparency  = true;
            ti.mipmapEnabled        = false;
            ti.spritePixelsPerUnit  = frameW; // 1 frame = 1 Unity unit

            // Tạo danh sách SpriteMetaData
            var metas = new List<SpriteMetaData>();
            int idx = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // Unity Rect: Y tính từ BOTTOM của texture
                    float x = col * frameW;
                    float y = texH - (row + 1) * frameH; // flip Y
                    metas.Add(new SpriteMetaData
                    {
                        name   = $"{prefix}_{idx}",
                        rect   = new Rect(x, y, frameW, frameH),
                        pivot  = new Vector2(0.5f, 0.5f),
                        alignment = (int)SpriteAlignment.Center
                    });
                    idx++;
                }
            }

            ti.spritesheet = metas.ToArray();
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();

            return idx;
        }

        /// <summary>
        /// Xem trước số frame sẽ được slice — chạy trước khi slice thực sự.
        /// </summary>
        [MenuItem("DungeonEater/Setup/Preview Slice Count")]
        public static void Preview()
        {
            string msg = "Kết quả slice dự kiến:\n\n";
            foreach (var (path, fw, fh, prefix) in Sheets)
            {
                if (!File.Exists(path)) { msg += $"❌ {Path.GetFileName(path)} — không tồn tại\n"; continue; }
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) { msg += $"⚠ {Path.GetFileName(path)} — chưa import\n"; continue; }
                int cols = tex.width / fw, rows = tex.height / fh;
                msg += $"✅ {Path.GetFileName(path)}: {cols}×{rows} = {cols*rows} frames ({fw}×{fh}px)\n";
            }
            Debug.Log("[SliceTool] " + msg);
            EditorUtility.DisplayDialog("Preview Slice", msg, "OK");
        }
    }
}
#endif
