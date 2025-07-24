using UnityEngine;
using Signals;

public class PlayerAttack : MonoBehaviour
{
    public Reciever<int> DamageAmount = new Reciever<int>();

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Hurtable"))
        {
            Rigidbody2D rb = col.attachedRigidbody;
            if (rb != null)
            {
        
                Health TargetHealth = col.GetComponent<Health>();
                if (TargetHealth != null)
                {
                    TargetHealth.TakeDamage(DamageAmount.ReceivedData);
                }
               
            }        
        }
    }
}   
