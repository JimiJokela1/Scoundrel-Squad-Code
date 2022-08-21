using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObstacleManager))]
public class ObstacleManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ObstacleManager obstacleManager = (ObstacleManager)target;

        if (obstacleManager != null)
        {
            if (GUILayout.Button("Snap to grid"))
            {
                obstacleManager.SnapToGrid<Obstacle>(obstacleManager.cellGrid, obstacleManager.sceneObstacleParents);
            }
        }
    }
}
