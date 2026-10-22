using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface dành cho các đối tượng có thể tương tác trong game (Bàn ăn, Bếp nấu, Quầy thu ngân, NPC...).
/// Tuân thủ đặc tả phím F (Interact/Pickup) trong Dungeon Eater.md.
/// </summary>
public interface IInteractable
{
    void Interact(ChiefPlayer player);
    string GetInteractPrompt();
}

/// <summary>
/// ChiefPlayer - Script điều khiển nhân vật Đầu bếp (Chef) trong pha Nhà hàng / Làng (Restaurant / Village Phase).
/// Chức năng chính:
/// 1. Nhận input 8 hướng (WASD / Mũi tên), chuẩn hóa vector di chuyển và xử lý vật lý qua Rigidbody2D.
/// 2. Điều khiển Animator Blend Tree (MoveX, MoveY, Speed, IsMoving) và lưu hướng nhìn (Facing Direction).
/// 3. Hệ thống tương tác (Phím F - Interact) với các đối tượng trước mặt (Bàn ăn, Bếp, Quầy thu ngân).
/// 4. Hỗ trợ khóa di chuyển (CanMove) khi đang thực hiện nấu nướng hoặc mở giao diện.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ChiefPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của nhân vật Đầu bếp")]
    [SerializeField] private float moveSpeed = 4.5f;

    [Header("Interaction Settings")]
    [Tooltip("Khoảng cách tương tác phía trước mặt nhân vật")]
    [SerializeField] private float interactionDistance = 0.55f;
    [Tooltip("Hệ số khoảng cách khi nhìn xuống dưới (Down) giúp vòng tương tác gần chân hơn, phù hợp phối cảnh 2D Topdown")]
    [SerializeField] private float downDistanceMultiplier = 0.75f;
    [Tooltip("Độ lệch tâm quét tương tác so với vị trí nhân vật (mặc định Vector2.zero)")]
    [SerializeField] private Vector2 interactionOriginOffset = Vector2.zero;
    [Tooltip("Bán kính vùng quét tương tác")]
    [SerializeField] private float interactionRadius = 0.35f;
    [Tooltip("LayerMask chứa các vật thể tương tác (Table, Stove, CashTable, Storage...)")]
    [SerializeField] private LayerMask interactableLayer;
    [Tooltip("Phím tương tác chính theo đặc tả Dungeon Eater.md")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Header("Animator Parameter Names")]
    [SerializeField] private string moveXParamName = "MoveX";
    [SerializeField] private string moveYParamName = "MoveY";
    [SerializeField] private string isMovingParamName = "IsMoving";
    [SerializeField] private string speedParamName = "Speed";
    [SerializeField] private string lastMoveXParamName = "LastMoveX";
    [SerializeField] private string lastMoveYParamName = "LastMoveY";

    // Component references
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Movement & Facing state
    private Vector2 moveInput;
    private Vector2 lastFacingDirection = Vector2.down; // Mặc định quay mặt xuống dưới (Down)
    private bool isMoving;
    private bool canMove = true;

    // Cached Animator Parameter Hashes
    private int hashMoveX;
    private int hashMoveY;
    private int hashIsMoving;
    private int hashSpeed;
    private int hashLastMoveX;
    private int hashLastMoveY;

    // Flags kiểm tra tham số tồn tại trong Animator (Tránh lỗi/warning nếu Controller chưa thêm tham số)
    private bool hasParamMoveX;
    private bool hasParamMoveY;
    private bool hasParamIsMoving;
    private bool hasParamSpeed;
    private bool hasParamLastMoveX;
    private bool hasParamLastMoveY;

    // Properties công khai để các hệ thống khác (UI, Cooking minigame, Restaurant Manager) truy xuất
    public Vector2 FacingDirection => lastFacingDirection;
    public bool IsMoving => isMoving;
    public bool CanMove
    {
        get => canMove;
        set
        {
            canMove = value;
            if (!canMove && rb != null)
            {
                rb.velocity = Vector2.zero;
                UpdateAnimator(Vector2.zero, 0f);
            }
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Cấu hình tối ưu Rigidbody2D cho thể loại Top-down 2D
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        InitializeAnimatorHashes();
    }

    private void Start()
    {
        // Khởi tạo trạng thái ban đầu của Animator
        UpdateAnimator(Vector2.zero, 0f);
    }

    private void Update()
    {
        HandleInput();
        HandleInteraction();
    }

    private void FixedUpdate()
    {
        MoveCharacter();
    }

    /// <summary>
    /// Đọc và chuẩn hóa input người chơi (WASD / Mũi tên).
    /// </summary>
    private void HandleInput()
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            return;
        }

        // Đọc raw input để phản hồi chuyển động lập tức (Pixel Art snappy feel)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(horizontal, vertical);

        // Chuẩn hóa vector nếu đi chéo để tránh đi nhanh hơn (1.414x)
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        // Cập nhật hướng nhìn cuối cùng khi có di chuyển
        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastFacingDirection = moveInput.normalized;
        }

        float currentSpeed = moveInput.magnitude * moveSpeed;
        UpdateAnimator(moveInput, currentSpeed);
    }

    /// <summary>
    /// Di chuyển nhân vật thông qua Rigidbody2D trong FixedUpdate để đảm bảo va chạm vật lý chính xác.
    /// </summary>
    private void MoveCharacter()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = moveInput * moveSpeed;
    }

    /// <summary>
    /// Cập nhật các tham số của Animator Controller một cách an toàn và tối ưu hiệu năng.
    /// </summary>
    private void UpdateAnimator(Vector2 direction, float speed)
    {
        if (animator == null) return;

        isMoving = direction.sqrMagnitude > 0.01f;

        // Cập nhật MoveX, MoveY (cho 2D Simple Directional Blend Tree)
        if (hasParamMoveX)
        {
            animator.SetFloat(hashMoveX, isMoving ? direction.x : lastFacingDirection.x);
        }

        if (hasParamMoveY)
        {
            animator.SetFloat(hashMoveY, isMoving ? direction.y : lastFacingDirection.y);
        }

        // Cập nhật IsMoving & Speed nếu Animator Controller có khai báo
        if (hasParamIsMoving)
        {
            animator.SetBool(hashIsMoving, isMoving);
        }

        if (hasParamSpeed)
        {
            animator.SetFloat(hashSpeed, speed);
        }

        // Cập nhật LastMoveX, LastMoveY (dành cho Idle Blend Tree 4 hướng nếu mở rộng)
        if (hasParamLastMoveX)
        {
            animator.SetFloat(hashLastMoveX, lastFacingDirection.x);
        }

        if (hasParamLastMoveY)
        {
            animator.SetFloat(hashLastMoveY, lastFacingDirection.y);
        }
    }

    /// <summary>
    /// Xử lý nhấn phím tương tác F theo đặc tả Dungeon Eater.md.
    /// </summary>
    private void HandleInteraction()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    /// <summary>
    /// Tính toán vị trí tâm vùng quét tương tác phía trước mặt nhân vật,
    /// tự động điều chỉnh khoảng cách khi nhìn xuống (Down) để phù hợp phối cảnh 2D.
    /// </summary>
    public Vector2 GetInteractionCheckPosition()
    {
        float currentDistance = interactionDistance;
        // Nếu nhân vật đang nhìn hướng xuống (Down), giảm khoảng cách lại gần hơn
        if (lastFacingDirection.y < -0.1f)
        {
            currentDistance *= downDistanceMultiplier;
        }

        Vector2 origin = (Vector2)transform.position + interactionOriginOffset;
        return origin + (lastFacingDirection * currentDistance);
    }

    /// <summary>
    /// Quét các collider trong vùng tương tác theo hướng mặt hiện tại.
    /// </summary>
    public void TryInteract()
    {
        Vector2 checkPosition = GetInteractionCheckPosition();

        Collider2D hit = Physics2D.OverlapCircle(checkPosition, interactionRadius, interactableLayer);
        if (hit != null)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
                return;
            }

            // Fallback gửi message nếu đối tượng dùng MonoBehaviour thông thường
            hit.SendMessage("OnInteract", this, SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// Cache Animator parameter hashes và kiểm tra tham số tồn tại để tránh warning log trên Unity Console.
    /// </summary>
    private void InitializeAnimatorHashes()
    {
        if (animator == null) return;

        hashMoveX = Animator.StringToHash(moveXParamName);
        hashMoveY = Animator.StringToHash(moveYParamName);
        hashIsMoving = Animator.StringToHash(isMovingParamName);
        hashSpeed = Animator.StringToHash(speedParamName);
        hashLastMoveX = Animator.StringToHash(lastMoveXParamName);
        hashLastMoveY = Animator.StringToHash(lastMoveYParamName);

        // Kiểm tra xem các parameter có tồn tại trong Controller không
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == moveXParamName) hasParamMoveX = true;
            else if (param.name == moveYParamName) hasParamMoveY = true;
            else if (param.name == isMovingParamName) hasParamIsMoving = true;
            else if (param.name == speedParamName) hasParamSpeed = true;
            else if (param.name == lastMoveXParamName) hasParamLastMoveX = true;
            else if (param.name == lastMoveYParamName) hasParamLastMoveY = true;
        }
    }

    /// <summary>
    /// Hiển thị trực quan vùng quét tương tác và hướng nhìn của nhân vật trong Scene view.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector2 checkPos = GetInteractionCheckPosition();
        Vector2 origin = (Vector2)transform.position + interactionOriginOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(checkPos, interactionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, checkPos);
    }
}
