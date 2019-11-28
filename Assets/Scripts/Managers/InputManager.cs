using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    #pragma warning disable 649
    [SerializeField] private OrbitCamera orbitingCamera;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private InGameUIController ingameUi;

    private Vector3 mouseSwipeStart;
    #pragma warning restore 649

    public bool allowRotations = false;
    
    //This would benefit from being a managed resourse (compatible with a using "statement")
    private int inputLock;
    public void AquireInputLock()
    {
        inputLock++;
    }

    public void ReleaseInputLock()
    {
        inputLock--;
    }

    //this.cubeHighlighter.gameObject.activeInHierarchy == false
    private bool startedSwipeOnCube = false;
    private void Update()
    {
        if (inputLock > 0)
            return;

        //if (Input.GetKeyDown(KeyCode.W))
        //{
        //    gameManager.GameWon();
        //}

        //if (Input.GetKeyUp(KeyCode.R))
        //{
        //    gameManager.RandomRotation();
        //}

        //if (Input.GetKeyDown(KeyCode.U))
        //{
        //    gameManager.UndoLastRotation();
        //}
        
        #if !UNITY_ANDROID
        //If no touches were detected there's a good chance we want to check what the mouse is doing
        //I could make some platform checking but these days pretty much anything has touch and/or mouse support
        if (Input.touchCount == 0) {
            //Mouse zoom is a separate thing
            orbitingCamera.AddZoomMouseScrollInput(-Input.GetAxis("Mouse ScrollWheel"));

            if (allowRotations)
            {
                if (startedSwipeOnCube == true && Input.GetMouseButtonUp(0))
                {
                    gameManager.CubeSwipePerformed(this.mouseSwipeStart, Input.mousePosition);
                    this.startedSwipeOnCube = false;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(orbitingCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                    {
                        startedSwipeOnCube = true;
                        this.mouseSwipeStart = Input.mousePosition;
                        gameManager.CubeSwipeStarted(hit.transform.position, hit.normal);
                    }
                }
                if (Input.GetMouseButton(0) && !this.startedSwipeOnCube)
                {
                    Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                    orbitingCamera.AddMouseInput(mouseDelta);
                }
            }
        }

        #endif
        // Handle touch input
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began && touch.phase == TouchPhase.Began)
            {
                if (allowRotations)
                {
                    if (Physics.Raycast(orbitingCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                    {
                        startedSwipeOnCube = true;
                        this.mouseSwipeStart = Input.mousePosition;
                        gameManager.CubeSwipeStarted(hit.transform.position, hit.normal);
                    }
                }
            }
            else if (!this.startedSwipeOnCube && touch.phase == TouchPhase.Moved)
            {
                Debug.Log(touch.deltaPosition);
                orbitingCamera.AddTouchInput(touch.deltaPosition);
            }
            else if (this.startedSwipeOnCube && touch.phase == TouchPhase.Ended)
            {
                gameManager.CubeSwipePerformed(this.mouseSwipeStart, Input.mousePosition);
                this.startedSwipeOnCube = false;
            }
        }
        //Pinch to zoom
        else if (Input.touchCount == 2)
        {
            
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.phase != TouchPhase.Moved && touchOne.phase != TouchPhase.Moved)
                return;

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
    }

    public void DisplayWinScreen(System.TimeSpan timeTaken)
    {
        this.ingameUi.ShowWinScreen(timeTaken);
        this.inputLock++;
    }

    #region Ui Input
    public void UI_Undo()
    {
        gameManager.UndoLastRotation();
    }

    public void UI_ShowPauseMenu()
    {
        this.ingameUi.ShowPauseMenu();
        this.inputLock++;
    }

    public void UI_ClosePauseMenu()
    {
        this.ingameUi.ClosePauseMenu();
        this.inputLock--;
    }

    public void UI_CloseWinDialog()
    {
        this.ingameUi.CloseWinScreen();
        this.inputLock--;
    }

    public void UI_BackToTitle()
    {
        SceneManager.LoadScene("Title");
    }

    public void UI_RestartGame()
    {
        PerSessionData.newGameMode = PerSessionData.GameModes.NewGame;
        SceneManager.LoadScene("Game");
    }
    #endregion
}
