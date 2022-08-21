using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractablePoint : MapElement
{
    public Cell cell;

    public event EventHandler PointClicked;

    public event EventHandler PointHighlighted;

    public event EventHandler PointUnhighlighted;

    public event EventHandler PointDestroyed;

    public event EventHandler PointClosed;

    protected bool inited = false;

    [HideInInspector]
    public bool closeConfirmRequired = false;
    [HideInInspector]
    public string confirmationMessage = "";

    public GameObject highlightIcon;

    private void OnDestroy()
    {
        OnPointDestroyed();
    }

    protected virtual void OnPointDestroyed()
    {
        if (PointDestroyed != null)
        {
            PointDestroyed.Invoke(this, new EventArgs());
        }
    }

    public virtual void OnPointClicked()
    {
        if (PointClicked != null)
        {
            PointClicked.Invoke(this, new EventArgs());
        }
    }

    public virtual void OnPointHighlighted()
    {
        if (highlightIcon != null)
        {
            highlightIcon.SetActive(true);
        }

        if (PointHighlighted != null)
        {
            PointHighlighted.Invoke(this, new EventArgs());
        }
    }

    public virtual void OnPointUnhighlighted()
    {
        if (highlightIcon != null)
        {
            highlightIcon.SetActive(false);
        }

        if (PointUnhighlighted != null)
        {
            PointUnhighlighted.Invoke(this, new EventArgs());
        }
    }

    protected virtual void OnPointClosed()
    {
        if (PointClosed != null)
        {
            PointClosed.Invoke(this, new EventArgs());
        }
    }

    public virtual void Init(Cell cell, InteractablePointContents contents, bool closeConfirmRequired = false, string confirmationMessage = "")
    {
        if (inited) return;
        inited = true;
        this.closeConfirmRequired = closeConfirmRequired;
        this.confirmationMessage = confirmationMessage;
        this.cell = cell;

        if (highlightIcon != null)
        {
            highlightIcon.SetActive(false);
            RotateHighlightIconTowardsCamera();

            CameraOgre.Instance.CameraTurned += RotateHighlightIconTowardsCamera;
        }
    }

    protected void RotateHighlightIconTowardsCamera()
    {
        if (highlightIcon == null) return;
        Quaternion rot = Quaternion.LookRotation(-CameraOgre.Instance.transform.forward);
        highlightIcon.transform.rotation = Quaternion.Euler(highlightIcon.transform.rotation.eulerAngles.x, rot.eulerAngles.y, highlightIcon.transform.rotation.eulerAngles.z);
    }

    public virtual void OnInteractWithPointTriggered(object sender, EventArgs e)
    {
        InteractWithPoint();

        if (sender is Unit)
        {
            ((Unit) sender).UnitMoved -= OnInteractWithPointTriggered;
        }
    }

    public virtual bool IsInteractable()
    {
        return false;
    }

    public virtual void InteractWithPoint()
    {

    }

    public virtual void ClosePoint()
    {
        OnPointClosed();
    }

    public virtual void AddItem(LootableItem item)
    {

    }

    public virtual bool RemoveItem(LootableItem item)
    {
        return false;
    }

    public abstract string GetPointName();
    public abstract string GetDescription();
}
