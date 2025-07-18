using Common;
using UnityEngine;

[CreateAssetMenu(menuName = "AnimationStateMachine/RunBackWardsState")]
public class RunBackWardsState : AnimState
{
    public Direction providedDirection;
    
    public override bool ShouldEnter(IHasDirection entity, Vector2 velocity, IHasBooleans flags)
    {
        if (flags.IsRolling || flags.IsAttacking) return false;

        return entity.CurrentDirection == providedDirection && flags.IsRunningBackwards;
    }
}
