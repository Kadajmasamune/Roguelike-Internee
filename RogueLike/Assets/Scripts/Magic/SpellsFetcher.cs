using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class SpellsFetcher : MonoBehaviour
{
    [SerializeField] private string _magicSpellsDataJsonPath= "Assets/StreamingAssets/MagicSpells.json";
    public Dictionary<string, MagicRecipe> ClonedDict { get; private set; }
    private SpellBook _spellBook = new SpellBook();

    public void LoadMagicSpells()
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, _magicSpellsDataJsonPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"MagicSpells file not found at: {fullPath}");
            return;
        }

        string data = File.ReadAllText(fullPath);

        Dictionary<string, MagicRecipe> loadedSpells =
            JsonConvert.DeserializeObject<Dictionary<string, MagicRecipe>>(data);

        _spellBook.magics = loadedSpells;
        ClonedDict = new Dictionary<string, MagicRecipe>(loadedSpells);
    }

    public MagicRecipe GetSpell(string name)
    {
        return ClonedDict != null && ClonedDict.ContainsKey(name) ? ClonedDict[name] : null;
    }
}
