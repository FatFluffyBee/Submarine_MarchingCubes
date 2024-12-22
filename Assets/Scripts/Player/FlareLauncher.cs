using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//HAndle the behavior of the flare launcher weapon, to launch flare or glowstick (or any projectiles)
public class FlareLauncher : WeaponSystem
{
    [SerializeField] private Transform launchPoint;
    
    [SerializeField] private GameObject projOnePrefab;
    [SerializeField] private float projOneRldDuration;
    [SerializeField] private float projOneSpeed;
    private float projOneRldTimer;

    [SerializeField] private GameObject projTwoPRefab;
    [SerializeField] private float projTwoRldDuration;
    [SerializeField] private float projTwoSpeed;
    private float projTwoRldTimer;

    public override void FireOne() {
        if(projOneRldTimer < Time.time) {
            GameObject instance = Instantiate(projOnePrefab, launchPoint.position, launchPoint.rotation);
            instance.GetComponent<Rigidbody>().velocity = launchPoint.forward * projOneSpeed;
            projOneRldTimer = Time.time + projOneRldDuration;
        }
    }

    public override void ReleaseOne() {}

    public override void FireTwo() {
        if(projTwoRldTimer < Time.time) {
            GameObject instance = Instantiate(projTwoPRefab, launchPoint.position, launchPoint.rotation);
            instance.GetComponent<Rigidbody>().velocity = launchPoint.forward * projTwoSpeed;
            projTwoRldTimer = Time.time + projTwoRldDuration;
        }   
    }

    public override void ReleaseTwo() {}
}
