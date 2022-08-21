using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingCrosshair : MonoBehaviour
{
    private static TargetingCrosshair _Instance = null;
    public static TargetingCrosshair Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<TargetingCrosshair>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find singleton instance of TargetingCrosshair");
                }
            }
            return _Instance;
        }
    }

    public GameObject contents;
    public float offset;

    private bool active = false;

    public void Show(UberUnit attacker, UberUnit target)
    {
        int chanceToHit = attacker.GetHitChance(target);

        active = true;
        contents.SetActive(true);
        SetPosition();
        TargetingUI.Instance.Show(chanceToHit);

        UberUnitHover unitHover = target.GetComponent<UberUnitHover>();
        if (unitHover != null)
        {
            unitHover.SetTargetingMode(attacker);
        }
    }

    public void Hide()
    {
        active = false;
        contents.SetActive(false);
        TargetingUI.Instance.Hide();
    }

    private void SetPosition()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + CameraOgre.Instance.transform.forward * offset;
    }

    private void Update()
    {
        if (active)
        {
            SetPosition();
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
