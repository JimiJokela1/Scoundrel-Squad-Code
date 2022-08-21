using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// AI for one unit
/// </summary>
public class AIUnit
{
    private AIUnitState _state;
    public AIUnitState state
    {
        get
        {
            return _state;
        }
        set
        {
            if (value != _state)
            {
                previousState = _state;
            }

            _state = value;
        }
    }

    UberUnit _unit;
    UberUnit _enemy;

    private AIPlayer aiPlayer;
    private AIUnitState previousState;
    private CellGrid cellGrid;

    public List<Cell> patrolWayPoints;
    public Cell currentPatrolWayPoint;

    private List<UberUnit> enemiesInSight;
    private List<UberUnit> enemiesInRange;

    private bool patrolWaypointBlocked = false;

    public AIUnit(UberUnit unit, AIPlayer aiPlayer, CellGrid cellGrid, bool patrolling, int minPatrolLength, int maxPatrolLength)
    {
        _unit = unit;
        this.aiPlayer = aiPlayer;
        this.cellGrid = cellGrid;

        if (patrolling)
        {
            patrolWayPoints = new List<Cell>();
            patrolWayPoints.Add(_unit.Cell);
            // Find patrol target cell randomly and check that it is reachable and within min and max patrol length from current cell
            Cell randomCell = null;
            for (int i = 0; i < 1000; i++)
            {
                randomCell = cellGrid.Cells[Random.Range(0, cellGrid.Cells.Count)];
                if (_unit.IsCellMovableTo(randomCell))
                {
                    List<Cell> path = _unit.FindPathCarelessly(cellGrid.Cells, randomCell);
                    if (path == null || path.Count == 0) continue;

                    int pathCost = GameMap.Instance.GetMovementPathCost(path, _unit.Cell, randomCell);
                    if (pathCost >= minPatrolLength && pathCost <= maxPatrolLength)
                    {
                        // Found ok patrol target cell
                        break;
                    }
                }
            }

            patrolWayPoints.Add(randomCell);
            currentPatrolWayPoint = randomCell;
            state = AIUnitState.Patrolling;
        }
        else
        {
            patrolWayPoints = new List<Cell>();
            currentPatrolWayPoint = _unit.Cell;
            state = AIUnitState.Stationary;
        }

    }

    public IEnumerator PlaySequence(List<UberUnit> enemies)
    {
        List<Cell> availableDestinations = _unit.GetAvailableDestinations(cellGrid.Cells).ToList();
        List<Cell> carelessAvailableDestinations = _unit.GetCarelessAvailableDestinations(cellGrid.Cells).ToList();
        List<Cell> attackableCells = _unit.GetPossibleActionTargetTiles(cellGrid.Cells).ToList();
        enemiesInSight = FindEnemiesInSight(enemies);
        enemiesInRange = FindEnemiesInRange(enemiesInSight);

        // Equip weapon if not equipped
        if (Equipment.IsEmptyEquipment(_unit.activeEquipment))
        {
            if (_unit.equipped != null && _unit.equipped.Count > 0)
            {
                foreach (Equipment equipment in _unit.equipped)
                {
                    if (equipment is Weapon)
                    {
                        _unit.activeEquipment = equipment;
                    }
                }
            }
        }

        // Check state and switch state if necessary
        switch (state)
        {
            case AIUnitState.Stationary:
                // Check if enemy sighted
                if (ChooseNewEnemyTarget(enemiesInRange))
                {
                    state = AIUnitState.Hunting;
                }
                else if (ChooseNewEnemyTarget(enemiesInSight))
                {
                    state = AIUnitState.Hunting;
                }
                break;
            case AIUnitState.Patrolling:
                // Check if enemy sighted
                if (ChooseNewEnemyTarget(enemiesInRange))
                {
                    state = AIUnitState.Hunting;
                }
                else if (ChooseNewEnemyTarget(enemiesInSight))
                {
                    state = AIUnitState.Hunting;
                }
                break;
            case AIUnitState.Hunting:
                // Check if current target is alive
                if (_enemy == null)
                {
                    Debug.LogError("_enemy is null, should not happen?");
                    break;
                }

                if (!_enemy.IsAlive())
                {
                    if (!ChooseNewEnemyTarget(enemiesInSight))
                    {
                        state = previousState;
                        break;
                    }
                }

                // Check if current target is still in sight
                if (!enemiesInSight.Contains(_enemy))
                {
                    // Lost sight of enemy
                    // TODO: go to cell where enemy previously seen
                    if (!ChooseNewEnemyTarget(enemiesInSight))
                    {
                        state = previousState;
                        break;
                    }
                }
                break;
            case AIUnitState.Sleep:
                break;
            default:
                break;
        }

        // Movement and actions
        switch (state)
        {
            case AIUnitState.Stationary:
                break;
            case AIUnitState.Patrolling:
                yield return aiPlayer.StartCoroutine(PatrolAct(enemies));
                break;
            case AIUnitState.Hunting:
                yield return aiPlayer.StartCoroutine(HuntAct(enemies));
                break;
            case AIUnitState.Sleep:
                break;
            default:
                break;
        }

        yield return null;
    }

