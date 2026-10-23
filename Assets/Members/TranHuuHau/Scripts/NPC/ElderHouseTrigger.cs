using UnityEngine;
using DungeonEater.TranHuuHau.UI;

namespace DungeonEater.TranHuuHau.NPC
{
    /// <summary>
    /// Điểm tương tác tại cửa nhà Trưởng làng Godwin (ElderHouse).
    /// Khi người chơi đến gần và nhấn [E], mở giao diện Cửa hàng của Trưởng làng:
    /// - Bên trái: Danh mục bán vật phẩm (Bình máu, Lựu đạn, Nguyên liệu).
    /// - Bên phải: Hòm đồ của người chơi, số vàng, danh tiếng và cơ chế bán đồ.
    /// </summary>
    public class ElderHouseTrigger : MonoBehaviour
    {
        [Header("Khoảng cách tương tác")]
        [SerializeField] private float interactRange = 3.2f;
        [SerializeField] private string promptText = "Vào Tiệm Tiếp Tế Trưởng Làng Godwin";

        private Transform playerTransform;
        private bool isPlayerInRange = false;
        private TextMesh nameplate;

        public bool IsPlayerInRange => isPlayerInRange;
        public string PromptText => promptText;

        private void Start()
        {
            FindPlayer();
            SetupNameplate();
        }

        private void SetupNameplate()
        {
            var npGo = transform.Find("Nameplate");
            if (npGo == null)
            {
                var go = new GameObject("Nameplate");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 1.8f, 0f);
                nameplate = go.AddComponent<TextMesh>();
                nameplate.characterSize = 0.045f;
                nameplate.fontSize = 26;
                nameplate.alignment = TextAlignment.Center;
                nameplate.anchor = TextAnchor.LowerCenter;
                nameplate.color = Color.white;

                var mr = go.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.sortingLayerName = "Entities";
                    mr.sortingOrder = 6500;
                }
            }
            else
            {
                nameplate = npGo.GetComponent<TextMesh>();
            }

            UpdateNameplateVisual();
        }

        private void UpdateNameplateVisual()
        {
            if (nameplate == null) return;

            if (isPlayerInRange)
            {
                nameplate.text = "<b>Tiệm Tiếp Tế Trưởng Làng</b>\n<color=#86efac><b>[E] Mua / Bán Đồ</b></color>";
            }
            else
            {
                nameplate.text = "<b>Tiệm Tiếp Tế Godwin</b>\n<color=#fef08a><size=18>Cửa Hàng Làng Londo</size></color>";
            }
        }

        private void FindPlayer()
        {
            var pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null)
            {
                playerTransform = pObj.transform;
            }
            else
            {
                var p = FindObjectOfType<Player.VillagePlayerController>();
                if (p != null) playerTransform = p.transform;
            }
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            bool inRange = dist <= interactRange;

            if (inRange != isPlayerInRange)
            {
                isPlayerInRange = inRange;
                UpdateNameplateVisual();
            }

            if (isPlayerInRange)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Chỉ mở shop khi không có dialogue hay UI nào khác
                    if (VillageShopUI.Instance != null && !VillageShopUI.Instance.IsOpen
                        && (VillageDialogueUI.Instance == null || !VillageDialogueUI.Instance.IsOpen))
                    {
                        Interact();
                    }
                }
            }
        }

        public void Interact()
        {
            if (VillageShopUI.Instance != null)
            {
                VillageShopUI.Instance.OpenShop();
            }
            else
            {
                Debug.Log("[ElderHouseTrigger] Mở tiệm trưởng làng!");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
