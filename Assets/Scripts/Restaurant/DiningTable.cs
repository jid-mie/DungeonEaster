using UnityEngine;

namespace DungeonEater.Restaurant
{
    public class DiningTable : MonoBehaviour
    {
        [SerializeField] private int tableId = 1;
        [SerializeField] private Transform customerSeatPoint;
        [SerializeField] private SpriteRenderer dishRenderer;
        [Tooltip("Vị trí khách ngồi (ghế băng cạnh bàn) khi không gán customerSeatPoint")]
        [SerializeField] private Vector3 seatOffset = new Vector3(-0.78f, 0.45f, 0f);

        [Header("Dish Sprites")]
        [SerializeField] private Sprite skewerPlate;
        [SerializeField] private Sprite soupPlate;
        [SerializeField] private Sprite steakPlate;
        [SerializeField] private Sprite feastPlate;

        public CustomerAI CurrentCustomer { get; private set; }
        public bool IsOccupied => CurrentCustomer != null;
        public int TableId => tableId;

        public void AssignCustomer(CustomerAI customer)
        {
            CurrentCustomer = customer;
            customer.transform.position = customerSeatPoint != null ? customerSeatPoint.position : transform.position + seatOffset;
        }

        public void ClearTable()
        {
            CurrentCustomer = null;
            if (dishRenderer != null) dishRenderer.enabled = false;
        }

        public void PlaceDish(string dishId)
        {
            if (dishRenderer != null)
            {
                dishRenderer.enabled = true;
                dishRenderer.color = Color.white;

                // Ưu tiên ảnh đĩa đã cắt sạch chữ dùng chung với màn Bếp
                var cleanPlate = DungeonEater.UI.KitchenUIKit.DishSprite(dishId.ToLower());
                if (cleanPlate != null)
                {
                    dishRenderer.sprite = cleanPlate;
                    return;
                }

                switch (dishId.ToLower())
                {
                    case "skewer":
                        if (skewerPlate != null) dishRenderer.sprite = skewerPlate;
                        break;
                    case "soup":
                        if (soupPlate != null) dishRenderer.sprite = soupPlate;
                        break;
                    case "steak":
                        if (steakPlate != null) dishRenderer.sprite = steakPlate;
                        break;
                    case "feast":
                        if (feastPlate != null) dishRenderer.sprite = feastPlate;
                        break;
                }
            }
        }
    }
}

