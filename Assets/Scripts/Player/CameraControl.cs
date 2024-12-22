using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// camera controls
/*Basic Idea
        - when the submarine turn up, right, left, down, the camera follow it
        - when the player uses its mouse, the camera follows it
        - the camera can pan close / away from the player and the max distance is determined by proximity to land to avoid clipping
        - the camera is not sharp but follow a target pos and lerprotate towards it 
        - after a little time, the camera will center back on a position looking in front of a submarine a little down
        - this func can be called to if the player press space and must be cancelled 
*/
public class CameraControl : MonoBehaviour
{
    private Player player;
    private Transform target;
    private Transform cam;
    bool debugLockCam = false;
    
    [Header("Rotation")]
    [SerializeField] private float cameraTurnSpeed;
    [SerializeField] private float posLerpSpeed;
    [SerializeField] private float rotLerpSpeed;
    [SerializeField] private float minRotationClamp;
    [SerializeField] private float maxRotationClamp;

    [Header("Reset")]
    [SerializeField] private float timeToResetCamera;
    [SerializeField] private float lerpResetCameraSpeed;
     float timeToResetCameraCount;

    [Header("Zoom")]
    [SerializeField] private float zoomSensitivity;
    [SerializeField] private float zoomLerpSpeed;
    [SerializeField] private float initZoom;
    [SerializeField] private float minManualZoom;
    [SerializeField] private float maxManualZoom;
    [SerializeField] private float minHardZoom;
    [SerializeField] private float zoomAdaptabilityTime; //when force into close camera by clipping, the actual zoom value will slowly lerp toward the actual value used
    [SerializeField] private float clippingDist;
    private float currentMaxZoomValue;
    private float zoomValue;
    private float lerpedZoomValue; //used to recover slowly from forced zoom to avoid clipping
    Vector3 camOffset;
    
    private void Start()
    {
        player = FindObjectOfType<Player>();
        target = new GameObject().transform;
        target.SetPositionAndRotation(player.transform.position, player.transform.rotation);
        cam = transform.GetChild(0).transform;
        camOffset = cam.localPosition;
        zoomValue = initZoom;
    }

    void Update()
    {
        if(!debugLockCam) {
            //---Calculate Rotation
            target.position = player.transform.position;
            float horizontalInput = Input.GetAxis("Mouse X");
            float verticalInput = Input.GetAxis("Mouse Y");

            if(horizontalInput != 0 || verticalInput != 0) { //if player move camera move target camera
                target.Rotate(player.transform.up, cameraTurnSpeed * horizontalInput, Space.World);
                target.Rotate(-Vector3.right, cameraTurnSpeed * verticalInput);
                timeToResetCameraCount = 0;
            } else {// if player dont move it reset target camera after a while
                if (timeToResetCameraCount > timeToResetCamera)
                    target.transform.rotation = Quaternion.Slerp(target.transform.rotation, player.transform.rotation, lerpResetCameraSpeed * Time.deltaTime);
                else
                    timeToResetCameraCount += Time.deltaTime;
            }

            //---Clamp Rotation        
            //Vector3 euler = target.eulerAngles;
            //euler.x = Mathf.Clamp((euler.x + 180) % 360, minRotationClamp, maxRotationClamp) - 180;
            //euler.y = Mathf.Clamp((euler.x + 180) % 360, minRotationClamp, maxRotationClamp) - 180;
            //euler.z = Mathf.Clamp((euler.z + 180) % 360, minRotationClamp, maxRotationClamp) - 180;
            //target.eulerAngles = euler;

            //---Zoom Calculation
            //determine max zoom distance to avoid clipping
            float zoomClipLimit = CalculateMaxZoom(-cam.forward);
            currentMaxZoomValue = Mathf.Max(zoomClipLimit, minHardZoom);

            //take player input
            if(Input.GetAxis("MouseScrollDown") != 0) {
                zoomValue -= zoomSensitivity * Input.GetAxis("MouseScrollDown"); 
                zoomValue = Mathf.Clamp(zoomValue, minManualZoom, currentMaxZoomValue);
            }

            //assign new zoomvalue + lerp old value if clipped to avoid grand camera range of movement
            //lerpedZoomValue = Mathf.Lerp(lerpedZoomValue, zoomValue, zoomAdaptabilityTime * Time.deltaTime);
            float clippedZoomValue = Mathf.Max(Mathf.Min(zoomValue, zoomClipLimit), minHardZoom);
            
            camOffset.z = clippedZoomValue;

            //---Reset Rotation
            if(Input.GetKeyDown(KeyCode.Space)) {
                target.rotation = player.transform.rotation;
            }
        }

        //Debug lock cam
        if(Input.GetKeyDown(KeyCode.P)) {
            debugLockCam = !debugLockCam;
        }
    }

    void FixedUpdate() {
        //rotation of the camera
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotLerpSpeed* Time.deltaTime); //lerp the current camera pos towards the target camera
        transform.position = Vector3.Lerp(transform.position, target.position, posLerpSpeed * Time.deltaTime);

        //position of the camera to avoid terrain clipping
        Vector3 finalCameraPos = transform.position - camOffset.x * cam.right + camOffset.y * cam.up - camOffset.z * cam.forward;
        cam.position = Vector3.Lerp(cam.position, finalCameraPos, zoomLerpSpeed * Time.deltaTime);
    }

    public void AddDelayedRotation(float horzRot, float vertRot, float rollRot) { // called when the submarine moves to be added to the camera
        target.Rotate(player.transform.up, horzRot, Space.World);
        target.Rotate(player.transform.right, vertRot, Space.World);
        target.Rotate(player.transform.forward, rollRot, Space.World);
    }

    public void ResetRotation() {
        target.transform.rotation = Quaternion.Slerp(target.transform.rotation, player.transform.rotation, lerpResetCameraSpeed * Time.deltaTime);
    }

    private float CalculateMaxZoom(Vector3 direction) {//calculate the maxzoom clipping distance
        int layerMask = LayerMask.GetMask("Terrain");

        Debug.DrawLine(transform.position, direction * 100f, Color.red, 10f);
        if(Physics.Raycast(transform.position, direction, out RaycastHit hit, maxManualZoom, layerMask)) 
            return hit.distance - clippingDist;

        return maxManualZoom;
    }
}