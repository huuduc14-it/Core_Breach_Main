
using System.Collections.Generic;
using UnityEngine;

public enum HazardKind
{
    Trap,
    Water
}

[RequireComponent(typeof(Collider2D))]
public class RoomHazard : MonoBehaviour
{
    [Header("Hazard Configuration")]
    [SerializeField] private HazardKind kind = HazardKind.Trap;

    [Header("Trap")]
    [SerializeField, Min(0f)]
    private float damageOnEnter = 15f;

    [Header("Water")]
    [SerializeField, Range(0.1f, 1f)]
    private float waterSpeedMultiplier = 0.55f;

    private readonly HashSet<PlayerMovement> slowedPlayers =
        new HashSet<PlayerMovement>();

    private readonly HashSet<PlayerHealth> trapOccupants =
        new HashSet<PlayerHealth>();

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement movement =
            other.GetComponentInParent<PlayerMovement>();

        if (movement == null) return;

        if (kind == HazardKind.Water)
        {
            movement.SetSpeedMultiplier(
                this,
                waterSpeedMultiplier
            );

            slowedPlayers.Add(movement);
            return;
        }

        PlayerHealth health =
            movement.GetComponent<PlayerHealth>();

        if (health != null && trapOccupants.Add(health))
        {
            health.TakeDamage(damageOnEnter);

            Debug.Log(
                $"[RoomHazard] Player ch?m b?y, nh?n {damageOnEnter} damage."
            );
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement movement =
            other.GetComponentInParent<PlayerMovement>();

        if (movement != null)
        {
            movement.RemoveSpeedMultiplier(this);
            slowedPlayers.Remove(movement);
        }

        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            trapOccupants.Remove(health);
        }
    }

    private void OnDisable()
    {
        foreach (PlayerMovement movement in slowedPlayers)
        {
            if (movement != null)
            {
                movement.RemoveSpeedMultiplier(this);
            }
        }

        slowedPlayers.Clear();
        trapOccupants.Clear();
    }
}
