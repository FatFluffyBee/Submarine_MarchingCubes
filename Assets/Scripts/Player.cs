using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    private void Awake()
    {
        instance = this;
    }
    public List<WeaponSystem> weaponList = new List<WeaponSystem>();

    public int currentWeaponIndex = 0;
    public CameraControl cameraControl;
    public FinsFeedback finsFeedback;

    public float maxSpeed;
    public float maxAcceleration;
    public float dropOffAccPerSec;

    float accelerationCount;
    public float turnSpeed;

    Rigidbody rb;

    public GameObject propeller;
    public float rotationDegreePerAcc;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(Input.GetAxis("TurnRightLeft") != 0) //left and right
        {
            transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime * Input.GetAxis("TurnRightLeft"));
        }

        if(Input.GetAxis("TurnUpDown") != 0)  //up and down
        {
            transform.Rotate(Vector3.right, turnSpeed * Time.deltaTime * Input.GetAxis("TurnUpDown"));
        }

        cameraControl.AddDelayedRotation(turnSpeed * Time.deltaTime * Input.GetAxis("TurnRightLeft"), turnSpeed * Time.deltaTime * Input.GetAxis("TurnUpDown"));
        finsFeedback.SetFeedbackSideFins(Input.GetAxis("TurnRightLeft"));

        /*if(Input.GetAxis("RollRightLeft") != 0)
        {
            transform.Rotate(Vector3.forward, turnSpeed * Time.deltaTime * Input.GetAxis("RollRightLeft"));
        }*/

        if (Input.GetAxis("SpeedForwardBack") != 0) //forward and back
        {
            accelerationCount += Time.deltaTime;

            if (accelerationCount > maxAcceleration) accelerationCount = maxAcceleration;
        }
        else
        {
            accelerationCount -= dropOffAccPerSec * Time.deltaTime;

            if (accelerationCount < 0) accelerationCount = 0;
        }

        float currentSpeed = accelerationCount / maxAcceleration * maxSpeed;
        
        rb.velocity = currentSpeed * transform.forward;


        if(accelerationCount > 0)
        {
            SetFeedBackPropeller(accelerationCount / maxAcceleration * rotationDegreePerAcc);
        }

        //Change weapons
        if(Input.GetButtonDown("ChangeWeapons"))
        {
            if(Input.GetAxis("ChangeWeapons") > 0)
            {
                Debug.Log("Weapon up");
                currentWeaponIndex++;
                if(currentWeaponIndex > weaponList.Count -1)
                    currentWeaponIndex = 0;
            }
            else
            {
                Debug.Log("Weapon down");
                currentWeaponIndex--;
                if(currentWeaponIndex < 0)
                    currentWeaponIndex = weaponList.Count - 1;
            }
        }

        //Fire Weapons
        if(Input.GetAxis("FireOne") > 0)
        {
            weaponList[currentWeaponIndex].FireOne();
        }
        else 
        {
            weaponList[currentWeaponIndex].ReleaseOne();
        }

        if(Input.GetAxis("FireTwo") > 0)
        {
            weaponList[currentWeaponIndex].FireTwo();
        }
        else
        {
            weaponList[currentWeaponIndex].ReleaseTwo();
        }
    }

    void SetFeedBackPropeller(float angle)
    {
        propeller.transform.Rotate(Vector3.forward, angle);
    }
}