    private IEnumerator PatrolAct(List<UberUnit> enemies)
    {
        // go to next waypoint
        //, if reached set new waypoint
        // Check if enemies visible while on path
        // Stop if enemy visible from mid-path
        // So gotta precheck the path tiles for enemy visibility
        // and set new destination to tile where enemy can be seen from if such a tile is on the path

        patrolWaypointBlocked = false;

        // Check if waypoint reached
        if (_unit.Cell == currentPatrolWayPoint)
        {
            currentPatrolWayPoint = patrolWayPoints.Find(c => c != currentPatrolWayPoint);
            if (currentPatrolWayPoint == null)
            {
                Debug.LogWarning("No patrol waypoints available.");
                state = AIUnitState.Stationary;
                yield break;
            }
        }

        // Find path to waypoint
        List<Cell> path = _unit.FindPath(cellGrid.Cells, currentPatrolWayPoint);
        if (path == null || path.Count == 0)
        {
            path = _unit.FindPathCarelessly(cellGrid.Cells, currentPatrolWayPoint);
            if (path == null || path.Count == 0)
            {
                Debug.Log("No path found to patrol waypoint. Something must be blocking path. Waypoint: X:" + currentPatrolWayPoint.OffsetCoord.x + ", Y:" + currentPatrolWayPoint.OffsetCoord.y);
                yield break;
            }
        }

        // Check each path cell for enemy visibility
        Cell enemySightedCell = null;
        foreach (Cell pathCell in path)
        {
            if (FindEnemiesInSight(enemies, pathCell).Count > 0)
            {
                enemySightedCell = pathCell;
                break;
            }
        }

        if (enemySightedCell != null)
        {
            path = _unit.FindPath(cellGrid.Cells, enemySightedCell);

            yield return aiPlayer.StartCoroutine(MoveToTargetCell(path, enemySightedCell));

            enemiesInSight = FindEnemiesInSight(enemies);
            enemiesInRange = FindEnemiesInRange(enemiesInSight);

            if (ChooseNewEnemyTarget(enemies))
            {
                state = AIUnitState.Hunting;
                yield return aiPlayer.StartCoroutine(HuntAct(enemies));
            }
        }
        else
        {
            // Brute force expend all movement if possible
            for (int i = 0; i < 10; i++)
            {
                yield return aiPlayer.StartCoroutine(MoveToTargetCell(path, currentPatrolWayPoint));

                if (!_unit.HasMovementLeft())
                {
                    break;
                }

                // If patrol blocked by other unit, turn back
                if (patrolWaypointBlocked)
                {
                    currentPatrolWayPoint = patrolWayPoints.Find(c => c != currentPatrolWayPoint);
                    if (currentPatrolWayPoint == null)
                    {
                        Debug.LogWarning("No patrol waypoints available.");
                        state = AIUnitState.Stationary;
                        yield break;
                    }
                    patrolWaypointBlocked = false;
                }
            }
        }
    }

    private IEnumerator HuntAct(List<UberUnit> enemies)
    {
        yield return aiPlayer.StartCoroutine(FindAndDestroyEnemySequence());

        // Find enemies in sight and range again because this unit might have moved
        enemiesInSight = FindEnemiesInSight(enemies);
        enemiesInRange = FindEnemiesInRange(enemiesInSight);

        // Brute force try 10 times to make sure to expend rest of move and actions
        for (int i = 0; i < 10; i++)
        {
            // enemy shouldnt be null here ever but heck it lets check anyways
            // If enemy is still alive keep attacking or moving towards if possible
            if (_enemy != null && _enemy.IsAlive())
            {
                if (_unit.HasActionsOrMovementLeft())
                {
                    yield return aiPlayer.StartCoroutine(FindAndDestroyEnemySequence());

                    // Find enemies in sight and range again because this unit might have moved
                    enemiesInSight = FindEnemiesInSight(enemies);
                    enemiesInRange = FindEnemiesInRange(enemiesInSight);
                }
                else
                {
                    break;
                }
            }
            else
            {
                // Find new target
                if (!ChooseNewEnemyTarget(enemiesInRange))
                {
                    ChooseNewEnemyTarget(enemiesInSight);
                }

                // If no targets found return to previous state before hunting
                if (_enemy == null)
                {
                    state = previousState;
                    break;
                }
            }
        }
    }

