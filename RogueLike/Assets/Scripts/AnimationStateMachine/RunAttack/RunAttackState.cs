using UnityEngine;
using Common;

[CreateAssetMenu(menuName = "AnimationStateMachine/RunAttackState")]
public class RunAttackState : AnimState
{
    public Direction providedDirection;
    
    public override bool ShouldEnter(IHasDirection entity, Vector2 velocity, IHasBooleans flags)
    {
        if (flags.IsRolling) return false;

        return flags.IsAttacking && flags.IsRunning && entity.CurrentDirection == providedDirection;
    }
}
