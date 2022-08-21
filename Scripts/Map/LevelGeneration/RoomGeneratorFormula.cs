using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Generators/RoomGenerator", fileName = "RoomGenerator")]
public class RoomGeneratorFormula : ScriptableObject
{
    [Tooltip("Room target count (might not reach)")]
    public int roomCount;
    // TODO: add separate width and length parameters (to enable narrow corridors specificly)
    [Tooltip("Room Width including walls")]
    public int minRoomWidth;
    [Tooltip("Room Width including walls")]
    public int maxRoomWidth;
    [Tooltip("How many tiles in inside floor area can the smallest room be")]
    public int minRoomArea;
    [Tooltip("How many tiles in inside floor area can the largest room be")]
    public int maxRoomArea;
    
    protected virtual void OnValidate()
    {
        if (roomCount < 0 ) roomCount = 0;

        if (minRoomWidth < 3) minRoomWidth = 3;
        if (maxRoomWidth < 3) maxRoomWidth = 3;
        if (maxRoomWidth < minRoomWidth) maxRoomWidth = minRoomWidth;
        if (minRoomArea < 0) minRoomArea = 0;
        if (maxRoomArea < 0) maxRoomArea = 0;
        if (maxRoomArea < minRoomArea) maxRoomArea = minRoomArea;
        
        if ((maxRoomWidth - 2) * (maxRoomWidth - 2) < minRoomArea)
        {
            Debug.LogError("Min Room Size cannot be fulfilled. Try decreasing Min Room Area or increasing Max Room Width.");
        }
        if ((minRoomWidth - 2) * (minRoomWidth - 2) > maxRoomArea)
        {
            Debug.LogError("Max Room Size cannot be fulfilled. Try increasing Max Room Area or decreasing Min Room Width.");
        }
    }

    // TODO: Make into a virtual method that can return any kind of room configuration, not just rectangle
    public RoomDimensions GenerateRoom()
    {
        // Create random room width and length and check that they satisfy the minimum room area
        int roomWidth = 0;
        int roomLength = 0;
        int roomArea = 0;

        for (int j = 0; j < 1000; j++)
        {
            roomWidth = Random.Range(minRoomWidth, maxRoomWidth + 1);
            roomLength = Random.Range(minRoomWidth, maxRoomWidth + 1);

            roomArea = (roomWidth - 2) * (roomLength - 2);
            if ((roomArea >= minRoomArea) && (roomArea <= maxRoomArea))
            {
                break;
            }
        }

        return new RoomDimensions(roomWidth, roomLength);
    }

    public struct RoomDimensions
    {
        public int width;
        public int length;

        public RoomDimensions(int width, int length)
        {
            this.width = width;
            this.length = length;
        }

        public bool IsZero()
        {
            return (width <= 0 || length <= 0);
        }
    }
}
