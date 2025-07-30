using UnityEngine;
using Signals;
using Common;

public class PlayerAttack : MonoBehaviour
{
    public Reciever<int> DamageAmount = new Reciever<int>();

    [Header("Damage PopUp")]
    public GameObject damagePopUpPrefab; // assign the prefab with the DamagePopUp script attached

    void OnTriggerEnter2D(Collider2D col)
    {
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
                        popupScript.Setup(damage, 0.45f, 0.45f);
                    }
                }
            }
        }
    }
}
