using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private static EnemySpawner _Instance = null;
    public static EnemySpawner Instance
    {
        get
        {
            if (_Instance != null)
            {
                return _Instance;
            }
            else
            {
                _Instance = FindObjectOfType<EnemySpawner>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find Instance of EnemySpawner in scene");
                }
                return _Instance;
            }
        }
    }

    public Transform unitParent;
    public List<GameObject> enemyPrefabs;

    public List<NPCUnitData> npcDataPool;

    public List<Unit> SpawnEnemies(EnemyProgress enemyProgress, List<Cell> cells)
    {
        // if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        // {
        //     Debug.LogError("Assign enemy prefabs in EnemySpawner");
        //     return new List<Unit>();
        // }

        List<GameObject> enemies = enemyProgress.ChooseEnemies();
        int count = enemies.Count;

        List<Cell> freeCells = cells.FindAll(c => !c.BlocksMovement && !GameMap.Instance.IsCellInElevator(c));
        if (freeCells.Count < count)
        {
            Debug.LogWarning("Did not find enough free cells to spawn full wanted enemy count. Enemy count reduced to found free cells count. Free cells found/enemy count: " + freeCells.Count.ToString() + "/" + count.ToString());
            count = freeCells.Count;
        }

        List<Unit> enemiesSpawned = new List<Unit>();

        for (int i = 0; i < count; i++)
        {
            Cell randomCell = freeCells[Random.Range(0, freeCells.Count)];
            freeCells.Remove(randomCell);
            
            GameObject enemy = Instantiate(enemies[i], unitParent);
            enemy.transform.position = randomCell.transform.position;

            if (enemy.GetComponent<Unit>() == null)
            {
                Debug.LogError("Invalid enemy prefab. It must include Unit script");
                Destroy(enemy);
                continue;
            }

            EnemyUnit enemyUnit = enemy.GetComponent<EnemyUnit>();
            if (enemyUnit != null)
            {
                if (npcDataPool != null && npcDataPool.Count > 0)
                {
                    enemyUnit.unitData = npcDataPool[Random.Range(0, npcDataPool.Count)];
                }
                
                Vector3 randomRotation = UnityEngine.Random.insideUnitSphere;
                randomRotation.y = 0f;
                enemyUnit.ForceRotate(randomRotation);
            }
            
            enemiesSpawned.Add(enemy.GetComponent<Unit>());
        }

        return enemiesSpawned;
    }
}
