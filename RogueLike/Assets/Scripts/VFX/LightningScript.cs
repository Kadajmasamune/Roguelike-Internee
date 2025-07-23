using UnityEngine;

//To Do: 
// Damage Logic
// Knockback Trajectory
public class LightningScript : MonoBehaviour
{

    // public float knockbackForce = 25f; 
    // private float KnockbackForceCurved = 4;
    public float damageAmount; 

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
                    TargetHealth.TakeDamage(damageAmount);
                }
               
            }        
        }
    }
}
