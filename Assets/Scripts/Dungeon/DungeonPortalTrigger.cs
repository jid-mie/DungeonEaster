using UnityEngine;
using DungeonEater.Core;

namespace DungeonEater.Dungeon
{
    public class DungeonPortalTrigger : MonoBehaviour
    {
        [SerializeField] private string targetScene = "VillageLevel";

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
            {
                Debug.Log("🌀 [Portal] Người chơi bước vào cổng ma thuật! Đang dịch chuyển về Làng Dun...");
                if (DungeonManager.Instance != null)
                {
                    DungeonManager.Instance.TeleportToVillage();
                }
                
                // If RestaurantLevel exists in build/scenes, load it, otherwise toggle phase
                if (UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + targetScene + ".unity") >= 0 ||
                    System.IO.File.Exists("Assets/Scenes/" + targetScene + ".unity"))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
                }
            }
        }
    }
}