    private IEnumerator FindAndDestroyEnemySequence()
    {
        if (_unit.IsUnitAttackable(_enemy, _unit.Cell))
        {
            yield return aiPlayer.StartCoroutine(AttackSequence(_enemy));
        }
        else
        {
            if (_unit.MovementPoints <= 0)
            {
                yield break;
            }

            // List<Cell> possibleMoves = _unit.GetAvailableDestinations(cellGrid.Cells).ToList();
            List<Cell> shortestPath = null;
            Cell shortestDestination = null;
            int shortestPathCost = int.MaxValue;

            // Positions from where we could attack enemy
            List<Cell> desiredDestinations = cellGrid.Cells.FindAll(c => _unit.IsCellTraversable(c) && _unit.IsUnitAttackable(_enemy, c));

            foreach (Cell destination in desiredDestinations)
            {
                List<Cell> path = _unit.FindPath(cellGrid.Cells, destination);
                int pathCost = GameMap.Instance.GetMovementPathCost(path, _unit.Cell, destination);

                if (pathCost <= 0 || path.Count == 0)
                {
                    continue;
                }

                if ((shortestPath == null) || (shortestPath != null && pathCost < shortestPathCost))
                {
                    shortestPath = path;
                    shortestPathCost = pathCost;
                    shortestDestination = destination;
                }
            }

            if (shortestPath != null)
            {
                yield return aiPlayer.StartCoroutine(MoveToTargetCell(shortestPath, shortestDestination));

                if (_unit.IsUnitAttackable(_enemy, _unit.Cell))
                {
                    yield return aiPlayer.StartCoroutine(AttackSequence(_enemy));
                }
            }
            else
            {
                Debug.Log("Cant find path to possible attack position");
            }
        }
    }

    private IEnumerator MoveToTargetCell(List<Cell> path, Cell destinationCell)
    {
        Cell doorCell = null;
        foreach (Cell pathCell in path)
        {
            if (pathCell.IsDoor() && !_unit.IsCellMovableTo(pathCell))
            {
                doorCell = pathCell;
                break;
            }
        }

        if (doorCell != null)
        {
            yield return aiPlayer.StartCoroutine(OpenDoorOnPath(doorCell, path));
        }
        else
        {
            if (_unit.CanMoveToCell(destinationCell))
            {
                yield return aiPlayer.StartCoroutine(MoveSequence(destinationCell, path));
            }
            else // move as close as possible
            {
                Cell closestDestination = FindFurthestMovableCellOnPath(path, destinationCell);

                if (closestDestination == null)
                {
                    Debug.Log("Cannot find path. Setting new patrol waypoint if patrolling.");
                    patrolWaypointBlocked = true;
                    yield break;
                }
                else
                {
                    yield return aiPlayer.StartCoroutine(MoveSequence(closestDestination, _unit.FindPath(cellGrid.Cells, closestDestination)));
                }
            }
        }
    }

    private Cell FindFurthestMovableCellOnPath(List<Cell> path, Cell destinationCell)
    {
        Cell closestDestination = null;
        int closestDistance = 0;

        foreach (Cell cell in path)
        {
            // GetDistance is unreliable
            // path can at least go around tiles, so distance should be calculated with actual path tiles
            int distance = cell.GetDistance(destinationCell);
            if ((closestDestination == null && _unit.CanMoveToCell(cell)) || (closestDestination != null && distance < closestDistance && _unit.CanMoveToCell(cell)))
            {
                closestDestination = cell;
                closestDistance = distance;
            }
        }

        return closestDestination;
    }

