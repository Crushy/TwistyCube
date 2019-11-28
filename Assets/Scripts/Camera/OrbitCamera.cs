using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{   

    //Serializable struct to support platform-dependent settings and so it looks nicer in the editor
    [System.Serializable]
    private struct CameraSettings {
        public float xSpeed;
        public float ySpeed;

        public float zoomSpeed;

        public float minZoom;
        public float maxZoom;
    }

    public float MinZoom
    {
        get
        {
            return cameraSettings.minZoom;
        }
    }

    public float MaxZoom
    {
        get
        {
            return cameraSettings.maxZoom;
        }
    }

    [SerializeField]
    private CameraSettings cameraSettings = new CameraSettings() {xSpeed=10, ySpeed=10, zoomSpeed=5, minZoom=9, maxZoom=20};

    [SerializeField]
    private Vector3 cameraCentre = Vector3.zero;



    private new Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        this.camera = this.GetComponent<Camera>();
        UpdateCameraPosition();
    }

    //Our target rotation in euler degrees
    private Vector3 targetRotation;

    private float targetDistance = 10f;

    public void SetCameraPosition(float xAngle, float yAngle, float distance)
    {
        this.targetRotation.x = xAngle;
        this.targetRotation.y = yAngle;
        this.targetDistance = distance;
    }

    public void AddTouchInput(Vector2 input) {
        targetRotation.x += input.x * this.cameraSettings.xSpeed * .05f;
        targetRotation.y += input.y * this.cameraSettings.ySpeed * .025f;
    }

    public void AddMouseInput(Vector2 input) {
        targetRotation.x += input.x * this.cameraSettings.xSpeed;
        targetRotation.y += input.y * this.cameraSettings.ySpeed;
    }

    public void AddZoomMouseScrollInput(float zoomInput) {
        targetDistance += zoomInput * this.cameraSettings.zoomSpeed;
    }

    public void AddZoomInputPinch(float zoomInput) {
        targetDistance += zoomInput * this.cameraSettings.zoomSpeed * .01f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Adjust limits
        this.targetDistance = Mathf.Clamp( this.targetDistance, this.cameraSettings.minZoom, this.cameraSettings.maxZoom );

        //Free rotation is not allowed to avoid disorientation but that can easily be changed in the future
        targetRotation.y = Mathf.Clamp(targetRotation.y,-89,89);
        targetRotation.x = targetRotation.x % 360.0f;

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition() {
        var targetQuaternion = Quaternion.Euler(-targetRotation.y,targetRotation.x,0);
        var targetDistanceVector = new Vector3(0.0f, 0.0f, -this.targetDistance) + cameraCentre;

        var slerp = Quaternion.SlerpUnclamped(this.transform.rotation, targetQuaternion, Time.deltaTime*4.0f);
        Vector3 position = slerp * targetDistanceVector;

        transform.rotation = targetQuaternion;
        transform.position = targetQuaternion* targetDistanceVector;
    }

    
}
