using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingLaser : MonoBehaviour
{
    private static TargetingLaser _Instance = null;
    public static TargetingLaser Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<TargetingLaser>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find singleton instance of TargetingLaser");
                }
            }
            return _Instance;
        }
    }

    public LineRenderer lineRenderer;
    public float targetingHeight;
    public Color validTargetLaserColor;
    public Color invalidTargetLaserColor;

    private UberUnit activeUnit;
    private bool shown = false;


    private void Update()
    {
        if (shown)
        {
            SetLinePositions();
        }
    }

    public void Show(UberUnit activeUnit)
    {
        shown = true;
        this.activeUnit = activeUnit;
        lineRenderer.enabled = true;
        SetLinePositions();
    }

    public void Hide()
    {
        shown = false;
        this.activeUnit = null;
        lineRenderer.enabled = false;
    }

    private void SetLinePositions()
    {
        // Set start position of laser to shooting units gun barrel
        lineRenderer.SetPosition(0, activeUnit.gunBarrelPoint.position);
        
        // Find end position of laser where mouse overlaps the game world (TargetingLaserPlane)
        RaycastHit hit;
        Ray mousePosRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = LayerMask.GetMask("TargetingLaserPlane");

        if (Physics.Raycast(mousePosRay, out hit, 300f, mask))
        {
            lineRenderer.enabled = true;
            Vector3 targetPoint = new Vector3(hit.point.x, targetingHeight, hit.point.z);
            lineRenderer.SetPosition(1, targetPoint);

            // Rotate unit to aiming direction
            targetPoint.y = 0f;
            Vector3 unitPos = activeUnit.transform.position;
            unitPos.y = 0f;
            Vector3 dir = targetPoint - unitPos;
            dir.y = 0f;
            dir = dir.normalized;
            activeUnit.ForceRotate(dir);
        }
        else
        {
            // No hit so turn off laser
            lineRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Colors laser to green if target cell is a valid target or red if not
    /// </summary>
    /// <param name="isTargetableCell"></param>
    public void CellSelected(bool isTargetableCell)
    {
        if (isTargetableCell)
        {
            lineRenderer.material.color = validTargetLaserColor;
            lineRenderer.material.SetColor("_EmissionColor", validTargetLaserColor);
        }
        else
        {
            lineRenderer.material.color = invalidTargetLaserColor;
            lineRenderer.material.SetColor("_EmissionColor", invalidTargetLaserColor);
        }
    }

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }
        else if (_Instance != this)
        {
            Debug.Log("Multiple Singleton Instance GameObjects in scene. Class: " + this.GetType().ToString() + " in GameObject: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Hide();
    }
}
