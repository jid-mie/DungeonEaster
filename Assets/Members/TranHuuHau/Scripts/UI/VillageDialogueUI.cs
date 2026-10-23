using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DungeonEater.TranHuuHau.Common;

namespace DungeonEater.TranHuuHau.UI
{
    /// <summary>
    /// Giao diện hộp thoại hội thoại khi tương tác với NPC bằng phím [E].
    /// Tự động dựng bằng code procedural uGUI, hỗ trợ:
    /// - Khung ảnh đại diện NPC + Tên + Vai trò trong làng.
    /// - Hiệu ứng hiển thị chữ (typewriter effect).
    /// - Bấm [E], [Space], [Enter] hoặc click chuột để tua nhanh / chuyển sang câu tiếp theo.
    /// </summary>
    public class VillageDialogueUI : MonoBehaviour
    {
        public static VillageDialogueUI Instance { get; private set; }

        private RectTransform rootPanel;
        private Image portraitImage;
        private Text speakerNameText;
        private Text speakerRoleText;
        private Text contentText;
        private Text promptHintText;

        private string[] currentLines;
        private int currentLineIndex = 0;
        private bool isTyping = false;
        private string targetSentence = "";
        private Coroutine typeCoroutine;

        public bool IsOpen => rootPanel != null && rootPanel.gameObject.activeSelf;
        public event Action OnDialogueClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildUI();
            CloseDialogue();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void BuildUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var cGo = new GameObject("VillageDialogueCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = cGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = cGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                transform.SetParent(canvas.transform, false);
            }

            // Toàn bộ khung hội thoại đặt ở đáy màn hình
            rootPanel = HauUITheme.CreatePanel(transform, "DialogueRoot", HauUITheme.WoodDark, true).rectTransform;
            HauUITheme.Anchor(rootPanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(1040, 220));

            // Khung ảnh chân dung bên trái
            var portraitBox = HauUITheme.CreatePanel(rootPanel, "PortraitBox", HauUITheme.WoodDarkest, false);
            HauUITheme.Anchor(portraitBox.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(95, 0), new Vector2(150, 175));

            portraitImage = HauUITheme.CreateIcon(portraitBox.transform, "PortraitImage", null);
            HauUITheme.FillParent(portraitImage.rectTransform, 8, 8, 8, 8);

            // Cột nội dung bên phải
            float leftPad = 195f;
            float rightPad = 35f;

            // Tên người nói
            speakerNameText = HauUITheme.CreateLabel(rootPanel, "SpeakerName", "Tên NPC", 24, HauUITheme.GoldYellow, TextAnchor.MiddleLeft, FontStyle.Bold);
            HauUITheme.Anchor(speakerNameText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(leftPad, -16), new Vector2(-leftPad - rightPad, 30));

            // Vai trò trong làng
            speakerRoleText = HauUITheme.CreateLabel(rootPanel, "SpeakerRole", "Vai trò", 16, HauUITheme.TextDim, TextAnchor.MiddleLeft, FontStyle.Italic);
            HauUITheme.Anchor(speakerRoleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(leftPad, -46), new Vector2(-leftPad - rightPad, 22));

            // Đường kẻ ngăn cách mỏng
            var divider = HauUITheme.CreateBox(rootPanel, "Divider", HauUITheme.WoodBorder, false);
            HauUITheme.Anchor(divider.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(leftPad, -72), new Vector2(-leftPad - rightPad, 2));

            // Nội dung hội thoại - ĐƯỢC CĂN CHỈNH NẰM GỌN GÀNG 100% BÊN TRONG KHUNG HỘI THOẠI
            contentText = HauUITheme.CreateLabel(rootPanel, "ContentText", "Lời thoại...", 20, HauUITheme.TextCream, TextAnchor.UpperLeft);
            contentText.rectTransform.anchorMin = new Vector2(0f, 0f);
            contentText.rectTransform.anchorMax = new Vector2(1f, 1f);
            contentText.rectTransform.pivot = new Vector2(0f, 1f);
            contentText.rectTransform.offsetMin = new Vector2(leftPad, 42f);   // Cách đáy 42px để không chạm vào promptHint
            contentText.rectTransform.offsetMax = new Vector2(-rightPad, -82f); // Cách đỉnh 82px nằm ngay dưới divider
            contentText.lineSpacing = 1.15f;

            // Gợi ý bấm phím góc dưới phải
            promptHintText = HauUITheme.CreateLabel(rootPanel, "PromptHint", "[E] / [Space] Tiếp tục ▶", 16, HauUITheme.GoldYellow, TextAnchor.MiddleRight, FontStyle.Bold);
            HauUITheme.Anchor(promptHintText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-rightPad, 15), new Vector2(250, 24));

            // Nút bấm vô hình toàn màn hình để click chuột tiếp tục
            var clickOverlay = HauUITheme.CreateBox(rootPanel, "ClickOverlay", new Color(0, 0, 0, 0), true);
            HauUITheme.FillParent(clickOverlay.rectTransform);
            var btn = clickOverlay.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(OnInteractTriggered);
        }

        private void Update()
        {
            if (!IsOpen) return;

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                OnInteractTriggered();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseDialogue();
            }
        }

        private void OnInteractTriggered()
        {
            if (isTyping)
            {
                // Bấm lần nữa để hoàn thành ngay câu thoại đang gõ
                if (typeCoroutine != null) StopCoroutine(typeCoroutine);
                contentText.text = targetSentence;
                isTyping = false;
                UpdatePromptHint();
            }
            else
            {
                // Chuyển sang câu tiếp theo
                currentLineIndex++;
                if (currentLineIndex < currentLines.Length)
                {
                    ShowLine(currentLineIndex);
                }
                else
                {
                    CloseDialogue();
                }
            }
        }

        public void StartDialogue(string speakerName, string speakerRole, Sprite portrait, string[] lines)
        {
            if (lines == null || lines.Length == 0) return;

            currentLines = lines;
            currentLineIndex = 0;

            speakerNameText.text = speakerName;
            speakerRoleText.text = speakerRole;

            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.sprite = HauUITheme.White;
                portraitImage.enabled = false;
            }

            rootPanel.gameObject.SetActive(true);
            ShowLine(0);
        }

        private void ShowLine(int index)
        {
            targetSentence = currentLines[index];
            if (typeCoroutine != null) StopCoroutine(typeCoroutine);
            typeCoroutine = StartCoroutine(TypeSentence(targetSentence));
            UpdatePromptHint();
        }

        private void UpdatePromptHint()
        {
            bool isLast = currentLineIndex >= currentLines.Length - 1;
            promptHintText.text = isLast ? "[E] Đóng cuộc trò chuyện ✕" : "[E] Tiếp tục ▶";
        }

        private IEnumerator TypeSentence(string sentence)
        {
            isTyping = true;
            contentText.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                contentText.text += letter;
                yield return new WaitForSeconds(0.018f);
            }
            isTyping = false;
        }

        public void CloseDialogue()
        {
            if (typeCoroutine != null) StopCoroutine(typeCoroutine);
            isTyping = false;
            if (rootPanel != null) rootPanel.gameObject.SetActive(false);
            OnDialogueClosed?.Invoke();
        }
    }
}
