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
    public int CurrentSlotIndex => currentSlotIndex;
    public Weapon Slot1Weapon => slot1Weapon;
    public Weapon Slot2Weapon => slot2Weapon;
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
        // Khóa bắn súng nếu không phải trạng thái Playing
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        // Khóa bắn súng nếu đang ở Menu chọn nhân vật
        if (CharacterSelectSystem.Instance != null && CharacterSelectSystem.Instance.IsSelectionOpen) return;

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
            bool shot = currentWeapon.TryShoot(aimDirection);
            if (shot)
            {
                PlayerVisuals visuals = GetComponent<PlayerVisuals>();
                if (visuals != null) visuals.TriggerRecoil(aimDirection);
            }
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
        if (targetWeapon == null)
        {
            // Nếu slot2 chưa có, thử tìm MeleeWeapon
            if (slotIndex == 1)
            {
                slot2Weapon = GetComponentInChildren<MeleeWeapon>();
                if (slot2Weapon == null) slot2Weapon = GetComponent<MeleeWeapon>();
                targetWeapon = slot2Weapon;
            }
        }
        if (targetWeapon == null) return;

        // Bật/tắt an toàn không làm tắt Player GameObject
        if (slot1Weapon != null)
        {
            if (slot1Weapon.gameObject != gameObject) slot1Weapon.gameObject.SetActive(slotIndex == 0);
            else slot1Weapon.enabled = (slotIndex == 0);
        }
        if (slot2Weapon != null)
        {
            if (slot2Weapon.gameObject != gameObject) slot2Weapon.gameObject.SetActive(slotIndex == 1);
            else slot2Weapon.enabled = (slotIndex == 1);
        }

        currentSlotIndex = slotIndex;
        currentWeapon = targetWeapon;

        // Đảm bảo luôn có Sprite vũ khí độc quyền hiển thị
        if (currentWeapon.WeaponSprite == null)
        {
            currentWeapon.WeaponSprite = PlayerVisuals.CreateFallbackWeaponSprite(currentWeapon.WeaponName.ToLower());
        }

        // Cập nhật hiển thị vũ khí cầm trên tay nhân vật (Soul Knight Held Weapon)
        PlayerVisuals visuals = GetComponent<PlayerVisuals>();
        if (visuals == null) visuals = GetComponentInParent<PlayerVisuals>();
        if (visuals == null) visuals = GetComponentInChildren<PlayerVisuals>();

        if (visuals != null)
        {
            visuals.SetWeaponSprite(currentWeapon.WeaponSprite);
        }

        // Thông báo đổi vũ khí rõ ràng trên đầu nhân vật (Game Feel)
        string categoryName = slotIndex == 0 ? "🔫 SÚNG: " : "⚔️ CẬN CHIẾN: ";
        Color notifColor = slotIndex == 0 ? Color.cyan : new Color(1f, 0.75f, 0.2f);
        CombatFeedback.ShowNotice(transform.position + Vector3.up * 0.9f, categoryName + currentWeapon.WeaponName, notifColor);

        // Âm thanh rút/nạp vũ khí
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShoot();
        }

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

