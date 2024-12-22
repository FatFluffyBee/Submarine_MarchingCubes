using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//control the movable spotlight which follow camera view with a delay
public class MovableSpotLight : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform projectorStand;
    [SerializeField] private Transform projectorHead;
    
    [SerializeField] private float projStandRotSpeed;
    [SerializeField] private float projectorLength;
    [SerializeField] private float minAngle;
    [SerializeField] private float lerpSpeed;
    [SerializeField] private bool minAngleEnabled;

    private Transform playerTransform;

    void Start()
    {
        playerTransform = Player.instance.transform;
    }

    void Update()
    {
        //todo make projector stand turn
        //todo lock light from goinf under when travelling from two points (pass by non usable positions)

        //we setup a point where the camera is looking that the spotlight try to lock on to
        Vector3 aimPos = cameraTransform.position + cameraTransform.forward * projectorLength;
        Quaternion targetRot = Quaternion.LookRotation(aimPos - projectorHead.position, Vector3.forward);

        //We dont apply rotation if projector cant go there (under submarine look)
        float currentAngle = Vector3.Angle(-playerTransform.up, targetRot* Vector3.forward); //lock projector from going under
        if(!minAngleEnabled) {
            currentAngle = 10000;
        }

        if(currentAngle > minAngle) {
            projectorHead.rotation = Quaternion.Slerp(projectorHead.rotation, targetRot, lerpSpeed * Time.deltaTime);
        }

    }
}
