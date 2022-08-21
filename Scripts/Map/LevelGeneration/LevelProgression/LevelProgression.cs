using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progress/LevelProgression", fileName = "LevelProgression")]
public partial class LevelProgression : ScriptableObject
{
    public List<LootProgress> lootProgress;
    public List<ShopProgress> shopProgress;
    public List<ChargePointProgress> chargePointProgress;
    public List<EnemyProgress> enemyProgress;
    public List<LevelGeneratorProgress> levelGeneratorProgress;

    [Tooltip("Player needs to finish this level to win")]
    public int winLevel;

    private void OnValidate()
    {
        if (winLevel < 0) winLevel = 0;
    }

    public LootProgress GetActiveLootProgress(int level)
    {
        int closestLevel = int.MinValue;
        LootProgress closestProgress = null;
        foreach(LootProgress progress in lootProgress)
        {
            if (progress.activateLevel <= level && progress.activateLevel > closestLevel)
            {
                closestLevel = progress.activateLevel;
                closestProgress = progress;
            }
        }

        return closestProgress;
    }

    public ShopProgress GetActiveShopProgress(int level)
    {
        int closestLevel = int.MinValue;
        ShopProgress closestProgress = null;
        foreach(ShopProgress progress in shopProgress)
        {
            if (progress.activateLevel <= level && progress.activateLevel > closestLevel)
            {
                closestLevel = progress.activateLevel;
                closestProgress = progress;
            }
        }

        return closestProgress;
    }

    public ChargePointProgress GetActiveChargePointProgress(int level)
    {
        int closestLevel = int.MinValue;
        ChargePointProgress closestProgress = null;
        foreach(ChargePointProgress progress in chargePointProgress)
        {
            if (progress.activateLevel <= level && progress.activateLevel > closestLevel)
            {
                closestLevel = progress.activateLevel;
                closestProgress = progress;
            }
        }

        return closestProgress;
    }

    public EnemyProgress GetActiveEnemyProgress(int level)
    {
        int closestLevel = int.MinValue;
        EnemyProgress closestProgress = null;
        foreach(EnemyProgress progress in enemyProgress)
        {
            if (progress.activateLevel <= level && progress.activateLevel > closestLevel)
            {
                closestLevel = progress.activateLevel;
                closestProgress = progress;
            }
        }

        return closestProgress;
    }

    public LevelGeneratorProgress GetActiveLevelGeneratorProgress(int level)
    {
        int closestLevel = int.MinValue;
        LevelGeneratorProgress closestProgress = null;
        foreach(LevelGeneratorProgress progress in levelGeneratorProgress)
        {
            if (progress.activateLevel <= level && progress.activateLevel > closestLevel)
            {
                closestLevel = progress.activateLevel;
                closestProgress = progress;
            }
        }

        return closestProgress;
    }
}
