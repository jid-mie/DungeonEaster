using System.Collections;
using UnityEngine;

/// <summary>
/// Bẫy trong ngục tối theo đặc tả TaiLieu.md mục 5.2:
/// 1. Bẫy gai sàn (Spikes): Chồi gai lên theo chu kỳ gây sát thương khi dẫm phải.
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

    [Header("Cấu hình Bẫy Gai (Floor Spikes)")]
    [SerializeField] private float spikeDamage = 15f;
    [SerializeField] private float spikeCycleTime = 2.5f;
    [SerializeField] private float spikeActiveTime = 1.0f;

    [Header("Cấu hình Đầm Lầy Độc (Poison Marsh)")]
    [SerializeField] private float poisonDamagePerTick = 5f;
    [SerializeField] private float poisonTickInterval = 2.0f;
    [SerializeField] private float slowMultiplier = 0.5f; // Làm chậm x2 lần

    private bool isSpikeActive = false;
    private Collider2D trapCol;
    private SpriteRenderer sr;

    private void Awake()
    {
        trapCol = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (trapCol != null) trapCol.isTrigger = true;
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
            // Trạng thái thụt gai (An toàn)
            isSpikeActive = false;
            if (sr != null) sr.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
            yield return new WaitForSeconds(spikeCycleTime);

            // Báo hiệu chuẩn bị chồi gai
            if (sr != null) sr.color = new Color(1f, 0.5f, 0.5f, 0.9f);
            yield return new WaitForSeconds(0.4f);

            // Chồi gai lên (Nguy hiểm)
            isSpikeActive = true;
            if (sr != null) sr.color = Color.white;
            yield return new WaitForSeconds(spikeActiveTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<HunterPlayer>();
        if (player == null) player = other.GetComponentInParent<HunterPlayer>();
        if (player == null) return;

        if (type == TrapType.FloorSpikes && isSpikeActive)
        {
            player.TakeDamage(spikeDamage);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        var player = other.GetComponent<HunterPlayer>();
        if (player == null) player = other.GetComponentInParent<HunterPlayer>();
        if (player == null) return;

        if (type == TrapType.FloorSpikes && isSpikeActive)
        {
            // Gây sát thương nếu người chơi đứng lì trên bẫy gai đang bật
            player.TakeDamage(spikeDamage * Time.deltaTime * 2f);
        }
        else if (type == TrapType.PoisonMarsh)
        {
            // Làm chậm x2 lần và trừ máu theo chu kỳ 2s
            player.ApplyPoisonMarsh(poisonDamagePerTick, poisonTickInterval, slowMultiplier);
        }
    }
}
