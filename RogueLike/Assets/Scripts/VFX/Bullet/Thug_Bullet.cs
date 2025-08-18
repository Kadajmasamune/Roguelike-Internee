using System.Collections;
using UnityEngine;

public class Thug_Bullet : MonoBehaviour
{
    private Vector2 velocity;
    private Rigidbody2D rb;

    float bulletlifetime = 2f;


    public void Init(Vector2 velocity)
    {
        this.velocity = velocity;
    }

    PlayerController playerController;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = FindFirstObjectByType<PlayerController>();
        fadeOut = fadein;

        Destroy(gameObject, bulletlifetime);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    public GameObject damagePopUpPrefab;

    private float fadein = 0.19f;
    private float fadeOut;



    public int damageAmount;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (col.TryGetComponent(out PlayerController pc))
            {
                if (pc.IsInPerfectDodgeWindow)
                {
                    pc.TriggerPerfectDodge(col.ClosestPoint(transform.position));
                    return;
                }

                if (pc.IsInvulnerable)
                    return;
            }

            Health targetHealth = col.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damageAmount);
                Vector3 popupPosition = col.ClosestPoint(col.transform.position);
                GameObject popupClone = Instantiate(damagePopUpPrefab, popupPosition, Quaternion.identity);

                if (popupClone.TryGetComponent(out DamagePopUp popupScript))
                {
                    popupScript.Setup(damageAmount, fadein, fadeOut);
                }
            }
        }

        // Destroy bullet no matter what it hits
        Destroy(gameObject);
    }
}
