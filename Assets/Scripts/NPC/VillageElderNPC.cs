using UnityEngine;
using DungeonEater.UI;

namespace DungeonEater.NPC
{
    public class VillageElderNPC : MonoBehaviour
    {
        [Header("NPC Info")]
        [SerializeField] private string npcName = "Trưởng Làng Dun";
        [SerializeField] private Sprite portraitSprite;

        [Header("Dialogue")]
        [SerializeField] private string[] dialogueLines = new string[]
        {
            "Chào mừng ngươi đã trở về từ hầm ngục, dũng sĩ!",
            "Ta là Trưởng làng Dun. Ngôi làng nhỏ bé này đang cần sự giúp đỡ của ngươi.",
            "Ta có một căn nhà hoang bên rìa làng... đổi lấy thức ăn, ngươi thấy sao?",
            "Hãy biến nó thành quán ăn và phục vụ dân làng! Cả ngôi làng sẽ mang ơn ngươi."
        };

        [Header("Interaction")]
        [SerializeField] private float interactRange = 2f;
        [SerializeField] private KeyCode interactKey = KeyCode.F;

        private int currentLine = 0;
        private bool isDialogueActive = false;
        private bool hasCompletedDialogue = false;
        private Animator animator;
        private Transform playerTransform;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player_HunterCombat");
                if (player == null)
                    player = GameObject.Find("Player_HunterChef");
            }
            if (player != null)
                playerTransform = player.transform;
        }

        private void Update()
        {
            if (playerTransform == null) return;

            float dist = Vector2.Distance(transform.position, playerTransform.position);

            if (dist <= interactRange)
            {
                // Face toward player
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = playerTransform.position.x < transform.position.x;
                }

                if (Input.GetKeyDown(interactKey))
                {
                    if (!isDialogueActive)
                    {
                        StartDialogue();
                    }
                    else
                    {
                        AdvanceDialogue();
                    }
                }
            }
        }

        private void StartDialogue()
        {
            isDialogueActive = true;
            currentLine = 0;

            if (animator != null)
                animator.SetBool("isTalking", true);

            ShowCurrentLine();
        }

        private void AdvanceDialogue()
        {
            currentLine++;

            if (currentLine >= dialogueLines.Length)
            {
                EndDialogue();
                return;
            }

            ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            if (DialogUI.Instance != null)
            {
                string avatar = portraitSprite != null ? "👴" : "🏘️";
                DialogUI.Instance.ShowDialog(npcName, avatar, dialogueLines[currentLine]);
            }
            else
            {
                Debug.Log($"💬 [{npcName}]: {dialogueLines[currentLine]}");
            }
        }

        private void EndDialogue()
        {
            isDialogueActive = false;
            hasCompletedDialogue = true;

            if (animator != null)
                animator.SetBool("isTalking", false);

            if (DialogUI.Instance != null)
                DialogUI.Instance.CloseDialog();

            Debug.Log($"✅ [{npcName}] Đã hoàn thành hội thoại. Mở khóa quán ăn!");
        }

        public bool HasCompletedDialogue => hasCompletedDialogue;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}

