using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// WallFade - Tự động làm trong suốt / mờ tường hoặc cột khi Player đi ra phía sau.
/// Giải quyết triệt để vấn đề tầm nhìn trong Dungeon 2D Top-down:
/// - Khi bình thường: Tường hiển thị 100% độ đậm, che khuất hoàn toàn căn phòng/khoảng trống phía sau.
/// - Khi Player đi ra sau bức tường: Tường mờ dần xuống (ví dụ 35% Alpha) để người chơi nhìn rõ nhân vật của mình.
/// Hỗ trợ cả SpriteRenderer thông thường và Tilemap.
/// </summary>
public class WallFade : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("Độ trong suốt khi Player đứng sau tường (0 = tàng hình hoàn toàn, 1 = đặc hoàn toàn). Mặc định 0.35")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeAlpha = 0.35f;

    [Tooltip("Tốc độ chuyển đổi độ mờ (càng lớn mờ càng nhanh)")]
    [SerializeField] private float fadeSpeed = 8f;

    private SpriteRenderer spriteRenderer;
    private Tilemap tilemap;
    private float targetAlpha = 1f;
    private float currentAlpha = 1f;
    private int playerInsideCount = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        tilemap = GetComponent<Tilemap>();
    }

    private void Update()
    {
        // Chuyển đổi mượt mà giữa độ đậm và độ mờ
        if (!Mathf.Approximately(currentAlpha, targetAlpha))
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            SetAlpha(currentAlpha);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsPlayer(collision))
        {
            playerInsideCount++;
            targetAlpha = fadeAlpha;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsPlayer(collision))
        {
            playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
            if (playerInsideCount == 0)
            {
                targetAlpha = 1f;
            }
        }
    }

    private bool IsPlayer(Collider2D collision)
    {
        return collision.CompareTag("Player") || collision.GetComponent<ChiefPlayer>() != null;
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }

        if (tilemap != null)
        {
            Color c = tilemap.color;
            c.a = alpha;
            tilemap.color = c;
        }
    }
}
