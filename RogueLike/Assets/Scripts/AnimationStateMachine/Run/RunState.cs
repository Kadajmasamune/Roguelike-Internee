using UnityEngine;
using Common;

[CreateAssetMenu(menuName = "AnimationStateMachine/RunState")]
public class RunState : AnimState
{
    public Direction requiredDirection;
    public float minSpeed = 0.1f;
    
    public override bool ShouldEnter(IHasDirection entity, Vector2 velocity , IHasBooleans flags)
    {
        if (flags.IsRolling) return false;
        
        return entity.CurrentDirection == requiredDirection && velocity.magnitude > minSpeed;
    }
}
