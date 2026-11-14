using System;
using UnityEngine;

/// <summary>
/// Quản lý Máu (HP) và Giáp (Armor) cho Unit-7 theo Game Concept:
/// HP: 100, Giáp: 20
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Stats theo Game Concept")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxArmor = 20f;

    public float CurrentHealth { get; private set; }
    public float CurrentArmor { get; private set; }

    public event Action<float, float> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action<float, float> OnArmorChanged;  // (currentArmor, maxArmor)
    public event Action OnPlayerDeath;

    private bool isDead = false;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        CurrentArmor = maxArmor;

        if (GetComponent<DamageFlash>() == null)
        {
            gameObject.AddComponent<DamageFlash>();
        }
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnArmorChanged?.Invoke(CurrentArmor, maxArmor);
    }

    /// <summary>
    /// Nhận sát thương: Giáp hấp thụ trước, sát thương dư trừ tiếp vào máu
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f) return;

        float remainingDamage = damage;

        // Trừ vào Giáp trước
        if (CurrentArmor > 0f)
        {
            if (CurrentArmor >= remainingDamage)
            {
                CurrentArmor -= remainingDamage;
                remainingDamage = 0f;
            }
            else
            {
                remainingDamage -= CurrentArmor;
                CurrentArmor = 0f;
            }
            OnArmorChanged?.Invoke(CurrentArmor, maxArmor);
        }

        // Rung màn hình khi nhận sát thương
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Shake(0.18f, 0.25f);
        }

        // Chớp đỏ cảnh báo
        DamageFlash flash = GetComponent<DamageFlash>();
        if (flash != null) flash.Flash();

        // Nếu còn sát thương sót lại -> trừ vào Máu
        if (remainingDamage > 0f)
        {
            CurrentHealth -= remainingDamage;
            if (CurrentHealth <= 0f)
            {
                CurrentHealth = 0f;
                Die();
            }
            else
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPlayerHurt();
                }
            }
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void RepairArmor(float amount)
    {
        if (isDead) return;
        CurrentArmor = Mathf.Min(CurrentArmor + amount, maxArmor);
        OnArmorChanged?.Invoke(CurrentArmor, maxArmor);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerDeath();
        }

        Debug.LogWarning("[PlayerHealth] Unit-7 đã bị phá hủy!");
        OnPlayerDeath?.Invoke();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}
