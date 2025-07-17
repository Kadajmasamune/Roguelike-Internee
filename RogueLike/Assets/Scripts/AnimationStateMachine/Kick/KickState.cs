using Common;
using UnityEngine;

[CreateAssetMenu(menuName = "AnimationStateMachine/KickState")]
public class KickState : AnimState
{
    public Direction providedDirection;

    public override bool ShouldEnter(IHasDirection entity, Vector2 velocity, IHasBooleans flags)
    {
        if (flags.IsCasting || flags.IsRolling) return false;

        return entity.CurrentDirection == providedDirection && flags.IsRunning && flags.IsHeavyAttacking;
    }
}
