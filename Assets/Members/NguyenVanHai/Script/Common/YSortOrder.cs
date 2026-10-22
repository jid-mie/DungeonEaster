using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// YSortOrder - Tự động sắp xếp độ sâu hiển thị (Sorting Order) của Sprite theo trục Y (Y-Sorting).
/// Giải quyết triệt để bài toán:
/// - Player đi ra trước đồ vật (Y thấp hơn) -> Player che đồ vật.
/// - Player đi ra sau đồ vật (Y cao hơn) -> Đồ vật che Player.
/// Áp dụng cho cả vật thể động (Player, NPC, Quái) và vật thể tĩnh (Bàn, Bếp, Tủ, Cây cối).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[ExecuteAlways]
public class YSortOrder : MonoBehaviour
{
    [Header("Sorting Configuration")]
    [Tooltip("Mức Sorting Order cơ sở để đảm bảo kết quả luôn là số dương, lớn hơn sàn nhà (Ground layer order = 0), tránh bị lặn mất khi Y > 0")]
    [SerializeField] private int baseSortingOrder = 5000;

    [Tooltip("Hệ số nhân độ chính xác tọa độ Y (Mặc định 100 nghĩa là mỗi 0.01 đơn vị Y là 1 bậc Sorting Order)")]
    [SerializeField] private int precision = 100;

    [Tooltip("Độ lệch bù trừ Sorting Order thủ công nếu cần ưu tiên lớp hiển thị")]
    [SerializeField] private int sortingOrderOffset = 0;

    [Tooltip("Điểm tiếp đất thực tế (Offset theo trục Y so với Transform position). Nên căn chỉnh đúng chân nhân vật hoặc chân bàn.")]
    [SerializeField] private float yOffset = 0f;

    [Tooltip("Nếu vật thể này nằm trên vật thể khác (ví dụ: Cốc/Đĩa nằm trên Bàn), kéo vật thể đỡ vào đây để dùng chung tọa độ tiếp đất của vật đỡ")]
    [SerializeField] private Transform sortTarget;

    [Tooltip("Tích chọn nếu là vật thể tĩnh (Bàn ghế, Bếp nấu, Giường...) để script chỉ tính toán 1 lần lúc Start(), tối ưu tuyệt đối hiệu năng CPU")]
    [SerializeField] private bool isStatic = false;

    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// Giá trị Sorting Order hiện tại của đối tượng
    /// </summary>
    public int CurrentSortingOrder => spriteRenderer != null ? spriteRenderer.sortingOrder : baseSortingOrder;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSortingOrder();
    }

    private void Start()
    {
        UpdateSortingOrder();

        // Nếu trong Play Mode và là vật thể tĩnh, tắt Update để không tiêu tốn CPU mỗi frame
        if (Application.isPlaying && isStatic)
        {
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (!isStatic || !Application.isPlaying)
        {
            UpdateSortingOrder();
        }
    }

    /// <summary>
    /// Tính toán và gán Sorting Order theo công thức: -(Y + yOffset) * precision + offset
    /// </summary>
    public void UpdateSortingOrder()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
        }

        float baseY = (sortTarget != null ? sortTarget.position.y : transform.position.y);
        float actualFootY = baseY + yOffset;
        spriteRenderer.sortingOrder = baseSortingOrder + Mathf.RoundToInt(-actualFootY * precision) + sortingOrderOffset;
    }

    /// <summary>
    /// Vẽ đường chỉ dẫn vị trí tiếp đất màu tím (Magenta) trong Scene view để dễ căn chỉnh chân bàn/chân nhân vật.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector3 basePos = sortTarget != null ? sortTarget.position : transform.position;
        Vector3 footPosition = basePos + new Vector3(0, yOffset, 0);
        Gizmos.DrawLine(footPosition + Vector3.left * 0.4f, footPosition + Vector3.right * 0.4f);
        Gizmos.DrawWireCube(footPosition, new Vector3(0.1f, 0.05f, 0));
    }
}
