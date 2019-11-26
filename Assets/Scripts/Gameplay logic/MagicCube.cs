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
        get
        {
            return gameSize;
        }
    }

    private float individualCubeSize;

    private IndividualCubeController individualCubePrefab;

    private Transform cubesParent;

    private List<IndividualCubeController> allCubes;

    public MagicCube(int gameSize, float individualCubeSizeInWorldUnits, IndividualCubeController individualCubePrefab)
    {
        this.gameSize = gameSize;
        this.cubesParent = new GameObject().transform;
        this.individualCubePrefab = individualCubePrefab;

        //this.individualCubeSize = individualCubePrefab.GetComponentInChildren<Renderer>().bounds.extents.x*2;
        this.individualCubeSize = individualCubeSizeInWorldUnits;
    }

    private void CreateSubCube(int x, int y, int z)
    {
        var newCube = GameObject.Instantiate(this.individualCubePrefab, cubesParent);

        newCube.transform.position =
            CubeCoordinatesToWorld(x, y, z);

        //Cube was placed, now let's do housekeeping
        newCube.goalCoordinates = new Vector3Int(x, y, z);
        newCube.currentCoordinates = newCube.goalCoordinates;
        allCubes.Add(newCube);
    }

    //Creates the entire Rubik cube out of smaller cubes that make up a "shell"
    public void InstantiateCube()
    {
        this.allCubes = new List<IndividualCubeController>(Mathf.RoundToInt(Mathf.Pow(gameSize, 3)));

        //Possibly this could be refactored into a helper method that creates planes of cubes of a certain side

        //X plates
        for (int j = 0; j < gameSize; j++)
            for (int k = 0; k < gameSize; k++)
                CreateSubCube(0, j, k);

        for (int j = 0; j < gameSize; j++)
            for (int k = 0; k < gameSize; k++)
                CreateSubCube(gameSize - 1, j, k);

        //y plates
        for (int i = 0; i < gameSize; i++)
            for (int k = 0; k < gameSize; k++)
                CreateSubCube(i, 0, k);

        for (int i = 0; i < gameSize; i++)
            for (int k = 0; k < gameSize; k++)
                CreateSubCube(i, gameSize - 1, k);

        //Z plates
        for (int i = 1; i < gameSize - 1; i++)
            for (int j = 1; j < gameSize - 1; j++)
                CreateSubCube(i, j, 0);

        for (int i = 1; i < gameSize - 1; i++)
            for (int j = 1; j < gameSize - 1; j++)
                CreateSubCube(i, j, gameSize - 1);

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
        // So they can be rotate together
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
        //Vector3 startPosition = cube.transform.position;
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
                //Debug.Log("Done");
                //Update the cube's current coordinates

                foreach (var cube in cubesToRotate)
                {
                    cube.transform.SetParent(this.cubesParent, true);
                    cube.currentCoordinates = WorldCoordinatesToCube(cube.transform.position);
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
        return allCubes.TrueForAll(x => { return x.goalCoordinates == x.currentCoordinates; });
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
}