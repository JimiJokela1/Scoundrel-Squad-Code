using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    bool failed = false;
    int x = 0;
    int y = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LevelGenerator gen = (LevelGenerator) target;

        // if (GUILayout.Button("Generate New Level"))
        // {
        //     if (Application.isPlaying)
        //     {
        //         gen.ResetLevel();
        //         gen.GenerateNewLevel(new List<PlayerUnit>() { }, 0, 0);
        //         failed = false;
        //     }
        //     else
        //     {
        //         failed = true;
        //     }
        // }

        // if (GUILayout.Button("Clear level"))
        // {
        //     if (Application.isPlaying)
        //     {
        //         gen.ResetLevel();
        //         failed = false;
        //     }
        //     else
        //     {
        //         failed = true;
        //     }
        // }

        if (Application.isPlaying && gen.currentLevelData != null)
        {
            EditorGUILayout.LabelField("Room check utility:");
            x = EditorGUILayout.IntField("X: ", x);
            y = EditorGUILayout.IntField("Y: ", y);
            LevelData.RoomData selectedRoom = gen.currentLevelData.FindRoomContainingPoint(x, y);
            if (selectedRoom != null)
            {
                GUILayout.Box("Room has " + selectedRoom.tiles.Count + " tiles.");
            }
        }

        if (failed)
        {
            EditorGUILayout.HelpBox("Must be in play mode.", MessageType.Error);
        }
    }
}
