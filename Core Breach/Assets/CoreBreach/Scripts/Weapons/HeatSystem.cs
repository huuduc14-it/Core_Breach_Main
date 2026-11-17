using System;
using UnityEngine;

/// <summary>
/// Cơ chế Nạp Nhiệt (Overheat) độc quyền của Core Breach:
/// Bắn liên tục làm thanh nhiệt đầy dần.
/// Khi đạt 100%, vũ khí tạm khóa 2 giây để nguội trước khi bắn lại được.
/// </summary>
public class HeatSystem : MonoBehaviour
{
    [Header("Heat Settings")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float heatPerShot = 12f;      // Lượng nhiệt tích luỹ mỗi phát bắn
    [SerializeField] private float coolingRate = 30f;     // Tốc độ hạ nhiệt mỗi giây khi ngừng bắn
    [SerializeField] private float coolDelay = 0.4f;      // Thời gian chờ sau phát bắn cuối để bắt đầu làm mát
    [SerializeField] private float overheatLockDuration = 2.0f; // Khóa 2s theo Game Concept khi quá nhiệt 100%

    public float CurrentHeat { get; private set; } = 0f;
    public float MaxHeat => maxHeat;
    public bool IsOverheated { get; private set; } = false;

    public event Action<float, float, bool> OnHeatChanged; // (current, max, isOverheated)

    private float lastShotTime = 0f;
    private float overheatTimer = 0f;

    private void Update()
    {
        // Xử lý khi bị khóa do Overheat
        if (IsOverheated)
        {
            overheatTimer -= Time.deltaTime;
            // Trong thời gian quá tải, nhiệt giảm dần về 0
            CurrentHeat = Mathf.Max(0f, CurrentHeat - (maxHeat / overheatLockDuration) * Time.deltaTime);

            if (overheatTimer <= 0f)
            {
                IsOverheated = false;
                CurrentHeat = 0f;
                Debug.Log("[HeatSystem] Vũ khí đã nguội! Có thể bắn tiếp.");
            }

            OnHeatChanged?.Invoke(CurrentHeat, maxHeat, IsOverheated);
            return;
        }

        // Tự động làm mát nếu sau một khoảng delay không bắn
        if (Time.time >= lastShotTime + coolDelay && CurrentHeat > 0f)
        {
            float rate = coolingRate;
            if (CombatFeedback.Instance != null && CombatFeedback.Instance.IsOverdriveActive)
            {
                rate *= 2.0f; // Tản nhiệt nhanh gấp đôi trong trạng thái Overdrive
            }

            CurrentHeat = Mathf.Max(0f, CurrentHeat - rate * Time.deltaTime);
            OnHeatChanged?.Invoke(CurrentHeat, maxHeat, IsOverheated);
        }
    }

    /// <summary>
    /// Kiểm tra vũ khí có đủ điều kiện bắn không
    /// </summary>
    public bool CanShoot()
    {
        return !IsOverheated;
    }

    /// <summary>
    /// Gọi mỗi khi bắn 1 viên đạn
    /// </summary>
    public void AddHeat(float customHeat = -1f)
    {
        if (IsOverheated) return;

        float added = customHeat > 0f ? customHeat : heatPerShot;
        CurrentHeat += added;
        lastShotTime = Time.time;

        if (CurrentHeat >= maxHeat)
        {
            CurrentHeat = maxHeat;
            IsOverheated = true;
            overheatTimer = overheatLockDuration;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOverheatWarning();
            }

            Debug.LogWarning("[HeatSystem] CẢNH BÁO: VŨ KHÍ QUÁ TẢI (OVERHEAT)! Khóa 2 giây.");
        }

        OnHeatChanged?.Invoke(CurrentHeat, maxHeat, IsOverheated);
    }
}
