using Common;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/MeleeTwoState")]
public class MeleeTwoState : AnimState
{
    public Direction providedDirection;

    public override bool ShouldEnter(IHasDirection entity, Vector2 velocity, IHasBooleans flags)
    {
        
        throw new System.NotImplementedException();
    }
}
