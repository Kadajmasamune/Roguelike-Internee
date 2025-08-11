using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor.Animations;
using UnityEditor;
#endif

//Fix jitter during Timescale in Player Animations;



//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ PLEASE FIX




[RequireComponent(typeof(Animator))]
public class AnimatorC : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private bool IgnoreTimeScale = false;
    

    private int currentAnimationHash = -1;

    float SpeedMultiplier = 1.0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (IgnoreTimeScale)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.speed = SpeedMultiplier;
        }
        else
        {
            animator.updateMode = default;
            animator.speed = default;
        }
    }

    public void ChangeAnimation(Dictionary<string, int> animatoinDict, int targetHash, float delay = 0.0f, float crossfade = 0.05f)
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
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, delay - crossfade));
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