using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    public static LootSpawner Instance = null;

    public GameObject lootPointPrefab;

    public LootChest emptyLoot;

    public LootPoint SpawnAmmoStack(Cell cell, int ammoCount, AmmoData ammoData)
    {
        AmmoStack ammoStack = new AmmoStack(ammoCount, ammoData);
        return DropItem(cell, ammoStack);
    }

    public LootPoint SpawnAmmoStack(Cell cell, AmmoStack ammoStack)
    {
        return DropItem(cell, ammoStack);
    }

    public LootPoint SpawnRandomLootPoint(Cell cell, List<LootChest> lootPool, CameraOgre.Direction dir)
    {
        LootChest randomChest = lootPool[Random.Range(0, lootPool.Count)];

        return SpawnLootPoint(cell, randomChest, dir);
    }

    public LootPoint SpawnLootPoint(Cell cell, LootChest lootChest, CameraOgre.Direction dir)
    {
        Loot loot = GetRandomLoot(lootChest);

        LootPoint point = CreateLootPoint(cell, lootChest, CameraOgre.Instance.directionVectors[dir]);
        point.Init(cell, loot);
        InteractablePointManager.Instance.AddPoint(point, cell);

        return point;
    }

    // public virtual List<LootPoint> SpawnRandomLootPoints(List<Cell> cells, int chestCount, int level)
    // {
    //     List<Cell> possibleCells = new List<Cell>(cells);
    //     List<LootPoint> lootPoints = new List<LootPoint>();

    //     if (lootChests == null || lootChests.Count == 0)
    //     {
    //         Debug.LogError("No loot chests defined in LootSpawner");
    //         return lootPoints;
    //     }

    //     for (int i = 0; i < chestCount; i++)
    //     {
    //         LootChest randomChest = lootChests[Random.Range(0, lootChests.Count)];
    //         if (possibleCells == null || possibleCells.Count == 0)
    //         {
    //             Debug.LogWarning("No possible cells for loot chests");
    //             break;
    //         }
    //         Cell cell = possibleCells[Random.Range(0, possibleCells.Count)];
    //         possibleCells.Remove(cell);

    //         Loot loot = GetRandomLoot(randomChest);

    //         LootPoint point = CreateLootPoint(cell, randomChest);
    //         point.Init(cell, loot);
    //         InteractablePointManager.Instance.AddPoint(point, cell);
    //         lootPoints.Add(point);
    //     }

    //     return lootPoints;
    // }

    public LootPoint SpawnEmptyLootPoint(Cell cell)
    {
        Loot loot = new Loot();
        loot.lootkeeper = emptyLoot;

        LootPoint lootPoint = CreateLootPoint(cell, CameraOgre.Instance.GetRandomDirectionVector());
        lootPoint.Init(cell, loot);

        InteractablePointManager.Instance.AddPoint(lootPoint, cell);

        return lootPoint;
    }

    public LootPoint DropItem(Cell cell, LootableItem item)
    {
        LootPoint point = InteractablePointManager.Instance.GetLootPointOnCell(cell);
        if (point == null)
        {
            point = SpawnEmptyLootPoint(cell);
        }

        point.AddItem(item);
        return point;
    }

    public void SpawnDeathLoot(UberUnit unit, Cell cell)
    {
        Loot loot = new Loot();
        if (unit is PlayerUnit)
        {
            loot.items.AddRange(unit.equipped);
            loot.lootkeeper = unit.deathLoot;
        }
        else if (unit is EnemyUnit)
        {
            loot = GetRandomDeathLoot((EnemyUnit) unit);
        }

        LootPoint lootPoint = CreateLootPoint(cell, CameraOgre.Instance.GetRandomDirectionVector());
        lootPoint.Init(cell, loot);

        InteractablePointManager.Instance.AddPoint(lootPoint, cell);
    }

    protected virtual LootPoint CreateLootPoint(Cell cell, Vector2Int dir)
    {
        GameObject lootPointObject = Instantiate(lootPointPrefab, LevelGenerator.Instance.levelObjectsParent);
        lootPointObject.transform.position = cell.transform.position;

        Quaternion destination_rot = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.y));
        destination_rot = Quaternion.Euler(0f, destination_rot.eulerAngles.y, 0f);
        lootPointObject.transform.localRotation = destination_rot;

        LootPoint lootPoint = lootPointObject.GetComponent<LootPoint>();
        if (lootPoint == null)
        {
            Debug.LogError("Invalid Loot Point Prefab. It must have LootPoint script attached to it.");
            Destroy(lootPointObject);
            return null;
        }

        return lootPoint;
    }

    protected virtual LootPoint CreateLootPoint(Cell cell, LootChest chest, Vector2Int dir)
    {
        GameObject lootPointObject = Instantiate(lootPointPrefab, LevelGenerator.Instance.levelObjectsParent);
        lootPointObject.transform.position = cell.transform.position;

        Quaternion destination_rot = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.y));
        destination_rot = Quaternion.Euler(0f, destination_rot.eulerAngles.y, 0f);
        lootPointObject.transform.localRotation = destination_rot;

        LootPoint lootPoint = lootPointObject.GetComponent<LootPoint>();
        if (lootPoint == null)
        {
            Debug.LogError("Invalid Loot Point Prefab. It must have LootPoint script attached to it.");
            Destroy(lootPointObject);
            return null;
        }

        return lootPoint;
    }

    protected Loot GetRandomLoot(LootChest chest)
    {
        Loot loot = new Loot();
        loot.lootkeeper = chest;
        loot.credits = Random.Range(chest.minCredits, chest.maxCredits + 1);

        List<LootableItem> items = chest.GetRandomItems();
        foreach (LootableItem item in items)
        {
            if (item != null)
            {
                loot.items.Add(item);
            }
        }

        return loot;
    }

    protected Loot GetRandomDeathLoot(EnemyUnit unit)
    {
        if (unit.deathLoot == null)
        {
            return GetRandomLoot(emptyLoot);
        }
        return GetRandomLoot(unit.deathLoot);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple Singleton Instance GameObjects in scene. Class: " + this.GetType().ToString() + " in GameObject: " + gameObject.name);
            Destroy(this);
            return;
        }

        if (emptyLoot == null)
        {
            Debug.LogError("Empty loot is null, please assign it");
        }
    }
}
