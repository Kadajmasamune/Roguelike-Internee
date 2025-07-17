using Common;
using UnityEngine;

[CreateAssetMenu(menuName = "AnimationStateMachine/MeleeOne")]
public class MeleeOneState : AnimState
{
    public Direction providedDirection;

    public override bool ShouldEnter(IHasDirection entity, Vector2 velocity, IHasBooleans flags)
    {
        if (flags.IsRolling || flags.IsHeavyAttacking || flags.IsCasting) return false;

        return entity.CurrentDirection == providedDirection && flags.IsAttacking && flags.IsLockedOn;
    }
}
