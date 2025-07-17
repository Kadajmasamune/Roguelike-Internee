using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AnimationStateMachine/AnimStateMachine")]
public class AnimStateMachine : ScriptableObject
{
    public List<AnimState> states;
}
