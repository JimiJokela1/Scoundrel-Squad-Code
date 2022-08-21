using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Generators/Level Generator Theme", fileName = "LevelGeneratorTheme.asset")]
public class LevelGeneratorTheme : ScriptableObject
{
    public List<GameObject> wallPrefabs;
    public List<GameObject> coverPrefabs;
    public List<GameObject> wallPropPrefabs;

    public List<GameObject> doorPrefabs;

    public GameObject GetRandomWallPrefab()
    {
        if (wallPrefabs == null || wallPrefabs.Count == 0)
        {
            Debug.LogError("Assign wall prefab in Level Generator Theme");
            GameObject obj = new GameObject("Wall");
            obj.AddComponent<Obstacle>();
            return obj;
        }
        return wallPrefabs[UnityEngine.Random.Range(0, wallPrefabs.Count)];
    }

    public GameObject GetRandomCoverPrefab()
    {
        if (coverPrefabs == null || coverPrefabs.Count == 0)
        {
            Debug.LogError("Assign cover prefab in Level Generator Theme");
            GameObject obj = new GameObject("Cover");
            obj.AddComponent<Cover>();
            return obj;
        }
        return coverPrefabs[UnityEngine.Random.Range(0, coverPrefabs.Count)];
    }

    public GameObject GetRandomDoorPrefab()
    {
        if (doorPrefabs == null || doorPrefabs.Count == 0)
        {
            Debug.LogError("Assign door prefab in Level Generator Theme");
            GameObject obj = new GameObject("Door");
            obj.AddComponent<DoorPoint>();
            return obj;
        }
        return doorPrefabs[UnityEngine.Random.Range(0, doorPrefabs.Count)];
    }
}
