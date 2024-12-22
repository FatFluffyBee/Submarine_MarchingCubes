using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

//handle the glowstick behavior, its initial rotation and intensity
public class GlowStick : MonoBehaviour
{
    [SerializeField] private Light pointLight;
    [SerializeField] private MeshRenderer mR;

    [SerializeField] private float startRotationSpeed;
    [SerializeField] private float timeToDie; 

    [SerializeField] private float lightMaxIntensity;

    [SerializeField] private AnimationCurve lightOverTime;
    [SerializeField] private float lightMaxRange;
    [SerializeField] private Color glowEnd;
    [SerializeField] private Color glowStart;
    private float aliveTimeCount;

    public void Start()
    {
        pointLight.color = glowStart;
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
        aliveTimeCount += Time.deltaTime;
        float ratio = aliveTimeCount/ timeToDie;

        pointLight.intensity = lightOverTime.Evaluate(ratio) * lightMaxIntensity;
        pointLight.range = lightOverTime.Evaluate(ratio) * lightMaxRange;

        mR.material.SetColor("_EmissionColor", Color.Lerp(glowStart * pointLight.intensity, glowEnd, ratio));
    }
}
