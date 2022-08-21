using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progress/Level Generator Progress", fileName = "LevelGeneratorProgress")]
public class LevelGeneratorProgress : ScriptableObject
{
    public int activateLevel;
    public List<LevelGeneratorTier> generatorTable;

    public LevelGeneratorFormula ChooseLevelGenerator()
    {
        TierElement chosen = TierElement.ChooseRandom(new List<TierElement>(generatorTable));

        if (chosen != null && chosen is LevelGeneratorTier)
        {
            LevelGeneratorTier tier = (LevelGeneratorTier) chosen;
            return (tier.generator);
        }

        return null;
    }

    public LevelGeneratorProgress()
    {
        generatorTable = new List<LevelGeneratorTier>()
        {
            new LevelGeneratorTier()
        };
    }

    private void Reset()
    {
        generatorTable = new List<LevelGeneratorTier>()
        {
            new LevelGeneratorTier()
        };
    }
}

[System.Serializable]
public class LevelGeneratorTier : TierElement
{
    public LevelGeneratorFormula generator;

    public override bool IsElementNull()
    {
        return generator == null;
    }
}
