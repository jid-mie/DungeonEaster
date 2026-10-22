using UnityEngine;
using UnityEngine.UI;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Nhắc "[F] ..." phía trên vật tương tác mà ChiefPlayer đang hướng tới
    /// (cùng vùng quét với ChiefPlayer.TryInteract).
    /// </summary>
    public class InteractPrompt : MonoBehaviour
    {
        [SerializeField] private float checkRadius = 0.35f;

        private TavernUI owner;
        private RectTransform box;
        private Text label;
        private int interactableMask;

        public void Build(TavernUI ui)
        {
            owner = ui;
            interactableMask = LayerMask.GetMask("Interactable");

            var bg = UIKit.Panel(transform, "Prompt", new Color(0.16f, 0.1f, 0.06f, 0.9f));
            box = bg.rectTransform;
            box.anchorMin = box.anchorMax = Vector2.zero;
            box.pivot = new Vector2(0.5f, 0f);
            box.sizeDelta = new Vector2(200f, 44f);
            label = UIKit.Label(box, "Text", "", 22, UIKit.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIKit.Stretch(label.rectTransform, 10, 2, 10, 2);
            box.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            var player = owner.Player;
            var cam = Camera.main;
            if (player == null || cam == null || owner.IsModalOpen)
            {
                box.gameObject.SetActive(false);
                return;
            }

            var hit = Physics2D.OverlapCircle(player.GetInteractionCheckPosition(), checkRadius, interactableMask);
            var interactable = hit != null ? hit.GetComponent<IInteractable>() : null;
            if (interactable == null)
            {
                box.gameObject.SetActive(false);
                return;
            }

            label.text = $"<color=#{UIKit.Hex(UIKit.Highlight)}>[F]</color> {interactable.GetInteractPrompt()}";
            box.sizeDelta = new Vector2(label.preferredWidth + 28f, 44f);

            Vector3 world = new Vector3(hit.bounds.center.x, hit.bounds.max.y + 0.2f, 0f);
            Vector3 screen = cam.WorldToScreenPoint(world);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screen, null, out Vector2 local);
            // local tính từ tâm -> đổi sang hệ anchor góc trái dưới
            var parentRect = ((RectTransform)transform).rect;
            box.anchoredPosition = local - parentRect.min;
            box.gameObject.SetActive(true);
        }
    }
}
