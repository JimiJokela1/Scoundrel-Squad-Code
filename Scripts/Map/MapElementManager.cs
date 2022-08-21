using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MapElementManager : MonoBehaviour
{
    public abstract void FadeElement(Cell cell);
    public abstract void UnfadeAll();
    public abstract void AssignToCells(List<Transform> elementParents);

    /// <summary>
    /// Method snaps obstacle objects to the nearest cell.
    /// </summary>
    public virtual void SnapToGrid<T>(CellGrid cellGrid, List<Transform> objectsParents)
    {
        List<Transform> cells = new List<Transform>();

        foreach (Transform cell in cellGrid.transform)
        {
            cells.Add(cell);
        }

        foreach (T element in FindAllElements<T>(objectsParents))
        {
            if (!(element is MonoBehaviour))
            {
                continue;
            }
            MonoBehaviour elementMono = element as MonoBehaviour;
            Transform eleTransform = elementMono.transform;
            // var bounds = GameMap.GetRendererBounds(obstacleTransform);
            var closestCell = cells.OrderBy(h => Mathf.Abs((h.transform.position - eleTransform.position).magnitude)).First();
            if (!closestCell.GetComponent<Cell>().BlocksMovement)
            {
                // Vector3 offset = new Vector3(0, bounds.y, 0);
                eleTransform.position = closestCell.transform.position; // + offset;
            }
        }
    }

    protected List<T> FindAllElements<T>(List<Transform> objectsParents)
    {
        List<T> elementList = new List<T>();

        foreach (Transform objParent in objectsParents)
        {
            List<T> elements = objParent.GetComponentsInChildren<T>().ToList();

            if (elements != null)
            {
                elementList.AddRange(elements);
            }
        }

        return elementList;
    }
}
