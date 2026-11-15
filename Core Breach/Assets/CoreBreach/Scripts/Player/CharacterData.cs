using System;
using UnityEngine;

/// <summary>
/// Định nghĩa các lớp nhân vật có thể lựa chọn trong Core Breach
/// </summary>
public enum CharacterClass
{
    Unit7_Recon,    // Trinh sát cân bằng, Súng Dual Plasma + Song đoản đao Nano Daggers
    Unit0_Striker,  // Cơ động cận chiến, Tiểu liên Viper SMG + Đại kiếm Cyber Katana
    Unit9_Titan     // Pháo đài bọc thép, Pháo Heavy Scatter Cannon + Búa Gravity Hammer
}

[System.Serializable]
public class CharacterProfile
{
    public CharacterClass characterClass;
    public string codeName;
    public string roleTitle;
    public float maxHealth;
    public float maxArmor;
    public float moveSpeed;
    public string gunName;
    public string gunType;
    public string meleeName;
    public string meleeType;
    public string description;
    public Color themeColor;
}

/// <summary>
/// Quản lý dữ liệu và cấu hình 3 nhân vật có thể lựa chọn trước khi chơi
/// </summary>
public static class CharacterDatabase
{
    private static CharacterProfile[] profiles;

    public static CharacterProfile[] GetAllProfiles()
    {
        if (profiles == null)
        {
            profiles = new CharacterProfile[]
            {
                new CharacterProfile
                {
                    characterClass = CharacterClass.Unit7_Recon,
                    codeName = "UNIT-7 [RECON]",
                    roleTitle = "Robot Trinh Sát Cân Bằng",
                    maxHealth = 100f,
                    maxArmor = 20f,
                    moveSpeed = 4.8f,
                    gunName = "Dual Plasma Carbine",
                    gunType = "Súng 2 tia song song [Tầm xa]",
                    meleeName = "Nano Energy Daggers",
                    meleeType = "Song đoản đao liên hoàn [Cận chiến]",
                    description = "Cỗ máy trinh sát đa năng tốc độ cao. Trang bị pháo plasma nòng đôi và song dao nano chém liên hoàn cực nhanh.",
                    themeColor = new Color(0f, 0.9f, 1f, 1f) // Neon Cyan
                },
                new CharacterProfile
                {
                    characterClass = CharacterClass.Unit0_Striker,
                    codeName = "UNIT-0 [STRIKER]",
                    roleTitle = "Chiến Binh Kiếm Năng Lượng (Cận Chiến)",
                    maxHealth = 120f,
                    maxArmor = 30f,
                    moveSpeed = 5.3f,
                    gunName = "Viper Laser SMG",
                    gunType = "Tiểu liên liên thanh xả đạn [Tầm gần]",
                    meleeName = "Cyber Muramasa Katana",
                    meleeType = "Đại kiếm chém 150° & Phản đạn [Cận chiến]",
                    description = "Chiến binh cơ động cao, chuyên áp sát. Đại kiếm nano chém quét cung rộng 150° phá nổ toàn bộ đạn địch, kết hợp tiểu liên SMG.",
                    themeColor = new Color(1f, 0.15f, 0.45f, 1f) // Neon Crimson Pink
                },
                new CharacterProfile
                {
                    characterClass = CharacterClass.Unit9_Titan,
                    codeName = "UNIT-9 [HEAVY TITAN]",
                    roleTitle = "Pháo Đài Bọc Thép (Hạng Nặng)",
                    maxHealth = 150f,
                    maxArmor = 40f,
                    moveSpeed = 3.9f,
                    gunName = "Heavy Scatter Cannon",
                    gunType = "Pháo phân mảnh 4 tia Shotgun [Hỏa lực]",
                    meleeName = "Gravity Thunder Hammer",
                    meleeType = "Búa nện đất 360° Choáng quái [Cận chiến]",
                    description = "Cỗ máy titan bọc thép hạng nặng. Pháo shotgun 4 tia thổi bay đám đông và búa trọng lực nện đất hất văng mọi quái xung quanh.",
                    themeColor = new Color(1f, 0.75f, 0.1f, 1f) // Neon Amber Gold
                }
            };
        }
        return profiles;
    }

    public static CharacterProfile GetProfile(CharacterClass cClass)
    {
        CharacterProfile[] all = GetAllProfiles();
        foreach (var p in all)
        {
            if (p.characterClass == cClass) return p;
        }
        return all[0];
    }
}
