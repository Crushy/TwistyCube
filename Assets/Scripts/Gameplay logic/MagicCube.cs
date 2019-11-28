using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basically a wrapper around the "mostly pure" game logic. 
/// It generates lambda functions that perform a cube rotation (that can be animated by the GameManager)
/// and generates new cubes when the game starts/resumes
/// </summary>
public class MagicCube
{
    private int gameSize;
    public int CubeSize
    {
        get { return gameSize; }
    }

    private float individualCubeSize;

    private IndividualCubeController individualCubePrefab;

    private Transform cubesParent;

    private List<IndividualCubeController> allCubes;

    //Nullable is here because Vector3Ints can't be defaulted to null
    private IndividualCubeController CreateSubCube(int x, int y, int z, System.Nullable<Vector3Int> goal = null)
    {
        var newCube = GameObject.Instantiate(this.individualCubePrefab, cubesParent);

        newCube.transform.position =
            CubeCoordinatesToWorld(x, y, z);

        //Cube was placed, now let's do housekeeping
        newCube.currentCoordinates = new Vector3Int(x, y, z);

        if (goal.HasValue)
        {
            newCube.goalCoordinates = goal.Value;
        }
        else
        {
            newCube.goalCoordinates = newCube.currentCoordinates;
        }
        allCubes.Add(newCube);
        return newCube;
    }

    private MagicCube(int gameSize, float individualCubeSizeInWorldUnits, IndividualCubeController individualCubePrefab)
    {
        this.gameSize = gameSize;
        this.cubesParent = new GameObject().transform;
        this.individualCubePrefab = individualCubePrefab;

        this.individualCubeSize = individualCubeSizeInWorldUnits;
        this.allCubes = new List<IndividualCubeController>(Mathf.RoundToInt(Mathf.Pow(gameSize, 3)));
    }

    //Creates the entire Rubik cube out of smaller cubes that make up a "shell"
    public static MagicCube CreateFromNewGame(int gameSize, float individualCubeSizeInWorldUnits, IndividualCubeController individualCubePrefab)
    {
        Debug.Assert(gameSize > 1);
        var newCube = new MagicCube(gameSize, individualCubeSizeInWorldUnits, individualCubePrefab);

        //Possibly this could be refactored into a helper method that creates planes of cubes of a certain side

        //X plates (full size)
        for (int j = 0; j < gameSize; j++)
            for (int k = 0; k < gameSize; k++)
                newCube.CreateSubCube(0, j, k);

        for (int j = 0; j < gameSize; j++)
            for (int k = 0; k < gameSize; k++)
                newCube.CreateSubCube(gameSize - 1, j, k);

        //Second plates (smaller)

        //y plates
        for (int i = 1; i < gameSize-1; i++)
            for (int k = 0; k < gameSize; k++)
                newCube.CreateSubCube(i, 0, k);

        for (int i = 1; i < gameSize-1; i++)
            for (int k = 0; k < gameSize; k++)
                newCube.CreateSubCube(i, gameSize - 1, k);

        //Z plates
        for (int i = 1; i < gameSize - 1; i++)
            for (int j = 1; j < gameSize - 1; j++)
                newCube.CreateSubCube(i, j, 0);

        for (int i = 1; i < gameSize - 1; i++)
            for (int j = 1; j < gameSize - 1; j++)
                newCube.CreateSubCube(i, j, gameSize - 1);

        // Code for a full cube
        //for (int i = 0; i<gameSize ; i++) {
        //    for (int j = 0; j<gameSize ; j++) {
        //        for (int k = 0; k<gameSize ; k++) {
        //            var newCube = GameObject.Instantiate(this.individualCubePrefab, cubesParent);

        //            newCube.transform.position = 
        //                CubeCoordinatesToWorld(i,j,k);

        //            //Cube was placed, now let's do housekeeping
        //            newCube.goalCoordinates = new Vector3Int(i,j,k);
        //            newCube.currentCoordinates = newCube.goalCoordinates;
        //            allCubes.Add(newCube);
        //        }
        //    }
        //}
        return newCube;
    }

