using System;
using UnityEngine;

/// <summary>
/// Cơ chế Run Modifier ngẫu nhiên đầu mỗi lượt chơi theo Game Concept Core Breach:
/// ① LowGravity: Trọng lực yếu - đạn bay chậm hơn nhưng xa hơn
/// ② Blackout: Mất điện - hạn chế tầm nhìn, địch khó phát hiện hơn
/// ③ Overclock: Quá tải - địch nhanh và bắn dồn dập hơn nhưng rơi x2 Scrap
/// </summary>
public enum RunModifierType
{
    None,
    LowGravity,
    Blackout,
    Overclock
}

public class RunModifierSystem : MonoBehaviour
{
    public static RunModifierSystem Instance { get; private set; }

    [Header("Current Modifier")]
    [SerializeField] private RunModifierType activeModifier = RunModifierType.None;

    public RunModifierType ActiveModifier => activeModifier;

    public event Action<RunModifierType, string> OnModifierActivated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RollRandomModifier();
    }

    /// <summary>
    /// Xúc xắc chọn ngẫu nhiên 1 Modifier cho toàn bộ lượt chơi Vault
    /// </summary>
    public void RollRandomModifier()
    {
        Array values = Enum.GetValues(typeof(RunModifierType));
        // Bỏ qua giá trị None (index 0), random từ 1 đến hết
        int randIndex = UnityEngine.Random.Range(1, values.Length);
        activeModifier = (RunModifierType)values.GetValue(randIndex);

        string description = GetModifierDescription(activeModifier);
        Debug.Log($"[RunModifierSystem] KÍCH HOẠT RUN MODIFIER: {activeModifier} - {description}");

        OnModifierActivated?.Invoke(activeModifier, description);
    }

    public string GetModifierDescription(RunModifierType modifier)
    {
        switch (modifier)
        {
            case RunModifierType.LowGravity:
                return "TRỌNG LỰC YẾU: Đạn bay chậm hơn nhưng tầm bắn xa hơn!";
            case RunModifierType.Blackout:
                return "MẤT ĐIỆN HỆ THỐNG: Tầm nhìn hạn chế trong bóng tối!";
            case RunModifierType.Overclock:
                return "QUÁ TẢI LÕI: Kẻ địch hung hãn hơn nhưng rơi gấp đôi Scrap!";
            default:
                return "Tiêu chuẩn: Không có biến số.";
        }
    }

    public float GetScrapMultiplier()
    {
        return activeModifier == RunModifierType.Overclock ? 2.0f : 1.0f;
    }

    public float GetBulletSpeedMultiplier()
    {
        return activeModifier == RunModifierType.LowGravity ? 0.75f : 1.0f;
    }

    public float GetBulletLifeMultiplier()
    {
        return activeModifier == RunModifierType.LowGravity ? 1.5f : 1.0f;
    }
}