    private IEnumerator OpenDoorOnPath(Cell doorCell, List<Cell> path)
    {
        // If next to door already, open door
        var neighbours = _unit.Cell.GetNeighbours(cellGrid.Cells);

        if (_unit.Cell.GetNeighbours(cellGrid.Cells).Contains(doorCell))
        {
            if (OpenDoor(doorCell))
            {
                Debug.Log("Opening door that is next to me");
                yield return new WaitForSeconds(0.5f);
                // TODO: here is where we gotta jump back to pathfinding again
            }
        }
        else // Path to previous cell on path, open door, then path to destination
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (path[i] == doorCell)
                {
                    if (i - 1 < 0)
                    {
                        // Debugging which door unit is trying to open and whether it is next to unit
                        neighbours.ForEach(c => Debug.Log(c + ": X:" + c.OffsetCoord.x + ", Y:" + c.OffsetCoord.y));
                        Debug.Log("Door cell: " + doorCell + ": X:" + doorCell.OffsetCoord.x + ", Y:" + doorCell.OffsetCoord.y);
                        Debug.Log("Current patrol waypoint: " + currentPatrolWayPoint + ": X:" + currentPatrolWayPoint.OffsetCoord.x + ", Y:" + currentPatrolWayPoint.OffsetCoord.y);
                        Debug.LogError("Door is not next to me, but then it do be?");
                        yield break;
                    }

                    Cell nextToDoor = path[i - 1];
                    List<Cell> pathToDoor = _unit.FindPathCarelessly(cellGrid.Cells, nextToDoor);

                    if (_unit.CanMoveToCell(nextToDoor))
                    {
                        yield return aiPlayer.StartCoroutine(MoveSequence(nextToDoor, pathToDoor));
                        if (OpenDoor(doorCell))
                        {
                            yield return new WaitForSeconds(0.5f);
                            // TODO: here is where we gotta jump back to pathfinding again
                        }
                    }
                    else // Move towards door as close as possible
                    {
                        Cell closestDestination = FindFurthestMovableCellOnPath(pathToDoor, nextToDoor);

                        if (closestDestination == null)
                        {
                            Debug.Log("Cannot find path to next to door cell. Unit: " 
                            + _unit.gameObject.name + ", Next to door cell: " + nextToDoor 
                            + ": X:" + nextToDoor.OffsetCoord.x + ", Y:" + nextToDoor.OffsetCoord.y);
                            yield break;
                        }
                        else
                        {
                            yield return aiPlayer.StartCoroutine(MoveSequence(closestDestination, _unit.FindPath(cellGrid.Cells, closestDestination)));
                        }
                    }
                }
            }
        }
    }

    private bool OpenDoor(Cell doorCell)
    {
        DoorPoint door = InteractablePointManager.Instance.GetDoorPointOnCell(doorCell);
        if (door == null)
        {
            Debug.LogError("No door found on cell");
            return false;
        }

        door.InteractWithPoint();
        return true;
    }

    /// <summary>
    /// Chooses a random target from the list and sets it to _enemy or sets _enemy to null if list is empty.
    /// Always set AI state after doing this, because _enemy can be set to null and some states will assume _enemy is not null.
    /// </summary>
    /// <returns>Whether a target was chosen.</returns>
    private bool ChooseNewEnemyTarget(List<UberUnit> enemies)
    {
        _enemy = null;
        if (enemies.Count > 0)
        {
            _enemy = enemies[Random.Range(0, enemies.Count)];
        }

        return _enemy != null;
    }

    public void UnitDied()
    {
        state = AIUnitState.Sleep;
        _enemy = null;
    }

    public void UnitAttacked(UberUnit attacker)
    {
        _enemy = attacker;
        state = AIUnitState.Hunting;
    }

    private IEnumerator MoveSequence(Cell moveTarget, List<Cell> path)
    {
        _unit.Move(moveTarget, path);
        yield return null;
        while (_unit.isMoving)
        {
            yield return null;
        }
    }

    private IEnumerator AttackSequence(UberUnit targetUnit)
    {
        if (Attack(targetUnit))
        {
            yield return new WaitForSeconds(0.5f);

            if (targetUnit.IsAlive())
            {
                yield return aiPlayer.StartCoroutine(AttackSequence(targetUnit));
            }
        }
    }

    private bool Attack(UberUnit targetUnit)
    {
        if (_unit.activeEquipment.data is WeaponData)
        {
            return _unit.TakeAction(_enemy);
        }

        return false;
    }

    /// <summary>
    /// Finds all enemies in same room 
    /// </summary>
    private List<UberUnit> FindEnemiesInSight(List<UberUnit> enemies)
    {
        return FindEnemiesInSight(enemies, _unit.Cell);
    }

    /// <summary>
    /// Finds all enemies in same room 
    /// </summary>
    private List<UberUnit> FindEnemiesInSight(List<UberUnit> enemies, Cell sourceCell)
    {
        List<UberUnit> enemiesInSight = new List<UberUnit>();

        foreach (UberUnit enemy in enemies)
        {
            if (!GameMap.Instance.IsLineOfSightPathBlocked(GameMap.Instance.FindLineOfSight(sourceCell, enemy.Cell), sourceCell, enemy.Cell))
            {
                enemiesInSight.Add(enemy);
            }
            // if (GameMap.Instance.AreCellsInSameRoom(enemy.Cell, sourceCell, true))
            // {
            //     enemiesInSight.Add(enemy);
            // }
        }

        return enemiesInSight;
    }

    private List<UberUnit> FindEnemiesInRange(List<UberUnit> enemies)
    {
        List<UberUnit> enemiesInRange = new List<UberUnit>();

        foreach (UberUnit enemy in enemies)
        {
            if (_unit.IsUnitAttackable(enemy, _unit.Cell))
            {
                enemiesInRange.Add(enemy);
            }
        }

        return enemiesInRange;
    }

    public enum AIUnitState
    {
        Stationary,
        Patrolling,
        Hunting,
        Sleep
    }
}
