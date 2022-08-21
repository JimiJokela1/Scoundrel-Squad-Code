using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargePointSpawner : MonoBehaviour
{
    private static ChargePointSpawner _Instance = null;
    public static ChargePointSpawner Instance
    {
        get
        {
            if (_Instance != null)
            {
                return _Instance;
            }
            else
            {
                _Instance = FindObjectOfType<ChargePointSpawner>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find Instance of ChargePointSpawner in scene");
                }
                return _Instance;
            }
        }
    }

    public GameObject chargePointPrefab;
    
    public ChargePoint SpawnChargePoint(Cell cell, ChargePointData data, CameraOgre.Direction dir)
    {
        ChargePointContents contents = new ChargePointContents(data);
        ChargePoint point = CreateChargePoint(cell, CameraOgre.Instance.directionVectors[dir]);
        point.Init(cell, contents);
        InteractablePointManager.Instance.AddPoint(point, cell);

        return point;
    }

    protected virtual ChargePoint CreateChargePoint(Cell cell, Vector2Int dir)
    {
        GameObject chargePointObject = Instantiate(chargePointPrefab, LevelGenerator.Instance.levelObjectsParent);
        chargePointObject.transform.position = cell.transform.position;

        Quaternion destination_rot = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.y));
        destination_rot = Quaternion.Euler(0f, destination_rot.eulerAngles.y, 0f);
        chargePointObject.transform.localRotation = destination_rot;

        ChargePoint chargePoint = chargePointObject.GetComponent<ChargePoint>();
        if (chargePoint == null)
        {
            Debug.LogError("Invalid Charge Point Prefab. It must have ChargePoint script attached to it.");
            Destroy(chargePointObject);
            return null;
        }

        return chargePoint;
    }
}
