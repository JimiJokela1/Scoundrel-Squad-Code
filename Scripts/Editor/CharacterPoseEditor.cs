using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterPose))]
public class CharacterPoseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CharacterPose pose = (CharacterPose)target;

        if (pose != null)
        {
            if (GUILayout.Button("Copy pose"))
            {
                Undo.RecordObjects(pose.copyPoseToRoot.GetComponentsInChildren<Transform>(), "Copied pose");
                pose.CopyPose();
            }
        }
    }
}
