using UnityEngine;

public class MagicManager : MonoBehaviour
{
    [SerializeField] private SpellsFetcher spellsFetcher;

    public MagicRecipe bolt;
    public MagicRecipe darkBolt;
    void Start()
    {
        if (spellsFetcher == null)
        {
            Debug.LogError("SpellsFetcher not assigned in Inspector!");
            return;
        }

        spellsFetcher.LoadMagicSpells();

        MagicRecipe bolt = spellsFetcher.GetSpell("Bolt");
        MagicRecipe darkBolt = spellsFetcher.GetSpell("DarkBolt");
        
    }
}
