using Signals;
using Unity.VisualScripting;
using UnityEngine;

public class LightningScript : MonoBehaviour
{

    public GameObject damagePopUpPrefab;

    private float fadein = 0.19f;
    private float fadeOut;

    PlayerController playerController;

    void Awake()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        fadeOut = fadein;
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
                Vector3 popupPosition = col.ClosestPoint(col.transform.position);
                GameObject popupClone = Instantiate(damagePopUpPrefab, popupPosition, Quaternion.identity);

                DamagePopUp popupScript = popupClone.GetComponent<DamagePopUp>();
                if (popupScript != null)
                {
                    popupScript.Setup(damageAmount, fadein, fadeOut);
                }
            }
        }
    }
}
