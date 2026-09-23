using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.UI
{
    public class DialogUI : MonoBehaviour
    {
        public static DialogUI Instance { get; private set; }

        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text contentText;
        [SerializeField] private Text avatarText;

        private void Awake()
        {
            Instance = this;
        }

        public void ShowDialog(string speaker, string avatar, string message)
        {
            if (dialogPanel != null) dialogPanel.SetActive(true);
            if (speakerText != null) speakerText.text = speaker;
            if (avatarText != null) avatarText.text = avatar;
            if (contentText != null) contentText.text = message;
        }

        public void CloseDialog()
        {
            if (dialogPanel != null) dialogPanel.SetActive(false);
        }

        private void Update()
        {
            if (dialogPanel != null && dialogPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
                {
                    CloseDialog();
                }
            }
        }
    }
}

