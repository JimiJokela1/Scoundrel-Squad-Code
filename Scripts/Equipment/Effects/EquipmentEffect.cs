using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentEffect : MonoBehaviour
{
    // public List<ParticleSystem> sourceEffects;
    // public List<ParticleSystem> targetEffects;

    public AudioSource audioSource;
    // [Tooltip("One sound is chosen at random")]
    // public List<AudioClip> sounds;

    public virtual void PlayEffect(Transform sourcePoint, Transform targetPoint, EquipmentEffectArgs e)
    {
        // Vector3 dir = targetPoint.position - sourcePoint.position;

        // foreach(ParticleSystem effect in sourceEffects)
        // {
        //     effect.transform.position = sourcePoint.position;
        //     effect.transform.rotation = Quaternion.LookRotation(dir);
        //     effect.Play();
        // }

        // foreach(ParticleSystem effect in targetEffects)
        // {
        //     effect.transform.position = targetPoint.position;
        //     effect.transform.rotation = Quaternion.LookRotation(-dir);
        //     effect.Play();
        // }

        if (e.audioPool != null && e.audioPool.Count > 0)
        {
            audioSource.clip = e.audioPool[Random.Range(0, e.audioPool.Count)];
            audioSource.Play();
        }

        Destroy(gameObject, 15f);
    }
}

[System.Serializable]
public class EquipmentEffectArgs
{
    [Tooltip("One clip is chosen at random")]
    public List<AudioClip> audioPool;

    public EquipmentEffectArgs(List<AudioClip> audioPool)
    {
        this.audioPool = audioPool;
    }
}
