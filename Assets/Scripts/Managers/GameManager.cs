using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using UnityEngine.Events;
using System;

public class GameManager : MonoBehaviour
{
    #pragma warning disable 649
    [Header("Prefabs")]
    [SerializeField]
    private IndividualCubeController IndividualCubePrefab;
    [SerializeField]
    private Transform HighlightCubePrefab;

    [Header("Scene references")]
    [SerializeField] private OrbitCamera orbitingCamera;

    [SerializeField] public InGameUIController ingameUI;
    [SerializeField] public InputManager inputManager;

    [Header("Settings")]
    [Tooltip("How many random rotations are applied when starting a new game")]
    [Range(0,10)]
    [SerializeField] private int numberOfRandomShuffles = 3;

    private MagicCube magicCube;
    private Transform cubeHighlighter;

    #pragma warning restore 649

    //Custom event used by the in-game timer
    [System.Serializable]
    public class MyStringEvent : UnityEvent<System.TimeSpan>
    {
    }
    
    public MyStringEvent GameTimerTick;

    private GameTimer currentGameTimer;

    private Coroutine RefreshUITimeCounter;

    #region Undo
    public Stack<RubikCubeRotation> accumulatedUserRotations = new Stack<RubikCubeRotation>();

    public void UndoLastRotation()
    {
        if (this.accumulatedUserRotations.Count > 0)
        {
            var lastRot = this.accumulatedUserRotations.Pop();
            lastRot.Direction = !lastRot.Direction;
            PerformRotation(lastRot, false);
        }
    }
    #endregion

    private IEnumerator TimerTick() {
        while (true) {
            var gameDuration = this.currentGameTimer.GetCurrentGameDuration();

            GameTimerTick.Invoke(gameDuration);
            yield return new WaitForSecondsRealtime(1);
        }
    }

