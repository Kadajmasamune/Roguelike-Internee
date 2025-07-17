using UnityEngine;
using Common;


[CreateAssetMenu(menuName = "AnimationStateMachine/RollState")]
public class RollState : AnimState
{
    public Direction requiredDirection;
    public float minSpeed = 0.1f;

    public override bool ShouldEnter(IHasDirection entity, Vector2 velocity , IHasBooleans flags)
    {
        return flags.IsRolling && entity.CurrentDirection == requiredDirection && velocity.magnitude > 0.01f ;
    }
}
