using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

//Handle most of player input and movement  //todo separate those inputs and those in camera controls in a controller script if time and needed 
public class Player : MonoBehaviour
{
    public static Player instance;
    private void Awake()
    {
        instance = this;
    }

    private Rigidbody rb;
    private Transform targetRot;

    [SerializeField] private CameraControl cameraControl;
    [SerializeField] private FinsFeedback finsFeedback;

    [Header("Weapons")]
    [SerializeField] private List<WeaponSystem> weaponList = new List<WeaponSystem>();
    private int currentWeaponIndex = 0;

    [Header("Movement")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxAcc;
    [SerializeField] private float accPerSec;
    [SerializeField] private float dropOffAccPerSec;
    [SerializeField] private float turnSpeed;
    private float currentAcc;

    [Header("Propeller")] //todo need its own script
    [SerializeField] private GameObject propeller;
    [SerializeField] private float rotationDegreePerAcc;

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
        if (Input.GetAxis("SpeedForwardBack") != 0) {//forward and back 
            if(Input.GetAxis("SpeedForwardBack") > 0) {
                currentAcc += Time.deltaTime * accPerSec;
                currentAcc = (currentAcc > maxAcc)? maxAcc : currentAcc;
            } else {
                currentAcc -= Time.deltaTime * accPerSec;
                currentAcc = (currentAcc < -maxAcc)? -maxAcc : currentAcc;
            }
        } else {
            if(currentAcc > 0) {
                currentAcc -= dropOffAccPerSec * Time.deltaTime;
                currentAcc = (currentAcc < 0)? 0 : currentAcc;
            } else {
                currentAcc += dropOffAccPerSec * Time.deltaTime;
                currentAcc = (currentAcc > 0)? 0 : currentAcc;
            }
        }

        if(currentAcc != 0) {
            SetFeedBackPropeller(currentAcc / maxAcc * rotationDegreePerAcc);
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

    void FixedUpdate()
    {
        transform.rotation = targetRot.rotation;
        targetRot = transform; 

        float currentSpeed = currentAcc / maxAcc * maxSpeed;  
        rb.velocity = currentSpeed * transform.forward;
    }

    void SetFeedBackPropeller(float angle) {
        propeller.transform.Rotate(Vector3.forward, angle);
    }
}
