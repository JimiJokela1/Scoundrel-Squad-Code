using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room
{
    public List<Cell> cellsInRoom;

    private List<Cell> cellsWithWalls;

    public Room()
    {
        cellsInRoom = new List<Cell>();
        cellsWithWalls = new List<Cell>();
    }

    public Room(List<Cell> cellsInRoom)
    {
        this.cellsInRoom = cellsInRoom;
        cellsWithWalls = new List<Cell>();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || (!(obj is Room)))
        {
            return false;
        }

        return ((Room)obj).cellsInRoom.Equals(cellsInRoom);
    }

    public override int GetHashCode()
    {
        return cellsInRoom.GetHashCode();
    }

    public List<Cell> GetCellsWithWalls()
    {
        if (cellsWithWalls == null || cellsWithWalls.Count == 0)
        {
            cellsWithWalls = new List<Cell>(cellsInRoom);
            foreach(Cell cell in cellsInRoom)
            {
                List<Cell> surrounding = GameMap.Instance.cellGrid.GetSurroundingCells(cell);
                foreach(Cell surroundingCell in surrounding)
                {
                    if (!cellsWithWalls.Contains(surroundingCell))
                    {
                        cellsWithWalls.Add(surroundingCell);
                    }
                }
            }
        }

        return cellsWithWalls;
    }
}
