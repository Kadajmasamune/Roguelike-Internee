using System.Collections;
using UnityEngine;

public class Thug_Bullet : MonoBehaviour
{
    private Vector2 velocity;
    private GameObject prefabKey;
    private Rigidbody2D rb;

    float bulletlifetime = 2f;

    private ObjectPooler _objectPooler;

    public void Init(Vector2 velocity , GameObject prefabkey)
    {
        this.velocity = velocity;
        this.prefabKey = prefabkey;
        StopAllCoroutines();
        StartCoroutine(destroyObj(bulletlifetime));
    }

    PlayerController playerController;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        fadeOut = fadein;
        playerController = FindFirstObjectByType<PlayerController>();
        _objectPooler = FindFirstObjectByType<ObjectPooler>();
    }

    IEnumerator destroyObj(float time)
    {
        yield return new WaitForSeconds(time);

        _objectPooler.ReturnObject(prefabKey, gameObject);

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
                    _objectPooler.ReturnObject(prefabKey, gameObject);
                }
            }
        }


    }
}
