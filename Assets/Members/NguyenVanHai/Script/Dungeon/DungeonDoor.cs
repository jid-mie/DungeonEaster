using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NguyenVanHai.Dungeon
{
    /// <summary>
    /// Điều khiển cửa ngục trong Dungeon với 4 trạng thái rõ ràng:
    /// - Closed: Đóng bình thường, người chơi lại gần nhấn [F] để mở.
    /// - Open: Đã mở (tắt Collider2D chặn đường và ẩn sprite cửa, giữ GameObject active để có thể phong ấn lại khi vào phòng quái/Boss).
    /// - LockedByCondition: Bị khóa do chưa đủ điều kiện (VD: chưa nhặt đủ Key Tầng 1/2 để mở phòng Boss).
    /// - SealedInCombat: Bị phong ấn trong phòng chiến đấu (khi đã bước vào phòng quái/Boss, đóng chặt lại cho tới khi diệt hết).
    /// </summary>
    public class DungeonDoor : MonoBehaviour
    {
        public enum DoorState
        {
            Closed,
            Open,
            LockedByCondition,
            SealedInCombat
        }

        [Header("Khoảng cách & Phím tương tác")]
        [SerializeField] private float interactRadius = 1.4f;
        [SerializeField] private KeyCode interactKey = KeyCode.F;
        [Tooltip("Yêu cầu người chơi quay mặt về phía cửa khi nhấn F")]
        [SerializeField] private bool requireFacingDoor = true;

        [Header("Khóa phòng Boss (Tầng 1 & Tầng 2)")]
        [Tooltip("Tick TRUE cho cửa phòng Boss Tầng 1 & Tầng 2. Để FALSE cho mọi cửa phòng thường và cửa Boss Tầng 4.")]
        [SerializeField] private bool lockedUntilUnlocked = false;
        [SerializeField] private string lockedPromptMessage = "Cần đủ Chìa Khóa để mở!";
        [SerializeField] private string sealedPromptMessage = "Diệt hết quái để mở cửa!";

        [Header("Sprites & Hình ảnh")]
        [SerializeField] private Sprite[] openFrames;
        [SerializeField] private float frameRate = 0.08f;
        [Tooltip("Ẩn hình cánh cửa khi mở xong để giải phóng lối đi 100%")]
        [SerializeField] private bool hideSpriteWhenOpen = true;
        [SerializeField] private Color lockedTintColor = new Color(1f, 0.82f, 0.35f, 1f);
        [SerializeField] private Color sealedTintColor = new Color(1f, 0.32f, 0.32f, 1f);

        private HunterPlayer hunter;
        private SpriteRenderer sr;
        private Sprite closedSprite;
        private Color originalColor = Color.white;
        private Vector3 originalLocalPos;
        private readonly List<Collider2D> solidColliders = new List<Collider2D>();

        private DoorState currentState = DoorState.Closed;
        private DoorState stateBeforeSeal = DoorState.Closed;
        private bool isOpeningAnimPlaying = false;
        private bool isPlayerNearby = false;
        private Coroutine openRoutine;
        private Coroutine shakeRoutine;

        public DoorState CurrentState => currentState;
        public bool IsOpen => currentState == DoorState.Open;
        public bool IsSealed => currentState == DoorState.SealedInCombat;
        public bool IsConditionLocked => currentState == DoorState.LockedByCondition;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                closedSprite = sr.sprite;
                originalColor = sr.color;
            }

            originalLocalPos = transform.localPosition;

            // Thu thập toàn bộ Collider2D chặn đường (non-trigger) trên cửa và con của nó
            var cols = GetComponentsInChildren<Collider2D>(true);
            foreach (var c in cols)
            {
                if (c != null && !c.isTrigger)
                {
                    solidColliders.Add(c);
                }
            }

            if (lockedUntilUnlocked)
            {
                currentState = DoorState.LockedByCondition;
                ApplyVisualAndColliderForCurrentState();
            }
            else
            {
                currentState = DoorState.Closed;
                ApplyVisualAndColliderForCurrentState();
            }
        }

        private void Update()
        {
            if (currentState == DoorState.Open || isOpeningAnimPlaying)
            {
                isPlayerNearby = false;
                return;
            }

            if (hunter == null)
            {
                hunter = FindObjectOfType<HunterPlayer>();
                if (hunter == null) return;
            }

            isPlayerNearby = CheckPlayerCanInteract();

            if (isPlayerNearby && Input.GetKeyDown(interactKey))
            {
                HandleInteractPressed();
            }
        }

        private bool CheckPlayerCanInteract()
        {
            Vector2 toDoor = (Vector2)transform.position - (Vector2)hunter.transform.position;
            float dist = toDoor.magnitude;
            if (dist > interactRadius) return false;

            if (requireFacingDoor && dist > 0.25f)
            {
                Vector2 facing = hunter.FacingDirection;
                if (facing.sqrMagnitude > 0.001f)
                {
                    float dot = Vector2.Dot(facing.normalized, toDoor.normalized);
                    if (dot <= 0.2f) return false;
                }
            }

            // Kiểm tra cản tầm nhìn: nếu có tường chắn giữa Player và Cửa thì không hiện
            int wallMask = LayerMask.GetMask("Obstacle", "Default");
            RaycastHit2D hit = Physics2D.Linecast(hunter.transform.position, transform.position, wallMask);
            if (hit.collider != null
                && !hit.collider.isTrigger
                && hit.collider.gameObject != gameObject
                && !hit.collider.transform.IsChildOf(transform)
                && hit.collider.GetComponent<HunterPlayer>() == null
                && hit.collider.GetComponentInParent<HunterPlayer>() == null)
            {
                return false;
            }

            return true;
        }

        private void HandleInteractPressed()
        {
            switch (currentState)
            {
                case DoorState.Closed:
                    OpenDoor();
                    break;

                case DoorState.LockedByCondition:
                    TriggerLockedFeedback(lockedPromptMessage, new Color(1f, 0.8f, 0.3f, 1f));
                    break;

                case DoorState.SealedInCombat:
                    TriggerLockedFeedback(sealedPromptMessage, new Color(1f, 0.4f, 0.4f, 1f));
                    break;
            }
        }

        /// <summary>
        /// Mở cửa (chạy animation nếu có, sau đó tắt Collider2D và ẩn sprite thay vì tắt GameObject).
        /// </summary>
        public void OpenDoor()
        {
            if (currentState == DoorState.Open || isOpeningAnimPlaying) return;

            if (openRoutine != null) StopCoroutine(openRoutine);
            openRoutine = StartCoroutine(OpenDoorRoutine());
        }

        /// <summary>
        /// Khóa hoặc mở khóa cửa theo điều kiện tầng (VD: cần đủ Key ở Tầng 1, Tầng 2).
        /// </summary>
        public void SetConditionLocked(bool locked, string customMessage = null)
        {
            if (!string.IsNullOrEmpty(customMessage))
            {
                lockedPromptMessage = customMessage;
            }

            lockedUntilUnlocked = locked;

            // Nếu đang bị phong ấn chiến đấu thì giữ nguyên SealedInCombat, chỉ lưu trạng thái nền
            if (currentState == DoorState.SealedInCombat)
            {
                stateBeforeSeal = locked ? DoorState.LockedByCondition : DoorState.Closed;
                return;
            }

            if (locked)
            {
                if (openRoutine != null)
                {
                    StopCoroutine(openRoutine);
                    openRoutine = null;
                    isOpeningAnimPlaying = false;
                }
                currentState = DoorState.LockedByCondition;
                ApplyVisualAndColliderForCurrentState();
            }
            else if (currentState == DoorState.LockedByCondition)
            {
                currentState = DoorState.Closed;
                ApplyVisualAndColliderForCurrentState();
            }
        }

        /// <summary>
        /// Mở khóa điều kiện (khi người chơi đã thu thập đủ Key).
        /// Mặc định chuyển về Closed để người chơi chạy tới nhấn [F] mở.
        /// </summary>
        public void UnlockCondition(bool autoOpen = false)
        {
            SetConditionLocked(false);
            if (autoOpen && currentState == DoorState.Closed)
            {
                OpenDoor();
            }
        }

        /// <summary>
        /// Phong ấn cửa ngay lập tức khi Player bước vào phòng quái hoặc phòng Boss.
        /// Bật lại toàn bộ Collider chặn lối và hiện cánh cửa màu đỏ phong ấn.
        /// </summary>
        public void SealForCombat(string customMessage = null)
        {
            if (!string.IsNullOrEmpty(customMessage))
            {
                sealedPromptMessage = customMessage;
            }

            if (currentState == DoorState.SealedInCombat) return;

            if (openRoutine != null)
            {
                StopCoroutine(openRoutine);
                openRoutine = null;
                isOpeningAnimPlaying = false;
            }

            stateBeforeSeal = currentState;
            currentState = DoorState.SealedInCombat;
            ApplyVisualAndColliderForCurrentState();
        }

        /// <summary>
        /// Gỡ phong ấn sau khi tiêu diệt hết quái/Boss trong phòng.
        /// </summary>
        public void UnsealAfterCombat(bool openDoorsImmediately = true)
        {
            if (currentState != DoorState.SealedInCombat) return;

            if (lockedUntilUnlocked)
            {
                currentState = DoorState.LockedByCondition;
                ApplyVisualAndColliderForCurrentState();
                return;
            }

            if (openDoorsImmediately || stateBeforeSeal == DoorState.Open)
            {
                currentState = DoorState.Open;
                ApplyVisualAndColliderForCurrentState();
            }
            else
            {
                currentState = DoorState.Closed;
                ApplyVisualAndColliderForCurrentState();
            }
        }

        private IEnumerator OpenDoorRoutine()
        {
            isOpeningAnimPlaying = true;
            isPlayerNearby = false;

            if (sr != null)
            {
                sr.enabled = true;
                sr.color = originalColor;
            }

            if (openFrames != null && openFrames.Length > 0 && sr != null)
            {
                for (int i = 0; i < openFrames.Length; i++)
                {
                    sr.sprite = openFrames[i];
                    yield return new WaitForSeconds(frameRate);
                }
            }
            else
            {
                yield return new WaitForSeconds(0.12f);
            }

            isOpeningAnimPlaying = false;
            openRoutine = null;
            currentState = DoorState.Open;
            ApplyVisualAndColliderForCurrentState();
        }

        private void ApplyVisualAndColliderForCurrentState()
        {
            bool shouldBlock = currentState != DoorState.Open;
            SetSolidCollidersEnabled(shouldBlock);

            if (sr == null) return;

            switch (currentState)
            {
                case DoorState.Open:
                    if (hideSpriteWhenOpen)
                    {
                        sr.enabled = false;
                    }
                    else
                    {
                        sr.enabled = true;
                        if (openFrames != null && openFrames.Length > 0)
                            sr.sprite = openFrames[openFrames.Length - 1];
                        sr.color = originalColor;
                    }
                    break;

                case DoorState.Closed:
                    sr.enabled = true;
                    if (closedSprite != null) sr.sprite = closedSprite;
                    sr.color = originalColor;
                    break;

                case DoorState.LockedByCondition:
                    sr.enabled = true;
                    if (closedSprite != null) sr.sprite = closedSprite;
                    sr.color = lockedTintColor;
                    break;

                case DoorState.SealedInCombat:
                    sr.enabled = true;
                    if (closedSprite != null) sr.sprite = closedSprite;
                    sr.color = sealedTintColor;
                    break;
            }
        }

        private void SetSolidCollidersEnabled(bool enabled)
        {
            for (int i = 0; i < solidColliders.Count; i++)
            {
                if (solidColliders[i] != null)
                {
                    solidColliders[i].enabled = enabled;
                }
            }
        }

        private void TriggerLockedFeedback(string message, Color toastColor)
        {
            DungeonBannerToast.Show(message, toastColor, 2.2f);
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(DoorShakeRoutine());
        }

        private IEnumerator DoorShakeRoutine()
        {
            float elapsed = 0f;
            const float duration = 0.22f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float offsetX = Mathf.Sin(elapsed * 55f) * 0.06f;
                transform.localPosition = originalLocalPos + new Vector3(offsetX, 0f, 0f);
                yield return null;
            }
            transform.localPosition = originalLocalPos;
            shakeRoutine = null;
        }

        private void OnGUI()
        {
            if (!isPlayerNearby || isOpeningAnimPlaying || currentState == DoorState.Open) return;
            if (Camera.main == null) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.0f);
            if (screenPos.z <= 0f) return;

            string label;
            Color contentColor;
            float boxWidth;

            switch (currentState)
            {
                case DoorState.LockedByCondition:
                    label = lockedPromptMessage;
                    contentColor = new Color(1f, 0.8f, 0.35f, 1f);
                    boxWidth = Mathf.Max(180f, label.Length * 8.5f);
                    break;

                case DoorState.SealedInCombat:
                    label = sealedPromptMessage;
                    contentColor = new Color(1f, 0.42f, 0.42f, 1f);
                    boxWidth = Mathf.Max(190f, label.Length * 8.5f);
                    break;

                default:
                    label = "Nhấn [F] Mở Cửa";
                    contentColor = new Color(1f, 0.85f, 0.4f, 1f);
                    boxWidth = 150f;
                    break;
            }

            GUI.backgroundColor = new Color(0.15f, 0.1f, 0.05f, 0.9f);
            GUI.contentColor = contentColor;
            GUI.Box(new Rect(screenPos.x - boxWidth * 0.5f, Screen.height - screenPos.y - 16f, boxWidth, 30f), label);
        }
    }
}
