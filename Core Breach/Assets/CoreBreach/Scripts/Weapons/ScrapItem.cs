using UnityEngine;

/// <summary>
/// Mảnh kim loại vụn (Scrap) rơi ra từ kẻ địch khi bị tiêu diệt trong Core Breach:
/// - Tự động bay về phía Unit-7 khi ở gần (Hiệu ứng từ tính Magnet)
/// - Hồi máu (HP) và phục hồi giáp trực tiếp khi nhặt
/// - Cộng Scrap vào GameManager và phát âm thanh nhặt đồ khi thu thập
/// - Có hoạt ảnh nhịp đập phát quang thu hút người chơi
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ScrapItem : MonoBehaviour
{
    [Header("Scrap Value")]
    [Tooltip("Lượng Scrap nhận được")]
    [SerializeField] private int scrapValue = 1;

    [Header("Heal & Repair")]
    [Tooltip("Lượng máu (HP) hồi phục khi nhặt")]
    [SerializeField] private float healAmount = 5f;
    [Tooltip("Lượng giáp (Armor) phục hồi nếu máu đã đầy")]
    [SerializeField] private float armorRepairAmount = 2f;

    [Header("Magnet Settings")]
    [SerializeField] private float magnetRadius = 3.5f;
    [SerializeField] private float flySpeed = 8f;

    private Transform playerTransform;
    private bool isFlyingToPlayer = false;
    private Vector3 baseScale = Vector3.one;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        baseScale = transform.localScale;
        FindPlayer();
    }

    public void SetValues(int scrap, float heal, float armor = 2f)
    {
        scrapValue = scrap;
        healAmount = heal;
        armorRepairAmount = armor;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // Hoạt ảnh nhịp đập phát quang khi đang nằm trên sàn
        if (!isFlyingToPlayer)
        {
            float pulse = Mathf.Sin(Time.time * 6f) * 0.12f;
            transform.localScale = baseScale * (1f + pulse);
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= magnetRadius)
        {
            isFlyingToPlayer = true;
        }

        if (isFlyingToPlayer)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, flySpeed * Time.deltaTime);

            if (distance < 0.45f)
            {
                Collect();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            Collect();
        }
    }

    private void Collect()
    {
        int multiplier = 1;
        if (RunModifierSystem.Instance != null)
        {
            multiplier = Mathf.RoundToInt(RunModifierSystem.Instance.GetScrapMultiplier());
        }

        int finalValue = scrapValue * multiplier;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScrap(finalValue);
        }

        // Hồi Máu & Giáp cho Unit-7
        if (playerTransform == null) FindPlayer();
        if (playerTransform != null)
        {
            PlayerHealth ph = playerTransform.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                if (healAmount > 0f && ph.CurrentHealth < ph.MaxHealth)
                {
                    ph.Heal(healAmount);
                    CombatFeedback.ShowNotice(playerTransform.position + Vector3.up * 0.7f, $"+{healAmount} HP", new Color(0.2f, 1f, 0.4f));
                    CombatFeedback.SpawnSparks(playerTransform.position, Vector2.up, new Color(0.2f, 1f, 0.4f), 6);
                }
                else if (armorRepairAmount > 0f && ph.CurrentArmor < ph.MaxArmor)
                {
                    ph.RepairArmor(armorRepairAmount);
                    CombatFeedback.ShowNotice(playerTransform.position + Vector3.up * 0.7f, $"+{armorRepairAmount} GIÁP", new Color(0.2f, 0.85f, 1f));
                    CombatFeedback.SpawnSparks(playerTransform.position, Vector2.up, new Color(0.2f, 0.85f, 1f), 4);
                }
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayScrapCollect();
        }

        Destroy(gameObject);
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }
}
