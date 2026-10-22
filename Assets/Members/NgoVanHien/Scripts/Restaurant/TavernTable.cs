using System.Collections.Generic;
using UnityEngine;

namespace DungeonEater.Tavern
{
    /// <summary>
    /// Bàn ăn: giữ chỗ cho 1 khách, hiển thị đĩa món lên mặt bàn khi phục vụ (README mục 5),
    /// và là điểm tương tác (phím F) để mở hòm đồ chọn món giao cho khách.
    /// Gắn lên GameObject bàn có Collider2D ở layer Interactable.
    /// </summary>
    public class TavernTable : MonoBehaviour, IInteractable
    {
        [SerializeField] private int tableNumber = 1;
        [Tooltip("Chỗ khách đứng/ngồi chờ. Để trống sẽ tự đặt phía trên bàn.")]
        [SerializeField] private Transform seatPoint;
        [Tooltip("Vị trí đặt đĩa món trên mặt bàn. Để trống sẽ dùng tâm bàn.")]
        [SerializeField] private Transform dishPoint;
        [Tooltip("Các điểm khách đi qua từ lối đi chung tới chỗ ngồi (tránh đi xuyên bàn), theo thứ tự")]
        [SerializeField] private List<Transform> approachPath = new List<Transform>();
        [SerializeField] private float dishSize = 0.7f;

        private SpriteRenderer dishRenderer;
        private SpriteRenderer tableRenderer;

        public int TableNumber => tableNumber;
        public TavernCustomer Customer { get; private set; }
        public bool IsFree => Customer == null;

        public Vector3 SeatPosition
        {
            get
            {
                if (seatPoint != null) return seatPoint.position;
                var col = GetComponent<Collider2D>();
                return col != null ? new Vector3(col.bounds.center.x, col.bounds.max.y + 0.1f, 0f) : transform.position + Vector3.up;
            }
        }

        /// <summary>Thứ tự vẽ của bàn, dùng để đặt đĩa món ngay trên mặt bàn.</summary>
        public SpriteRenderer TableRenderer => tableRenderer;

        private void Awake()
        {
            tableRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>Thêm các điểm tiếp cận bàn (từ lối đi chung tới cạnh bàn) vào path.</summary>
        public void AppendApproachPath(List<Vector3> path)
        {
            foreach (var p in approachPath)
            {
                if (p != null) path.Add(p.position);
            }
        }

        public void Reserve(TavernCustomer customer)
        {
            Customer = customer;
        }

        public void Release(TavernCustomer customer)
        {
            if (Customer == customer) Customer = null;
        }

        public void ShowDish(ItemData dish)
        {
            if (dish == null || dish.icon == null) return;
            if (dishRenderer == null)
            {
                var go = new GameObject("DishOnTable");
                go.transform.SetParent(transform, false);
                dishRenderer = go.AddComponent<SpriteRenderer>();
            }

            dishRenderer.transform.position = dishPoint != null ? dishPoint.position : transform.position;
            // bỏ scale của bàn (bàn trong map scale 4) để đĩa có kích thước world cố định
            Vector2 size = dish.icon.bounds.size;
            float worldScale = dishSize / Mathf.Max(0.01f, Mathf.Max(size.x, size.y));
            Vector3 parentScale = transform.lossyScale;
            dishRenderer.transform.localScale = new Vector3(worldScale / parentScale.x, worldScale / parentScale.y, 1f);

            dishRenderer.sprite = dish.icon;
            if (tableRenderer != null)
            {
                dishRenderer.sortingLayerID = tableRenderer.sortingLayerID;
                dishRenderer.sortingOrder = tableRenderer.sortingOrder + 1;
            }
            dishRenderer.enabled = true;
        }

        public void HideDish()
        {
            if (dishRenderer != null) dishRenderer.enabled = false;
        }

        private void LateUpdate()
        {
            // YSortOrder của bàn có thể cập nhật order mỗi frame -> giữ đĩa luôn trên mặt bàn
            if (dishRenderer != null && dishRenderer.enabled && tableRenderer != null)
            {
                dishRenderer.sortingOrder = tableRenderer.sortingOrder + 1;
            }
        }

        public void Interact(ChiefPlayer player)
        {
            var ui = TavernUI.Instance;
            if (ui == null || ui.IsModalOpen) return;

            if (Customer == null)
            {
                ui.ShowToast($"Bàn {tableNumber} đang trống.", TavernUI.ColorInfo);
                return;
            }

            switch (Customer.State)
            {
                case CustomerState.Waiting:
                    ui.OpenServe(Customer);
                    break;
                case CustomerState.Entering:
                    ui.ShowToast("Khách đang đi tới bàn, chờ khách ngồi đã.", TavernUI.ColorInfo);
                    break;
                case CustomerState.Eating:
                    ui.ShowToast($"Khách bàn {tableNumber} đang thưởng thức món ăn.", TavernUI.ColorInfo);
                    break;
            }
        }

        public string GetInteractPrompt()
        {
            if (Customer != null && Customer.State == CustomerState.Waiting) return "Phục vụ món";
            return $"Bàn {tableNumber}";
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(SeatPosition, 0.2f);
            if (dishPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(dishPoint.position, Vector3.one * dishSize);
            }
        }
    }
}
