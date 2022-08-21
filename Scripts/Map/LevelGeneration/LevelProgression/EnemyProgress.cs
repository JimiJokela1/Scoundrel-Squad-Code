using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progress/EnemyProgress", fileName = "EnemyProgress")]
public class EnemyProgress : ScriptableObject
{
    public int activateLevel;
    public List<EnemyTier> enemyPool;

    public List<GameObject> ChooseEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();
        foreach (EnemyTier tier in enemyPool)
        {
            List<TierElement> chosen = tier.ChooseElements();
            foreach (TierElement element in chosen)
            {
                if (element is EnemyWithWeight)
                {
                    enemies.Add(((EnemyWithWeight) element).enemy);
                }
            }
        }
        return enemies;
    }

    private void Reset()
    {
        enemyPool = new List<EnemyTier>()
        {
            new EnemyTier()
        };
    }

    protected virtual void OnValidate()
    {
        if (enemyPool != null && enemyPool.Count > 0)
        {
            foreach (var tier in enemyPool)
            {
                tier.OnValidate();
            }
        }
    }
}

[System.Serializable]
public class EnemyTier : ChoiceTier
{
    [Tooltip("List of items with chance weights")]
    public List<EnemyWithWeight> enemyTable;

    public EnemyTier()
    {
        enemyTable = new List<EnemyWithWeight>()
        {
            new EnemyWithWeight()
        };
    }

    protected override List<TierElement> GetElements()
    {
        return new List<TierElement>(enemyTable);
    }
}

[System.Serializable]
public class EnemyWithWeight : TierElement
{
    public GameObject enemy;

    public override bool IsElementNull()
    {
        return enemy == null;
    }
}
