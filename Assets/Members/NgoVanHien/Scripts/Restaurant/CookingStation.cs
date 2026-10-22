using UnityEngine;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Quầy nấu ăn (bếp lò). Người chơi đứng trước quầy bấm F -> mở UI chọn món.
    /// Gắn lên GameObject bếp có Collider2D ở layer Interactable.
    /// </summary>
    public class CookingStation : MonoBehaviour, IInteractable
    {
        public void Interact(ChiefPlayer player)
        {
            var ui = TavernUI.Instance;
            if (ui == null || ui.IsModalOpen) return;
            ui.OpenCooking();
        }

        public string GetInteractPrompt()
        {
            return "Nấu ăn";
        }
    }
}
