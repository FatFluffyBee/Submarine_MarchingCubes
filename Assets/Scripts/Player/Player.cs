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

    [SerializeField] private List<WeaponSystem> weaponList = new List<WeaponSystem>();
    private int currentWeaponIndex = 0;

    [SerializeField] private CameraControl cameraControl;
    [SerializeField] private FinsFeedback finsFeedback;

    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxAcceleration;
    [SerializeField] private float dropOffAccPerSec;
    float accelerationCount;

    [SerializeField] private float turnSpeed;

    [SerializeField] private GameObject propeller;
    [SerializeField] private float rotationDegreePerAcc;

    Rigidbody rb;
    Transform targetRot;

    void Start()
    {
        targetRot = new GameObject().transform;
        targetRot.rotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
         //Steering
        if(Input.GetAxis("TurnRightLeft") != 0) { //left and right 
            targetRot.Rotate(Vector3.up, turnSpeed * Time.deltaTime * Input.GetAxis("TurnRightLeft"));
            finsFeedback.SetFeedbackSideFins(Input.GetAxis("TurnRightLeft"));
        }

        if(Input.GetAxis("TurnUpDown") != 0) {//up and down
            targetRot.Rotate(Vector3.right, turnSpeed * Time.deltaTime * Input.GetAxis("TurnUpDown"));
        }

        if(Input.GetAxis("RollRightLeft") != 0) {
            targetRot.Rotate(Vector3.forward, turnSpeed * Time.deltaTime * Input.GetAxis("RollRightLeft"));
        }

        cameraControl.AddDelayedRotation(turnSpeed * Time.deltaTime * Input.GetAxis("TurnRightLeft"), turnSpeed * Time.deltaTime * Input.GetAxis("TurnUpDown"), 
        turnSpeed * Time.deltaTime * Input.GetAxis("RollRightLeft"));

        //Acceleration 
        if (Input.GetAxis("SpeedForwardBack") != 0) {//forward and back //TODO this cause only aceeleration forward
            accelerationCount += Time.deltaTime;
            accelerationCount = (accelerationCount > maxAcceleration)? maxAcceleration : accelerationCount;
        } else {
            accelerationCount -= dropOffAccPerSec * Time.deltaTime;
            accelerationCount = (accelerationCount < 0)? 0 : accelerationCount;
        }

        if(accelerationCount > 0) {
            SetFeedBackPropeller(accelerationCount / maxAcceleration * rotationDegreePerAcc);
        }

        //Change weapons
        if(Input.GetButtonDown("ChangeWeapons")) {
            if(Input.GetAxis("ChangeWeapons") > 0) {
                Debug.Log("Weapon up");
                currentWeaponIndex++;
                if(currentWeaponIndex > weaponList.Count -1)
                    currentWeaponIndex = 0;
            } else {
                Debug.Log("Weapon down");
                currentWeaponIndex--;
                if(currentWeaponIndex < 0)
                    currentWeaponIndex = weaponList.Count - 1;
            }
        }

        //Fire Weapons
        if(Input.GetAxis("FireOne") > 0) {
            weaponList[currentWeaponIndex].FireOne();
        } else {
            weaponList[currentWeaponIndex].ReleaseOne();
        }

        if(Input.GetAxis("FireTwo") > 0) {
            weaponList[currentWeaponIndex].FireTwo();
        } else {
            weaponList[currentWeaponIndex].ReleaseTwo();
        }
    }

    void FixedUpdate() //todo separate input from physics
    {
        transform.rotation = targetRot.rotation;
        float currentSpeed = accelerationCount / maxAcceleration * maxSpeed;  
        rb.velocity = currentSpeed * transform.forward;

        targetRot = transform; 
    }

    void SetFeedBackPropeller(float angle) {
        propeller.transform.Rotate(Vector3.forward, angle);
    }
}
