using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private OrbitCamera orbitingCamera;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private InGameUIController ingameUi;

    //private Transform cubeHighlighter;
    //private Vector3 hitCubeNormal;
    private Vector3 mouseSwipeStart;
    

    int inputLock = 0;

    //this.cubeHighlighter.gameObject.activeInHierarchy == false
    private bool startedSwipeOnCube = false;
    private void Update()
    {
        if (inputLock != 0)
            return;

        if (Input.GetKeyDown(KeyCode.U))
        {
            gameManager.UndoLastRotation();
        }

        orbitingCamera.AddZoomMouseScrollInput(-Input.GetAxis("Mouse ScrollWheel"));


        if (startedSwipeOnCube==true && Input.GetMouseButtonUp(0))
        {
            gameManager.CubeSwipePerformed(this.mouseSwipeStart, Input.mousePosition);
            this.startedSwipeOnCube = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(orbitingCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                startedSwipeOnCube = true;
                //Debug.Log($"Hit a {hit.transform.gameObject}");
                //this.cubeHighlighter.transform.position = hit.transform.position;
                //this.hitCubeNormal = hit.normal;

                //this.cubeHighlighter.gameObject.SetActive(true);
                this.mouseSwipeStart = Input.mousePosition;
                gameManager.CubeSwipeStarted(hit.transform.position,hit.normal);
            }
        }
        if (Input.GetMouseButton(0) && !this.startedSwipeOnCube)
        {

            //this.mouseSwipeStart = Input.mousePosition;

            //Vector2 movement = (Input.mousePosition-this.swipeStart);
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            orbitingCamera.AddMouseInput(mouseDelta);

        }

        if (Input.touchCount == 1)
        {
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

        if (Input.GetKeyUp(KeyCode.R))
        {
            gameManager.RandomRotation();
        }
    }
}
