using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class GlowStick : MonoBehaviour
{
    public Light light;
    public MeshRenderer mR;
    public float timeToDie, lightMaxIntensity, lightMaxRange, startRotationSpeed;
    public AnimationCurve lightOverTime;
    public Color glowStart, glowEnd;
    float timeCount;

    public void Start()
    {
        light.color = glowStart;
        mR.material.color = glowStart;
        mR.material.EnableKeyword("_EMISSION");

        //Create an initial rotation and random orientation to the glowstick
        Quaternion randomRot = Quaternion.Euler(new Vector3 (Random.Range(0f, 360), Random.Range(0, 360f), Random.Range(0f, 360f)));
        transform.rotation = randomRot;
        GetComponent<Rigidbody>().angularVelocity = randomRot.eulerAngles.normalized * Time.deltaTime * startRotationSpeed;

        Destroy(this.gameObject, timeToDie + 10f);
    }

    void Update()
    {
        timeCount += Time.deltaTime;
        float ratio = timeCount/ timeToDie;

        light.intensity = lightOverTime.Evaluate(ratio) * lightMaxIntensity;
        light.range = lightOverTime.Evaluate(ratio) * lightMaxRange;

        mR.material.SetColor("_EmissionColor", Color.Lerp(glowStart * light.intensity, glowEnd, ratio));
    }
}
