using Signals;
using Unity.VisualScripting;
using UnityEngine;

public class LightningScript : MonoBehaviour
{
    PlayerController playerController;

    void Awake()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    public int damageAmount;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != LayerMask.NameToLayer("Hurtable")) return;

        if (col.TryGetComponent(out PlayerController pc))
        {
            if (pc.IsInPerfectDodgeWindow)
            {
                pc.TriggerPerfectDodge(col.ClosestPoint(transform.position));
                return;
            }

            if (pc.IsInvulnerable)
            {
                return;
            }
        }

        Rigidbody2D rb = col.attachedRigidbody;
        if (rb != null)
        {
            Health targetHealth = col.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damageAmount);
            }
        }
    }
}
