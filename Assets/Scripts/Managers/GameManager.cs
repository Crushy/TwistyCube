using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using UnityEngine.Events;
using System;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private IndividualCubeController IndividualCubePrefab;
    [SerializeField]
    private Transform HighlightCubePrefab;
    
    //References
    [SerializeField]
    private OrbitCamera orbitingCamera;

    private MagicCube magicCube;

    
    [System.Serializable]
    public class MyStringEvent : UnityEvent<System.TimeSpan>
    {
    }
    
    
    public MyStringEvent GameTimerTick;

    private GameTimer currentGameTimer;


    private Coroutine timerCounting;

    private void OnEnable() {
        magicCube = new MagicCube(6,1,IndividualCubePrefab);
        magicCube.InstantiateCube();
        this.currentGameTimer = new GameTimer(System.TimeSpan.Zero);
        timerCounting = StartCoroutine(TimerTick());
    }

    private void OnDisable() {
        //StartCoroutine(TimerTick());
    }

    private IEnumerator TimerTick() {
        while (true) {
            var gameDuration = this.currentGameTimer.GetCurrentGameDuration();

            GameTimerTick.Invoke(gameDuration);
            yield return new WaitForSecondsRealtime(1);
        }
    }

    int inputLock = 0;

    private Transform cubeHighlighter;

    private void Start() {
        this.cubeHighlighter = GameObject.Instantiate(this.HighlightCubePrefab);
        this.cubeHighlighter.gameObject.SetActive(false);
        Application.targetFrameRate = 60;
        Input.simulateMouseWithTouches = false;
    }


    

    private CubeRotationAxis GetLargestDimensionAsAxis(Vector3 vec)
    {
        Vector3 absDelta = new Vector3(
        Mathf.Abs(vec.x),
        Mathf.Abs(vec.y),
        Mathf.Abs(vec.z)
        );

        if (absDelta.x >= absDelta.y && absDelta.x >= absDelta.z)
        {
            return CubeRotationAxis.XAxis;
        }
        else if (absDelta.y >= absDelta.x && absDelta.y >= absDelta.z)
        {
            return CubeRotationAxis.YAxis;
        }
        else if (absDelta.z >= absDelta.x && absDelta.z >= absDelta.y)
        {
            return CubeRotationAxis.ZAxis;
        }
        Debug.LogError("No best coordinate");
        return CubeRotationAxis.ZAxis;
    }

    

    private CubeRotationAxis VectorToRotationAxis(Vector3 vec)
    {
        if (Mathf.Abs(Vector3.Dot(vec, Vector3.right)) > .95f)
        {
            return CubeRotationAxis.XAxis;
        }
        else if (Mathf.Abs(Vector3.Dot(vec, Vector3.up)) > .95f)
        {
            return CubeRotationAxis.XAxis;
        }
        else if (Mathf.Abs(Vector3.Dot(vec, Vector3.forward)) > .95f)
        {
            return CubeRotationAxis.ZAxis;
        }
        else
        {
            Debug.LogError("Not close enough to any axis");
            return CubeRotationAxis.XAxis;
        }
    }

    
    public void CubeSwipeStarted(Vector3 cubePos, Vector3 hitNormal)
    {
        this.cubeHighlighter.gameObject.SetActive(true);
        this.cubeHighlighter.position = cubePos;
    }

    public void CubeSwipePerformed(Vector3 hitStart, Vector3 hitEnd)
    {
        //Alternatively this could have just been passed from the input manager but this way we can decouple the logic further
        Physics.Raycast(orbitingCamera.GetComponent<Camera>().ScreenPointToRay(hitStart), out RaycastHit firstHitRaycast);
        
        Vector3 firstHit = firstHitRaycast.transform.position;
        Vector3 firstHitNormal = firstHitRaycast.normal;

        var projectionPlane = new Plane(firstHitNormal, firstHit);
        var ray = orbitingCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        projectionPlane.Raycast(ray, out float enter);
        var collisionPoint = ray.GetPoint(enter);


        Vector3Int cubePos = this.magicCube.WorldCoordinatesToCube(firstHit);
        Vector3Int otherCubePos = this.magicCube.WorldCoordinatesToCube(collisionPoint);

        Vector3 cubeDelta = otherCubePos - cubePos;

        //The following section is ugly and could likely have been better fixed with creative usage of the cross
        //product but unfortunately I was running out of time and some of the edge cases were really weird

        //Check what the closest world direction to our normal is, use the cube coordinate system offsets to guess where the
        //player likely wants to rotate to, by figuring out the largest coordinate in that system.
        //Rotations are flipped if we're on the opposite end of the world vector

        //In theory the switch could be removed by taking the cross product of the swipe direction
        // in cube coordinate space and the cube surface normal.

        //Top
        if (Vector3.Dot(firstHitNormal, Vector3.up) is float dot1 && Mathf.Abs(dot1) > .9f)
        {
            //Debug.Log($"Top");
            switch (GetLargestDimensionAsAxis(cubeDelta))
            {
                case CubeRotationAxis.XAxis:
                    PerformRotation(CubeRotationAxis.ZAxis, cubePos.z, Mathf.Sign(dot1) * cubeDelta.x < 0);
                    break;
                case CubeRotationAxis.ZAxis:
                    PerformRotation(CubeRotationAxis.XAxis, cubePos.x, Mathf.Sign(dot1) * cubeDelta.z > 0);
                    break;
            }
        }
        //Front
        else if (Vector3.Dot(firstHitNormal, Vector3.forward) is float dot2 && Mathf.Abs(dot2) > .9f)
        {
            //Debug.Log($"Front");
            //Debug.Log("It would be great if this was Y " + Vector3.Cross(new Vector3(1, 0, 0), Vector3.forward));
            switch (GetLargestDimensionAsAxis(cubeDelta))
            {
                case CubeRotationAxis.XAxis:

                    PerformRotation(CubeRotationAxis.YAxis, cubePos.y, Mathf.Sign(dot2) * cubeDelta.x > 0);
                    break;
                case CubeRotationAxis.YAxis:
                    PerformRotation(CubeRotationAxis.XAxis, cubePos.x, Mathf.Sign(dot2) * cubeDelta.y < 0);
                    break;
            }
        }
        //Left
        else if (Vector3.Dot(firstHitNormal, Vector3.right) is float dot3 && Mathf.Abs(dot3) > .9f)
        {
            //Debug.Log($"Right");
            switch (GetLargestDimensionAsAxis(cubeDelta))
            {
                case CubeRotationAxis.YAxis:
                    PerformRotation(CubeRotationAxis.ZAxis, cubePos.z, Mathf.Sign(dot3) * cubeDelta.y > 0);
                    break;
                case CubeRotationAxis.ZAxis:
                    PerformRotation(CubeRotationAxis.YAxis, cubePos.y, Mathf.Sign(dot3) * cubeDelta.z < 0);
                    break;
            }
        }

        this.cubeHighlighter.gameObject.SetActive(false);
    }

    internal void RandomRotation()
    {
        int pivotIndex = UnityEngine.Random.Range(0, (int)magicCube.CubeSize);

        CubeRotationAxis[] rotationAxis = (CubeRotationAxis[])System.Enum.GetValues(typeof(CubeRotationAxis));
        var chosenAxis = rotationAxis.ChooseRandomly();
        bool rotationDirection = UnityEngine.Random.value < .5f;

        PerformRotation(chosenAxis, pivotIndex, rotationDirection);
    }

    public void PerformRotation(CubeRotationAxis axis, int pivotIndex, bool direction)
    {
        PerformRotation(new RubikCubeRotation() { RotationAxis = axis, Direction = direction, PivotIndex = pivotIndex });
    }

    public Stack<RubikCubeRotation> perfFormedRotations = new Stack<RubikCubeRotation>();

    public void UndoLastRotation()
    {
        if (this.perfFormedRotations.Count > 0)
        {
            var lastRot = this.perfFormedRotations.Pop();
            lastRot.Direction = !lastRot.Direction;
            PerformRotation(lastRot, false);
        }
    }

    /// <summary>
    /// Executes a rotation animation
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="pivotIndex"></param>
    /// <param name="direction"></param>
    /// <remarks></remarks>
    public void PerformRotation(RubikCubeRotation rotationToPerform, bool addToUndo = true)
    {
        CubeRotationAxis axis = rotationToPerform.RotationAxis;
        int pivotIndex = rotationToPerform.PivotIndex;
        bool direction = rotationToPerform.Direction;

        if (addToUndo)
        {
            perfFormedRotations.Push(rotationToPerform);
        }

        var rotationData =
            this.magicCube.RotatePivot(
                axis,
                pivotIndex,
                direction
            );



        inputLock++;
        StartCoroutine(InterpolationUtils.InterpolateAction
        (
            rotationData.pivotingAction,
            .25f,
            (timeFactor) =>
            {
                rotationData.finalAction();
                

                if (magicCube.CheckVictory())
                {
                    Debug.Log("You won");
                }
                else
                {
                    //inputLock--;
                }

                inputLock--;
            }
        ));
    }

    private void OnDestroy()
    {
        //Serialize current cube state

    }

    //Timers are on another thread so unity can't properly stop them unless we explicitly say so
    private void OnApplicationQuit() {
        
    }
    
}
