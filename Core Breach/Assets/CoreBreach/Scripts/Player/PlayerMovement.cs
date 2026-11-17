using System.Collections;
using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// Điều khiển di chuyển 8 hướng và kỹ năng Dash cho Unit-7 (Player) trong Core Breach
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }
    public static Transform PlayerTransform { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4.5f; // Tốc độ cơ bản của Unit-7 theo Game Concept

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.0f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.up;

    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    public bool IsDashing => isDashing;
    public Vector2 MoveInput => moveInput;
    public float MoveSpeed => moveSpeed;
    public Rigidbody2D Rigidbody => rb;

    private readonly Dictionary<RoomHazard, float> speedModifiers =
    new Dictionary<RoomHazard, float>();

    public void SetSpeedMultiplier(RoomHazard source, float multiplier)
    {
        if (source == null) return;

        speedModifiers[source] = Mathf.Clamp(multiplier, 0.1f, 1f);
    }

    public void RemoveSpeedMultiplier(RoomHazard source)
    {
        if (source == null) return;

        speedModifiers.Remove(source);
    }

    private float GetCombinedSpeedMultiplier()
    {
        float result = 1f;

        foreach (float multiplier in speedModifiers.Values)
        {
            result = Mathf.Min(result, multiplier);
        }

        return result;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    private void Awake()
    {
        Instance = this;
        PlayerTransform = transform;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Góc nhìn top-down không dùng trọng lực rơi
        rb.freezeRotation = true; // Khóa xoay Z bởi vật lý
    }

    private void LateUpdate()
    {
        // Khóa tuyệt đối không bao giờ cho phép thân nhân vật bị xoay lộn nhào 360 độ (Chân luôn hướng xuống đất)
        transform.rotation = Quaternion.identity;
        if (rb != null) rb.rotation = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            PlayerTransform = null;
        }
    }

    private void Update()
    {
        // Khóa điều khiển nếu không phải trạng thái đang chơi (MainMenu, CharacterSelect, Paused, GameOver)
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Khóa điều khiển nếu đang ở Menu chọn nhân vật
        if (CharacterSelectSystem.Instance != null && CharacterSelectSystem.Instance.IsSelectionOpen)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Khóa điều khiển nếu chết hoặc Game Over
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null && health.CurrentHealth <= 0f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Giảm thời gian hồi dash
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (isDashing) return;

        // Nhận input di chuyển W A S D
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized;

        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput;
        }

        // Kích hoạt Dash bằng phím Space
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0f)
        {
            StartCoroutine(PerformDash());
        }
    }


    private void FixedUpdate()
    {
        if (isDashing) return;

        // Tốc độ cơ bản kết hợp với hiệu ứng nước
        float effectiveSpeed =
            moveSpeed * GetCombinedSpeedMultiplier();

        // Core Overdrive tăng thêm 25% tốc độ
        if (CombatFeedback.Instance != null &&
            CombatFeedback.Instance.IsOverdriveActive)
        {
            effectiveSpeed *= 1.25f;
        }

        // Di chuyển bằng Rigidbody2D
        rb.linearVelocity = moveInput * effectiveSpeed;
    }


    private IEnumerator PerformDash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        // Lướt theo hướng đang bấm, nếu đứng yên thì lướt theo hướng đi gần nhất
        Vector2 dashDirection = moveInput != Vector2.zero ? moveInput : lastMoveDirection;
        //rb.linearVelocity = dashDirection * dashSpeed;
        rb.linearVelocity = dashDirection * dashSpeed * GetCombinedSpeedMultiplier();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDash();
        }

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }
}
