using System.IO;
using Unity.VisualScripting.YamlDotNet.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

public class AnimationStateMachine_StatePlugger : EditorWindow
{
    private AnimStateMachine AnimationStateMachine;
    private DefaultAsset TargetFolder;

    [MenuItem("Tools/ Plug In Animation States in Animation State Machines")]
    public static void ShowWindow()
    {
        GetWindow<AnimationStateMachine_StatePlugger>("Plug In Animation States");
    }

    private void OnGUI()
    {
        GUILayout.Label("Plug In Animation State Objects To Animation State Machine Object", EditorStyles.boldLabel);


        AnimationStateMachine = (AnimStateMachine)EditorGUILayout.ObjectField("Target Animation State Machine",
        AnimationStateMachine,
        typeof(AnimStateMachine),
        false);


        TargetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Target Folder that Contains Animation State Objects",
        TargetFolder,
        typeof(DefaultAsset),
        false);


        if (GUILayout.Button("Plug In Animation States"))
        {
            if (TargetFolder == null || AnimationStateMachine == null)
            {
                Debug.LogError("Error, one of the given objects is missing");
                return;
            }

            string folderPath = AssetDatabase.GetAssetPath(TargetFolder);
            PlugInAnimationStateObjects(folderPath, AnimationStateMachine);
        }
    }


    public static void PlugInAnimationStateObjects(string folderPath, AnimStateMachine animStateMachine)
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimState", new[] { folderPath });
        int count = 0;

        if (animStateMachine.states == null)
            animStateMachine.states = new System.Collections.Generic.List<AnimState>();
        else
            animStateMachine.states.Clear();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".asset")) continue;

            AnimState animationState = AssetDatabase.LoadAssetAtPath<AnimState>(path);
            if (animationState == null) continue;

            animStateMachine.states.Add(animationState);
            count++;
        }

        EditorUtility.SetDirty(animStateMachine);
        AssetDatabase.SaveAssets();
        Debug.Log($"Plugged in {count} animation states to the state machine.");
    }
}
