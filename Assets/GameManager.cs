using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using UnityEngine.Events;
using static GameManager.MagicCube;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private IndividualCubeController IndividualCubePrefab;
    [SerializeField]
    private Transform HighlightCubePrefab;
    
    //References
    [SerializeField]
    private OrbitCamera orbitingCamera;


    public class MagicCube {
        private int gameSize;
        public int CubeSize {
            get {
                return gameSize;
            }
        }

        private float individualCubeSize;

        private IndividualCubeController individualCubePrefab;

        private Transform cubesParent;

        private List<IndividualCubeController> allCubes; 

        public MagicCube(int gameSize, float individualCubeSizeInWorldUnits, IndividualCubeController individualCubePrefab) {
            this.gameSize = gameSize;
            this.cubesParent = new GameObject().transform;
            this.individualCubePrefab = individualCubePrefab;
            
            //this.individualCubeSize = individualCubePrefab.GetComponentInChildren<Renderer>().bounds.extents.x*2;
            this.individualCubeSize = individualCubeSizeInWorldUnits;
        }

        private void CreateSubCube(int x,int y,int z)
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
        public void InstantiateCube() {
            this.allCubes = new List<IndividualCubeController> (Mathf.RoundToInt(Mathf.Pow(gameSize,3)));

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
                    CreateSubCube(i, gameSize-1, k);

            //Z plates
            for (int i = 1; i < gameSize - 1; i++)
                for (int j = 1; j < gameSize - 1; j++)
                    CreateSubCube(i, j, 0);

            for (int i = 1; i < gameSize-1; i++)
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

        public struct PivotRotation
        {
            public System.Action startAction;
            public InterpolationUtils.InterpolatedMethod pivotingAction;
            public System.Action finalAction;
        }
        
        public PivotRotation RotatePivot(CubeRotationAxis chosenAxis, int pivotIndex, bool directionUpOrLeft) {
            List<IndividualCubeController> cubesToRotate = new List<IndividualCubeController>(gameSize*gameSize); 
            Vector3 rotationCentre;
            Quaternion targetQuaternion;
            float rotationDirection = directionUpOrLeft?1f:-1f;

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

            foreach (var cube in cubesToRotate) {
                cube.transform.SetParent(rotationalAidGO.transform,true);
            }
            

            Quaternion startRotation = rotationalAidGO.transform.rotation;
            //Vector3 startPosition = cube.transform.position;
            return new PivotRotation()
            {
                startAction = ()=>
                {
                    
                },
                pivotingAction =
                time =>
                {
                    var currQ = Quaternion.Slerp(startRotation, startRotation * targetQuaternion, time);

                    rotationalAidGO.transform.rotation = currQ;
                },

                finalAction = ()=>
                {
                    //Debug.Log("Done");
                    //Update the cube's current coordinates

                    foreach (var cube in cubesToRotate)
                    {
                        cube.transform.SetParent(this.cubesParent,true);
                        cube.currentCoordinates = WorldCoordinatesToCube(cube.transform.position);
                    }
                    Destroy(rotationalAidGO);
                }
            };
            
        }

        private void GetAllCubesAffectedByPivot(CubeRotationAxis pivot, int index, ref List<IndividualCubeController> cubesToReturn) {
            if (pivot == CubeRotationAxis.XAxis)
                cubesToReturn.AddRange(this.allCubes.FindAll(item=>{ return item.currentCoordinates.x==index; }));
            else if(pivot == CubeRotationAxis.YAxis)
                cubesToReturn.AddRange(this.allCubes.FindAll(item => { return item.currentCoordinates.y == index; }));
            else if (pivot == CubeRotationAxis.ZAxis)
                cubesToReturn.AddRange(this.allCubes.FindAll(item => { return item.currentCoordinates.z == index; }));
        }
        #endregion

        #region Coordinate Transformations

        //Note that if we use world coordinates around the hundred million mark the 
        // precision of the float type will lead to cube coordinates that will be off by 1 or 2 units.
        public Vector3Int WorldCoordinatesToCube(float x, float y, float z) {
            //Subtract this value from the generated coordinates to make them centred around 0,0,0
            float centreAdjustOffset = (gameSize-1)*individualCubeSize*.5f;
            return new Vector3Int(
                Mathf.RoundToInt((x+centreAdjustOffset)/individualCubeSize),
                Mathf.RoundToInt((y+centreAdjustOffset)/individualCubeSize),
                Mathf.RoundToInt((z+centreAdjustOffset)/individualCubeSize)
            );
        }

        public Vector3Int WorldCoordinatesToCube(Vector3 worldCoords) {
            return WorldCoordinatesToCube(worldCoords.x,worldCoords.y,worldCoords.z);
        }

        private Vector3 CubeCoordinatesToWorld(int x, int y, int z) {
            //Subtract this value from the generated coordinates to make them centred around 0,0,0
            float centreAdjustOffset = (gameSize-1)*individualCubeSize*.5f;
            return new Vector3(
                individualCubeSize*x-centreAdjustOffset,
                individualCubeSize*y-centreAdjustOffset,
                individualCubeSize*z-centreAdjustOffset
            );
        }

        private Vector3 CubeCoordinatesToWorld(Vector3Int cubeCoords) {
            return CubeCoordinatesToWorld(cubeCoords.x,cubeCoords.y,cubeCoords.z);
        }
        #endregion

        public bool CheckVictory() {
            return allCubes.TrueForAll(x=>{return x.goalCoordinates==x.currentCoordinates;});
        }

        //This would have been put into a proper unit test system in any real project
        private void Test() {
            for (int i=0; i<2000; i++) {

                int randi = Random.Range(0,5000);
                int randj = Random.Range(0,5000);
                int randk = Random.Range(0,5000);
                Vector3Int cubeCoordinates = new Vector3Int(randi,randj,randk);
                var worldcoords = CubeCoordinatesToWorld(cubeCoordinates);
                var convertedCubeCoordinates = WorldCoordinatesToCube(worldcoords);

                Debug.Assert(
                    cubeCoordinates==convertedCubeCoordinates,
                    $"Cube coordinate conversion test failed. Input was {cubeCoordinates}, output was {convertedCubeCoordinates}"
                );
            }
        }
    }

    private MagicCube magicCube;

    
    [System.Serializable]
    public class MyStringEvent : UnityEvent<System.TimeSpan>
    {
    }
    
    public class GameTimer {
        private System.DateTime timerStart;

        //Time from serialized runs
        private System.TimeSpan accumulatedTime = System.TimeSpan.Zero;

        public GameTimer(System.TimeSpan accumulatedTime) {
            this.accumulatedTime = accumulatedTime;
            timerStart = System.DateTime.UtcNow;
        }

        public System.TimeSpan GetCurrentGameDuration() {
            return this.accumulatedTime + (System.DateTime.UtcNow - timerStart);
        }
    }
    public MyStringEvent GameTimerTick;

    private GameTimer currentGameTimer;

    
    private void OnEnable() {
        magicCube = new MagicCube(6,1,IndividualCubePrefab);
        magicCube.InstantiateCube();
        this.currentGameTimer = new GameTimer(System.TimeSpan.Zero);
        StartCoroutine(TimerTick());
    }

    private void OnDisable() {
        //StartCoroutine(TimerTick());
    }

    private IEnumerator TimerTick() {
        while (true) {
            var gameDuration = this.currentGameTimer.GetCurrentGameDuration();
            //Debug.Log("Tick "+gameDuration.ToShortForm());

            GameTimerTick.Invoke(gameDuration);
            yield return new WaitForSecondsRealtime(1);
        }
    }

    int inputLock = 0;

    private Transform cubeHighlighter;

    private void Start() {
        this.cubeHighlighter = GameObject.Instantiate(this.HighlightCubePrefab);
        Application.targetFrameRate = 60;
        Input.simulateMouseWithTouches = false;
    }


    private Vector3 mouseSwipeStart;
    private Vector3 hitCubeNormal;

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

    private Vector3 GetLargestDimension(Vector3 vec)
    {
        Vector3 absDelta = new Vector3(
        Mathf.Abs(vec.x),
        Mathf.Abs(vec.y),
        Mathf.Abs(vec.z)
        );

        if (absDelta.x >= absDelta.y && absDelta.x >= absDelta.z)
        {
            return new Vector3(1, 0, 0);
        }
        else if (absDelta.y >= absDelta.x && absDelta.y >= absDelta.z)
        {
            return new Vector3(0, 1, 0);
        }
        else if (absDelta.z >= absDelta.x && absDelta.z >= absDelta.y)
        {
            return new Vector3(0, 0, 1);
        }
        Debug.LogError("No best coordinate");
        return new Vector3(0, 0, 0);
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

    private void Update() {
        if (inputLock!=0)
            return;

        RaycastHit hit;

        orbitingCamera.AddZoomMouseScrollInput(-Input.GetAxis("Mouse ScrollWheel"));


        if (this.cubeHighlighter.gameObject.activeInHierarchy && Input.GetMouseButtonUp(0)) {
            var projectionPlane = new Plane(this.hitCubeNormal, this.cubeHighlighter.transform.position);
            var ray = orbitingCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            projectionPlane.Raycast(ray, out float enter);
            var collisionPoint = ray.GetPoint(enter);


            Vector3Int cubePos = this.magicCube.WorldCoordinatesToCube(this.cubeHighlighter.transform.position);
            Vector3Int otherCubePos = this.magicCube.WorldCoordinatesToCube(collisionPoint);

            Vector3 cubeDelta = otherCubePos - cubePos;
            
            //The following section is ugly and could likely have been better fixed with creative usage of the cross
            //product but unfortunately I was running out of time and some of the edge cases were really weird

            //Top
            if (Vector3.Dot(this.hitCubeNormal, Vector3.up) is float dot1 && Mathf.Abs(dot1) > .9f)
            {
                Debug.Log($"Top");

                switch (GetLargestDimensionAsAxis(cubeDelta))
                {
                    case CubeRotationAxis.XAxis:
                        PerformRotation(CubeRotationAxis.ZAxis, cubePos.z, Mathf.Sign(dot1)*cubeDelta.x < 0);
                        break;
                    case CubeRotationAxis.ZAxis:
                        PerformRotation(CubeRotationAxis.XAxis, cubePos.x, Mathf.Sign(dot1)*cubeDelta.z > 0);
                        break;
                }
            }
            //Front
            else if(Vector3.Dot(this.hitCubeNormal, Vector3.forward) is float dot2 && Mathf.Abs(dot2) > .9f)
            {
                Debug.Log($"Front");

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
            else if (Vector3.Dot(this.hitCubeNormal, Vector3.right) is float dot3 && Mathf.Abs(dot3) > .9f)
            {
                Debug.Log($"Right");

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

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(orbitingCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit))
            {

                //Debug.Log($"Hit a {hit.transform.gameObject}");
                this.cubeHighlighter.transform.position = hit.transform.position;
                this.hitCubeNormal = hit.normal;
                
                this.cubeHighlighter.gameObject.SetActive(true);
                this.mouseSwipeStart = Input.mousePosition;
            }
        }
        if (Input.GetMouseButton(0) && this.cubeHighlighter.gameObject.activeInHierarchy==false)
        {
            
            //this.mouseSwipeStart = Input.mousePosition;
                
            //Vector2 movement = (Input.mousePosition-this.swipeStart);
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y")); 
            orbitingCamera.AddMouseInput(mouseDelta);
            
        }

        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                Debug.Log(touch.deltaPosition);
                orbitingCamera.AddTouchInput(touch.deltaPosition);
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
            orbitingCamera.AddZoomInputPinch(deltaMagnitudeDiff);
        }




        if (Input.GetKeyUp(KeyCode.R)) {
            int pivotIndex = Random.Range(0, (int)magicCube.CubeSize);

            CubeRotationAxis[] rotationAxis = (CubeRotationAxis[])System.Enum.GetValues(typeof(CubeRotationAxis));
            var chosenAxis = rotationAxis.ChooseRandomly();
            bool rotationDirection = Random.value < .5f;



            PerformRotation(chosenAxis, pivotIndex, rotationDirection);
        }
    }

    public void PerformRotation(CubeRotation rotationToPerform)
    {
        PerformRotation(
            rotationToPerform.RotationAxis,
            rotationToPerform.pivotIndex,
            rotationToPerform.direction
        );
    }

    /// <summary>
    /// Executes a rotation animation
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="pivotIndex"></param>
    /// <param name="direction"></param>
    /// <remarks></remarks>
    public void PerformRotation(CubeRotationAxis axis, int pivotIndex, bool direction)
    {
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

    //Timers are on another thread so unity can't properly stop them unless we explicitly say so
    private void OnApplicationQuit() {
        
    }
    
}
