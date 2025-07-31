using UnityEngine;
using Signals;
using Common;

public class PlayerAttack : MonoBehaviour
{
    public Reciever<int> DamageAmount = new Reciever<int>();

    [Header("Damage PopUp")]
    public GameObject damagePopUpPrefab;

    public float fadein;
    private float fadeOut;


    void OnTriggerEnter2D(Collider2D col)
    {
        fadeOut = fadein;


        if (col.gameObject.layer == LayerMask.NameToLayer("Hurtable"))
        {
            Rigidbody2D rb = col.attachedRigidbody;
            if (rb != null)
            {
                Health targetHealth = col.GetComponent<Health>();
                if (targetHealth != null)
                {
                    int damage = DamageAmount.ReceivedData;
                    targetHealth.TakeDamage(damage);


                    Vector3 popupPosition = col.ClosestPoint(col.transform.position);
                    GameObject popupClone = Instantiate(damagePopUpPrefab, popupPosition, Quaternion.identity);

                    DamagePopUp popupScript = popupClone.GetComponent<DamagePopUp>();
                    if (popupScript != null)
                    {
                        popupScript.Setup(damage, fadein, fadeOut);
                    }
                }
            }
        }
    }
}
