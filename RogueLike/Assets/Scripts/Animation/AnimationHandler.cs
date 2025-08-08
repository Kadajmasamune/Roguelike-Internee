using UnityEngine;
using Common;
using System.Collections.Generic;
using UnityEditor.Animations;

public class AnimationHandler : EntityAnimationController
{
    AnimHashGenerator animHashGenerator;
    Dictionary<string, int> ObjAnimationDict;


    [SerializeField] private AnimatorController animatorController;

    private void Awake()
    {
        animHashGenerator = new AnimHashGenerator();
        ObjAnimationDict = new Dictionary<string, int>();

        animHashGenerator.GenerateAnimHash(ObjAnimationDict, animatorController);
    }

    private void Update()
    {
        Animate(ObjAnimationDict);
    }
}
