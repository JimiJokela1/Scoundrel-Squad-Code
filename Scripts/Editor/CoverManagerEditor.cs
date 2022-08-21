using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CoverManager))]
public class CoverManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CoverManager coverManager = (CoverManager)target;

        if (coverManager != null)
        {
            if (GUILayout.Button("Snap to grid"))
            {
                coverManager.SnapToGrid<Obstacle>(coverManager.cellGrid, coverManager.sceneCoverParents);
            }
        }
    }
}
