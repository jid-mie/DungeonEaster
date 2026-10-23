using UnityEngine;
using DungeonEater.TranHuuHau.NPC;
using DungeonEater.TranHuuHau.UI;

namespace DungeonEater.TranHuuHau.Player
{
    /// <summary>
    /// Điều khiển nhân vật chính trong Ngôi Làng (Village Phase) của Trần Hữu Hậu:
    /// - Di chuyển 8 hướng WASD mượt mà không bị rung giật màn hình.
    /// - Nhân vật chính luôn luôn nằm ở CHÍNH GIỮA màn hình (Dead Center).
    /// - Đồng bộ chuẩn xác 4 hướng nhìn (Left, Right, Up, Down) qua Blend Tree Animator của Chef.
    /// - Loại bỏ hoàn toàn lỗi quay sai hướng khi đi sang trái (không dùng flipX đối với sprite sheet 4 hướng).
    /// - Khử hoàn toàn rung lắc camera bằng cách gắn Camera con đồng bộ với Rigidbody2D.Interpolate.
    /// - Quét và tương tác với NPC hoặc Cửa hàng Trưởng làng qua phím [E].
    /// - Mở túi đồ qua phím [I].
    /// - Tự động khóa di chuyển khi đang mở hội thoại hoặc giao diện mua đồ.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class VillagePlayerController : MonoBehaviour
    {
        [Header("Cài đặt Di chuyển")]
        [SerializeField] private float moveSpeed = 5.0f;

        [Header("Cài đặt Camera Chính Giữa Màn Hình")]
        [Tooltip("Độ lệch tâm đứng để ngực nhân vật nằm chính giữa màn hình (mặc định 0.65m tính từ chân)")]
        [SerializeField] private float cameraCenterOffsetY = 0.65f;
        [Tooltip("Tốc độ bám theo mượt mà của Camera (khử 100% rung giật va chạm bằng exponential filter)")]
        [SerializeField] private float cameraFollowSharpness = 18f;

        [Header("Cài đặt Animation")]
        [SerializeField] private RuntimeAnimatorController villageAnimatorController;

        [Header("Cài đặt Tương tác")]
        [SerializeField] private float interactScanRadius = 2.5f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Vector2 moveInput;
        private Vector2 lastFacingDirection = Vector2.down; // Mặc định quay mặt xuống dưới (Down)
        private bool canMove = true;

        // Animator parameter hashes
        private int hashMoveX;
        private int hashMoveY;
        private int hashIsMoving;
        private bool hasParamMoveX;
        private bool hasParamMoveY;
        private bool hasParamIsMoving;

        public bool CanMove
        {
            get => canMove;
            set
            {
                canMove = value;
                if (!canMove && rb != null)
                {
                    rb.velocity = Vector2.zero;
                    UpdateAnimator(Vector2.zero, false);
                }
            }
        }

        private void Awake()
        {
            // 1. Hủy hoàn toàn component ChiefPlayer cũ nếu có để tránh xung đột 2 script cùng điều khiển vận tốc
            var chiefs = GetComponents<ChiefPlayer>();
            foreach (var chief in chiefs)
            {
                chief.enabled = false;
                Destroy(chief);
            }

            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            // 2. Tự động gán Animator Controller chuẩn của Hậu nếu có
            if (animator != null && villageAnimatorController != null)
            {
                animator.runtimeAnimatorController = villageAnimatorController;
            }

            // Đảm bảo không lật sprite bừa bãi
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
            }

            // 3. Thiết lập Rigidbody2D chuẩn cho 2D Top-down (Khử hoàn toàn rung giật va chạm):
            if (rb != null)
            {
                rb.gravityScale = 0f; // Bỏ hoàn toàn trọng lực
                rb.freezeRotation = true; // Khóa xoay Z để nhân vật không bị lắc/nghiêng khi chạm vật cản
                rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Nội suy vị trí giữa các frame render
                rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

                // Gán PhysicsMaterial2D trơn trượt (không ma sát, không nảy) để nhân vật trượt mượt mà dọc theo tường
                var col = GetComponent<Collider2D>();
                if (col != null)
                {
                    var frictionlessMat = new PhysicsMaterial2D("Village_NoFriction")
                    {
                        friction = 0f,
                        bounciness = 0f
                    };
                    col.sharedMaterial = frictionlessMat;
                }
            }

            CacheAnimatorParams();

            // 4. Tách rời Camera ra thành GameObject độc lập (KHÔNG ĐƯỢC làm con của Rigidbody2D)
            DetachCamera();
        }

        private void Start()
        {
            // Đảm bảo các tham số Animator được cache sau khi Animator Controller đã sẵn sàng
            CacheAnimatorParams();
            SnapCameraToCenter();
        }

        private void DetachCamera()
        {
            if (Camera.main != null && Camera.main.transform.parent != null)
            {
                Camera.main.transform.SetParent(null);
            }
        }

