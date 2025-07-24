using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnimationClipRescaler : EditorWindow
{
    private float scaleFactor = 0.7f;
    private DefaultAsset targetFolder;

    [MenuItem("Tools/Rescale .anim Clips in Folder")]
    public static void ShowWindow()
    {
        GetWindow<AnimationClipRescaler>("Rescale .anim Clips");
    }

    private void OnGUI()
    {
        GUILayout.Label("Rescale .anim Clips (Not FBX)", EditorStyles.boldLabel);
        scaleFactor = EditorGUILayout.Slider("Scale Factor", scaleFactor, 0.1f, 2f);
        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Target Folder", targetFolder, typeof(DefaultAsset), false);

        if (GUILayout.Button("Rescale Clips"))
        {
            if (targetFolder == null)
            {
                Debug.LogError("You must select a folder containing .anim clips.");
                return;
            }

            string folderPath = AssetDatabase.GetAssetPath(targetFolder);
            RescaleAnimClipsInFolder(folderPath, scaleFactor);
        }
    }

    private static void RescaleAnimClipsInFolder(string folderPath, float scale)
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { folderPath });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".anim")) continue;

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) continue;

            Undo.RegisterCompleteObjectUndo(clip, "Rescale Animation Clip");

            // Handle regular keyframe curves
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve == null) continue;

                Keyframe[] newKeys = new Keyframe[curve.length];
                for (int i = 0; i < curve.length; i++)
                {
                    var key = curve.keys[i];
                    newKeys[i] = new Keyframe(
                        key.time * scale,
                        key.value,
                        key.inTangent,
                        key.outTangent
                    );
                }

                curve.keys = newKeys;
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }

            // Handle object reference curves (e.g., animator events, visibility toggles)
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                for (int i = 0; i < keyframes.Length; i++)
                    keyframes[i].time *= scale;

                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            }

            // Rescale animation events
            var events = AnimationUtility.GetAnimationEvents(clip);
            foreach (var evt in events)
                evt.time *= scale;

            AnimationUtility.SetAnimationEvents(clip, events);

            // Mark and save
            EditorUtility.SetDirty(clip);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(); // Forces internal recalculation
        Debug.Log($"✅ Rescaled {count} .anim clip(s) in folder: {folderPath}");
    }
}
