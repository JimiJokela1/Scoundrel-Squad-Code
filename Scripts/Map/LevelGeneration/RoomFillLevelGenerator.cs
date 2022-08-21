using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu (menuName = "Level Generators/RoomFillLevelGenerator", fileName = "RoomFillLevelGenerator")]
public class RoomFillLevelGenerator : LevelGeneratorFormula
{
    public int roomTries;

    public List<RoomGeneratorFormula> roomGenerators;

    public List<RoomGeneratorFormula> overlappingRoomGenerators;

    public int marchingFillIterations;
    public int marchMinSurroundingWalls;
    public int marchMinSurroundingOpen;
    public int marchMaxSurroundingIslands;

    [Range (0f, 1f)]
    public float chanceToOpenExtraConnector;

    public int minCovers;
    public int maxCovers;

    public int minWallProps;
    public int maxWallProps;

    protected override void OnValidate ()
    {
        base.OnValidate ();

        if (roomTries < 0) roomTries = 0;
        if (marchingFillIterations < 0) marchingFillIterations = 0;
        if (marchMinSurroundingWalls < 0) marchMinSurroundingWalls = 0;
        if (marchMinSurroundingOpen < 0) marchMinSurroundingOpen = 0;
        if (marchMaxSurroundingIslands < 0) marchMaxSurroundingIslands = 0;
        if (minCovers < 0) minCovers = 0;
        if (maxCovers < 0) maxCovers = 0;
        if (maxCovers < minCovers) maxCovers = minCovers;
        if (minWallProps < 0) minWallProps = 0;
        if (maxWallProps < 0) maxWallProps = 0;
        if (maxWallProps < minWallProps) maxWallProps = minWallProps;
    }

    public override LevelData GenerateLevel (int levelWidth, int levelLength, int shopCount, int lootCount, int chargePointCount)
    {
        LevelData levelData = new LevelData (levelWidth, levelLength, LevelData.TileData.Wall);

        levelData = GenerateStartElevator (levelData, levelWidth, levelLength);
        levelData = GenerateEndElevator (levelData, levelWidth, levelLength);

        // Slap rooms on map
        // Slap extra overlapping rooms on map
        // Fill remaining areas by cellular automata
        // Add doors by finding tiles with 2 different rooms adjacent (von neumann neighbourhood (4 direct wall neighbours))

        // Initial rooms
        // Try to slap down random rooms, checking that they are not overlapping any other rooms
        foreach (RoomGeneratorFormula roomGen in roomGenerators)
        {
            int count = 0;
            if (roomGen.roomCount > 0)
            {
                for (int i = 0; i < roomTries; i++)
                {
                    RoomGeneratorFormula.RoomDimensions room = roomGen.GenerateRoom ();
                    RoomPosition roomPos = GenerateRoomPosition (levelData, levelWidth, levelLength, room, false);

                    // Disqualify possible room if no valid positions found
                    if (roomPos.blocked) continue;

                    // Add room
                    levelData.AddRoom (roomPos.x, roomPos.y, room.width, room.length);
                    count++;
                    if (count >= roomGen.roomCount) break;
                }
            }

            Debug.Log ("Generated " + count + " rooms from room generator \"" + roomGen.name + "\"");
        }

        // Overlapping rooms
        // Slap down random rooms that can overlap any room other than elevators
        foreach (RoomGeneratorFormula roomGen in overlappingRoomGenerators)
        {
            int count = 0;
            if (roomGen.roomCount > 0)
            {
                for (int i = 0; i < roomTries; i++)
                {
                    RoomGeneratorFormula.RoomDimensions room = roomGen.GenerateRoom ();
                    RoomPosition roomPos = GenerateRoomPosition (levelData, levelWidth, levelLength, room, true);

                    // Disqualify possible room if no valid positions found
                    if (roomPos.blocked) continue;

                    // Add room 
                    levelData.AddRoom (roomPos.x, roomPos.y, room.width, room.length, true);
                    count++;
                    if (count >= roomGen.roomCount) break;
                }
            }

            Debug.Log ("Generated " + count + " overlapping rooms from room generator \"" + roomGen.name + "\"");
        }

        // Fill remaining space by cellular automata
        for (int i = 0; i < marchingFillIterations; i++)
        {
            for (int y = 0; y < levelLength; y++)
            {
                for (int x = 0; x < levelWidth; x++)
                {
                    List<LevelData.RoomData> rooms = levelData.GetNeighbouringRooms (x, y);
                    LevelData.RoomData room = null;
                    if (rooms.Count > 0)
                    {
                        // First room is this tile's room
                        room = rooms[0];
                    }
                    levelData = CarveWall (levelData, room, x, y, levelWidth, levelLength, marchMaxSurroundingIslands, marchMinSurroundingOpen, marchMinSurroundingWalls);
                }
            }
            // levelData = CarveExtraSpaceOut(levelData, levelWidth, levelLength);
        }

        // Add doors
        levelData = ConnectRooms (levelData, levelWidth, levelLength, chanceToOpenExtraConnector);

        // Add shop and loot locations
        levelData = AddShopsAndLoot (levelData, levelWidth, levelLength, shopCount + lootCount + chargePointCount);

        // Add cover and props locations
        levelData = AddCoverAndProps (levelData, levelWidth, levelLength, minCovers, maxCovers);

        // Add wall props
        // levelData = AddWallProps(levelData, levelWidth, levelLength, minWallProps, maxWallProps);

        return levelData;
    }

