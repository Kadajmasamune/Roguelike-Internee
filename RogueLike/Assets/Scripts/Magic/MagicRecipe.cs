using System;
using UnityEngine;

[Serializable]
public class MagicRecipe
{
    public int Dmg;
    public int Cost;

    public int GetDamage() => Dmg;
    public int GetCost() => Cost;

    public MagicRecipe(int dmg, int cost)
    {
        Dmg = dmg;
        Cost = cost;
    }
}
