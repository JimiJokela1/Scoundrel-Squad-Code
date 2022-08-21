using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SquareTile3D : Square
{
    public LevelData.TileData tileData;

    public GameObject openFloorModel;
    public GameObject startElevatorFloorModel;
    public GameObject endElevatorFloorModel;
    public GameObject doorFloorModel;

    public GameObject pathGraphic;
    public GameObject pathMiddleGraphic;
    public GameObject pathDestinationGraphic;

    public GameObject highlightedGraphic;

    public GameObject coverIndicators;
    public GameObject coverIndicatorLeft;
    public GameObject coverIndicatorUp;
    public GameObject coverIndicatorRight;
    public GameObject coverIndicatorDown;

    public override bool IsWall()
    {
        return (tileData == LevelData.TileData.Wall || tileData == LevelData.TileData.Door);
    }

    public override bool IsDoor()
    {
        return InteractablePointManager.Instance.GetDoorPointOnCell(this) != null;
    }

    public void SetCoverIndicators(List<CameraOgre.Direction> coverDirections)
    {
        coverIndicatorDown.SetActive(false);
        coverIndicatorLeft.SetActive(false);
        coverIndicatorRight.SetActive(false);
        coverIndicatorUp.SetActive(false);

        foreach (CameraOgre.Direction direction in coverDirections)
        {
            switch (direction)
            {
                case CameraOgre.Direction.Down:
                    coverIndicatorDown.SetActive(true);
                    break;
                case CameraOgre.Direction.Left:
                    coverIndicatorLeft.SetActive(true);
                    break;
                case CameraOgre.Direction.Up:
                    coverIndicatorUp.SetActive(true);
                    break;
                case CameraOgre.Direction.Right:
                    coverIndicatorRight.SetActive(true);
                    break;
                default:
                    break;
            }
        }

        coverIndicators.SetActive(false);
    }

    public void ChangeTile(LevelData.TileData tileData)
    {
        this.tileData = tileData;

        switch (tileData)
        {
            case LevelData.TileData.Floor:
                ChangeFloorModel(openFloorModel);
                break;
            case LevelData.TileData.StartElevator:
                ChangeFloorModel(startElevatorFloorModel);
                break;
            case LevelData.TileData.EndElevator:
                ChangeFloorModel(endElevatorFloorModel);
                break;
            case LevelData.TileData.Door:
                ChangeFloorModel(doorFloorModel);
                break;
            default:
                ChangeFloorModel(openFloorModel);
                break;
        }
    }

    protected void ChangeFloorModel(GameObject toActivate)
    {
        openFloorModel.SetActive(false);
        startElevatorFloorModel.SetActive(false);
        endElevatorFloorModel.SetActive(false);
        doorFloorModel.SetActive(false);

        toActivate.SetActive(true);
    }

    public override Vector3 GetCellDimensions()
    {
        return GetComponentInChildren<Renderer>().bounds.size;
    }

    public override void MarkAsHighlighted()
    {
        UnMark();
        // GetComponentInChildren<Renderer>().material.color = Color.magenta;
        highlightedGraphic.SetActive(true);

        coverIndicators.SetActive(true);
    }

    public override void MarkAsPath()
    {
        GetComponentInChildren<Renderer>().material.color = Color.green;

        coverIndicators.SetActive(true);
    }

    public override void MarkAsPath(Vector2Int direction, bool destination = false)
    {
        UnMark();

        pathGraphic.SetActive(true);

        if (destination)
        {
            pathDestinationGraphic.SetActive(true);
            pathMiddleGraphic.SetActive(false);
        }
        else
        {
            pathDestinationGraphic.SetActive(false);
            pathMiddleGraphic.SetActive(true);
        }

        if (direction != Vector2Int.zero)
        {
            pathGraphic.transform.forward = new Vector3(direction.x, 0f, direction.y);
        }

        coverIndicators.SetActive(true);
    }

    public override void MarkAsReachable()
    {
        UnMark();
        GetComponentInChildren<Renderer>().material.color = Color.cyan;

        coverIndicators.SetActive(true);
    }

    public override void UnMark()
    {
        GetComponentInChildren<Renderer>().material.color = Color.white;
        pathGraphic.SetActive(false);
        highlightedGraphic.SetActive(false);

        coverIndicators.SetActive(false);
    }
}
