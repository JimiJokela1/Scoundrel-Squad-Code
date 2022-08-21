using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEffect : EquipmentEffect
{
    public ParticleSystem gunBarrelEffect;
    [Tooltip("Shot count sets the max particles in this effect")]
    public ParticleSystem mainShotEffect;
    public List<ParticleSystem> additionalShotEffects;
    public List<ParticleSystem> targetEffects;

    public override void PlayEffect(Transform sourcePoint, Transform targetPoint, EquipmentEffectArgs e)
    {
        WeaponEffectArgs weaponArgs = e as WeaponEffectArgs;
        if (weaponArgs == null)
        {
            Debug.LogWarning("Effect args null or not of right type.");
            return;
        }

        Vector3 dir = targetPoint.position - sourcePoint.position;

        transform.position = sourcePoint.position;
        transform.rotation = Quaternion.LookRotation(dir);

        if (gunBarrelEffect != null)
        {
            gunBarrelEffect.Play();
        }

        if (mainShotEffect != null)
        {
            ParticleSystem.MainModule main = mainShotEffect.main;
            main.maxParticles = weaponArgs.shotCount;
            mainShotEffect.Play();
        }

        if (additionalShotEffects != null && additionalShotEffects.Count > 0)
        {
            foreach (ParticleSystem effect in additionalShotEffects)
            {
                if (effect == null) continue;
                effect.Play();
            }
        }

        if (targetEffects != null && targetEffects.Count > 0)
        {
            foreach (ParticleSystem effect in targetEffects)
            {
                if (effect == null) continue;
                effect.transform.position = targetPoint.position;
                effect.transform.rotation = Quaternion.LookRotation(-dir);
                effect.Play();
            }
        }

        base.PlayEffect(sourcePoint, targetPoint, e);
    }
}

[System.Serializable]
public class WeaponEffectArgs : EquipmentEffectArgs
{
    public int shotCount;

    public WeaponEffectArgs(int shotCount, List<AudioClip> audioPool) : base(audioPool)
    {
        this.shotCount = shotCount;
    }
}
