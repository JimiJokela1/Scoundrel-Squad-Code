using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnit : UberUnit
{
    public Color PlayerColor;
    public Renderer unitRenderer;
    public Renderer highlighter;

    [HideInInspector]
    public NPCUnitData unitData;

    private float offset = 1f;

    public override void Initialize()
    {
        base.Initialize();

        SetColor(PlayerColor);
        // gameObject.transform.localPosition = Cell.transform.localPosition + new Vector3(0, offset, 0);
    }

    public override string ToDescription()
    {
        string desc = base.ToDescription();

        Player player = GameMap.Instance.cellGrid.GetPlayer(this);
        if (player != null && player is AIPlayer)
        {
            AIUnit aiUnit = ((AIPlayer) player).GetAIUnit(this);
            if (aiUnit != null)
            {
                desc += System.Enum.GetName(typeof(AIUnit.AIUnitState), aiUnit.state) + "\n";
            }
        }

        desc += "\n";

        if (unitData != null)
        {
            desc += unitData.description;
        }

        return desc;
    }

    public override bool EquipItem(Equipment equipment, bool log = true)
    {
        if (base.EquipItem(equipment))
        {
            // TODO: check if enemy visible and only log if visible
            if (log)
            {
                GameLog.Instance.AddMessage(GetUnitName() + " equipped " + equipment.GetItemName() + ".");
            }
            return true;
        }
        return false;
    }

    public override bool EquipAmmoStack(AmmoStack ammoStack, bool log = true)
    {
        if (base.EquipAmmoStack(ammoStack, false))
        {
            // TODO: check if enemy visible and only log if visible
            if (log)
            {
                GameLog.Instance.AddMessage(GetUnitName() + " picked up " + ammoStack.ammoCount + " " + ammoStack.GetItemName() + ".");
            }
            return true;
        }
        return false;
    }

    public override string GetUnitName()
    {
        if (unitData != null)
        {
            return unitData.characterName;
        }

        return "Glorbos";
    }

    public override void MarkAsAttacking(Unit other)
    {
        StartCoroutine(Jerk(other));
    }

    public override void MarkAsDefending(Unit other)
    {
        StartCoroutine(Glow(new Color(1, 0.5f, 0.5f), 1));
    }

    public override void MarkAsDestroyed() { }

    private IEnumerator Jerk(Unit other)
    {
        var heading = other.transform.localPosition - transform.localPosition;
        var direction = heading / heading.magnitude;
        float startTime = Time.time;

        while (startTime + 0.25f > Time.time)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + (direction / 2.5f), ((startTime + 0.25f) - Time.time));
            yield return 0;
        }
        startTime = Time.time;
        while (startTime + 0.25f > Time.time)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition - (direction / 2.5f), ((startTime + 0.25f) - Time.time));
            yield return 0;
        }
        transform.localPosition = Cell.transform.localPosition + new Vector3(0, offset, 0);;
    }

    private IEnumerator Glow(Color color, float cooloutTime)
    {
        float startTime = Time.time;

        while (startTime + cooloutTime > Time.time)
        {
            SetColor(Color.Lerp(PlayerColor, color, (startTime + cooloutTime) - Time.time));
            yield return 0;
        }

        SetColor(PlayerColor);
    }

    public override void MarkAsFriendly()
    {
        SetHighlighterColor(new Color(0.8f, 1, 0.8f));
    }

    public override void MarkAsReachableEnemy()
    {
        SetHighlighterColor(Color.red);
    }

    public override void MarkAsSelected()
    {
        SetHighlighterColor(new Color(0, 1, 0));
    }

    public override void MarkAsFinished()
    {
        SetColor(PlayerColor - Color.gray);
        SetHighlighterColor(new Color(0.8f, 1, 0.8f));
    }

    public override void UnMark()
    {
        SetColor(PlayerColor);
        SetHighlighterColor(Color.white);
    }

    private void SetColor(Color color)
    {
        if (unitRenderer != null)
        {
            unitRenderer.material.color = color;
        }
    }

    private void SetHighlighterColor(Color color)
    {
        if (highlighter != null)
        {
            highlighter.material.color = color;
        }
    }
}
