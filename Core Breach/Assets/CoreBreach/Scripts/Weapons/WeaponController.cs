using System;
using UnityEngine;

/// <summary>
/// Quản lý 2 khe vũ khí của Unit-7 và nhận lệnh bắn (Chuột trái) / Đổi vũ khí (Phím R)
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Slots (2 khe vũ khí)")]
    [SerializeField] private Weapon slot1Weapon;
    [SerializeField] private Weapon slot2Weapon;

    [Header("References")]
    [SerializeField] private PlayerAim playerAim;

    private Weapon currentWeapon;
    private int currentSlotIndex = 0; // 0: slot 1, 1: slot 2

    public Weapon CurrentWeapon => currentWeapon;
    public event Action<Weapon, int> OnWeaponChanged; // (weapon, slotIndex)

    private void Awake()
    {
        if (playerAim == null)
        {
            playerAim = GetComponent<PlayerAim>();
            if (playerAim == null) playerAim = GetComponentInParent<PlayerAim>();
            if (playerAim == null) playerAim = GetComponentInChildren<PlayerAim>();
        }

        // Tự động tìm vũ khí trong các object con nếu chưa được kéo vào slot
        if (slot1Weapon == null)
        {
            slot1Weapon = GetComponentInChildren<Weapon>();
            if (slot1Weapon == null)
            {
                slot1Weapon = GetComponent<Weapon>();
            }
        }
    }

    private void Start()
    {
        // Khởi tạo vũ khí ban đầu (ưu tiên slot 1)
        if (slot1Weapon != null)
        {
            EquipWeapon(0);
        }
        else if (slot2Weapon != null)
        {
            EquipWeapon(1);
        }
        else
        {
            // Tìm lại lần nữa trong trường hợp sinh ra muộn
            Weapon w = GetComponentInChildren<Weapon>();
            if (w != null)
            {
                slot1Weapon = w;
                EquipWeapon(0);
            }
        }
    }

    private void Update()
    {
        // Khóa bắn súng nếu Game Over
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver) return;

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null && health.CurrentHealth <= 0f) return;

        HandleShooting();
        HandleWeaponSwitch();
    }

    private void HandleShooting()
    {
        // Tự động trang bị nếu currentWeapon bị mất
        if (currentWeapon == null)
        {
            if (slot1Weapon != null) EquipWeapon(0);
            else if (slot2Weapon != null) EquipWeapon(1);
            else return;
        }

        // Bắn khi giữ chuột trái
        if (Input.GetMouseButton(0))
        {
            Vector2 aimDirection = playerAim != null ? playerAim.GetAimDirection() : (Vector2)transform.right;
            currentWeapon.TryShoot(aimDirection);
        }
    }

    private void HandleWeaponSwitch()
    {
        // Nhấn phím R để đổi qua lại giữa 2 khe vũ khí
        if (Input.GetKeyDown(KeyCode.R))
        {
            SwitchWeapon();
        }
    }

    public void SwitchWeapon()
    {
        int newSlot = currentSlotIndex == 0 ? 1 : 0;
        EquipWeapon(newSlot);
    }

    public void EquipWeapon(int slotIndex)
    {
        Weapon targetWeapon = slotIndex == 0 ? slot1Weapon : slot2Weapon;
        if (targetWeapon == null) return;

        if (slot1Weapon != null) slot1Weapon.gameObject.SetActive(slotIndex == 0);
        if (slot2Weapon != null) slot2Weapon.gameObject.SetActive(slotIndex == 1);

        currentSlotIndex = slotIndex;
        currentWeapon = targetWeapon;

        OnWeaponChanged?.Invoke(currentWeapon, currentSlotIndex);
        Debug.Log($"[WeaponController] Đã trang bị: {currentWeapon.WeaponName} (Khe {currentSlotIndex + 1})");
    }

    /// <summary>
    /// Gán vũ khí nhặt được vào khe
    /// </summary>
    public void SetWeaponToSlot(Weapon newWeapon, int slotIndex)
    {
        if (slotIndex == 0) slot1Weapon = newWeapon;
        else slot2Weapon = newWeapon;

        if (currentSlotIndex == slotIndex)
        {
            EquipWeapon(slotIndex);
        }
    }
}

