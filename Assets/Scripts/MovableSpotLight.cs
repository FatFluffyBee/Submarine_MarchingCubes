using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovableSpotLight : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform projectorStand, projector;
    public float projectorRotationSpeed, projectorLength;
    public float minAngle, lerpSpeed;
    public bool minAngleEnabled;
    Transform playerTransform;
    void Start()
    {
        playerTransform = Player.instance.transform;
    }

    void Update()
    {
        //stop projector from doing stupid and lerp the pos
        //make projector stand turn
        //lock light from goinf under 

        Vector3 aimPos = cameraTransform.position + cameraTransform.forward * projectorLength;

        Quaternion targetRot = Quaternion.LookRotation(aimPos - projector.position, Vector3.forward);

        float currentAngle = Vector3.Angle(-playerTransform.up, targetRot* Vector3.forward); //lock projector from going under

        if(minAngleEnabled) currentAngle = 10000;

        if(currentAngle > minAngle)
        {
            projector.rotation = Quaternion.Slerp(projector.rotation, targetRot, lerpSpeed * Time.deltaTime);
        }

    }
}
