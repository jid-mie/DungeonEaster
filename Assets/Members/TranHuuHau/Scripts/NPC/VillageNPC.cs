using System.Collections;
using UnityEngine;
using DungeonEater.TranHuuHau.Data;
using DungeonEater.TranHuuHau.UI;

namespace DungeonEater.TranHuuHau.NPC
{
    /// <summary>
    /// Script đại diện cho 1 trong 10 NPC làng Londo.
    /// Xử lý:
    /// - Quét khoảng cách với nhân vật người chơi.
    /// - Hiển thị gợi ý phím [E] tương tác.
    /// - Kích hoạt giao diện hội thoại VillageDialogueUI khi bấm [E].
    /// - Quay mặt hướng về phía người chơi & hiệu ứng idle nhấp nhô nhẹ.
    /// </summary>
    public class VillageNPC : MonoBehaviour
    {
        [Header("Thông tin NPC")]
        [SerializeField] private string npcName = "Dân làng";
        [SerializeField] private string roleTitle = "Cư dân làng Londo";
        [SerializeField] private Sprite portraitSprite;
        [SerializeField] [TextArea(2, 4)] private string[] dialogueLines;

        [Header("Tương tác")]
        [SerializeField] private float interactRange = 2.2f;

        private Transform playerTransform;
        private SpriteRenderer spriteRenderer;
        private Vector3 initialScale;
        private float bobTimer = 0f;
        private bool isPlayerInRange = false;
        private TextMesh nameplate;

        public string NpcName => npcName;
        public string RoleTitle => roleTitle;
        public Sprite Portrait => portraitSprite;
        public string[] DialogueLines => dialogueLines;
        public bool IsPlayerInRange => isPlayerInRange;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            initialScale = transform.localScale;
            bobTimer = Random.Range(0f, Mathf.PI * 2f);

            // Đảm bảo hiển thị trên layer 'Entities' và đúng độ sâu Y-Sort để không bị chìm dưới map/sàn nhà
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingLayerName = "Entities";
                spriteRenderer.sortingOrder = 5000 + Mathf.RoundToInt(-transform.position.y * 100);
            }
        }

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
                nameplate = go.AddComponent<TextMesh>();

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

            if (nameplate != null)
            {
                nameplate.characterSize = 0.045f;
                nameplate.fontSize = 26;
                nameplate.alignment = TextAlignment.Center;
                nameplate.anchor = TextAnchor.LowerCenter;
                nameplate.color = Color.white;

                // Bù tỷ lệ scale để chữ TextMesh không bị méo theo tỷ lệ scale của NPC
                float invX = transform.localScale.x != 0 ? 1f / Mathf.Abs(transform.localScale.x) : 1f;
                float invY = transform.localScale.y != 0 ? 1f / Mathf.Abs(transform.localScale.y) : 1f;
                nameplate.transform.localScale = new Vector3(invX, invY, 1f);

                // Căn chiều cao bảng tên nằm ngay trên đầu của NPC
                float headY = 1.9f;
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    headY = (spriteRenderer.sprite.rect.height / (spriteRenderer.sprite.pixelsPerUnit * 2f)) + 0.35f;
                }
                nameplate.transform.localPosition = new Vector3(0f, headY, 0f);
            }

            UpdateNameplateVisual();
        }

        private void UpdateNameplateVisual()
        {
            if (nameplate == null) return;

            if (isPlayerInRange)
            {
                nameplate.text = $"<b>{npcName}</b>\n<color=#86efac><b>[E] Trò chuyện</b></color>";
            }
            else
            {
                nameplate.text = $"<b>{npcName}</b>\n<color=#fef08a><size=18>{roleTitle}</size></color>";
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
                // Quay mặt về phía người chơi
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = playerTransform.position.x < transform.position.x;
                }

                // Tương tác khi bấm E (chỉ khi không có UI nào đang mở)
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (VillageDialogueUI.Instance != null && !VillageDialogueUI.Instance.IsOpen
                        && (VillageShopUI.Instance == null || !VillageShopUI.Instance.IsOpen))
                    {
                        Interact();
                    }
                }
            }

            // Hiệu ứng nhấp nhô nhẹ lúc đứng thở
            bobTimer += Time.deltaTime * 2.5f;
            float bob = Mathf.Sin(bobTimer) * 0.03f;
            transform.localScale = new Vector3(initialScale.x, initialScale.y + bob, initialScale.z);
        }

        public void Setup(string name, string role, string[] lines, Sprite portrait = null)
        {
            npcName = name;
            roleTitle = role;
            dialogueLines = lines;
            if (portrait != null) portraitSprite = portrait;
        }

        public void Interact()
        {
            if (dialogueLines == null || dialogueLines.Length == 0) return;

            if (VillageDialogueUI.Instance != null)
            {
                VillageDialogueUI.Instance.StartDialogue(npcName, roleTitle, portraitSprite, dialogueLines);
            }
            else
            {
                Debug.Log($"[VillageNPC] {npcName}: {dialogueLines[0]}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
