//using UnityEngine;
//using UnityEngine.Rendering.Universal;

//public class BlackoutLightingEffect : MonoBehaviour
//{
//    [Header("Light References")]
//    [SerializeField] private Light2D globalLight;
//    [SerializeField] private Light2D playerLight;

//    [Header("Blackout Settings")]
//    [SerializeField] private float blackoutGlobalIntensity = 0.15f;
//    [SerializeField] private float blackoutPlayerIntensity = 1.5f;

//    private float normalGlobalIntensity;
//    private float normalPlayerIntensity;

//    private bool isBlackoutActive;

//    private void Awake()
//    {
//        if (globalLight != null)
//            normalGlobalIntensity = globalLight.intensity;

//        if (playerLight != null)
//            normalPlayerIntensity = playerLight.intensity;
//    }

//    public void SetBlackout(bool active)
//    {
//        isBlackoutActive = active;

//        if (globalLight != null)
//        {
//            globalLight.intensity = active
//                ? blackoutGlobalIntensity
//                : normalGlobalIntensity;
//        }

//        if (playerLight != null)
//        {
//            playerLight.enabled = active;

//            playerLight.intensity = active
//                ? blackoutPlayerIntensity
//                : normalPlayerIntensity;
//        }

//        Debug.Log(
//            active
//            ? "[Blackout] M?t ?i?n! T?m nhìn b? h?n ch?."
//            : "[Blackout] H? th?ng chi?u sáng ?ã khôi ph?c."
//        );
//    }

//    public bool IsBlackoutActive => isBlackoutActive;
//}

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BlackoutLightingEffect : MonoBehaviour
{
    [Header("Light References")]
    [SerializeField] private Light2D globalLight;

    [Header("Blackout Settings")]
    [SerializeField] private float blackoutGlobalIntensity = 0.15f;
    [SerializeField] private float blackoutPlayerIntensity = 1.5f;

    private Light2D playerLight;
    private float normalGlobalIntensity;
    private float normalPlayerIntensity = 1f;

    private void Awake()
    {
        if (globalLight != null)
            normalGlobalIntensity = globalLight.intensity;
    }

    private void OnEnable()
    {
        if (RunModifierSystem.Instance != null)
        {
            RunModifierSystem.Instance.OnModifierActivated
                += HandleModifierActivated;
        }
    }

    private void Start()
    {
        // ??ng b? n?u Modifier ?ã ???c ch?n tr??c khi script Start
        if (RunModifierSystem.Instance != null)
        {
            HandleModifierActivated(
                RunModifierSystem.Instance.ActiveModifier,
                RunModifierSystem.Instance.GetModifierDescription(
                    RunModifierSystem.Instance.ActiveModifier
                )
            );
        }
    }

    private void OnDisable()
    {
        if (RunModifierSystem.Instance != null)
        {
            RunModifierSystem.Instance.OnModifierActivated
                -= HandleModifierActivated;
        }
    }

    private void HandleModifierActivated(
        RunModifierType modifier,
        string description)
    {
        SetBlackout(modifier == RunModifierType.Blackout);
    }

    private void SetBlackout(bool active)
    {
        if (globalLight != null)
        {
            globalLight.intensity = active
                ? blackoutGlobalIntensity
                : normalGlobalIntensity;
        }

        // Tìm l?i Player Light n?u Player ?ã ???c t?o l?i
        Transform player = PlayerMovement.PlayerTransform;

        if (player != null)
        {
            playerLight = player.GetComponentInChildren<Light2D>(true);
        }

        if (playerLight != null)
        {
            if (active)
            {
                normalPlayerIntensity = playerLight.intensity;
                playerLight.enabled = true;
                playerLight.intensity = blackoutPlayerIntensity;
            }
            else
            {
                playerLight.intensity = normalPlayerIntensity;
                playerLight.enabled = false;
            }
        }

        Debug.Log("[Blackout] Lighting: " + active);
    }
}