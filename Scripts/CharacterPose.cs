using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPose : MonoBehaviour
{
    public Transform copyPoseFromRoot;
    public Transform copyPoseToRoot;

    public void CopyPose()
    {
        if (copyPoseFromRoot == null || copyPoseToRoot == null)
        {
            Debug.LogWarning("Assign both from and to roots");
            return;
        }

        foreach(Transform copyFromT in copyPoseFromRoot.GetComponentsInChildren<Transform>())
        {
            Transform copyToT = FindDeepChild(copyFromT.name, copyPoseToRoot);

            if (copyToT == null)
            {
                Debug.LogError("Rigs don't match. Make sure all bones are in both Rigs. Cannot find: " + copyFromT.name);
                return;
            }

            copyToT.position = copyFromT.position;
            copyToT.rotation = copyFromT.rotation;
            copyToT.localScale = copyFromT.localScale;
        }
    }

    private Transform FindDeepChild(string name, Transform t)
    {
        if (t.name == name)
        {
            return t;
        }
        else
        {
            foreach(Transform tChild in t)
            {
                Transform ret = FindDeepChild(name, tChild);
                if (ret != null)
                {
                    return ret;
                }
            }
        }

        return null;
    }
}
