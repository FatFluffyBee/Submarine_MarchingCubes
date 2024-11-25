using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flare : MonoBehaviour
{
    public Light light;
    public MeshRenderer mR;
    public float timeToDie, minLightIntensity, maxLightIntensity, lightFlickerSpeed, lightMaxRange, startRotationSpeed;
    public float variationMin, variationMax;
    float variationFactor;
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

    void FixedUpdate()
    {
        variationFactor = Random.Range(variationMin, variationMax);
    }

    void Update()
    {
        timeCount += Time.deltaTime;
        float ratio = timeCount/ timeToDie;
        
        float newIntensity = minLightIntensity + Mathf.PingPong(Time.time * lightFlickerSpeed, maxLightIntensity - minLightIntensity) 
        * lightOverTime.Evaluate(ratio) * variationFactor;
        light.intensity = newIntensity;

        light.range = lightOverTime.Evaluate(ratio) * lightMaxRange;

        mR.material.SetColor("_EmissionColor", Color.Lerp(glowStart * newIntensity, glowEnd, ratio));
    }
}
