using UnityEngine;

//To Do: 
// Damage Logic
// Knockback Trajectory
public class LightningScript : MonoBehaviour
{

    public float knockbackForce = 25f; 
    private float KnockbackForceCurved = 4;
    public float damageAmount = 10f; 

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            Rigidbody2D rb = col.attachedRigidbody;
            if (rb != null)
            {
                Vector2 knockbackDirection = (col.transform.position - transform.position).normalized;
                rb.AddForce(knockbackDirection * knockbackForce
                + Vector2.right * KnockbackForceCurved,
                ForceMode2D.Impulse);

                // EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
                // if(enemyHealth != null)
                // {
                //     enemyHealth.TakeDamage(damageAmount);
                // }
               
            }        
        }
    }
}
