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

        public float minZoom,maxZoom;
    }

    [SerializeField]
    private CameraSettings cameraSettings = new CameraSettings() {xSpeed=1, ySpeed=1, zoomSpeed=1};

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

    private float distance = 10f;

    public void AddTouchInput(Vector2 input) {
        targetRotation.x += input.x * this.cameraSettings.xSpeed * .05f;
        targetRotation.y += input.y * this.cameraSettings.ySpeed * .01f;
    }

    public void AddMouseInput(Vector2 input) {
        targetRotation.x += input.x * this.cameraSettings.xSpeed;
        targetRotation.y += input.y * this.cameraSettings.ySpeed;
    }

    public void AddZoomMouseScrollInput(float zoomInput) {
        distance += zoomInput * this.cameraSettings.zoomSpeed;
    }

    public void AddZoomInputPinch(float zoomInput) {
        distance += zoomInput * this.cameraSettings.zoomSpeed * .01f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Adjust limits
        this.distance = Mathf.Clamp( this.distance, this.cameraSettings.minZoom, this.cameraSettings.maxZoom );

        //Free rotation is not allowed to avoid disorientation but that can easily be changed in the future
        targetRotation.y = Mathf.Clamp(targetRotation.y,-89,89);
        targetRotation.x = targetRotation.x % 360.0f;

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition() {
        var targetQuaternion = Quaternion.Euler(-targetRotation.y,targetRotation.x,0);
        var targetDistance = new Vector3(0.0f, 0.0f, -distance) + cameraCentre;

        var slerp = Quaternion.SlerpUnclamped(this.transform.rotation, targetQuaternion, Time.deltaTime*4.0f);
        Vector3 position = slerp * targetDistance;

        transform.rotation = targetQuaternion;
        transform.position = targetQuaternion*targetDistance;
    }

    
}
