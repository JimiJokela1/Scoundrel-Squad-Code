using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Generators/RandomLevelGenerator", fileName = "RandomLevelGenerator")]
public class RandomLevelGenerator : LevelGeneratorFormula
{
    public int obstacleCount;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (obstacleCount < 0) obstacleCount = 0;
    }

    public override LevelData GenerateLevel(int levelWidth, int levelLength, int shopCount, int lootCount, int chargePointCount)
    {
        LevelData levelData = new LevelData(levelWidth, levelLength, LevelData.TileData.Floor);

        levelData = GenerateStartElevator(levelData, levelWidth, levelLength);
        levelData = GenerateEndElevator(levelData, levelWidth, levelLength);
        
        for (int i = 0; i < obstacleCount; i++)
        {
            int x = Random.Range(0, levelWidth);
            int y = Random.Range(0, levelLength);

            Vector2Int tile = new Vector2Int(x, y);

            if (levelData.tileData[x,y] != LevelData.TileData.StartElevator
            && levelData.tileData[x,y] != LevelData.TileData.EndElevator
            && levelData.tileData[x,y] != LevelData.TileData.Wall)
            {
                levelData.tileData[x,y] = LevelData.TileData.Wall;
                // GameObject wall = Instantiate(theme.GetRandomWallPrefab(), levelParent);
                // wall.transform.position = cell.transform.position;
            }
        }

        return levelData;
    }
}
