using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Cho scene quán chạy độc lập với hệ thống chung của nhóm (SharedGameDataManager + UnifiedGameHubCanvas
    /// của NguyenMinhHiep): gỡ 2 object đó trong phiên chơi này, rồi trả ví / hòm đồ / HUD của quán về như cũ.
    /// Không tham chiếu class của thành viên khác (nhận diện theo tên) để 2 phần compile và chạy tách rời.
    /// Bỏ tick Run Standalone khi muốn ghép quán vào hệ thống chung.
    /// </summary>
    public class TavernStandalone : MonoBehaviour
    {
        // Tên class các hệ thống chung tự bật ở mọi scene
        private static readonly string[] SharedSystemTypes = { "UnifiedGameHubCanvas", "SharedGameDataManager" };

        [SerializeField] private bool runStandalone = true;
        [Tooltip("Kiểm tra lại định kỳ phòng trường hợp hệ thống chung bị tạo lại giữa chừng (giây)")]
        [SerializeField] private float recheckInterval = 1f;

        private float recheckTimer;

        public bool RunStandalone => runStandalone;

        private void Start()
        {
            if (!runStandalone) return;
            // Ví + hòm đồ của quán chỉ sống trong scene này, không mang sang scene khác
            KeepInThisScene(PlayerWallet.Instance);
            KeepInThisScene(PlayerInventory.Instance);
        }

        // Update đầu tiên chạy sau mọi Start -> hệ thống chung đã khởi động và đã ghi đè xong, giờ mới gỡ
        private void Update()
        {
            if (!runStandalone) return;
            recheckTimer -= Time.unscaledDeltaTime;
            if (recheckTimer > 0f) return;
            recheckTimer = recheckInterval;

            if (RemoveSharedSystems()) RestoreTavernState();
        }

        private void KeepInThisScene(Component c)
        {
            if (c == null || c.transform.parent != null || c.gameObject.scene == gameObject.scene) return;
            SceneManager.MoveGameObjectToScene(c.gameObject, gameObject.scene);
        }

        /// <summary>Gỡ các object hệ thống chung. Trả về true nếu có gỡ.</summary>
        private static bool RemoveSharedSystems()
        {
            bool removed = false;
            foreach (var behaviour in FindObjectsOfType<MonoBehaviour>(true))
            {
                if (behaviour == null) continue;
                string typeName = behaviour.GetType().Name;
                foreach (var shared in SharedSystemTypes)
                {
                    if (typeName != shared) continue;
                    Destroy(behaviour.gameObject);
                    removed = true;
                    break;
                }
            }
            return removed;
        }

        private static void RestoreTavernState()
        {
            if (PlayerWallet.Instance != null) PlayerWallet.Instance.ResetToStart();
            if (PlayerInventory.Instance != null) PlayerInventory.Instance.ResetToStart();
            if (TavernUI.Instance != null) TavernUI.Instance.RestoreOwnHud();
            Debug.Log("[TavernStandalone] Quán chạy độc lập: đã gỡ hệ thống chung, trả ví/hòm đồ/HUD của quán về như cũ.");
        }
    }
}
