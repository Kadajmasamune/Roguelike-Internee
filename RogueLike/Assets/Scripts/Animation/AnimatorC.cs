using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations; // EDITOR-ONLY CODE, SHOULD BE IN #if UNITY_EDITOR
using Common;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Animator))]
public class AnimatorC : MonoBehaviour
{
    private Animator animator;
    private int currentAnimationHash = -1;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ChangeAnimation(Dictionary<string, int> animationDict, int targetHash, float delay = 0.0f, float crossfade = 0.05f , float SpeedMultiplier = 1.0f)
    {
        if (currentAnimationHash == targetHash) return;

        if (delay > 0f)
        {
            StartCoroutine(WaitAndPlay());
        }
        else
        {
            Play();
        }

        IEnumerator WaitAndPlay()
        {
            yield return new WaitForSeconds(delay - crossfade);
            Play();
        }

        void Play()
        {
            animator.CrossFadeInFixedTime(targetHash, crossfade);
            currentAnimationHash = targetHash;
        }
    }

#if UNITY_EDITOR
    public AnimatorController CreateAnimatorController(string assetPath)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
        if (existing != null)
        {
            Debug.Log($"Animator Controller already exists at: {assetPath}");
            return existing;
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(assetPath);
        Debug.Log($"Created Animator Controller at: {assetPath}");
        return controller;
    }

    public AnimationClip GenerateAnimationClip(Sprite[] frames, string animationName, float frameGap, string savePath, AnimatorController controller)
    {
        var clip = new AnimationClip();
        var keyframes = new ObjectReferenceKeyframe[frames.Length];

        for (int i = 0; i < frames.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * frameGap,
                value = frames[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(
            clip,
            EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"),
            keyframes
        );

        clip.name = animationName;

        string fullPath = System.IO.Path.Combine(savePath, animationName + ".anim");

        AssetDatabase.CreateAsset(clip, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        controller.AddMotion(clip);
        return clip;
    }
#endif
}