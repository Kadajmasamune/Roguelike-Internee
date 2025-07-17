using Common;
using UnityEngine;

[CreateAssetMenu(menuName = "AnimationStateMachine/IdleState")]
public class IdleState : AnimState
{
    public Direction requiredDirection;

    public override bool ShouldEnter(IHasDirection entity, Vector2 velocity , IHasBooleans flags)
    {
        if (flags.IsRolling) return false;
        
        return entity.CurrentDirection == requiredDirection && velocity.magnitude < 0.01f;
    }

}
