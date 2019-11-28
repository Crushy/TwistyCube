using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is mostly used as a data container and a way to bind gameobjects to cube coordinates
/// </summary>
public class IndividualCubeController : MonoBehaviour
{

    public Vector3Int goalCoordinates;
    public Vector3Int currentCoordinates;

    public SaveGameSystem.IndividualCubeData GetSerializableData()
    {
        var dataToSerialize = new SaveGameSystem.IndividualCubeData()
        {
            currentCoordinates = currentCoordinates,
            goalCoordinates = goalCoordinates,
            rotation = this.transform.localRotation
        };
        return dataToSerialize;
    }
}
