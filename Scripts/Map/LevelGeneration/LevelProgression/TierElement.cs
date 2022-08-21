using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class TierElement
{
    [Tooltip("Weight of this when randomly selecting. If weight is 0, item is never chosen.")]
    [Range(0f, 1f)]
    public float chanceWeight = 1f;

    public abstract bool IsElementNull();

    public TierElement()
    {
        
    }

    /// <summary>
    /// Has to be called from a monobehaviour onvalidate method from the class holding objects of this class.
    /// </summary>
    public virtual void OnValidate()
    {

    }

    public static TierElement ChooseRandom(List<TierElement> tierElements)
    {
        float totalWeight = tierElements.Sum(t => t.chanceWeight);
        bool foundItem = false;
        float randomChoice = Random.Range(0f, totalWeight);

        foreach (TierElement tierElement in tierElements)
        {
            if (tierElement == null || tierElement.IsElementNull())
            {
                Debug.LogWarning("Null TierElement");
                randomChoice -= tierElement.chanceWeight;
                continue;
            }

            if (randomChoice < tierElement.chanceWeight)
            {
                foundItem = true;
                return tierElement;
            }
            else
            {
                randomChoice -= tierElement.chanceWeight;
            }
        }

        if (foundItem == false)
        {
            Debug.LogWarning("Choice table is empty or only has zero weight elements or some elements are null.");
        }

        return null;
    }
}