    #region Pivoting

    public struct PivotRotationActions
    {
        public System.Action startAction;
        public InterpolationUtils.InterpolatedMethod pivotingAction;
        public System.Action finalAction;
    }

    public PivotRotationActions RotatePivot(CubeRotationAxis chosenAxis, int pivotIndex, bool directionUpOrLeft)
    {
        List<IndividualCubeController> cubesToRotate = new List<IndividualCubeController>(gameSize * gameSize);
        Vector3 rotationCentre;
        Quaternion targetQuaternion;
        float rotationDirection = directionUpOrLeft ? 1f : -1f;

        Vector3 axis = Vector3.zero;
        switch (chosenAxis)
        {
            case CubeRotationAxis.XAxis:
                axis = new Vector3(1, 0, 0);
                break;
            case CubeRotationAxis.YAxis:
                axis = new Vector3(0, 1, 0);
                break;
            case CubeRotationAxis.ZAxis:
                axis = new Vector3(0, 0, 1);
                break;
            default:
                Debug.LogError("Invalid axis");
                break;
        }

        GetAllCubesAffectedByPivot(chosenAxis, pivotIndex, ref cubesToRotate);
        rotationCentre = -pivotIndex * individualCubeSize * axis;
        targetQuaternion = Quaternion.AngleAxis(rotationDirection * 90, axis);

        // I will now parent all the cubes to be rotated into a new temporary GameObject
        // So they can be rotated together

        // You can do this with pure math
        // but this way makes the code simpler for not much overhead
        // and allows us to animate all the cubes at once with minimal effort
        var rotationalAidGO = new GameObject();
        rotationalAidGO.transform.position = rotationCentre;

        foreach (var cube in cubesToRotate)
        {
            cube.transform.SetParent(rotationalAidGO.transform, true);
        }


        Quaternion startRotation = rotationalAidGO.transform.rotation;
        
        //You can do really cool things here in the pivoting action
        //to animate the rotation, all that matters is that evertything
        //is in the right place for the final action
        return new PivotRotationActions()
        {
            startAction = () =>
            {

            },
            pivotingAction =
            time =>
            {
                var currQ = Quaternion.Slerp(startRotation, startRotation * targetQuaternion, time);

                rotationalAidGO.transform.rotation = currQ;
            },

            finalAction = () =>
            {
                //Update the cube's current coordinates
                foreach (var cube in cubesToRotate)
                {
                    cube.transform.SetParent(this.cubesParent, true);
                    cube.currentCoordinates = WorldCoordinatesToCube(cube.transform.position);
                    cube.transform.localRotation = cube.transform.localRotation.RoundAngle(90);
                }
                GameObject.Destroy(rotationalAidGO);
            }
        };

    }

    private void GetAllCubesAffectedByPivot(CubeRotationAxis pivot, int index, ref List<IndividualCubeController> cubesToReturn)
    {
        if (pivot == CubeRotationAxis.XAxis)
            cubesToReturn.AddRange(this.allCubes.FindAll(item => { return item.currentCoordinates.x == index; }));
        else if (pivot == CubeRotationAxis.YAxis)
            cubesToReturn.AddRange(this.allCubes.FindAll(item => { return item.currentCoordinates.y == index; }));
        else if (pivot == CubeRotationAxis.ZAxis)
            cubesToReturn.AddRange(this.allCubes.FindAll(item => { return item.currentCoordinates.z == index; }));
    }
    #endregion

    #region Coordinate Transformations

