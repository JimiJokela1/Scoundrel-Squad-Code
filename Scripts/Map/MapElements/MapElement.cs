using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapElement : MonoBehaviour, IRevealable
{
    public GameObject normalModel;
    public GameObject fadedModel;

    public virtual void Fade()
    {
        if (normalModel != null && fadedModel != null)
        {
            normalModel.SetActive(false);
            fadedModel.SetActive(true);
        }
    }

    public virtual void Unfade()
    {
        if (normalModel != null && fadedModel != null)
        {
            normalModel.SetActive(true);
            fadedModel.SetActive(false);
        }
    }
}
