using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameMaster))]
public class GameMasterEditor : Editor
{
    private bool failed = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GameMaster gameMaster = (GameMaster) target;

        if (GUILayout.Button("Generate New Level"))
        {
            if (Application.isPlaying)
            {
                gameMaster.ElevatorActivated(true);
                failed = false;
            }
            else
            {
                failed = true;
            }
        }

        if (failed)
        {
            EditorGUILayout.HelpBox("Must be in play mode.", MessageType.Error);
        }
    }
}
