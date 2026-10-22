using UnityEngine;

namespace DungeonEater.Tavern
{
    /// <summary>Chữ nổi bay lên rồi mờ dần (vd "+42 vàng" khi phục vụ xong).</summary>
    public class FloatingText : MonoBehaviour
    {
        private TextMesh textMesh;
        private Color baseColor;
        private float lifetime = 1.6f;
        private float age;

        public static void Spawn(Vector3 position, string text, Color color)
        {
            var go = new GameObject("FloatingText");
            go.transform.position = position;
            var ft = go.AddComponent<FloatingText>();

            var tm = go.AddComponent<TextMesh>();
            tm.font = TavernSprites.Font;
            tm.text = text;
            tm.fontSize = 48;
            tm.characterSize = 0.06f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            tm.color = color;

            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = tm.font.material;
            mr.sortingLayerID = TavernSprites.SortingLayerOrDefault("Overhead");
            mr.sortingOrder = 200;

            ft.textMesh = tm;
            ft.baseColor = color;
        }

        private void Update()
        {
            age += Time.deltaTime;
            transform.position += Vector3.up * (0.8f * Time.deltaTime);
            var c = baseColor;
            c.a = Mathf.Clamp01(1f - (age / lifetime) * (age / lifetime));
            textMesh.color = c;
            if (age >= lifetime) Destroy(gameObject);
        }
    }
}
