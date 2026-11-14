using System;
using UnityEngine;

/// <summary>
/// Quản lý máu và hủy diệt kẻ địch khi HP <= 0 trong Core Breach:
/// - Sentry Drone: HP 20, Scrap 2
/// - Turret Bot: HP 15, Scrap 3
/// - Core Guardian: HP 180, Scrap 20
/// Tự động cộng Scrap vào GameManager và phát âm thanh tương ứng khi bị hạ.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private int scrapReward = 2;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public event Action OnDeath;

    private bool isDead = false;

    private void Awake()
    {
        CurrentHealth = maxHealth;

        if (GetComponent<DamageFlash>() == null)
        {
            gameObject.AddComponent<DamageFlash>();
        }
    }

    public void SetMaxHealth(float hp, int scrap)
    {
        maxHealth = hp;
        CurrentHealth = hp;
        scrapReward = scrap;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        CurrentHealth -= damage;
        Debug.Log($"[EnemyHealth] {gameObject.name} nhận {damage} sát thương. HP: {CurrentHealth}/{maxHealth}");

        // Chớp trắng khi trúng đòn
        DamageFlash flash = GetComponent<DamageFlash>();
        if (flash != null) flash.Flash();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHit();
        }

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Shake(0.15f, 0.2f);
        }

        OnDeath?.Invoke();
        Debug.Log($"[EnemyHealth] {gameObject.name} bị tiêu diệt! Thưởng {scrapReward} Scrap.");

        // Cộng Scrap và điểm số vào GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScrap(scrapReward);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }

        Destroy(gameObject);
    }
}
