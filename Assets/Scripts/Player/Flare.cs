using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handle the flare behavior, its flickering light and initial rotation
public class Flare : MonoBehaviour
{
    [SerializeField] private Light pointLight;
    [SerializeField] private MeshRenderer mR;

    [SerializeField] private float startRotationSpeed;
    [SerializeField] private float timeToDie;

    [SerializeField] private float minLightIntensity; 
    [SerializeField] private float maxLightIntensity;
    [SerializeField] private float lightPingPongIncr;

    [SerializeField] private float minLightFlicker;
    [SerializeField] private float maxLightFlicker;

    [SerializeField] private AnimationCurve lightOverTime;
    [SerializeField] private float lightMaxRange;
    [SerializeField] private Color glowStart;
    [SerializeField] private Color glowEnd;
    
    float lightFlickerValue;
    float aliveTimeCount;

    public void Start()
    {
        pointLight.color = glowStart;
        mR.material.color = glowStart;
        mR.material.EnableKeyword("_EMISSION");

        //Create an initial rotation and random orientation to the glowstick
        Quaternion randomRot = Quaternion.Euler(new Vector3 (Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)));
        transform.rotation = randomRot;
        GetComponent<Rigidbody>().angularVelocity = randomRot.eulerAngles.normalized * Time.deltaTime * startRotationSpeed;

        Destroy(this.gameObject, timeToDie + 10f);
    }

    void FixedUpdate()
    {
        lightFlickerValue = Random.Range(minLightFlicker, maxLightFlicker);
    }

    void Update()
    {
        aliveTimeCount += Time.deltaTime;
        float ratio = aliveTimeCount/ timeToDie;
        float pingPongValue = Mathf.PingPong(Time.time * lightPingPongIncr, maxLightIntensity - minLightIntensity);
        float overTimeIntensity = lightOverTime.Evaluate(ratio);

        //formula to calculate light intensity which combine a ping pong (alternate between 0 and b in a increment) an overtime curve evaluation and a random number to generate 
        //a pulsating light similar to flare 
        float newIntensity = minLightIntensity + pingPongValue * overTimeIntensity * lightFlickerValue;
        pointLight.intensity = newIntensity;
        pointLight.range = lightOverTime.Evaluate(ratio) * lightMaxRange;
        mR.material.SetColor("_EmissionColor", Color.Lerp(glowStart * newIntensity, glowEnd, ratio));
    }
}
