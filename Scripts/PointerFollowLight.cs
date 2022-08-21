using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerFollowLight : MonoBehaviour
{
    public Transform followLightTransform;
    public float lightHeight;

    private void Start()
    {
        if (followLightTransform == null)
        {
            Debug.LogWarning("Assign follow light transform. Otherwise pointer follow light will not do anything.");
        }
    }

    private void Update()
    {
        if (followLightTransform == null) return;

        RaycastHit hit;
        Ray mousePosRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = LayerMask.GetMask("FollowLightPlane");

        if (Physics.Raycast(mousePosRay, out hit, 300f, mask))
        {
            followLightTransform.position = new Vector3(hit.point.x, lightHeight, hit.point.z);
        }
    }
}
