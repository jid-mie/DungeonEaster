using System.Collections;
using UnityEngine;

namespace NguyenVanHai.Combat
{
    /// <summary>
    /// Hiệu ứng nhấp nháy trắng/đỏ khi nhân vật hoặc quái vật trúng đòn theo chuẩn Game Feel (TaiLieu.md mục 3.2.1).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageFlash : MonoBehaviour
    {
        [Header("Cấu hình màu sắc & thời gian")]
        [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private float flashDuration = 0.12f;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Coroutine flashRoutine;

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
            if (spriteRenderer == null) return;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(DoFlashRoutine());
        }

        private IEnumerator DoFlashRoutine()
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            flashRoutine = null;
        }

        private void OnDisable()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
            flashRoutine = null;
        }
    }
}
