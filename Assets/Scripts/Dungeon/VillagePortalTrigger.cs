using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonEater.Dungeon
{
    public class VillagePortalTrigger : MonoBehaviour
    {
        [Header("Target Destination")]
        [SerializeField] private string targetScene = "RestaurantLevel";
        [SerializeField] private string promptMessage = "Bấm [F] để vào quán ăn";
        [SerializeField] private bool autoTrigger = false;

        private bool playerInRange = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
            {
                playerInRange = true;
                if (autoTrigger)
                {
                    LoadTarget();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
            {
                playerInRange = false;
            }
        }

        private void Update()
        {
            if (playerInRange && !autoTrigger)
            {
                if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                {
                    LoadTarget();
                }
            }
        }

        private void LoadTarget()
        {
            Debug.Log($"[VillagePortal] Chuyển cảnh sang: {targetScene}");
            SceneManager.LoadScene(targetScene);
        }

        private void OnGUI()
        {
            if (playerInRange && !autoTrigger)
            {
                GUI.color = Color.yellow;
                GUIStyle style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                GUI.Box(new Rect((Screen.width - 320) / 2f, Screen.height - 130, 320, 35), promptMessage, style);
            }
        }
    }
}

