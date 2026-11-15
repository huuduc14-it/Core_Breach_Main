using UnityEngine;

/// <summary>
/// Hack Terminal (cơ chế đột phá của Game Concept).
/// Player đứng trong vùng trigger và GIỮ phím E trong vài giây -> mọi Turret Bot trong phòng
/// bị hack (TurretBot.HackTerminal()) và quay sang bắn kẻ địch trong thời gian hackedDuration.
/// Cần: Collider2D (Is Trigger = true) bao quanh terminal.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HackTerminalStation : MonoBehaviour
{
    [Header("Hack")]
    [SerializeField] private float hackSeconds = 2.5f;
    [SerializeField] private float decayPerSecond = 1.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool oneUse = true;

    [Header("Hiển thị")]
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Color idleColor = new Color(0.4f, 0.9f, 1f, 1f);
    [SerializeField] private Color doneColor = new Color(0.4f, 1f, 0.5f, 1f);

    private bool playerInside;
    private bool used;
    private float progress;

    private void Reset()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;

        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null) sprite.color = idleColor;
    }

    private void Update()
    {
        if (used && oneUse) return;

        if (playerInside && Input.GetKey(interactKey))
        {
            progress += Time.deltaTime / Mathf.Max(0.1f, hackSeconds);

            if (progress >= 1f) Complete();
        }
        else if (progress > 0f)
        {
            progress = Mathf.Max(0f, progress - decayPerSecond * Time.deltaTime / Mathf.Max(0.1f, hackSeconds));
        }

        // Nhấp nháy cho thấy đang hack
        if (sprite != null && !used)
        {
            sprite.color = Color.Lerp(idleColor, Color.white, progress * Mathf.PingPong(Time.time * 6f, 1f));
        }
    }

    private void Complete()
    {
        used = true;
        progress = 0f;

        TurretBot[] turrets = FindObjectsByType<TurretBot>(FindObjectsSortMode.None);
        int hacked = 0;
        for (int i = 0; i < turrets.Length; i++)
        {
            if (turrets[i] == null) continue;
            turrets[i].HackTerminal();
            hacked++;
        }

        Debug.Log("[HackTerminalStation] Hack thành công " + hacked + " Turret Bot.");

        if (sprite != null) sprite.color = doneColor;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayScrapCollect();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInside = false;
    }

    private void OnGUI()
    {
        if (!playerInside || (used && oneUse)) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 sp = cam.WorldToScreenPoint(transform.position + Vector3.up * 1f);
        if (sp.z < 0f) return;

        string label = progress > 0f
            ? "HACKING " + Mathf.RoundToInt(progress * 100f) + "%"
            : "Giữ [" + interactKey + "] để hack";

        GUI.Label(new Rect(sp.x - 90f, Screen.height - sp.y - 12f, 180f, 24f), label);
    }
}
