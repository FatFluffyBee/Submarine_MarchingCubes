using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handle the fins feedback movement of the submarine
public class FinsFeedback : MonoBehaviour 
{
    private Player player;

    [SerializeField] private GameObject topFin;
    [SerializeField] private GameObject botFin;
    [SerializeField] private float angleFinRotation;
    [SerializeField] private float lerpSpeed;

    private Quaternion baseRotation;
    private Quaternion fullRotationLeft;
    private Quaternion fullRotationRight;
    private Quaternion targetRotation;
    private float steeringDir;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        baseRotation = topFin.transform.rotation;
        fullRotationLeft = Quaternion.Inverse(Quaternion.AngleAxis(angleFinRotation, Vector3.forward));
        fullRotationRight = Quaternion.Inverse(Quaternion.AngleAxis(angleFinRotation, -Vector3.forward));
    }

    void Update()
    {
        //Calculate the rotation to add (or not) then rotate the fins accordingly
        targetRotation = CalculateTargetRotation(steeringDir);
        steeringDir = 0;

        Quaternion finalRotation = Quaternion.Lerp(topFin.transform.rotation, targetRotation, lerpSpeed * Time.deltaTime);
        topFin.transform.rotation = finalRotation;
        botFin.transform.rotation = finalRotation;
    }

    private Quaternion CalculateTargetRotation(float steeringDir) {
        if(steeringDir == 0) {
            return player.transform.rotation * baseRotation;
        } else if (steeringDir > 0) {
            return player.transform.rotation * baseRotation * fullRotationLeft;
        } else {
            return player.transform.rotation * baseRotation * fullRotationRight;
        }
    }

    public void SetFeedbackSideFins(float steeringDir) {
        this.steeringDir = steeringDir;
    }
}
