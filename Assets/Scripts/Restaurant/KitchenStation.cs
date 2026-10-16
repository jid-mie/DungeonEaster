using UnityEngine;
using DungeonEater.Dungeon;
using DungeonEater.UI;

namespace DungeonEater.Restaurant
{
    /// <summary>
    /// Lò nướng trong quán: đứng gần và bấm [F] để mở màn Bếp (KitchenScreen).
    /// Tự sinh vầng sáng lửa nhấp nháy để người chơi dễ nhận ra vị trí bếp.
    /// </summary>
    public class KitchenStation : MonoBehaviour
    {
        [SerializeField] private float interactRange = 1.9f;
        [SerializeField] private KeyCode interactKey = KeyCode.F;
        [SerializeField] private Vector2 glowSize = new Vector2(3.2f, 2.4f);

        private SpriteRenderer glow;
        private SpriteRenderer marker;
        private Transform player;
        private bool playerInRange;

        public bool PlayerInRange => playerInRange;

        private void Start()
        {
            var glowObj = new GameObject("StoveGlow");
            glowObj.transform.SetParent(transform, false);
            glow = glowObj.AddComponent<SpriteRenderer>();
            glow.sprite = KitchenUIKit.SoftCircle;
            glow.sortingOrder = -50;
            glowObj.transform.localScale = new Vector3(glowSize.x / 0.64f, glowSize.y / 0.64f, 1f);

            var markerObj = new GameObject("StoveMarker");
            markerObj.transform.SetParent(transform, false);
            marker = markerObj.AddComponent<SpriteRenderer>();
            marker.sprite = KitchenUIKit.Star;
            marker.color = KitchenUIKit.GoldBright;
            marker.sortingOrder = 200;
            markerObj.transform.localScale = Vector3.one * 0.5f;
        }

        private void Update()
        {
            if (player == null)
            {
                var pc = PlayerController.Instance != null ? PlayerController.Instance : FindObjectOfType<PlayerController>();
                if (pc != null) player = pc.transform;
            }

            playerInRange = player != null && !KitchenScreen.IsOpen &&
                            Vector2.Distance(player.position, transform.position) <= interactRange;

            float flick = Mathf.PerlinNoise(Time.time * 2.7f, 1.3f);
            glow.color = new Color(1f, 0.55f, 0.18f, (playerInRange ? 0.45f : 0.22f) + flick * 0.15f);

            marker.enabled = !playerInRange && !KitchenScreen.IsOpen;
            marker.transform.localPosition = new Vector3(0f, 1.1f + Mathf.Sin(Time.time * 3f) * 0.12f, 0f);
            marker.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 2f) * 12f);

            if (playerInRange && Input.GetKeyDown(interactKey))
            {
                KitchenScreen.Open();
            }
        }

        private void OnGUI()
        {
            if (!playerInRange || KitchenScreen.IsOpen) return;
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = new Color(1f, 0.87f, 0.5f);
            GUI.Box(new Rect((Screen.width - 340) / 2f, Screen.height - 150, 340, 40), "Bấm [F] để vào bếp nấu ăn", style);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.1f);
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