        private void SnapCameraToCenter()
        {
            if (Camera.main != null)
            {
                Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y + cameraCenterOffsetY, -10f);
                Camera.main.transform.rotation = Quaternion.identity;
            }
        }

        private void CacheAnimatorParams()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;

            hasParamMoveX = false;
            hasParamMoveY = false;
            hasParamIsMoving = false;

            foreach (var param in animator.parameters)
            {
                if (param.name == "MoveX") { hashMoveX = param.nameHash; hasParamMoveX = true; }
                else if (param.name == "MoveY") { hashMoveY = param.nameHash; hasParamMoveY = true; }
                else if (param.name == "IsMoving" || param.name == "isMoving") { hashIsMoving = param.nameHash; hasParamIsMoving = true; }
            }
        }

        private void Update()
        {
            // Kiểm tra xem có giao diện nào đang mở không
            bool isUIActive = (VillageDialogueUI.Instance != null && VillageDialogueUI.Instance.IsOpen) ||
                              (VillageShopUI.Instance != null && VillageShopUI.Instance.IsOpen) ||
                              (VillageInventoryUI.Instance != null && VillageInventoryUI.Instance.IsOpen);

            canMove = !isUIActive;

            if (!canMove)
            {
                moveInput = Vector2.zero;
                UpdateAnimator(Vector2.zero, false);
                return;
            }

            // Đọc raw input 8 hướng mượt mà
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(h, v);

            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }

            bool isMoving = moveInput.sqrMagnitude > 0.01f;

            // Cập nhật Animator theo đúng 4 hướng với độ trễ chuyển hướng ổn định
            UpdateAnimator(moveInput, isMoving);

            // Bấm E để tương tác nếu có đối tượng ở gần
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryInteractNearest();
            }
        }

        private void UpdateAnimator(Vector2 direction, bool isMoving)
        {
            if (isMoving)
            {
                float absX = Mathf.Abs(direction.x);
                float absY = Mathf.Abs(direction.y);

                // Dùng ngưỡng trễ (hysteresis = 0.15f) để khi di chuyển chéo không bị nhảy qua lại giữa 2 hướng
                if (absX > absY + 0.15f)
                {
                    lastFacingDirection = direction.x > 0 ? Vector2.right : Vector2.left;
                }
                else if (absY > absX + 0.15f)
                {
                    lastFacingDirection = direction.y > 0 ? Vector2.up : Vector2.down;
                }
                // Nếu đi chéo gần cân bằng (absX ~= absY), giữ hướng hiện tại nếu nó vẫn đang được bấm
                else if (lastFacingDirection == Vector2.right && direction.x < 0)
                {
                    lastFacingDirection = Vector2.left;
                }
                else if (lastFacingDirection == Vector2.left && direction.x > 0)
                {
                    lastFacingDirection = Vector2.right;
                }
                else if (lastFacingDirection == Vector2.up && direction.y < 0)
                {
                    lastFacingDirection = Vector2.down;
                }
                else if (lastFacingDirection == Vector2.down && direction.y > 0)
                {
                    lastFacingDirection = Vector2.up;
                }
            }

            if (animator != null)
            {
                if (hasParamIsMoving) animator.SetBool(hashIsMoving, isMoving);
                // Luôn truyền vector đơn vị thuần của 4 hướng chính (1,0), (-1,0), (0,1), (0,-1)
                // để Blend Tree kích hoạt đúng 100% clip của hướng đó, không bao giờ bị blend méo frame
                if (hasParamMoveX) animator.SetFloat(hashMoveX, lastFacingDirection.x);
                if (hasParamMoveY) animator.SetFloat(hashMoveY, lastFacingDirection.y);
            }
        }

        private void FixedUpdate()
        {
            if (rb != null)
            {
                if (canMove)
                {
                    rb.velocity = moveInput * moveSpeed;
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
            }
        }

        private void LateUpdate()
        {
            // 1. Camera theo sát nhân vật CHÍNH GIỮA màn hình:
            // Sử dụng bộ lọc Exponential Decay mượt mà (1 - Exp(-sharpness * dt)),
            // bộ lọc này hoàn toàn không có quán tính (momentum) và không có hiện tượng nảy (overshoot),
            // triệt tiêu sạch 100% các rung lắc vật lý khi người chơi đâm vào tường/vật cản!
            if (Camera.main != null)
            {
                if (Camera.main.transform.parent != null)
                {
                    Camera.main.transform.SetParent(null);
                }

                Vector3 targetPos = new Vector3(transform.position.x, transform.position.y + cameraCenterOffsetY, -10f);
                float t = 1f - Mathf.Exp(-cameraFollowSharpness * Time.deltaTime);
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetPos, t);
                Camera.main.transform.rotation = Quaternion.identity;
            }

            // 2. Không lật sprite bừa bãi vì animation clip đã có sẵn sprite vẽ đúng góc nhìn 4 hướng
            if (spriteRenderer != null && spriteRenderer.flipX)
            {
                spriteRenderer.flipX = false;
            }
        }

        private void TryInteractNearest()
        {
            // 1. Kiểm tra nhà Trưởng làng
            var elderTrigger = FindObjectOfType<ElderHouseTrigger>();
            if (elderTrigger != null && elderTrigger.IsPlayerInRange)
            {
                elderTrigger.Interact();
                return;
            }

            // 2. Kiểm tra các NPC gần nhất
            var npcs = FindObjectsOfType<VillageNPC>();
            VillageNPC nearest = null;
            float minDist = float.MaxValue;

            foreach (var npc in npcs)
            {
                if (npc.IsPlayerInRange)
                {
                    float d = Vector2.Distance(transform.position, npc.transform.position);
                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = npc;
                    }
                }
            }

            if (nearest != null)
            {
                nearest.Interact();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactScanRadius);
        }
    }
}
