using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameMap : MonoBehaviour
{
    private static GameMap _Instance = null;
    public static GameMap Instance
    {
        get
        {
            if (_Instance != null)
            {
                return _Instance;
            }
            else
            {
                _Instance = FindObjectOfType<GameMap>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find singleton Instance of type GameMap.");
                }
                else
                {
                    _Instance.Init();
                }
                return _Instance;
            }
        }
    }

    public FogOfWar fogOfWar;

    private Dictionary<DoorPoint, List<Room>> doorConnections;

    private List<Room> currentLevelRooms;
    [HideInInspector]
    public CellGrid cellGrid;

    public Cell limboCell;

    private List<Room> exploredRooms;
    private List<Cell> cellsInSight;

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }
        else if (_Instance != this)
        {
            Debug.Log("Multiple Singleton Instance GameObjects in scene. Class: " + this.GetType().ToString() + " in GameObject: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Init();
    }

    private bool initialized = false;

    private void Init()
    {
        if (initialized) return;

        currentLevelRooms = new List<Room>();
        exploredRooms = new List<Room>();
        cellsInSight = new List<Cell>();

        cellGrid = FindObjectOfType<CellGrid>();
        if (cellGrid == null)
        {
            Debug.LogError("GameMap cannot find CellGrid in the scene");
        }

        // GameMaster.Instance.LevelLoadingDone += MapNewLevel;
        GameMaster.Instance.LevelLoadingDone += OnLevelLoadingDone;

        initialized = true;
    }

    public void MarkMovementPath(List<Cell> path, Cell startCell, Cell destinationCell)
    {
        // // Version 1: Calc direction from previous
        // Cell previousCell = startCell;
        // foreach (Cell pathCell in path)
        // {
        //     Vector3 rawDir = pathCell.transform.position - previousCell.transform.position;
        //     Vector2Int dir = new Vector2Int(Mathf.RoundToInt(rawDir.x), Mathf.RoundToInt(rawDir.z));
        //     pathCell.MarkAsPath(dir);
        //     previousCell = pathCell;
        // }

        // // Mark the destination cell separately, direction shouldn't matter if destination is marked with circle or cross
        // destinationCell.MarkAsPath(new Vector2Int(1, 0), true);

        // Version 2: Calc direction to next
        Cell nextCell = null;
        if (path.Count > 1)
        {
            nextCell = path[1];
        }
        else
        {
            nextCell = destinationCell;
        }

        foreach (Cell pathCell in path)
        {
            Vector3 rawDir = nextCell.transform.position - pathCell.transform.position;
            Vector2Int dir = new Vector2Int(Mathf.RoundToInt(rawDir.x), Mathf.RoundToInt(rawDir.z));
            pathCell.MarkAsPath(dir);

            if (path.Count > path.IndexOf(nextCell) + 1)
            {
                nextCell = path[path.IndexOf(nextCell) + 1];
            }
            else
            {
                nextCell = destinationCell;
            }
        }

        // Mark the destination cell separately, direction shouldn't matter if destination is marked with circle or cross
        destinationCell.MarkAsPath(new Vector2Int(1, 0), true);
        PathCostIndicator.Instance.ShowPathCost(GetMovementPathCost(path, startCell, destinationCell), destinationCell);
    }

    public void UnMarkMovementPath()
    {
        PathCostIndicator.Instance.HidePathCost();
    }

    private void OnLevelLoadingDone()
    {
        exploredRooms = new List<Room>();
        UpdateFogOfWar(Squad.Instance.GetAllAliveUnits());
        UpdateWallFade(Squad.Instance.GetAllAliveUnits());
    }

    public void HideLevel()
    {
        exploredRooms = new List<Room>();
        UpdateFogOfWar(new List<PlayerUnit>());
    }

    public void MapNewLevel()
    {
        MapRooms();
        MapDoors();
    }

    public bool IsCellInElevator(Cell cell)
    {
        List<Room> rooms = GetCellRooms(cell, true);
        if (rooms != null && rooms.Count > 0)
        {
            foreach (Room room in rooms)
            {
                foreach (Cell roomCell in room.cellsInRoom)
                {
                    SquareTile3D tile = (SquareTile3D) roomCell;
                    if (tile != null)
                    {
                        if (tile.tileData == LevelData.TileData.StartElevator || tile.tileData == LevelData.TileData.EndElevator)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Fade walls that are in front from camera viewpoint in rooms that are in sight
    /// </summary>
    public void UpdateWallFade(List<PlayerUnit> playerUnits)
    {
        ObstacleManager.Instance.UnfadeAll();
        InteractablePointManager.Instance.UnfadeAll();
        CoverManager.Instance.UnfadeAll();

        List<Room> roomsInSight = GetRoomsInSight(playerUnits);
        roomsInSight = roomsInSight.Union(exploredRooms).ToList();
        exploredRooms = roomsInSight;

        cellsInSight = new List<Cell>();
        foreach (Room room in roomsInSight)
        {
            cellsInSight.AddRange(room.GetCellsWithWalls());
        }

        foreach (Cell cell in cellsInSight)
        {
            if (!cell.IsWall())
            {
                continue;
            }

            Vector2Int cellCoords = new Vector2Int((int) cell.OffsetCoord.x, (int) cell.OffsetCoord.y);
            // Vector2Int upCellCoords = cellCoords + CameraOgre.Instance.GetUpDirection();
            // Vector2Int leftCellCoords = cellCoords + CameraOgre.Instance.GetLeftDirection();
            Vector2Int upleftCellCoords = cellCoords + CameraOgre.Instance.GetLeftDirection() + CameraOgre.Instance.GetUpDirection();
            // Cell upCell = cellGrid.GetCellAt(upCellCoords.x, upCellCoords.y);
            // Cell leftCell = cellGrid.GetCellAt(leftCellCoords.x, leftCellCoords.y);
            Cell upleftCell = cellGrid.GetCellAt(upleftCellCoords.x, upleftCellCoords.y);

            if (upleftCell != null) //upCell != null && leftCell != null && 
            {
                if (cellsInSight.Contains(upleftCell)) //(visibleCells.Contains(upCell) && visibleCells.Contains(leftCell)) || 
                {
                    ObstacleManager.Instance.FadeElement(cell);
                    InteractablePointManager.Instance.FadeElement(cell);
                    CoverManager.Instance.FadeElement(cell);
                }
            }
        }
    }

    public void InitFogOFWar()
    {
        int levelWidth = (int) cellGrid.Cells.OrderByDescending(c => c.OffsetCoord.x).First().OffsetCoord.x + 1;
        int levelLength = (int) cellGrid.Cells.OrderByDescending(c => c.OffsetCoord.y).First().OffsetCoord.y + 1;

        fogOfWar.Init(cellGrid.Cells, levelWidth, levelLength);
    }

    public void UpdateFogOfWar(List<PlayerUnit> playerUnits)
    {
        List<Room> roomsInSight = GetRoomsInSight(playerUnits);

        roomsInSight = roomsInSight.Union(exploredRooms).ToList();
        exploredRooms = roomsInSight;

        cellsInSight = new List<Cell>();

        foreach (Room room in roomsInSight)
        {
            cellsInSight.AddRange(room.GetCellsWithWalls());
        }

        fogOfWar.SetFogOfWar(cellsInSight, GetAllCellsList());

        // temp: dont show fog of war
        // fogOfWar.SetFogOfWar(GetAllCellsList(), GetAllCellsList());
    }

    public bool IsElementRevealed(IRevealable element)
    {
        if (element is UberUnit)
        {
            if (cellsInSight.Contains(((UberUnit)element).Cell))
            {
                return true;
            }
        }
        if(cellsInSight.Contains(element))
        {
            return true;
        }

        return fogOfWar.IsElementRevealed(element);
    }

    /// <summary>
    /// Gets rooms where player units are and adjacent rooms that are connected by open doors
    /// </summary>
    /// <param name="playerUnits"></param>
    /// <returns></returns>
    public List<Room> GetRoomsInSight(List<PlayerUnit> playerUnits)
    {
        List<Room> roomsInSight = new List<Room>();
        foreach (PlayerUnit unit in playerUnits)
        {
            List<Room> rooms = GetCellRooms(unit.Cell, true);
            if (rooms != null)
            {
                roomsInSight.AddRange(rooms);
                foreach (Room room in rooms)
                {
                    foreach (var doorConnection in doorConnections)
                    {
                        if (!doorConnection.Key.isOpen)
                        {
                            continue;
                        }

                        if (doorConnection.Value.Contains(room))
                        {
                            foreach (Room connectedRoom in doorConnection.Value)
                            {
                                if (!roomsInSight.Contains(connectedRoom))
                                {
                                    roomsInSight.Add(connectedRoom);
                                }
                            }
                        }
                    }
                }
            }
        }

        return roomsInSight;
    }

    public List<Cell> GetAllCellsList()
    {
        return cellGrid.Cells;
    }

    public int GetLineDistanceBetweenCells(Cell cell1, Cell cell2)
    {
        return FindLineOfSight(cell1, cell2).Count;
    }

    public int GetMovementPathCost(List<Cell> path, Cell startCell, Cell destinationCell)
    {
        int sum = 0;
        foreach (Cell cell in path)
        {
            if (cell != startCell)
            {
                sum += cell.MovementCost;
            }
        }

        // cant know if path contains destination because who knows what pathfinding was used smh
        if (!path.Contains(destinationCell) && startCell != destinationCell)
        {
            sum += destinationCell.MovementCost;
        }
        return sum;
    }

    public List<Cell> GetCellNeighbours(Cell cell)
    {
        return cell.GetNeighbours(cellGrid.Cells);
    }

    private void MapDoors()
    {
        doorConnections = new Dictionary<DoorPoint, List<Room>>();

        foreach (InteractablePoint point in InteractablePointManager.Instance.allInteractablePoints)
        {
            if (point is DoorPoint)
            {
                List<Room> rooms = new List<Room>();
                List<Cell> neighbours = point.cell.GetNeighbours(cellGrid.Cells);
                foreach (Cell neighbour in neighbours)
                {
                    List<Room> neighbourRooms = GetCellRooms(neighbour);

                    foreach (Room room in neighbourRooms)
                    {
                        if (room != null && !rooms.Contains(room))
                        {
                            rooms.Add(room);
                        }
                    }
                }

                doorConnections.Add((DoorPoint) point, rooms);
            }
        }
    }

    /// <summary>
    /// Maps rooms in level by going through all cells and using a recursive marching algorithm. Stores rooms found in currentLevelRooms variable. 
    /// </summary>
    public void MapRooms()
    {
        currentLevelRooms.Clear();
        currentLevelRooms = new List<Room>();

        foreach (Cell cell in cellGrid.Cells)
        {
            if (!IsCellInAnyRoom(currentLevelRooms, cell) && !cell.IsWall())
            {
                List<Cell> cellsInRoom = new List<Cell>();
                cellsInRoom.Add(cell);
                cellsInRoom = MapCellsInRoom(cellsInRoom, currentLevelRooms);
                currentLevelRooms.Add(new Room(cellsInRoom));
            }
        }
    }

    /// <summary>
    /// Checks whether cell is in any room.
    /// </summary>
    /// <param name="rooms"></param>
    /// <param name="cell"></param>
    /// <returns>True if cell is part of a room. False otherwise.</returns>
    public bool IsCellInAnyRoom(List<Room> rooms, Cell cell, bool includeWalls = false)
    {
        foreach (Room room in currentLevelRooms)
        {
            if (includeWalls)
            {
                if (room.GetCellsWithWalls().Contains(cell))
                {
                    return true;
                }
            }
            else
            {
                if (room.cellsInRoom.Contains(cell))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsCellInRoom(Room room, Cell cell, bool includeWalls = false)
    {
        if (includeWalls)
        {
            return room.GetCellsWithWalls().Contains(cell);
        }
        else
        {
            return room.cellsInRoom.Contains(cell);
        }
    }

    /// <summary>
    /// Gets room that cell is in. 
    /// </summary>
    /// <param name="cell"></param>
    /// <returns>Room that contains cell. Null if cell is not in any room.</returns>
    public List<Room> GetCellRooms(Cell cell, bool includeWalls = false)
    {
        if (currentLevelRooms == null || currentLevelRooms.Count == 0)
        {
            return null;
        }

        List<Room> rooms = new List<Room>();

        foreach (Room room in currentLevelRooms)
        {
            if (includeWalls)
            {
                if (room.GetCellsWithWalls().Contains(cell))
                {
                    rooms.Add(room);
                }
            }
            else
            {
                if (room.cellsInRoom.Contains(cell))
                {
                    rooms.Add(room);
                }
            }
        }

        return rooms;
    }

    /// <summary>
    /// Recursively maps a room by marching through the cells to neighbouring cells.
    /// </summary>
    /// <param name="cellsInRoom"></param>
    /// <param name="rooms"></param>
    /// <returns>List of cells that form a room</returns>
    private List<Cell> MapCellsInRoom(List<Cell> cellsInRoom, List<Room> rooms)
    {
        Cell lastCell = cellsInRoom[cellsInRoom.Count - 1];
        foreach (Cell cell in lastCell.GetNeighbours(cellGrid.Cells))
        {
            if (!cell.IsWall() && !cellsInRoom.Contains(cell) && !IsCellInAnyRoom(rooms, cell))
            {
                cellsInRoom.Add(cell);
                cellsInRoom = MapCellsInRoom(cellsInRoom, rooms);
            }
        }

        return cellsInRoom;
    }

    /// <summary>
    /// Tells if cells are within the same room. Finds rooms that cells belong to and compares them. 
    /// </summary>
    /// <param name="cell1"></param>
    /// <param name="cell2"></param>
    /// <returns>True if both cells are in the same non-null room. False otherwise.</returns>
    public bool AreCellsInSameRoom(Cell cell1, Cell cell2, bool includeWalls = false)
    {
        List<Room> rooms1 = GetCellRooms(cell1, includeWalls);
        List<Room> rooms2 = GetCellRooms(cell2, includeWalls);
        foreach (Room room in rooms1)
        {
            if (room != null && rooms2.Contains(room))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if line of sight is blocked anywhere on the path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="startCell"></param>
    /// <param name="endCell"></param>
    /// <returns>True if any Cell (except those that equal startCell or endCell) on path blocks line of sight. False if no cell on path blocks line of sight (except start or end). </returns>
    public bool IsLineOfSightPathBlocked(List<Cell> path, Cell startCell, Cell endCell)
    {
        bool blocked = false;
        foreach (Cell cellOnPath in path)
        {
            if (cellOnPath.BlocksLineOfSight && cellOnPath != endCell && cellOnPath != startCell)
            {
                blocked = true;
            }
        }

        return blocked;
    }

    /// <summary>
    /// Checks if target cell is in cover. 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="startCell"></param>
    /// <param name="targetCell"></param>
    /// <returns>True, if second to last cell on path just before target cell is a cover cell. False otherwise.</returns>
    public bool IsTargetCellInCover(List<Cell> path, Cell startCell, Cell targetCell)
    {
        path = path.OrderByDescending(c => (c.GetDistance(targetCell))).ToList();
        // If close quarters, there's no cover
        if (path == null || path.Count <= 1)
        {
            return false;
        }

        // Checking here if path contains the endcell to figure out which cell in path is the second to last
        if (path[path.Count - 1] == targetCell)
        {
            if (path[path.Count - 2] == startCell)
            {
                return false;
            }
            else
            {
                return (path[path.Count - 2].IsCover);
            }
        }
        else
        {
            if (path[path.Count - 1] == startCell)
            {
                return false;
            }
            else
            {
                return path[path.Count - 1].IsCover;
            }
        }
    }

    /// <summary>
    /// Finds line of sight path from startCell to targetCell.
    /// </summary>
    /// <param name="startCell"></param>
    /// <param name="targetCell"></param>
    /// <returns>List of cells that form path. Empty list if no path is found.</returns>
    public List<Cell> FindLineOfSight(Cell startCell, Cell targetCell)
    {
        return FindLineOfSight(cellGrid.Cells, startCell, targetCell);
    }

    /// <summary>
    /// Finds line of sight path within given cells. 
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="startCell"></param>
    /// <param name="targetCell"></param>
    /// <returns>List of cells that form path. Empty list if no path is found.</returns>
    public List<Cell> FindLineOfSight(List<Cell> cells, Cell startCell, Cell targetCell)
    {
        Vector2Int startPoint = new Vector2Int((int) startCell.OffsetCoord.x, (int) startCell.OffsetCoord.y);
        Vector2Int targetPoint = new Vector2Int((int) targetCell.OffsetCoord.x, (int) targetCell.OffsetCoord.y);

        List<Vector2Int> linePoints = PlotLine(startPoint.x, startPoint.y, targetPoint.x, targetPoint.y);

        List<Cell> path = new List<Cell>();

        foreach (var point in linePoints)
        {
            Cell cellAtPoint = cells.Find(c => (int) c.OffsetCoord.x == point.x && (int) c.OffsetCoord.y == point.y);
            if (cellAtPoint != null)
            {
                path.Add(cellAtPoint);
            }
        }

        // flip path if necessary
        return OrientPath(path, startCell, targetCell);
    }

    /// <summary>
    /// Makes sure path is oriented so that startcell is next to first cell in path and destination cell is last cell in path
    /// </summary>
    /// <returns>Oriented path</returns>
    public List<Cell> OrientPath(List<Cell> path, Cell startCell, Cell destinationCell)
    {
        // flip path if necessary
        if (path.Count > 1)
        {
            bool flip = false;
            // if (startCell == path[0])
            // {
            //     flip = false;
            // }
            // else if (startCell == path[path.Count - 1])
            // {
            //     flip = true;
            // }

            // if (destinationCell == path[0] && path.Count > 1)
            // {
            //     flip = true;
            // }

            if (path[0].GetDistance(startCell) > 1)
            {
                flip = true;
            }

            if (flip)
            {
                path.Reverse();
            }

            // remove start cell if it is in path
            path.Remove(startCell);

            // add target cell if not already in path
            if (!path.Contains(destinationCell))
            {
                path.Add(destinationCell);
            }
        }

        return path;
    }

    private static List<Vector2Int> PlotLineLow(int x0, int y0, int x1, int y1)
    {
        List<Vector2Int> linePoints = new List<Vector2Int>();

        int dx = x1 - x0;
        int dy = y1 - y0;

        int yi = 1;

        if (dy < 0)
        {
            yi = -1;
            dy = -dy;
        }

        // Comparing errorValuation to 0 tells us whether line should move on the weaker axis
        // It would normally be calculated as float but it looks weird because we want to keep it integer to optimize
        int errorValuation = 2 * dy - dx;
        int y = y0;

        for (int x = x0; x < x1; x++)
        {
            if (errorValuation > 0)
            {
                y = y + yi;
                errorValuation -= 2 * dx;

            }
            errorValuation += 2 * dy;

            linePoints.Add(new Vector2Int(x + 1, y));
        }

        return linePoints;
    }

    private static List<Vector2Int> PlotLineHigh(int x0, int y0, int x1, int y1)
    {
        List<Vector2Int> linePoints = new List<Vector2Int>();

        int dx = x1 - x0;
        int dy = y1 - y0;

        int xi = 1;

        if (dx < 0)
        {
            xi = -1;
            dx = -dx;
        }

        // Comparing errorValuation to 0 tells us whether line should move on the weaker axis
        // It would normally be calculated as float but it looks weird because we want to keep it integer to optimize
        int errorValuation = 2 * dx - dy;
        int x = x0;

        for (int y = y0; y < y1; y++)
        {
            if (errorValuation > 0)
            {
                x = x + xi;
                errorValuation -= 2 * dy;
            }
            errorValuation += 2 * dx;

            linePoints.Add(new Vector2Int(x, y + 1));
        }

        return linePoints;
    }

    private static List<Vector2Int> PlotLine(int x0, int y0, int x1, int y1)
    {
        if (Mathf.Abs(y1 - y0) < Mathf.Abs(x1 - x0))
        {
            if (x0 > x1)
            {
                return PlotLineLow(x1, y1, x0, y0);
            }
            else
            {
                return PlotLineLow(x0, y0, x1, y1);
            }
        }
        else
        {
            if (y0 > y1)
            {
                return PlotLineHigh(x1, y1, x0, y0);
            }
            else
            {
                return PlotLineHigh(x0, y0, x1, y1);
            }
        }
    }

    public static Vector3 GetRendererBounds(Transform transform)
    {
        Renderer renderer = transform.GetComponent<Renderer>();
        Bounds combinedBounds = new Bounds();
        if (renderer != null)
        {
            combinedBounds = renderer.bounds;
        }
        Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer childRenderer in renderers)
        {
            if (childRenderer != renderer)
            {
                combinedBounds.Encapsulate(childRenderer.bounds);
            }
        }

        return combinedBounds.size;
    }

    // public static List<Cell> FindLineOfSight(List<Cell> cells, Cell startCell, Cell targetCell)
    // {
    //     List<Cell> line = new List<Cell>();

    //     Cell currentCell = startCell;
    //     int currentX = (int) currentCell.OffsetCoord.x;
    //     int currentY = (int) currentCell.OffsetCoord.y;

    //     int origDeltaX = (int) targetCell.OffsetCoord.x - (int) currentCell.OffsetCoord.x;
    //     int origDeltaY = (int) targetCell.OffsetCoord.y - (int) currentCell.OffsetCoord.y;

    //     float xyRatio = 1f;
    //     if (origDeltaY != 0)
    //     {
    //         xyRatio = origDeltaX / origDeltaY;
    //     }

    //     while (currentCell != targetCell)
    //     {
    //         int dx = (int) targetCell.OffsetCoord.x - (int) currentCell.OffsetCoord.x;
    //         int dy = (int) targetCell.OffsetCoord.y - (int) currentCell.OffsetCoord.y;
    //     }

    //     return line;
    // }
}