    /// <summary>
    /// Generates a room position (lower left corner) that doesn't make the given room go off map or overlap elevators. 
    /// </summary>
    /// <param name="levelData"></param>
    /// <param name="levelWidth"></param>
    /// <param name="levelLength"></param>
    /// <param name="room"></param>
    /// <returns>RoomPosition.blocked is true if no valid position was found.</returns>
    protected RoomPosition GenerateRoomPosition (LevelData levelData, int levelWidth, int levelLength, RoomGeneratorFormula.RoomDimensions room, bool overlapping)
    {
        RoomPosition roomPos = new RoomPosition ();
        roomPos.x = 0;
        roomPos.y = 0;
        roomPos.blocked = true;

        // Find all wall tiles
        List<TileCoordinate> possibleTiles = new List<TileCoordinate> ();
        for (int y = 0; y < levelLength; y++)
        {
            for (int x = 0; x < levelWidth; x++)
            {
                if (levelData.tileData[x, y] == LevelData.TileData.Wall)
                {
                    possibleTiles.Add (new TileCoordinate (x, y));
                }
            }
        }

        while (possibleTiles.Count > 0)
        {
            TileCoordinate randomTile = possibleTiles[Random.Range (0, possibleTiles.Count)];
            roomPos.x = randomTile.x;
            roomPos.y = randomTile.y;

            // Check if any tile in possible room is illegal(off map) or overlapping rooms
            roomPos.blocked = false;
            TileCoordinate blockedTile = new TileCoordinate (roomPos.x, roomPos.y);
            for (int y = roomPos.y; y < roomPos.y + room.length; y++)
            {
                for (int x = roomPos.x; x < roomPos.x + room.width; x++)
                {
                    if (overlapping) // Check for overlap only with elevators
                    {
                        if (!levelData.IsLegalMapPoint (x, y) ||
                            levelData.IsPointOverlappingRoom (x, y, levelData.startElevator) ||
                            levelData.IsPointOverlappingRoom (x, y, levelData.endElevator))
                        {
                            roomPos.blocked = true;
                            blockedTile = new TileCoordinate (x, y);
                            break;
                        }
                    }
                    else // Check for overlap with any room
                    {
                        if (!levelData.IsLegalMapPoint (x, y) ||
                            levelData.IsPointOverlappingAnyRoom (x, y))
                        {
                            roomPos.blocked = true;
                            blockedTile = new TileCoordinate (x, y);
                            break;
                        }
                    }
                }

                if (roomPos.blocked) break;
            }

            // Remove blocked tiles from list of possible tiles
            if (roomPos.blocked)
            {
                for (int y = roomPos.y; y <= blockedTile.y; y++)
                {
                    for (int x = roomPos.x; x <= blockedTile.x; x++)
                    {
                        possibleTiles.Remove (new TileCoordinate (x, y));
                    }
                }
                continue;
            }
            return roomPos;
        }
        return roomPos;
    }

    protected struct TileCoordinate
    {
        public int x;
        public int y;

        public TileCoordinate (int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals (object obj)
        {
            if (!(obj is TileCoordinate)) return false;
            TileCoordinate other = (TileCoordinate) obj;
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    protected struct RoomPosition
    {
        public int x;
        public int y;
        public bool blocked;
    }
}
