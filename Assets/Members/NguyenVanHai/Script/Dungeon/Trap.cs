using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bẫy trong ngục tối theo đặc tả TaiLieu.md mục 5.2:
/// 1. Bẫy gai sàn (Spikes): Chồi gai lên theo chu kỳ gây sát thương khi dẫm phải hoặc khi đứng yên trên bẫy.
/// 2. Vũng đầm lầy độc (Poison Marsh): Gây mất máu mỗi 2s và làm chậm x2 lần tốc độ chạy.
/// </summary>
public class Trap : MonoBehaviour
{
    public enum TrapType
    {
        FloorSpikes, // Bẫy gai sàn
        PoisonMarsh  // Vũng đầm lầy độc
    }

    [Header("Loại bẫy")]
    [SerializeField] private TrapType type = TrapType.FloorSpikes;

    [Header("--- Sprites Trạng Thái Bẫy Gai (Tùy chọn) ---")]
    [Tooltip("Sprite khi gai thụt xuống hoàn toàn (An toàn)")]
    [SerializeField] private Sprite retractedSprite;
    [Tooltip("Sprite khi gai nhú nhẹ lên cảnh báo (Warning 0.4s)")]
    [SerializeField] private Sprite warningSprite;
    [Tooltip("Sprite khi bẫy gai chồi lên kích hoạt gây sát thương (Active)")]
    [SerializeField] private Sprite activeSprite;
    [Tooltip("Chuỗi animation chồi gai nếu có nhiều frame")]
    [SerializeField] private Sprite[] popUpAnimationFrames;
    [SerializeField] private float animFrameRate = 0.06f;

    [Header("Cấu hình Bẫy Gai (Floor Spikes)")]
    [SerializeField] private float spikeDamage = 15f;
    [SerializeField] private float spikeCycleTime = 2.5f;
    [SerializeField] private float spikeActiveTime = 1.0f;
    [SerializeField] private float standingTickInterval = 0.4f; // Trừ máu định kỳ nếu đứng lì trên bẫy

    [Header("Cấu hình Đầm Lầy Độc (Poison Marsh)")]
    [SerializeField] private float poisonDamagePerTick = 5f;
    [SerializeField] private float poisonTickInterval = 2.0f;
    [SerializeField] private float slowMultiplier = 0.5f; // Làm chậm x2 lần

    private bool isSpikeActive = false;
    private Collider2D trapCol;
    private SpriteRenderer sr;
    private readonly Collider2D[] overlapResults = new Collider2D[16];
    private ContactFilter2D contactFilter;

    private void Awake()
    {
        trapCol = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (trapCol != null) trapCol.isTrigger = true;

        contactFilter = new ContactFilter2D();
        contactFilter.useTriggers = true;
    }

    private void Start()
    {
        if (type == TrapType.FloorSpikes)
        {
            StartCoroutine(SpikeCycleRoutine());
        }
    }

    private IEnumerator SpikeCycleRoutine()
    {
        while (true)
        {
            // 1. Trạng thái thụt gai (An toàn)
            isSpikeActive = false;
            if (retractedSprite != null && sr != null) sr.sprite = retractedSprite;
            else if (sr != null) sr.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);

            yield return new WaitForSeconds(spikeCycleTime);

            // 2. Báo hiệu chuẩn bị chồi gai (Warning 0.4s)
            if (warningSprite != null && sr != null) sr.sprite = warningSprite;
            else if (sr != null) sr.color = new Color(1f, 0.5f, 0.5f, 0.9f);

            yield return new WaitForSeconds(0.4f);

            // 3. Chồi gai lên kích hoạt (Active - Gây sát thương)
            isSpikeActive = true;
            if (sr != null) sr.color = Color.white;

            // Chạy chuỗi frame animation nếu có
            if (popUpAnimationFrames != null && popUpAnimationFrames.Length > 0 && sr != null)
            {
                for (int i = 0; i < popUpAnimationFrames.Length; i++)
                {
                    sr.sprite = popUpAnimationFrames[i];
                    yield return new WaitForSeconds(animFrameRate);
                }
            }
            else if (activeSprite != null && sr != null)
            {
                sr.sprite = activeSprite;
            }

            // GÂY SÁT THƯƠNG NGAY LẬP TỨC CHO BẤT KỲ AI ĐANG ĐỨNG TRÊN BẪY (kể cả đứng yên AFK)
            DamageAnyoneStandingOnTrap();

            // Duy trì trạng thái gai chồi lên
            float activeTimer = 0f;
            float tickTimer = 0f;

            while (activeTimer < spikeActiveTime)
            {
                activeTimer += Time.deltaTime;
                tickTimer += Time.deltaTime;

                // Nếu người chơi tiếp tục đứng lì trên gai, cứ mỗi standingTickInterval lại trừ thêm máu
                if (tickTimer >= standingTickInterval)
                {
                    tickTimer = 0f;
                    DamageAnyoneStandingOnTrap();
                }

                yield return null;
            }
        }
    }

    /// <summary>
    /// Quét trực tiếp collider để gây sát thương cho người chơi đang đứng trên bẫy.
    /// Phương pháp OverlapCollider không phụ thuộc vào Rigidbody Sleep hay việc nhân vật có di chuyển hay không.
    /// </summary>
    private void DamageAnyoneStandingOnTrap()
    {
        if (trapCol == null || !isSpikeActive) return;

        int count = trapCol.OverlapCollider(contactFilter, overlapResults);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = overlapResults[i];
            if (col == null || col == trapCol) continue;

            var player = col.GetComponent<HunterPlayer>() ?? col.GetComponentInParent<HunterPlayer>();
            if (player != null && player.isActiveAndEnabled)
            {
                player.TakeDamage(spikeDamage);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<HunterPlayer>() ?? other.GetComponentInParent<HunterPlayer>();
        if (player == null) return;

        // Nếu người chơi chạy vào bẫy gai khi nó ĐANG TRONG TRẠNG THÁI ACTIVE
        if (type == TrapType.FloorSpikes && isSpikeActive)
        {
            player.TakeDamage(spikeDamage);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        var player = other.GetComponent<HunterPlayer>() ?? other.GetComponentInParent<HunterPlayer>();
        if (player == null) return;

        if (type == TrapType.PoisonMarsh)
        {
            // Đầm lầy độc: Làm chậm x2 lần và trừ máu theo chu kỳ 2s
            player.ApplyPoisonMarsh(poisonDamagePerTick, poisonTickInterval, slowMultiplier);
        }
    }
}
