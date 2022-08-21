using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class LevelGeneratorFormula : ScriptableObject
{
    public int startElevatorWidth;
    public int startElevatorLength;
    public int endElevatorWidth;
    public int endElevatorLength;

    protected virtual void OnValidate()
    {
        if (startElevatorWidth < 2) startElevatorWidth = 2;
        if (startElevatorLength < 2) startElevatorLength = 2;
        if (endElevatorWidth < 2) endElevatorWidth = 2;
        if (endElevatorLength < 2) endElevatorLength = 2;
    }

    public abstract LevelData GenerateLevel(int levelWidth, int levelLength, int shopCount, int lootCount, int chargePointCount);

    public virtual LevelData GenerateStartElevator(LevelData levelData, int levelWidth, int levelLength)
    {
        if (startElevatorWidth < 2)
        {
            startElevatorWidth = 2;
        }
        if (startElevatorLength < 2)
        {
            startElevatorLength = 2;
        }

        int eleX = 1;
        int eleY = Random.Range(1, levelLength - 2 - startElevatorLength);

        // // Mark walls
        // for (int y = eleY - 1; y < eleY + startElevatorLength + 1; y++)
        // {
        //     for (int x = eleX - 1; x < eleX + startElevatorWidth; x++)
        //     {
        //         if (levelData.tileData[x, y] != LevelData.TileData.StartElevator)
        //         {
        //             levelData.tileData[x, y] = LevelData.TileData.Wall;
        //         }
        //     }
        // }

        // levelData.tileData[eleX + startElevatorWidth + 1, eleY - 1] = LevelData.TileData.Wall;
        // levelData.tileData[eleX + startElevatorWidth + 1, eleY + startElevatorLength + 1] = LevelData.TileData.Wall;

        LevelData.RoomData room = levelData.AddRoom(eleX - 1, eleY - 1, startElevatorWidth + 2, startElevatorLength + 2);

        // Change floor to elevator markers
        for (int y = eleY; y < eleY + startElevatorLength; y++)
        {
            for (int x = eleX; x < eleX + startElevatorWidth; x++)
            {
                levelData.tileData[x, y] = LevelData.TileData.StartElevator;
            }
        }

        // foreach (MapTile tile in room.tiles)
        // {
        //     if (levelData.tileData[tile.x, tile.y] == LevelData.TileData.Floor)
        //     {
        //         levelData.tileData[tile.x, tile.y] = LevelData.TileData.StartElevator;
        //     }
        // }

        // elevator doors
        for (int y = eleY; y < eleY + startElevatorLength; y++)
        {
            levelData.tileData[eleX + startElevatorWidth, y] = LevelData.TileData.Door;
        }

        // Add floor to front of elevator
        for (int y = eleY; y < eleY + startElevatorLength; y++)
        {
            for (int x = eleX + startElevatorWidth + 1; x < eleX + startElevatorWidth + 3; x++)
            {
                levelData.tileData[x, y] = LevelData.TileData.Floor;
                room.AddTile(x, y);
            }
        }

        levelData.startElevator = room;

        return levelData;
    }

    public virtual LevelData GenerateEndElevator(LevelData levelData, int levelWidth, int levelLength)
    {
        if (endElevatorWidth < 2)
        {
            endElevatorWidth = 2;
        }
        if (endElevatorLength < 2)
        {
            endElevatorLength = 2;
        }

        int eleX = levelWidth - 1 - endElevatorWidth;
        int eleY = Random.Range(1, levelLength - 2 - endElevatorLength);

        // // Mark walls
        // for (int y = eleY - 1; y < eleY + endElevatorLength + 1; y++)
        // {
        //     for (int x = eleX; x < eleX + endElevatorWidth + 1; x++)
        //     {
        //         if (levelData.tileData[x, y] != LevelData.TileData.EndElevator)
        //         {
        //             levelData.tileData[x, y] = LevelData.TileData.Wall;
        //         }
        //     }
        // }

        // levelData.tileData[eleX - 1, eleY - 1] = LevelData.TileData.Wall;
        // levelData.tileData[eleX - 1, eleY + endElevatorLength + 1] = LevelData.TileData.Wall;

        LevelData.RoomData room = levelData.AddRoom(eleX - 1, eleY - 1, endElevatorWidth + 2, endElevatorLength + 2);

        // Change floor to elevator markers
        for (int y = eleY; y < eleY + endElevatorLength; y++)
        {
            for (int x = eleX; x < eleX + endElevatorWidth; x++)
            {
                levelData.tileData[x, y] = LevelData.TileData.EndElevator;
            }
        }

        // foreach (MapTile tile in room.tiles)
        // {
        //     if (levelData.tileData[tile.x, tile.y] == LevelData.TileData.Floor)
        //     {
        //         levelData.tileData[tile.x, tile.y] = LevelData.TileData.EndElevator;
        //     }
        // }

        // elevator doors
        for (int y = eleY; y < eleY + endElevatorLength; y++)
        {
            levelData.tileData[eleX - 1, y] = LevelData.TileData.Door;
        }

        // Add floor to front of elevator 2x2
        for (int y = eleY; y < eleY + endElevatorLength + 1; y++)
        {
            for (int x = eleX - 3; x < eleX - 1; x++)
            {
                levelData.tileData[x, y] = LevelData.TileData.Floor;
                room.AddTile(x, y);
            }
        }

        levelData.endElevator = room;

        return levelData;
    }

    protected virtual LevelData AddCoverAndProps(LevelData levelData, int levelWidth, int levelLength, int minCovers, int maxCovers)
    {
        // Covers
        int covers = Random.Range(minCovers, maxCovers);
        int coverTiles = 0;
        List<MapTile> suitableTiles = new List<MapTile>();

        for (int y = 0; y < levelLength; y++)
        {
            for (int x = 0; x < levelWidth; x++)
            {
                if (IsSuitableCoverTile(levelData, x, y))
                {
                    suitableTiles.Add(new MapTile(x, y));
                }
            }
        }

        Debug.Log("First stage suitable tiles for cover: " + suitableTiles.Count);

        for (int i = 0; i < covers; i++)
        {
            if (suitableTiles.Count > 0)
            {
                MapTile chosen = suitableTiles[Random.Range(0, suitableTiles.Count)];
                suitableTiles.Remove(chosen);
                levelData.tileData[chosen.x, chosen.y] = LevelData.TileData.Cover;
                coverTiles++;

                foreach (MapTile tile in levelData.GetSurroundingTileCoords(chosen.x, chosen.y))
                {
                    suitableTiles.Remove(tile);
                }
            }
        }

        Debug.Log("Cover tiles found:" + coverTiles);

        return levelData;
    }

    protected virtual LevelData AddWallProps(LevelData levelData, int levelWidth, int levelLength, int minWallProps, int maxWallProps)
    {
        // TODO: wall props
        int wallProps = Random.Range(minWallProps, maxWallProps);

        return levelData;
    }

    protected virtual bool IsSuitableCoverTile(LevelData levelData, int x, int y)
    {
        if (levelData.tileData[x, y] != LevelData.TileData.Floor)
        {
            return false;
        }

        LevelData.SurroundingTiles tiles = levelData.GetSurroundingTilesData(x, y);

        // If next to door
        if (tiles.door) return false;
        // Check for elevator in any surrounding tile
        if (tiles.elevator) return false;
        // If next to level edge
        if (tiles.tileCount < 8) return false;

        // check that we arent blocking a shop
        if (tiles.shopOrLoot)
        {
            LevelData.SurroundingTiles shopOrLootSurTiles = levelData.GetSurroundingTilesData(tiles.shopOrLootTile.x, tiles.shopOrLootTile.y);
            if (shopOrLootSurTiles.neighbouringOpen < 2)
            {
                return false;
            }
        }

        if (tiles.open >= 4 && tiles.openIslands == 1)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Recursively expands a room by changing surrounding walls to floors based on a custom cellular automata principle
    /// </summary>
    /// <param name="levelData"></param>
    /// <param name="sourceRoom">Room that target cell is in or a wall of.</param>
    /// <param name="x">Target cell x coordinate</param>
    /// <param name="y">Target cell y coordinate</param>
    /// <param name="levelWidth"></param>
    /// <param name="levelLength"></param>
    /// <param name="maxSurroundingIslands"></param>
    /// <param name="minSurroundingOpen"></param>
    /// <param name="minSurroundingWalls"></param>
    /// <returns></returns>
    protected virtual LevelData CarveWall(LevelData levelData, LevelData.RoomData sourceRoom, int x, int y, int levelWidth, int levelLength, int maxSurroundingIslands, int minSurroundingOpen, int minSurroundingWalls)
    {
        if (levelData.tileData[x, y] != LevelData.TileData.Wall)
        {
            return levelData;
        }
        // Get data on tiles surrounding current tile
        LevelData.SurroundingTiles tiles = levelData.GetSurroundingTilesData(x, y);

        // If next to door, dont march
        if (tiles.door) return levelData;
        // Check for elevator in any surrounding tile, if found do not march
        if (tiles.elevator) return levelData;
        // If next to level edge, dont march
        if (tiles.tileCount < 8) return levelData;

        // TODO: Check for zigzag corners, eliminate them, drive them to the ground

        // Carve if conditions filled
        if (tiles.openIslands <= maxSurroundingIslands &&
            tiles.open >= minSurroundingOpen &&
            tiles.walls >= minSurroundingWalls)
        {
            levelData.tileData[x, y] = LevelData.TileData.Floor;

            if (sourceRoom == null)
            {
                sourceRoom = levelData.AddRoom(x - 1, y - 1, 3, 3, true);
            }
            else
            {
                sourceRoom.AddTile(x, y);
            }

            // Carve surrounding tiles recursively
            List<MapTile> surroundingTilesCoords = levelData.GetSurroundingTileCoords(x, y);
            foreach (MapTile tile in surroundingTilesCoords)
            {
                levelData = CarveWall(levelData, sourceRoom, tile.x, tile.y, levelWidth, levelLength, maxSurroundingIslands, minSurroundingOpen, minSurroundingWalls);
            }
        }

        return levelData;
    }

    protected virtual LevelData ConnectRooms(LevelData levelData, int levelWidth, int levelLength, float chanceToOpenExtraConnector)
    {
        Dictionary<LevelData.RoomData, List<LevelData.RoomConnector>> allPossibleConnectors = new Dictionary<LevelData.RoomData, List<LevelData.RoomConnector>>();

        foreach (LevelData.RoomData room in levelData.rooms)
        {
            allPossibleConnectors.Add(room, levelData.GetRoomConnectors(room));
        }

        List<LevelData.RoomData> mainRegion = new List<LevelData.RoomData>();
        mainRegion.Add(levelData.startElevator);

        for (int i = 0; i < levelData.rooms.Count; i++)
        {
            LevelData.RoomData currentRoom = null;
            List<LevelData.RoomData> mainRegionReversed = new List<LevelData.RoomData>(mainRegion);
            mainRegionReversed.Reverse();

            foreach (LevelData.RoomData room in mainRegionReversed)
            {
                if (allPossibleConnectors[room].Count > 0)
                {
                    currentRoom = room;
                }
            }

            if (currentRoom == null)
            {
                break;
            }

            List<LevelData.RoomConnector> connectors = new List<LevelData.RoomConnector>(allPossibleConnectors[currentRoom]);
            LevelData.RoomConnector randomConnector = connectors[Random.Range(0, connectors.Count)];
            LevelData.RoomData connectedRoom = randomConnector.GetOtherRoom(currentRoom);

            levelData.tileData[randomConnector.tile.x, randomConnector.tile.y] = LevelData.TileData.Door;

            // Remove extra connectors
            // Eech removed connector also has a chance to be turned into an extra door
            foreach (LevelData.RoomData room in mainRegion)
            {
                foreach (LevelData.RoomConnector connector in new List<LevelData.RoomConnector>(allPossibleConnectors[room]))
                {
                    if (connector.GetOtherRoom(room) == connectedRoom)
                    {
                        allPossibleConnectors[room].Remove(connector);
                        // Remove opposite connector also because dictionary has all twice
                        LevelData.RoomConnector oppositeConnector = allPossibleConnectors[connectedRoom].Find(c => c.tile.Equals(connector.tile));
                        allPossibleConnectors[connectedRoom].Remove(oppositeConnector);

                        // Chance to open extra connector
                        if (Random.Range(0f, 1f) < chanceToOpenExtraConnector)
                        {
                            levelData.tileData[connector.tile.x, connector.tile.y] = LevelData.TileData.Door;
                        }
                    }
                }
            }

            mainRegion.Add(connectedRoom);
        }

        return levelData;
    }

    protected virtual LevelData AddShopsAndLoot(LevelData levelData, int levelWidth, int levelLength, int shopAndLootCount)
    {
        List<MapTile> possibleTiles = new List<MapTile>();

        // Find loot and shop points in holes in walls first, so starting with tiles that have only one open surrounding tile
        for (int i = 1; i < 9; i++)
        {
            if (possibleTiles.Count > shopAndLootCount)
            {
                break;
            }

            for (int y = 0; y < levelLength; y++)
            {
                for (int x = 0; x < levelWidth; x++)
                {
                    if (levelData.tileData[x, y] != LevelData.TileData.Floor)
                    {
                        continue;
                    }

                    LevelData.SurroundingTiles tiles = levelData.GetSurroundingTilesData(x, y);

                    if (tiles.door) continue;

                    if (tiles.open == i && tiles.openIslands == 1)
                    {
                        possibleTiles.Add(new MapTile(x, y));
                    }
                }
            }
        }

        Debug.Log("Possible shop and loot points found: " + possibleTiles.Count);

        for (int i = 0; i < shopAndLootCount; i++)
        {
            if (possibleTiles.Count == 0)
            {
                Debug.LogError("Could not find enough points for shops and loot in level");
                break;
            }
            MapTile randomTile = possibleTiles[Random.Range(0, possibleTiles.Count)];
            possibleTiles.Remove(randomTile);
            levelData.tileData[randomTile.x, randomTile.y] = LevelData.TileData.ShopOrLoot;
        }

        return levelData;
    }
}

public class LevelData
{
    public TileData[, ] tileData;
    public List<RoomData> rooms;
    public RoomData startElevator;
    public RoomData endElevator;

    public LevelData()
    {
        rooms = new List<RoomData>();
    }

    public LevelData(int levelWidth, int levelLength, TileData fillWith)
    {
        tileData = new TileData[levelWidth, levelLength];
        for (int y = 0; y < levelLength; y++)
        {
            for (int x = 0; x < levelWidth; x++)
            {
                tileData[x, y] = fillWith;
            }
        }

        rooms = new List<RoomData>();
    }

    /// <summary>
    /// Gets surrounding TileDatas in all 8 directions (Moore neighbourhood)
    /// </summary>
    public List<TileData> GetSurroundingTiles(int x, int y)
    {
        List<TileData> surroundingTiles = new List<TileData>();

        if (IsLegalMapPoint(x - 1, y - 1)) surroundingTiles.Add(tileData[x - 1, y - 1]);
        if (IsLegalMapPoint(x - 1, y)) surroundingTiles.Add(tileData[x - 1, y]);
        if (IsLegalMapPoint(x - 1, y + 1)) surroundingTiles.Add(tileData[x - 1, y + 1]);
        if (IsLegalMapPoint(x, y + 1)) surroundingTiles.Add(tileData[x, y + 1]);
        if (IsLegalMapPoint(x + 1, y + 1)) surroundingTiles.Add(tileData[x + 1, y + 1]);
        if (IsLegalMapPoint(x + 1, y)) surroundingTiles.Add(tileData[x + 1, y]);
        if (IsLegalMapPoint(x + 1, y - 1)) surroundingTiles.Add(tileData[x + 1, y - 1]);
        if (IsLegalMapPoint(x, y - 1)) surroundingTiles.Add(tileData[x, y - 1]);

        return surroundingTiles;
    }

    /// <summary>
    /// Gets neighbouring TileDatas in the 4 main directions (von Neumann neighbourhood)
    /// </summary>
    public List<TileData> GetNeighbouringTiles(int x, int y)
    {
        List<TileData> neighbouringTiles = new List<TileData>();

        if (IsLegalMapPoint(x - 1, y)) neighbouringTiles.Add(tileData[x - 1, y]);
        if (IsLegalMapPoint(x, y + 1)) neighbouringTiles.Add(tileData[x, y + 1]);
        if (IsLegalMapPoint(x + 1, y)) neighbouringTiles.Add(tileData[x + 1, y]);
        if (IsLegalMapPoint(x, y - 1)) neighbouringTiles.Add(tileData[x, y - 1]);

        return neighbouringTiles;
    }

    /// <summary>
    /// Gets surrounding tile coordinates in all 8 directions (Moore neighbourhood)
    /// </summary>
    public List<MapTile> GetSurroundingTileCoords(int x, int y)
    {
        List<MapTile> surroundingTiles = new List<MapTile>();

        if (IsLegalMapPoint(x - 1, y - 1)) surroundingTiles.Add(new MapTile(x - 1, y - 1));
        if (IsLegalMapPoint(x - 1, y)) surroundingTiles.Add(new MapTile(x - 1, y));
        if (IsLegalMapPoint(x - 1, y + 1)) surroundingTiles.Add(new MapTile(x - 1, y + 1));
        if (IsLegalMapPoint(x, y + 1)) surroundingTiles.Add(new MapTile(x, y + 1));
        if (IsLegalMapPoint(x + 1, y + 1)) surroundingTiles.Add(new MapTile(x + 1, y + 1));
        if (IsLegalMapPoint(x + 1, y)) surroundingTiles.Add(new MapTile(x + 1, y));
        if (IsLegalMapPoint(x + 1, y - 1)) surroundingTiles.Add(new MapTile(x + 1, y - 1));
        if (IsLegalMapPoint(x, y - 1)) surroundingTiles.Add(new MapTile(x, y - 1));

        return surroundingTiles;
    }

    /// <summary>
    /// Gets surrounding tile coordinates in the 4 main directions (von Neumann neighbourhood)
    /// </summary>
    public List<MapTile> GetNeighbouringTileCoords(int x, int y)
    {
        List<MapTile> neighbouringTiles = new List<MapTile>();

        if (IsLegalMapPoint(x - 1, y)) neighbouringTiles.Add(new MapTile(x - 1, y));
        if (IsLegalMapPoint(x, y + 1)) neighbouringTiles.Add(new MapTile(x, y + 1));
        if (IsLegalMapPoint(x + 1, y)) neighbouringTiles.Add(new MapTile(x + 1, y));
        if (IsLegalMapPoint(x, y - 1)) neighbouringTiles.Add(new MapTile(x, y - 1));

        return neighbouringTiles;
    }

    /// <summary>
    /// Gathers a set of data (SurroundingTiles class) on tiles surrounding given coordinates. Useful during level generation for cellular automata, and tile validation.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public SurroundingTiles GetSurroundingTilesData(int x, int y)
    {
        SurroundingTiles tiles = new SurroundingTiles();

        TileData[] surroundingTiles = GetSurroundingTiles(x, y).ToArray();
        tiles.tileCount = surroundingTiles.Length;

        List<TileData> neighbouringTiles = GetNeighbouringTiles(x, y);

        // Go through and calculate open and wall tiles 
        // and open islands (separate groups of open tiles) in surrounding tiles
        for (int t = 0; t < surroundingTiles.Length; t++)
        {
            if (surroundingTiles[t] == TileData.Wall)
            {
                tiles.walls++;

                if (neighbouringTiles.Contains(surroundingTiles[t])) tiles.neighbouringWalls++;
            }
            else if (surroundingTiles[t] == TileData.Floor || surroundingTiles[t] == TileData.StartElevator || surroundingTiles[t] == TileData.EndElevator)
            {
                tiles.open++;

                if (neighbouringTiles.Contains(surroundingTiles[t])) tiles.neighbouringOpen++;

                if (surroundingTiles[t] == TileData.StartElevator || surroundingTiles[t] == TileData.EndElevator)
                {
                    tiles.elevator = true;
                }

                if (surroundingTiles.Length > 1)
                {
                    // Check if new island of open tiles
                    bool newIsland = true;
                    if (t == 0)
                    {
                        TileData previousTile = surroundingTiles[surroundingTiles.Length - 1];
                        if (previousTile == TileData.Floor || previousTile == TileData.StartElevator || previousTile == TileData.EndElevator)
                        {
                            newIsland = false;
                        }
                    }
                    else
                    {
                        TileData previousTile = surroundingTiles[t - 1];
                        if (previousTile == TileData.Floor || previousTile == TileData.StartElevator || previousTile == TileData.EndElevator)
                        {
                            newIsland = false;
                        }
                    }

                    if (newIsland)
                    {
                        tiles.openIslands++;
                    }
                }
            }
            else
            {
                tiles.other++;

                if (neighbouringTiles.Contains(surroundingTiles[t])) tiles.neighbouringOther++;

                if (surroundingTiles[t] == TileData.Door)
                {
                    tiles.door = true;
                }

                if (surroundingTiles[t] == TileData.ShopOrLoot)
                {
                    tiles.shopOrLoot = true;
                    MapTile shopOrLootTile = GetSurroundingTileCoords(x, y).ToArray() [t];
                    tiles.shopOrLootTile = shopOrLootTile;
                }
            }
        }

        if (tiles.openIslands == 0 && tiles.open > 0)
        {
            tiles.openIslands = 1;
        }

        return tiles;
    }

    /// <summary>
    /// Find all tiles that have at least 2 different rooms directly adjacent (von Neumann neighbourhood, 4 main directions)
    /// </summary>
    /// <param name="room"></param>
    /// <returns>List of tiles and the rooms they connect</returns>
    public List<RoomConnector> GetRoomConnectors(RoomData room)
    {
        List<RoomConnector> connectors = new List<RoomConnector>();

        foreach (MapTile tile in room.tiles)
        {
            List<MapTile> neighbours = GetNeighbouringTileCoords(tile.x, tile.y);
            foreach (MapTile neighbour in neighbours)
            {
                if (tileData[neighbour.x, neighbour.y] == TileData.Wall)
                {
                    bool nextToElevator = false;
                    List<TileData> neighbouringTiles = GetNeighbouringTiles(neighbour.x, neighbour.y);
                    foreach (TileData tileData in neighbouringTiles)
                    {
                        if (tileData == TileData.StartElevator || tileData == TileData.EndElevator)
                        {
                            nextToElevator = true;
                        }
                    }
                    if (nextToElevator) continue;

                    List<RoomData> neighbouringRooms = GetNeighbouringRooms(neighbour.x, neighbour.y);
                    if (neighbouringRooms.Count > 1 && neighbouringRooms.Contains(room))
                    {
                        connectors.Add(new RoomConnector(neighbour, neighbouringRooms));
                    }
                }
            }
        }

        return connectors;
    }

    public RoomData FindRoomContainingPoint(int x, int y)
    {
        foreach (RoomData room in rooms)
        {
            if (room.ContainsPoint(x, y))
            {
                return room;
            }
        }

        return null;
    }

    public List<RoomData> GetNeighbouringRooms(int x, int y)
    {
        List<RoomData> roomsInSurroundingTiles = new List<RoomData>();
        foreach (MapTile tile in GetNeighbouringTileCoords(x, y))
        {
            RoomData room = FindRoomContainingPoint(tile.x, tile.y);
            if (room != null && !roomsInSurroundingTiles.Contains(room))
            {
                roomsInSurroundingTiles.Add(room);
            }
        }

        return roomsInSurroundingTiles;
    }

    /// <summary>
    /// Creates a room by changing given tiles to floors and walls, while taking into account overlapping with existing rooms, if overlap parameter is true.
    /// TODO: return merged room RoomData if overlapping. Current behaviour returns new RoomData regardless of overlap. However no discernible resulting bugs detected (as of writing).
    /// </summary>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    /// <param name="width"></param>
    /// <param name="length"></param>
    /// <param name="overlap"></param>
    /// <returns></returns>
    public RoomData AddRoom(int posX, int posY, int width, int length, bool overlap = false)
    {
        // Fill room area with walls (considering overlap)
        for (int y = posY; y < posY + length; y++)
        {
            for (int x = posX; x < posX + width; x++)
            {
                if (overlap && (tileData[x, y] == TileData.Floor || tileData[x, y] == TileData.Door))
                {
                    continue;
                }
                if (tileData[x, y] == TileData.Door)
                {
                    continue;
                }
                tileData[x, y] = TileData.Wall;
            }
        }

        // Carve inside of room into floors
        for (int y = posY + 1; y < posY + length - 1; y++)
        {
            for (int x = posX + 1; x < posX + width - 1; x++)
            {
                tileData[x, y] = TileData.Floor;
            }
        }

        RoomData room = new RoomData(posX + 1, posY + 1, width - 2, length - 2);

        rooms.Add(room);
        return room;
    }

    /// <summary>
    /// Checks if coordinates are within grid limits.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsLegalMapPoint(int x, int y)
    {
        if (x < 0 || x >= tileData.GetLength(0) || y < 0 || y >= tileData.GetLength(1))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if any room contains point including room walls.
    /// </summary>
    public bool AnyRoomContainsPoint(int x, int y)
    {
        bool contains = false;
        foreach (RoomData room in rooms)
        {
            if (room.ContainsPoint(x, y))
            {
                contains = true;
                break;
            }
        }

        return contains;
    }

    /// <summary>
    /// Checks if point is overlapping any room interior. Points on walls or doors are not considered overlapping the room. 
    /// </summary>
    public bool IsPointOverlappingAnyRoom(int x, int y)
    {
        bool contains = false;
        foreach (RoomData room in rooms)
        {
            if (room.ContainsPoint(x, y) && (tileData[x, y] != TileData.Wall && tileData[x, y] != TileData.Door))
            {
                contains = true;
                break;
            }
        }

        return contains;
    }

    /// <summary>
    /// Checks if point is overlapping given room interior. Points on walls or doors are not considered overlapping the room. 
    /// </summary>
    public bool IsPointOverlappingRoom(int x, int y, RoomData room)
    {
        bool contains = false;
        if (room.ContainsPoint(x, y) && (tileData[x, y] != TileData.Wall && tileData[x, y] != TileData.Door))
        {
            contains = true;
        }

        return contains;
    }

    /// <summary>
    /// Data for a single room, listing tiles contained. 
    /// </summary>
    public class RoomData
    {
        public List<MapTile> tiles;

        // public int width;
        // public int length;
        // public int posX;
        // public int posY;

        public RoomData()
        {
            tiles = new List<MapTile>();
        }

        public RoomData(int posX, int posY, int width, int length)
        {
            tiles = new List<MapTile>();
            for (int y = posY; y < posY + length; y++)
            {
                for (int x = posX; x < posX + width; x++)
                {
                    tiles.Add(new MapTile(x, y));
                }
            }

            // this.width = width;
            // this.length = length;
            // this.posX = posX;
            // this.posY = posY;
        }

        public void AddTile(int x, int y)
        {
            if (!tiles.Contains(new MapTile(x, y)))
            {
                tiles.Add(new MapTile(x, y));
            }
        }

        public bool ContainsPoint(int x, int y)
        {
            return tiles.Contains(new MapTile(x, y));
        }
    }

    /// <summary>
    /// Data representing tiles and rooms that the tile connects (is directly adjacent to)
    /// </summary>
    public class RoomConnector
    {
        public MapTile tile;
        /// <summary>
        /// Directly adjacent rooms
        /// </summary>
        public List<RoomData> rooms;

        public RoomConnector()
        {
            rooms = new List<RoomData>();
        }

        public RoomConnector(MapTile tile, List<RoomData> rooms)
        {
            this.tile = tile;
            this.rooms = rooms;
        }

        public RoomData GetOtherRoom(RoomData room)
        {
            foreach (RoomData connected in rooms)
            {
                if (connected != room)
                {
                    return connected;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Value for determining static nature of tile.
    /// </summary>
    public enum TileData
    {
        Floor,
        Wall,
        Door,
        Cover,
        StartElevator,
        EndElevator,
        Shop,
        Loot,
        ShopOrLoot
    }

    /// <summary>
    /// Data on surrounding tiles. Useful during level generation for cellular automata, and tile validation. Gather data with GetSurroundingTilesData method.
    /// </summary>
    public class SurroundingTiles
    {
        public int tileCount = 0;
        public int open = 0;
        public int openIslands = 0;
        public int walls = 0;
        public int other = 0;
        public int blocked { get { return walls + other; } }

        public bool door = false;
        public bool elevator = false;
        public bool shopOrLoot = false;
        public MapTile shopOrLootTile = null;

        public int neighbouringOpen = 0;
        public int neighbouringWalls = 0;
        public int neighbouringOther = 0;
        public int neighbouringBlocked { get { return neighbouringWalls + neighbouringOther; } }
    }
}

/// <summary>
/// Represents a tile as a set of coordinates. Data and methods related to tile coordinates and pathfinding. 
/// </summary>
[System.Serializable]
public class MapTile : IGraphNode
{
    public int x, y;

    public override bool Equals(object obj)
    {
        if (!(obj is MapTile))
        {
            return false;
        }

        MapTile other = (MapTile) obj;

        if (x == other.x && y == other.y)
        {
            return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }

    public MapTile()
    {

    }

    public MapTile(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int GetDistance(IGraphNode other)
    {
        MapTile otherTile = (MapTile) other;
        return (int) (Mathf.Abs(x - otherTile.x) + Mathf.Abs(y - otherTile.y));
    }

    public static MapTile Random(int levelWidth, int levelLength)
    {
        return new MapTile(UnityEngine.Random.Range(0, levelWidth), UnityEngine.Random.Range(0, levelLength));
    }
}
