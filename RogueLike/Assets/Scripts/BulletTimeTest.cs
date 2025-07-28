using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class BulletTimeTest : MonoBehaviour
{

    MagicSpells.SpellBook magicSpells;
    public GameObject pfDarkBolt;
    public GameObject Player;
    SFXManager sFXManager;
    PlayerController playerController;
    public GameObject Troll;
    Vector3 newpos;

    void Start()
    {
        sFXManager = FindFirstObjectByType<SFXManager>();
        playerController = FindFirstObjectByType<PlayerController>();
        newpos = new Vector3(5, 5, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) == true)
        {
            StartCoroutine(UpdateBulletTime());
        }
        else if (Input.GetKeyDown(KeyCode.E) == true)
        {
            Instantiate(Troll, playerController.transform.position + newpos, quaternion.identity);
        }
    }

    private bool ShouldEnterBulletTime()
    {
        return false;
    }

    private IEnumerator UpdateBulletTime()
    {
        yield return null;
        StartCoroutine(Cast());
    }

    private IEnumerator Cast()
    {
        GameObject spell = Instantiate(pfDarkBolt, Player.transform.position, quaternion.identity);
        sFXManager.PlaySFX(sFXManager.DarkBoltSFX , 1);
        yield return new WaitForSeconds(0.8f);
        Destroy(spell);
    }
}
