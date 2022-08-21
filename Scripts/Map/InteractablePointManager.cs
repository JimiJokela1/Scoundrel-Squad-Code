using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractablePointManager : MapElementManager
{
    public static InteractablePointManager Instance = null;

    public List<Transform> scenePointParents;

    public CellGrid cellGrid;

    public List<InteractablePoint> allInteractablePoints;

    public override void AssignToCells(List<Transform> elementParents)
    {
        if (elementParents == null)
        {
            elementParents = this.scenePointParents;
        }
        List<InteractablePoint> pointList = FindAllElements<InteractablePoint>(elementParents);

        if (pointList != null && pointList.Count > 0)
        {
            foreach (InteractablePoint point in pointList)
            {
                if (allInteractablePoints.Contains(point))
                {
                    continue;
                }
                Cell cell = cellGrid.Cells.OrderBy(h => Mathf.Abs((h.transform.position - point.transform.position).sqrMagnitude)).First();

                point.Init(cell, null);
                AddPoint(point, cell);
            }
        }
    }

    public List<LootPoint> GetLootPointsInRooms(List<Room> rooms)
    {
        List<LootPoint> lootPoints = new List<LootPoint>();

        foreach (InteractablePoint point in allInteractablePoints)
        {
            if (point is LootPoint)
            {
                foreach (Room room in rooms)
                {
                    if (GameMap.Instance.IsCellInRoom(room, point.cell, true))
                    {
                        lootPoints.Add((LootPoint) point);
                        break;
                    }
                }
            }
        }

        return lootPoints;
    }

    public void AddPoint(InteractablePoint point, Cell cell)
    {
        allInteractablePoints.Add(point);
        point.PointDestroyed += OnPointDestroyed;

        cell.IsCover = true;
        cell.BlocksMovement = true;
        cell.BlocksLineOfSight = true;

        var bounds = GameMap.GetRendererBounds(cell.transform);
        Vector3 offset = new Vector3(0, bounds.y, 0);
        point.transform.position = cell.transform.position + offset;

        cellGrid.AddInteractablePoint(point);
    }

    private void OnPointDestroyed(object sender, EventArgs e)
    {
        allInteractablePoints.Remove((sender as InteractablePoint));
    }

    public DoorPoint GetDoorPointOnCell(Cell cell)
    {
        List<InteractablePoint> points = GetActivePointsOnCell(cell);
        foreach (InteractablePoint point in points)
        {
            if (point is DoorPoint)
            {
                return (DoorPoint) point;
            }
        }
        return null;
    }

    public ShopPoint GetShopPointOnCell(Cell cell)
    {
        List<InteractablePoint> points = GetActivePointsOnCell(cell);
        foreach (InteractablePoint point in points)
        {
            if (point is ShopPoint)
            {
                return (ShopPoint) point;
            }
        }
        return null;
    }

    public LootPoint GetLootPointOnCell(Cell cell)
    {
        List<InteractablePoint> points = GetActivePointsOnCell(cell);
        foreach (InteractablePoint point in points)
        {
            if (point is LootPoint)
            {
                return (LootPoint) point;
            }
        }
        return null;
    }

    public List<InteractablePoint> GetActivePointsOnCell(Cell cell)
    {
        List<InteractablePoint> points = new List<InteractablePoint>();
        if (allInteractablePoints != null)
        {
            foreach (InteractablePoint point in allInteractablePoints)
            {
                if (point.cell == cell)
                {
                    points.Add(point);
                }
            }
        }
        return points;
    }

    public InteractablePoint GetAnyActivePointOnCell(Cell cell)
    {
        if (allInteractablePoints != null)
        {
            foreach (InteractablePoint point in allInteractablePoints)
            {
                if (point.cell == cell)
                {
                    return point;
                }
            }
        }
        return null;
    }

    public List<InteractablePoint> GetAllLootPoints()
    {
        List<InteractablePoint> points = new List<InteractablePoint>();

        foreach (InteractablePoint point in allInteractablePoints)
        {
            if (point is LootPoint)
            {
                points.Add(point);
            }
        }

        return points;
    }

    public List<InteractablePoint> GetAllShopPoints()
    {
        List<InteractablePoint> points = new List<InteractablePoint>();

        foreach (InteractablePoint point in allInteractablePoints)
        {
            if (point is ShopPoint)
            {
                points.Add(point);
            }
        }

        return points;
    }

    public List<InteractablePoint> GetAllChargePointsOfType(ChargePoint.ChargeType chargeType)
    {
        List<InteractablePoint> points = new List<InteractablePoint>();

        foreach (InteractablePoint point in allInteractablePoints)
        {
            if (point is ChargePoint)
            {
                if (((ChargePoint) point).chargeType == chargeType)
                {
                    points.Add(point);
                }
            }
        }

        return points;
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

    public override void FadeElement(Cell cell)
    {
        InteractablePoint point = allInteractablePoints.Find(o => o.cell == cell);
        if (point != null)
        {
            point.Fade();
        }
    }

    public override void UnfadeAll()
    {
        foreach (InteractablePoint point in allInteractablePoints)
        {
            point.Unfade();
        }
    }
}
