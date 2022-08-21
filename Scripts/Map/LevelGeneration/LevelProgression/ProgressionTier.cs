using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class ChoiceTier
{
    [Tooltip("Name used in editor to identify the choicetier. Not used in game. Can be left blank.")]
    public string IDName = "";
    [Tooltip("Chance to pick anything at all from this tier's list. 1 is always, 0 is never, 0.5 is 50% chance and so on.")]
    [Range(0f, 1f)]
    public float useListChance = 1f;
    [Tooltip("Minimum number chosen. Actual number is random between min and max.")]
    public uint minChosen = 1;
    [Tooltip("Maximum number chosen. Actual number is random between min and max.")]
    public uint maxChosen = 1;

    public virtual void OnValidate()
    {
        if (minChosen < 0) minChosen = 0;
        if (maxChosen < 0) maxChosen = 0;
        if (maxChosen < minChosen) maxChosen = minChosen;
    }

    protected abstract List<TierElement> GetElements();

    public virtual List<TierElement> ChooseElements()
    {
        List<TierElement> chosenElements = new List<TierElement>();

        List<TierElement> tierElements = GetElements();
        if (tierElements == null || tierElements.Count == 0)
        {
            return chosenElements;
        }

        // Check random chance to use this tier
        if (Random.value < useListChance || useListChance == 1f)
        {
            int min = (int) minChosen;
            // max + 1 because Random.Range with int parameters excludes the maximum
            int max = (int) maxChosen + 1;
            // Choose randomly how many items are chosen
            int itemCount = Random.Range(min, max);

            if (itemCount == 0) return chosenElements;

            for (int i = 0; i < itemCount; i++)
            {
                TierElement chosen = TierElement.ChooseRandom(tierElements);
                if (chosen != null && !chosen.IsElementNull())
                {
                    chosenElements.Add(chosen);
                }
            }
        }

        return chosenElements;
    }
}
