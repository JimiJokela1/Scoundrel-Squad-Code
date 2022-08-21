using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CoverManager : MapElementManager
{
    public static CoverManager Instance = null;

    public List<Transform> sceneCoverParents;

    public CellGrid cellGrid;

    public List<Cover> allCovers;

    private void Start()
    {
        // AssignCoverToCells(sceneCoverParents);
    }

    public override void AssignToCells(List<Transform> elementParents)
    {
        if (elementParents == null)
        {
            elementParents = this.sceneCoverParents;
        }
        allCovers = FindAllElements<Cover>(elementParents);

        if (allCovers != null && allCovers.Count > 0)
        {
            foreach (Cover cover in allCovers)
            {
                Transform coverTransform = cover.transform;
                Cell cell = cellGrid.Cells.OrderBy(h => Mathf.Abs((h.transform.position - coverTransform.position).sqrMagnitude)).First();

                cell.IsCover = true;
                cell.BlocksMovement = true;

                cover.cell = cell;

                // var bounds = GameMap.GetRendererBounds(coverTransform);
                // Vector3 offset = new Vector3(0, bounds.y, 0);
                coverTransform.position = cell.transform.position; // + offset;
            }
        }
    }

    /// <summary>
    /// Returns first cover found on cell.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public Cover GetCoverOnCell(Cell cell)
    {
        if (allCovers != null)
        {
            foreach (Cover cover in allCovers)
            {
                if (cover.cell == cell)
                {
                    return cover;
                }
            }
        }

        return null;
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
        Cover cover = allCovers.Find(o => o.cell == cell);
        if (cover != null)
        {
            cover.Fade();
        }
    }

    public override void UnfadeAll()
    {
        foreach (Cover cover in allCovers)
        {
            cover.Unfade();
        }
    }
}
