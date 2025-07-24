using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float health;
    public bool TakingDamage = false;
    public bool Dead = false;

    private float OnDestroyWaitTime = 1.5f;


    GameObject CurrentGameobject;
    // EntityAnimationController currentGameObject;

    void Start()
    {
        // currentGameObject = GetComponent<EntityAnimationController>();
        CurrentGameobject = this.gameObject;
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        TakingDamage = true;

        if (health > 0)
        {
            StartCoroutine(ResetTakingDamage(0.1f));
            Debug.Log("Hurt");
        }

        if (health <= 0 && !Dead)
        {
            Dead = true;
            StartCoroutine(Die());
        }
    }

    IEnumerator ResetTakingDamage(float duration)
    {
        yield return new WaitForSeconds(duration);
        TakingDamage = false;
    }

    IEnumerator Die()
    {
        TakingDamage = false;
        yield return new WaitForSeconds(OnDestroyWaitTime);
        Destroy(CurrentGameobject); 
    }
}