    //Note that if we use world coordinates around the hundred million mark the 
    // precision of the float type will lead to cube coordinates that will be off by 1 or 2 units.
    public Vector3Int WorldCoordinatesToCube(float x, float y, float z)
    {
        //Subtract this value from the generated coordinates to make them centred around 0,0,0
        float centreAdjustOffset = (gameSize - 1) * individualCubeSize * .5f;
        return new Vector3Int(
            Mathf.RoundToInt((x + centreAdjustOffset) / individualCubeSize),
            Mathf.RoundToInt((y + centreAdjustOffset) / individualCubeSize),
            Mathf.RoundToInt((z + centreAdjustOffset) / individualCubeSize)
        );
    }

    public Vector3Int WorldCoordinatesToCube(Vector3 worldCoords)
    {
        return WorldCoordinatesToCube(worldCoords.x, worldCoords.y, worldCoords.z);
    }

    private Vector3 CubeCoordinatesToWorld(int x, int y, int z)
    {
        //Subtract this value from the generated coordinates to make them centred around 0,0,0
        float centreAdjustOffset = (gameSize - 1) * individualCubeSize * .5f;
        return new Vector3(
            individualCubeSize * x - centreAdjustOffset,
            individualCubeSize * y - centreAdjustOffset,
            individualCubeSize * z - centreAdjustOffset
        );
    }

    private Vector3 CubeCoordinatesToWorld(Vector3Int cubeCoords)
    {
        return CubeCoordinatesToWorld(cubeCoords.x, cubeCoords.y, cubeCoords.z);
    }
    #endregion

    public bool CheckVictory()
    {
        //Check if one pivots is made up entirely of cubes with the same rotation

        List<IndividualCubeController> cubesInPivot = new List<IndividualCubeController>(gameSize);

        for (int i=0;i<this.gameSize;i++)
        {
            GetAllCubesAffectedByPivot(CubeRotationAxis.XAxis, i, ref cubesInPivot);
            var firstRotation = cubesInPivot[0].transform.localRotation;
            foreach (var cube in cubesInPivot)
            {
                // Check if the rotations match, with some leeway allowed for floating point imprecisions
                float absDot = Mathf.Abs(Quaternion.Dot(firstRotation, cube.transform.rotation));
                if (absDot<.9f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    //This would have been put into a proper unit test system in any real project
    private void Test()
    {
        for (int i = 0; i < 2000; i++)
        {
            int randi = Random.Range(0, 5000);
            int randj = Random.Range(0, 5000);
            int randk = Random.Range(0, 5000);
            Vector3Int cubeCoordinates = new Vector3Int(randi, randj, randk);
            var worldcoords = CubeCoordinatesToWorld(cubeCoordinates);
            var convertedCubeCoordinates = WorldCoordinatesToCube(worldcoords);

            Debug.Assert(
                cubeCoordinates == convertedCubeCoordinates,
                $"Cube coordinate conversion test failed. Input was {cubeCoordinates}, output was {convertedCubeCoordinates}"
            );
        }
    }

    public static MagicCube CreateFromSerializedData(SaveGameSystem.SerializableCubeData data, int individualCubeSizeInWorldUnits, IndividualCubeController individualCubePrefab)
    {
        //Generate cube from data
        var newCube = new MagicCube(data.gameSize, individualCubeSizeInWorldUnits, individualCubePrefab);

        foreach (var cube in data.cubeData)
        {
            var cubego = newCube.CreateSubCube(cube.currentCoordinates.x, cube.currentCoordinates.y, cube.currentCoordinates.z, cube.goalCoordinates);
            cubego.transform.localRotation = cube.rotation;
        }

        return newCube;
    }

    public SaveGameSystem.SerializableCubeData GetSerializableCubeData()
    {
        List<SaveGameSystem.IndividualCubeData> listOfCubeData = new List<SaveGameSystem.IndividualCubeData>();
        //Serialize current cube state
        foreach (var cube in allCubes)
        {
            listOfCubeData.Add(cube.GetSerializableData());
        }

        var cubedata = new SaveGameSystem.SerializableCubeData()
        {
            cubeData = listOfCubeData.ToArray(),
            gameSize = this.gameSize
        };
        return cubedata;
        
    }
}