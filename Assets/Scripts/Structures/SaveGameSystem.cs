using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class SaveGameSystem
{
    [System.Serializable]
    public struct SerializableCubeData
    {
        public IndividualCubeData[] cubeData;
        //Could be cumputed from the amount of cubes but keeping it here makes it more future proof
        public int gameSize;

        //public int accumulatedTime;
    }

    [System.Serializable]
    public class IndividualCubeData
    {
        public Vector3Int goalCoordinates;
        public Vector3Int currentCoordinates;
        public Quaternion rotation;
    }

    private const string cubeDataKey = "cubes";
    private const string timeDataKey = "time";

    public static void SaveGameState(SerializableCubeData cubeData, System.TimeSpan timeTakenSoFar)
    {
        
        PlayerPrefs.SetString(cubeDataKey, JsonUtility.ToJson(cubeData));
        Debug.Log(timeTakenSoFar.TotalSeconds.ToString());
        PlayerPrefs.SetString(timeDataKey, timeTakenSoFar.TotalSeconds.ToString());
        
        PlayerPrefs.Save();

        Debug.Log(PlayerPrefs.GetString(timeDataKey));
        Debug.Log(PlayerPrefs.GetString(cubeDataKey));
    }

    public static void LoadGameState(out SerializableCubeData cubeData, out System.TimeSpan timeTakenSoFar)
    {
        

        if (!PlayerPrefs.HasKey(cubeDataKey) || !PlayerPrefs.HasKey(timeDataKey))
        {
            Debug.LogError("Serialized data not found");
            cubeData = default;
            timeTakenSoFar = default;
            return;
        }

        //Extract and unpack the cube grid data
        string jsonString = PlayerPrefs.GetString(cubeDataKey, "");
        cubeData = JsonUtility.FromJson<SerializableCubeData>(jsonString);


        //Extract and unpack the time data
        string timeAsDoubleString = PlayerPrefs.GetString(timeDataKey, "");
        timeTakenSoFar = System.TimeSpan.FromSeconds(double.Parse(timeAsDoubleString));
        Debug.Log(timeTakenSoFar);


    }
}