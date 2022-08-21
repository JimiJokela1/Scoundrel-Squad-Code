using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitWorldUI : MonoBehaviour
{
    public ProgressBar healthBar;
    public ProgressBar armorBar;

    private bool initialized = false;

    public void Init(int maxBarBlockCount, UberUnit unit)
    {
        if(initialized)return;
        initialized = true;

        if (healthBar != null) healthBar.Init(maxBarBlockCount);
        if (armorBar != null) armorBar.Init(maxBarBlockCount);
        UpdateBars(unit);

        // Not enough because we need to update it whenever the unit turns on the map too so just do it in update
        // RotateTowardsCamera();
        // CameraOgre.Instance.CameraTurned += RotateTowardsCamera;
    }

    private void Update()
    {
        RotateTowardsCamera();
    }

    private void RotateTowardsCamera()
    {
        Quaternion rot = Quaternion.LookRotation(-CameraOgre.Instance.transform.forward);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rot.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    public void UpdateBars(UberUnit unit)
    {
        if (!initialized) return;

        if (healthBar != null)
        {
            healthBar.SetEmptyBlockAmount(unit.TotalHitPoints);
            healthBar.SetFilledBlockAmount(unit.HitPoints);
        }
        if (armorBar != null)
        {
            armorBar.SetEmptyBlockAmount(unit.TotalArmor);
            armorBar.SetFilledBlockAmount(unit.currentArmor);
        }
    }
}
