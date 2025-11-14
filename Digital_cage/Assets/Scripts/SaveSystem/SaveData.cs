using System;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string sceneName;

    // Вместо Vector3 сохраняем отдельные float
    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;

    public string saveTime;

    // Свойство для удобного доступа к Vector3
    public Vector3 PlayerPosition
    {
        get { return new Vector3(playerPositionX, playerPositionY, playerPositionZ); }
        set
        {
            playerPositionX = value.x;
            playerPositionY = value.y;
            playerPositionZ = value.z;
        }
    }
}