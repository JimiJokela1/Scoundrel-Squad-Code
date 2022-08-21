using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleManager : MapElementManager
{
    public static ObstacleManager Instance = null;

    public List<Transform> sceneObstacleParents;

    public CellGrid cellGrid;

    public List<Obstacle> allObstacles;

    private void Start()
    {
        // AssignObstacleToCells(sceneObstacleParents);
    }

    public override void AssignToCells(List<Transform> elementParents)
    {
        if (elementParents == null)
        {
            elementParents = this.sceneObstacleParents;
        }
        allObstacles = new List<Obstacle>();
        allObstacles = FindAllElements<Obstacle>(elementParents);

        if (allObstacles != null && allObstacles.Count > 0)
        {
            foreach (Obstacle obstacle in allObstacles)
            {
                Transform obstacleTransform = obstacle.transform;
                Cell cell = cellGrid.Cells.OrderBy(h => Mathf.Abs((h.transform.position - obstacleTransform.position).sqrMagnitude)).First();

                cell.IsCover = true;
                cell.BlocksMovement = true;
                cell.BlocksLineOfSight = true;

                obstacle.cell = cell;

                // var bounds = GameMap.GetRendererBounds(obstacleTransform);
                // Vector3 offset = new Vector3(0, bounds.y, 0);
                obstacleTransform.position = cell.transform.position; // + offset;
            }
        }
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

    public override void UnfadeAll()
    {
        foreach (Obstacle obstacle in allObstacles)
        {
            obstacle.Unfade();
        }
    }

    public override void FadeElement(Cell cell)
    {
        Obstacle obstacle = allObstacles.Find(o => o.cell == cell);
        if (obstacle != null)
        {
            obstacle.Fade();
        }
    }
}
