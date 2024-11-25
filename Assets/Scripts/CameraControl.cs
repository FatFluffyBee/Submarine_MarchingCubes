using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// camera controls
// basic idea is we pivot the camera pivot object depending on mouse or controller input and add a little lerp to it. Presing a button can reset the camera
public class CameraControl : MonoBehaviour
{
    Player player;
    Transform rotationTarget;

    public float turnSpeed;
    public float lerpSpeed, lerpResetCameraSpeed;
    public float timeToResetCamera;
    float timeToResetCameraCount;
    bool debugLockCam = false;

    private void Start()
    {
        Cursor.visible = false;

        player = FindObjectOfType<Player>();
        rotationTarget = new GameObject().transform;
        rotationTarget.position = transform.position;
        rotationTarget.rotation = transform.rotation;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            debugLockCam = !debugLockCam;
        }

        if(!debugLockCam)
        {
            transform.position = player.transform.position;
            rotationTarget.position = transform.position;

            float horizontalInput = Input.GetAxis("Mouse X");
            float verticalInput = Input.GetAxis("Mouse Y");

            if(horizontalInput != 0 &&  verticalInput != 0) //if player move camera move target camera
            {
                rotationTarget.Rotate(player.transform.up, turnSpeed * horizontalInput, Space.World);
                rotationTarget.Rotate(-Vector3.right, turnSpeed * verticalInput);

                timeToResetCameraCount = 0;
            }
            else // if player dont move it reset target camera after a while
            {
                if (timeToResetCameraCount > timeToResetCamera)
                    rotationTarget.transform.rotation = Quaternion.Slerp(rotationTarget.transform.rotation, player.transform.rotation, lerpResetCameraSpeed * Time.deltaTime);
                else
                    timeToResetCameraCount += Time.deltaTime;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, rotationTarget.rotation, lerpSpeed* Time.deltaTime); //lerp the current camera pos towards the target camera

            if(Input.GetKeyDown(KeyCode.Space))
            {
                transform.rotation = player.transform.rotation;
            }
        }
    }

    public void AddDelayedRotation(float rightRot, float upRot) // called when the submarine moves up or down
    {
        rotationTarget.Rotate(player.transform.up, rightRot, Space.World);
        rotationTarget.Rotate(Vector3.right, upRot);
    }
}