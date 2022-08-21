using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveEquipmentData : EquipmentData
{
    [Tooltip("Prefab with all the effects for when equipment is activated. Prefab must have EquipmentEffect component.")]
    public GameObject effectPrefab;

    [Tooltip("One is chosen at random to play when equipment is activated")]
    public List<AudioClip> useEffectSounds;

    
    public override void Activate(Unit activatingUnit, Equipment equipment)
    {
        PlayEffects(activatingUnit, activatingUnit, 0);
    }
    
    public override void Activate(Unit activatingUnit, Unit targetUnit, Equipment equipment)
    {
        PlayEffects(activatingUnit, targetUnit, 0);
    }
    
    public override void Activate(Unit activatingUnit, Cell targetCell, Equipment equipment)
    {
        PlayEffects(activatingUnit, activatingUnit, 0);
    }

    protected virtual void PlayEffects(Unit activatingUnit, Unit targetUnit, int shots)
    {
        if (effectPrefab == null)
        {
            return;
        }

        GameObject effectsObj = Instantiate(effectPrefab, activatingUnit.transform);
        EquipmentEffect effects = effectsObj.GetComponent<EquipmentEffect>();
        if (effects == null)
        {
            Destroy(effectsObj);
            Debug.LogWarning("Equipment effect prefab must have EquipmentEffect component attached");
            return;
        }

        if (activatingUnit is UberUnit && targetUnit is UberUnit)
        {
            UberUnit unit = (UberUnit) activatingUnit;
            UberUnit target = (UberUnit) targetUnit;
            if (unit.gunBarrelPoint != null && target.targetPoint != null)
            {
                effects.PlayEffect(unit.gunBarrelPoint, target.targetPoint, new EquipmentEffectArgs(useEffectSounds));
            }
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        activationType = EquipmentActivationType.Active;
    }

    public override string ToDescription()
    {
        return base.ToDescription();
    }
}
