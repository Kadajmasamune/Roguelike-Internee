using Common;
using UnityEngine;

[CreateAssetMenu(menuName = "AnimationStateMachine/CastState")]
public class CastState : AnimState
{
    public Direction providedDirection;


    public override bool ShouldEnter(IHasDirection entity, Vector2 velocity, IHasBooleans flags)
    {
        if (flags.IsAttacking || flags.IsRolling) return false;

        return entity.CurrentDirection == providedDirection && flags.IsCasting;
    }
}
