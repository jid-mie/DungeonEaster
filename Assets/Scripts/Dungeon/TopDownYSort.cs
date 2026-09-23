using UnityEngine;

namespace DungeonEater.Dungeon
{
    /// <summary>
    /// Tự động cập nhật sortingOrder theo trục Y để tạo chiều sâu không gian 2.5D:
    /// Nhân vật / quái vật đứng ở vị trí thấp hơn (gần màn hình hơn) sẽ hiển thị đè lên đối tượng phía sau.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TopDownYSort : MonoBehaviour
    {
        [SerializeField] private float yOffset = 0f;
        [SerializeField] private bool isStatic = false;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateSorting();
        }

        private void LateUpdate()
        {
            if (!isStatic)
            {
                UpdateSorting();
            }
        }

        public void UpdateSorting()
        {
            if (spriteRenderer != null)
            {
                // Baseline tại chân nhân vật: Y càng nhỏ -> Order càng lớn (vẽ đè lên)
                spriteRenderer.sortingOrder = Mathf.RoundToInt((-(transform.position.y + yOffset)) * 100) + 1000;
            }
        }
    }
}

