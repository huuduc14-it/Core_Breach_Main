using System.Collections;
using UnityEngine;

/// <summary>
/// Hiệu ứng chớp sáng (Damage Flash) khi nhân vật hoặc quái bị trúng đạn
/// </summary>
public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private float flashDuration = 0.08f;

    private SpriteRenderer sr;
    private Color originalColor = Color.white;
    private Coroutine flashRoutine;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }

        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    public void Flash()
    {
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
            if (sr == null) return;
        }

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(DoFlash());
    }

    private IEnumerator DoFlash()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
        flashRoutine = null;
    }
}
