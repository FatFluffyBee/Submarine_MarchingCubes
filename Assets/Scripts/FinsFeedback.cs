using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinsFeedback : MonoBehaviour //Refaire, seems like a lot of calculation for a detail
{
    Player player;

    public GameObject topFin, botFin;
    public float angleFinRotation, lerpSpeed;

    Quaternion baseRotation, fullRotationLeft, fullRotationRight, targetRotation;
    float ratio;

    private void Start()
    {
        player = FindObjectOfType<Player>();

        baseRotation = topFin.transform.rotation;
        fullRotationLeft = Quaternion.Inverse(Quaternion.AngleAxis(angleFinRotation, Vector3.forward));
        fullRotationRight = Quaternion.Inverse(Quaternion.AngleAxis(angleFinRotation, -Vector3.forward));
    }

    public void SetFeedbackSideFins(float ratio)
    {
        this.ratio = ratio;
    }

    void Update()
    {
        if(ratio == 0)
        {
            targetRotation = player.transform.rotation * baseRotation;
        }
        else
        {
            if (ratio < 0)
            {
                targetRotation = player.transform.rotation * baseRotation * fullRotationLeft;
            }
            else
            {
                targetRotation = player.transform.rotation * baseRotation * fullRotationRight;
            }

            ratio = 0;
        }

        Quaternion finalRotation = Quaternion.Lerp(topFin.transform.rotation, targetRotation, lerpSpeed * Time.deltaTime);

        topFin.transform.rotation = finalRotation;
        botFin.transform.rotation = finalRotation;

        
    }
}
