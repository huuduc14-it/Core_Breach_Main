using UnityEngine;

/// <summary>
/// Mảnh kim loại vụn (Scrap) rơi ra từ kẻ địch khi bị tiêu diệt trong Core Breach:
/// - Tự động bay về phía Unit-7 khi ở gần (Hiệu ứng từ tính Magnet)
/// - Cộng Scrap vào GameManager và phát âm thanh nhặt đồ khi thu thập
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ScrapItem : MonoBehaviour
{
    [Header("Scrap Value")]
    [SerializeField] private int scrapValue = 1;

    [Header("Magnet Settings")]
    [SerializeField] private float magnetRadius = 3.5f;
    [SerializeField] private float flySpeed = 8f;

    private Transform playerTransform;
    private bool isFlyingToPlayer = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= magnetRadius)
        {
            isFlyingToPlayer = true;
        }

        if (isFlyingToPlayer)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, flySpeed * Time.deltaTime);

            if (distance < 0.4f)
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