    private void Start() {
        Application.targetFrameRate = 60;
        Input.simulateMouseWithTouches = false;

        this.ingameUI.FadeIn();

        this.cubeHighlighter = GameObject.Instantiate(this.HighlightCubePrefab);
        this.cubeHighlighter.gameObject.SetActive(false);

        //Hide the undo button until we start adding rotations.
        //Delete this if we end up serializing performed rotations at some point
        ingameUI.HideUndoButton(); 

        System.TimeSpan accumulatedTime = System.TimeSpan.Zero;

        switch (PerSessionData.newGameMode)
        {
            case PerSessionData.GameModes.Resume:
                Debug.Log("Loading cube from playerprefs");
                SaveGameSystem.LoadGameState(
                    out SaveGameSystem.SerializableCubeData cubeData,
                    out accumulatedTime
                );
                this.magicCube = MagicCube.CreateFromSerializedData(cubeData,1,this.IndividualCubePrefab);
                this.inputManager.allowRotations = true;
                break;
            case PerSessionData.GameModes.NewGame:
                Debug.Log("Creating a new game");
                this.magicCube = MagicCube.CreateFromNewGame(PerSessionData.CubeSize,1, this.IndividualCubePrefab);
                StartCoroutine(this.ScrambleCube());
                break;
        }

        this.currentGameTimer = new GameTimer(accumulatedTime);
        RefreshUITimeCounter = StartCoroutine(TimerTick());
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

    #region Rotation input
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
    #endregion

    #region Cube Scrambling
    private RubikCubeRotation GetRandomRotation()
    {
        int pivotIndex = UnityEngine.Random.Range(0, (int)magicCube.CubeSize);

        CubeRotationAxis[] rotationAxis = (CubeRotationAxis[])System.Enum.GetValues(typeof(CubeRotationAxis));
        var chosenAxis = rotationAxis.ChooseRandomly();
        bool rotationDirection = UnityEngine.Random.value < .5f;

        return new RubikCubeRotation() 
        {
            RotationAxis=chosenAxis,
            PivotIndex = pivotIndex,
            Direction = rotationDirection
        };
    }

    private IEnumerator ScrambleCube()
    {
        this.inputManager.allowRotations = false;

        for (int i = 0; i < this.numberOfRandomShuffles; i++)
        {
            var rotation = GetRandomRotation();

            var rotationData =
                this.magicCube.RotatePivot(
                    rotation.RotationAxis,
                    rotation.PivotIndex,
                    rotation.Direction
            );

            yield return StartCoroutine(InterpolationUtils.InterpolateAction
            (
                rotationData.pivotingAction,
                .25f,
                (timeFactor) =>
                {
                    rotationData.finalAction();
                }
            ));
            this.SaveGameState();
            //PerformRotation(rotation, addToUndo: false, duration: .1f);
        }

        this.inputManager.allowRotations = true;
    }
    #endregion
    public void PerformRotation(CubeRotationAxis axis, int pivotIndex, bool direction, bool addToUndo=true, float duration = .25f)
    {
        PerformRotation(new RubikCubeRotation() { RotationAxis = axis, Direction = direction, PivotIndex = pivotIndex }, addToUndo, duration);
    }

    /// <summary>
    /// Executes a rotation animation
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="pivotIndex"></param>
    /// <param name="direction"></param>
    /// <remarks></remarks>
    public void PerformRotation(RubikCubeRotation rotationToPerform, bool addToUndo = true, float duration=.25f)
    {
        CubeRotationAxis axis = rotationToPerform.RotationAxis;
        int pivotIndex = rotationToPerform.PivotIndex;
        bool direction = rotationToPerform.Direction;

        if (addToUndo)
        {
            accumulatedUserRotations.Push(rotationToPerform);
        }

        var rotationData =
            this.magicCube.RotatePivot(
                axis,
                pivotIndex,
                direction
        );

        inputManager.AquireInputLock();
        StartCoroutine(InterpolationUtils.InterpolateAction
        (
            rotationData.pivotingAction,
            .25f,
            (timeFactor) =>
            {
                rotationData.finalAction();
                

                if (magicCube.CheckVictory())
                {
                    GameWon();
                }

                this.SaveGameState();
                inputManager.ReleaseInputLock();
            }
        ));

        if (accumulatedUserRotations.Count > 0)
        {
            ingameUI.ShowUndoButton();
        }
        else
        {
            ingameUI.HideUndoButton();
        }
    }

    private void SaveGameState()
    {
        SaveGameSystem.SaveGameState(this.magicCube.GetSerializableCubeData(),this.currentGameTimer.GetCurrentGameDuration());
    }

    private void GameWon()
    {
        this.inputManager.allowRotations = false;
        this.inputManager.AquireInputLock();

        
        Debug.Log("You won");

        //Stop the timer
        StopCoroutine(RefreshUITimeCounter);
        this.currentGameTimer.StopTimer();

        ingameUI.HideUndoButton();

        ingameUI.FadeIn();

        //Do a few loops around the cube and stop at horizontal 45 degree angle
        float startXAngle = 0;
        float endXAngle = 360f * 2.125f;
        float startYAngle = 0;
        float endYAngle = -45;
        StartCoroutine(
            InterpolationUtils.InterpolateAction(
                (t)=>
                {
                    //Apply an easing function to t so the animation looks smoother
                    float easedT = Mathf.SmoothStep(0, 1, t);
                    float distance = Mathf.Lerp(orbitingCamera.MinZoom,orbitingCamera.MaxZoom, easedT);
                    float xangle = Mathf.Lerp(startXAngle, endXAngle, easedT);
                    float yangle = Mathf.Lerp(startYAngle, endYAngle, easedT);
                    orbitingCamera.SetCameraPosition(xangle, yangle, distance);
                },
                4,
                (t)=>
                {
                    //Finally, release the input lock and display the win dialog
                    inputManager.ReleaseInputLock();

                    ingameUI.ShowWinScreen(this.currentGameTimer.GetCurrentGameDuration());
                    this.inputManager.ReleaseInputLock();
                }
            )
        );
    }
    
}
