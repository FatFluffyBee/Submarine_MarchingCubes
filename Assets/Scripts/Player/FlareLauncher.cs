using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlareLauncher : WeaponSystem
{
    public Transform launchPoint;
    public GameObject projectileOne, projectileTwo;
    public float cDShotOne, cDShotTwo;
    float cDShotOneCount, cDShotTwoCount;

    public float shotOneSpeed, shotTwoSpeed;

    public override void FireOne()
    {
        if(cDShotOneCount < Time.time)
        {
            GameObject instance = Instantiate(projectileOne, transform.position, transform.rotation);
            instance.GetComponent<Rigidbody>().velocity = transform.forward * shotOneSpeed;
            cDShotOneCount = Time.time + cDShotOne;
        }
    }

    public override void ReleaseOne()
    {

    }

    public override void FireTwo()
    {
        if(cDShotTwoCount < Time.time)
        {
            GameObject instance = Instantiate(projectileTwo, transform.position, transform.rotation);
            instance.GetComponent<Rigidbody>().velocity = transform.forward * shotTwoSpeed;
            cDShotTwoCount = Time.time + cDShotTwo;
        }   
    }

    public override void ReleaseTwo()
    {

    }
}
