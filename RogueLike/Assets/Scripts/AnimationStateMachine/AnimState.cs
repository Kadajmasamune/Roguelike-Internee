using UnityEngine;
using Common;

[CreateAssetMenu(menuName = "AnimationStateMachine/AnimState")]
public abstract class AnimState : ScriptableObject
{
    public string AnimationName;
    public int Priority = 0; 

    public abstract bool ShouldEnter(IHasDirection entity, Vector2 velocity , IHasBooleans flags);
}
