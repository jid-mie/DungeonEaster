using System.Collections;
using UnityEngine;

namespace NguyenVanHai.Combat
{
    /// <summary>
    /// Hiệu ứng nhấp nháy khi nhân vật hoặc quái vật trúng đòn.
    /// Tự động nhường quyền hiển thị màu khi Quái/Boss đang trong trạng thái Đỏ Cảnh Báo (IsWarningActive).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageFlash : MonoBehaviour
    {
        [Header("Cấu hình màu sắc & thời gian")]
        [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.85f);
        [SerializeField] private float flashDuration = 0.10f;

        private SpriteRenderer spriteRenderer;
        private Color originalColor = Color.white;
        private Coroutine flashRoutine;

        /// <summary>
        /// Khi Quái/Boss đang chuyển màu Đỏ Cảnh Báo chuẩn bị ra đòn, không ghi đè mất màu cảnh báo.
        /// </summary>
        public bool IsWarningActive { get; set; } = false;

        public Color BaseColor
        {
            get => originalColor;
            set => originalColor = value;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        public void Flash()
        {
            if (spriteRenderer == null || IsWarningActive) return;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(DoFlashRoutine());
        }

        public void StopFlashImmediate()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }
        }

        private IEnumerator DoFlashRoutine()
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            if (!IsWarningActive && spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
            flashRoutine = null;
        }

        private void OnDisable()
        {
            if (spriteRenderer != null && !IsWarningActive)
            {
                spriteRenderer.color = originalColor;
            }
            flashRoutine = null;
        }
    }
}
