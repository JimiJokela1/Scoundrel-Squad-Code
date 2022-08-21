using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance = null;

    public CellGrid cellGrid;

    public Transform levelObjectsParent;

    public LevelGeneratorTheme theme;

    public LevelProgression levelProgression;

    [HideInInspector]
    public LevelData currentLevelData;
    
    private LevelGeneratorFormula currentGenerator;

    public void ResetCells()
    {
        foreach (Cell cell in cellGrid.Cells)
        {
            cell.BlocksLineOfSight = false;
            cell.BlocksMovement = false;
            cell.IsCover = false;
        }
    }

    public void ResetLevel()
    {
        foreach (Transform t in levelObjectsParent)
        {
            Destroy(t.gameObject);
        }

        ResetCells();
    }

    public Elevator CreateNewLevel(List<PlayerUnit> playerUnits, int currentLevel)
    {
        LevelGeneratorProgress levelGeneratorProgress = levelProgression.GetActiveLevelGeneratorProgress(currentLevel);
        currentGenerator = levelGeneratorProgress.ChooseLevelGenerator();
        
        LootProgress lootProgress = levelProgression.GetActiveLootProgress(currentLevel);
        List<LootChest> lootChests = lootProgress.ChooseLootChests();

        ShopProgress shopProgress = levelProgression.GetActiveShopProgress(currentLevel);
        List<Shopkeeper> shops = shopProgress.ChooseShops();

        ChargePointProgress chargePointProgress = levelProgression.GetActiveChargePointProgress(currentLevel);
        List<ChargePointData> chargePoints = chargePointProgress.ChooseChargePoints();
        
        Elevator elevator = GenerateNewLevel(playerUnits, lootChests, shops, chargePoints, currentLevel);

        return elevator;
    }

    public Elevator GenerateNewLevel(List<PlayerUnit> playerUnits, List<LootChest> lootChests, List<Shopkeeper> shops, List<ChargePointData> chargePoints, int currentLevel)
    {
        int levelWidth = (int) cellGrid.Cells.OrderByDescending(c => c.OffsetCoord.x).First().OffsetCoord.x + 1;
        int levelLength = (int) cellGrid.Cells.OrderByDescending(c => c.OffsetCoord.y).First().OffsetCoord.y + 1;

        currentLevelData = currentGenerator.GenerateLevel(levelWidth, levelLength, shops.Count, lootChests.Count, chargePoints.Count);

        List<Cell> startElevator = new List<Cell>();
        foreach (MapTile tile in currentLevelData.startElevator.tiles)
        {
            if (currentLevelData.tileData[tile.x, tile.y] == LevelData.TileData.StartElevator)
            {
                startElevator.Add(cellGrid.GetCellAt(tile.x, tile.y));
            }
        }

        List<Cell> endElevator = new List<Cell>();
        foreach (MapTile tile in currentLevelData.endElevator.tiles)
        {
            if (currentLevelData.tileData[tile.x, tile.y] == LevelData.TileData.EndElevator)
            {
                endElevator.Add(cellGrid.GetCellAt(tile.x, tile.y));
            }
        }

        CreateLevelObjects(currentLevelData, lootChests, shops, chargePoints, currentLevel);

        return new Elevator(playerUnits, startElevator, endElevator);
    }

    private void CreateLevelObjects(LevelData levelData, List<LootChest> lootChests, List<Shopkeeper> shops, List<ChargePointData> chargePoints, int currentLevel)
    {
        List<Cell> possibleShopAndLootCells = new List<Cell>();
        // List<Cell> possibleLootCells = new List<Cell>();

        for (int y = 0; y < levelData.tileData.GetLength(1); y++)
        {
            for (int x = 0; x < levelData.tileData.GetLength(0); x++)
            {
                Cell cell = cellGrid.GetCellAt(x, y);
                if (cell == null) continue;
                SquareTile3D tile = (SquareTile3D) cell;
                if (tile != null)
                {
                    // Change tile floor model to match tile type
                    tile.ChangeTile(levelData.tileData[x, y]);
                }

                cell.BlocksLineOfSight = false;
                cell.BlocksMovement = false;
                cell.IsCover = false;

                // Instantiate appropriate model onto tile 
                switch (levelData.tileData[x, y])
                {
                    case LevelData.TileData.Floor:
                        break;
                    case LevelData.TileData.Wall:
                        GameObject wall = Instantiate(theme.GetRandomWallPrefab(), levelObjectsParent);
                        wall.transform.position = cell.transform.position;

                        cell.BlocksLineOfSight = true;
                        cell.BlocksMovement = true;
                        cell.IsCover = true;
                        break;
                    case LevelData.TileData.Door:
                        GameObject door = Instantiate(theme.GetRandomDoorPrefab(), levelObjectsParent);
                        door.transform.position = cell.transform.position;

                        // TODO: align door to neighbouring wall

                        cell.BlocksLineOfSight = true;
                        cell.BlocksMovement = true;
                        cell.IsCover = true;

                        break;
                    case LevelData.TileData.Cover:
                        GameObject cover = Instantiate(theme.GetRandomCoverPrefab(), levelObjectsParent);
                        cover.transform.position = cell.transform.position;
                        Vector2Int dir = CameraOgre.Instance.GetRandomDirectionVector();
                        cover.transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.y));

                        cell.BlocksLineOfSight = false;
                        cell.BlocksMovement = true;
                        cell.IsCover = true;

                        break;
                    case LevelData.TileData.StartElevator:
                        // GameObject startMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        // startMarker.transform.parent = levelObjectsParent;
                        // startMarker.transform.position = cell.transform.position;

                        break;
                    case LevelData.TileData.EndElevator:
                        // GameObject endMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        // endMarker.transform.parent = levelObjectsParent;
                        // endMarker.transform.position = cell.transform.position;

                        break;
                    case LevelData.TileData.ShopOrLoot:
                        possibleShopAndLootCells.Add(cell);
                        // possibleLootCells.Add(cell);

                        break;
                    default:
                        break;
                }
            }
        }

        // Create shops in random shop marked tiles
        int shopsCreated = 0;
        if (shops.Count > 0)
        {
            foreach (Shopkeeper shop in shops)
            {
                if (possibleShopAndLootCells.Count > 0)
                {
                    Cell randomCell = possibleShopAndLootCells[Random.Range(0, possibleShopAndLootCells.Count)];
                    possibleShopAndLootCells.Remove(randomCell);
                    CameraOgre.Direction randomDir = CameraOgre.Instance.GetRandomDirection();
                    ShopSpawner.Instance.SpawnShop(randomCell, shop, randomDir);
                    shopsCreated++;
                }
                else
                {
                    Debug.LogWarning("Possible shop and loot positions ran out -> some loot or shops or charge points not spawned");
                }
            }
        }
        string shopNames = "";
        shops.ForEach(s => shopNames += s.name + ", ");
        Debug.Log(shopsCreated.ToString() + " shops generated: " + shopNames);

        // Create loot chests in random loot marked tiles
        int lootChestsCreated = 0;
        if (lootChests.Count > 0)
        {
            foreach(LootChest lootChest in lootChests)
            {
                if (possibleShopAndLootCells.Count > 0)
                {
                    Cell randomCell = possibleShopAndLootCells[Random.Range(0, possibleShopAndLootCells.Count)];
                    possibleShopAndLootCells.Remove(randomCell);
                    LootSpawner.Instance.SpawnLootPoint(randomCell, lootChest, CameraOgre.Instance.GetRandomDirection());
                    lootChestsCreated++;
                }
                else
                {
                    Debug.LogWarning("Possible shop and loot positions ran out -> some loot or shops or charge points not spawned");
                }
            }
        }
        string lootChestNames = "";
        lootChests.ForEach(s => lootChestNames += s.name + ", ");
        Debug.Log(lootChestsCreated.ToString() + " loot chests generated: " + lootChestNames);

        // Create loot chests in random loot marked tiles
        int chargePointsCreated = 0;
        if (chargePoints.Count > 0)
        {
            foreach(ChargePointData chargePoint in chargePoints)
            {
                if (possibleShopAndLootCells.Count > 0)
                {
                    Cell randomCell = possibleShopAndLootCells[Random.Range(0, possibleShopAndLootCells.Count)];
                    possibleShopAndLootCells.Remove(randomCell);
                    ChargePointSpawner.Instance.SpawnChargePoint(randomCell, chargePoint, CameraOgre.Instance.GetRandomDirection());
                    chargePointsCreated++;
                }
                else
                {
                    Debug.LogWarning("Possible shop and loot positions ran out -> some loot or shops or charge points not spawned");
                }
            }
        }
        string chargePointNames = "";
        chargePoints.ForEach(s => chargePointNames += s.name + ", ");
        Debug.Log(chargePointsCreated.ToString() + " charge points generated: " + chargePointNames);
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
    }
}
