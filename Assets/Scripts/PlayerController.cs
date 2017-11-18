using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum State
{
    Idle,
    Walking,
    Running,
    Shooting,
    Jumping
}

[System.Serializable]
public class Weapon
{
    public string Name;
    public float RateOfFire;
    public float AnimationTimePreparation;
    public int MaxAmmo;
    public int CurrentAmmo;
    public int Damages;
    public Transform FirePosition;
    public Transform WeaponPositionned;

}

public class PlayerController : NetworkBehaviour
{
    public List<Weapon> Weapons;
    private int weaponIndex = 0;
    private float deltaTimeShooting = 0.0f;

    public Transform Bullet;
    public Transform Camera;
    public float JumpSpeed = 10.0f;
    public float WalkSpeed = 5.0f;
    public float RunSpeed = 10.0f;
    public float XSensitivity = 2.0f;
    public float YSensitivity = 2.0f;
    public float GravityMultiplier = 2.0f;

    private CharacterController characterController;
    private Animator animator;
    
    private bool isJumping = false;
    private bool isWalking = false;
    private bool isRunning = false;
    private bool isShooting = false;

    private bool isPreviouslyGrounded = false;
    private bool wasPreviouslyShooting = false;
    private bool isFirstShoot;

    private Vector3 moveDirection;
    private Vector2 input;

    private Quaternion camTargetRot;
    private Quaternion charTargetRot;
    
    private Quaternion shootArmRot;
    private Quaternion shootLeftArmRot;
    
    private Transform rightArm;
    private Transform leftArm;
    private Transform head;

    void Awake()
    {
        //rightArm = GameObject.Find("RightArmDummy").transform;
        //leftArm = GameObject.Find("LeftArmDummy").transform;
        //head = GameObject.Find("HeadDummy").transform;
        head = transform.GetChild(0).GetChild(0).GetChild(1); // Crado mais fonctionne
        rightArm = transform.GetChild(0).GetChild(0).GetChild(4); // Crado mais fonctionne
        leftArm = transform.GetChild(0).GetChild(0).GetChild(5); // Crado mais fonctionne


        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    // Use this for initialization
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        camTargetRot = Camera.localRotation;
        shootArmRot = Camera.localRotation;
        shootLeftArmRot = Camera.localRotation;
        charTargetRot = transform.localRotation;

        Weapons[weaponIndex].WeaponPositionned.gameObject.SetActive(true);
        Weapons[weaponIndex].CurrentAmmo = Weapons[weaponIndex].MaxAmmo;

        if (!isLocalPlayer)
        {
            Camera.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Animate();

        if (!isLocalPlayer)
            return; // If the player isn't the player of the current client, we don't update his position

        if (!isPreviouslyGrounded && characterController.isGrounded)
        {
            moveDirection.y = 0f;
            SetState(State.Jumping, false);
        }
        if (!characterController.isGrounded && !isJumping && isPreviouslyGrounded)
        {
            moveDirection.y = 0f;
        }

        isPreviouslyGrounded = characterController.isGrounded;

    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return; // If the player isn't the player of the current client, we don't update his position

        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * input.y + transform.right * input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
                           characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        moveDirection.x = desiredMove.x * speed;
        moveDirection.z = desiredMove.z * speed;

        if (characterController.isGrounded)
        {
            moveDirection.y = -10;

            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = JumpSpeed;
                isJumping = true;
                CmdSetState(State.Jumping, true);

            }
        }
        else
        {
            moveDirection += Physics.gravity * GravityMultiplier * Time.fixedDeltaTime;
        }


        characterController.Move(moveDirection * Time.fixedDeltaTime); // move the player   
        transform.localRotation = charTargetRot; // rotate the player 
        Camera.parent.localRotation = camTargetRot; // rotate the cam
        head.localRotation = camTargetRot;

        CmdMove(moveDirection, Time.fixedDeltaTime, camTargetRot, charTargetRot);
    }

    private void LateUpdate()
    {
        if (isLocalPlayer)
        {
            CmdLateMove(shootArmRot, shootLeftArmRot);
        }
        
        if (isShooting)
        {
            rightArm.rotation *= shootArmRot;
            leftArm.rotation *= shootLeftArmRot;
        }
    }

    private void GetInput(out float speed)
    {
        ///////////////////// Walk
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        isWalking = !Input.GetKey(KeyCode.LeftShift) && (horizontal != 0.0f || vertical != 0.0f);
        isRunning = Input.GetKey(KeyCode.LeftShift) && (horizontal != 0.0f || vertical != 0.0f);

        CmdSetState(State.Walking, isWalking);
        CmdSetState(State.Running, isRunning);

        // set the desired speed tobe walking or running
        speed = isWalking ? WalkSpeed : RunSpeed;
        input = new Vector2(horizontal, vertical);

        animator.SetBool("IsWalking", isWalking || isRunning);

        ///////////////////// Cams
        float yRot = Input.GetAxis("Mouse X") * XSensitivity;
        float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

        Quaternion rotationMin = Quaternion.Euler(new Vector3(-50f, 0f, 0f));
        Quaternion rotationMax = Quaternion.Euler(new Vector3(50f, 0f, 0f));

        if (xRot < 0.0f && Camera.parent.rotation.x < rotationMax.x || xRot > 0.0f && Camera.parent.rotation.x > rotationMin.x)
        {
            camTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);
            shootArmRot *= Quaternion.Euler(0f, -xRot, 0f);
            shootLeftArmRot *= Quaternion.Euler(0f, xRot, 0f);
        }

        charTargetRot *= Quaternion.Euler(0f, yRot, 0f);

        ///////////////////// Fire
        if (Input.GetButton("Fire1"))
        {
            deltaTimeShooting += Time.fixedDeltaTime;
            if (deltaTimeShooting >= Weapons[weaponIndex].RateOfFire)
            {
                isShooting = true;
                CmdSetState(State.Shooting, true);
                //animator.SetBool("IsShooting", true);

                if (isShooting && !wasPreviouslyShooting)
                    isFirstShoot = true;

                if ((isFirstShoot && deltaTimeShooting >= Weapons[weaponIndex].AnimationTimePreparation) || !isFirstShoot)
                {
                    if (Weapons[weaponIndex].CurrentAmmo > 0 || Weapons[weaponIndex].MaxAmmo == -1) // NB : -1 maxammo = infinite ammo
                    {
                        Weapons[weaponIndex].CurrentAmmo--;
                        CmdFire(Weapons[weaponIndex].FirePosition.position);                        
                    }
                    deltaTimeShooting -= isFirstShoot ? Weapons[weaponIndex].AnimationTimePreparation : Weapons[weaponIndex].RateOfFire;
                    isFirstShoot = false;
                }
            }
        }

        if (!Input.GetButton("Fire1") && isShooting) // juste stop shooting
        {
            deltaTimeShooting = 0;
            isShooting = false;
            CmdSetState(State.Shooting, false);
            //animator.SetBool("IsShooting", false);
        }

        wasPreviouslyShooting = isShooting;

        // normalize input if it exceeds 1 in combined length:
        if (input.sqrMagnitude > 1)
        {
            input.Normalize();
        }
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    void Animate()
    {
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsWalking", isWalking || isRunning);
        animator.SetBool("IsShooting", isShooting);

    }


    // Command function is called from the client, but invoked on the server
    [Command]
    void CmdFire(Vector3 firePosition)
    {
        var bullet = Instantiate(Bullet, firePosition, Quaternion.identity);

        bullet.GetComponent<Rigidbody>().velocity = Camera.transform.forward * 40;
        //bullet.GetComponent<Rigidbody>().AddForce(Camera.transform.forward * 40, ForceMode.Impulse);

        NetworkServer.Spawn(bullet.gameObject);

        Destroy(bullet.gameObject, 2.0f); // destroy after 2 seconds
    }

    #region Move (Network)
    [Command]
    void CmdMove(Vector3 move, float dt, Quaternion camRotation, Quaternion playerRotation)
    {
        RpcMove(move, dt, camRotation, playerRotation);
    }

    [Command]
    void CmdLateMove(Quaternion rightArmRotation, Quaternion leftArmRotation)
    {
        RpcLateMove(rightArmRotation, leftArmRotation);
        
    }
    
    [ClientRpc]
    void RpcMove(Vector3 move, float dt, Quaternion camRotation, Quaternion playerRotation)
    {
        if (isLocalPlayer)
            return;

        characterController.Move(move * dt); // move the player   
        transform.localRotation = playerRotation; // rotate the player 
        Camera.localRotation = camRotation;
        head.localRotation = camRotation;// rotate the head
    } 

    [ClientRpc]
    void RpcLateMove(Quaternion rightArmRotation, Quaternion leftArmRotation)
    {
        if (isLocalPlayer)
            return;

        shootArmRot = rightArmRotation;
        shootLeftArmRot = leftArmRotation;

        //if (isShooting)
        //{
        //    rightArm.rotation *= rightArmRotation;
        //    leftArm.rotation *= leftArmRotation;
        //}
    }
    #endregion

    #region State
    void SetState(State stateToUpdate, bool value)
    {
        switch (stateToUpdate)
        {
            case State.Idle:
                break;
            case State.Walking:
                isWalking = value;
                break;
            case State.Running:
                isRunning = value;
                break;
            case State.Shooting:
                isShooting = value;
                break;
            case State.Jumping:
                isJumping = value;
                break;
            default:
                break;
        }

        CmdSetState(stateToUpdate, value);
    }

    [Command]
    void CmdSetState(State stateToUpdate, bool value)
    {
        RpcSetState(stateToUpdate, value);
    }

    [ClientRpc]
    void RpcSetState(State stateToUpdate, bool value)
    {
        if (isLocalPlayer)
            return;

        switch (stateToUpdate)
        {
            case State.Idle:
                break;
            case State.Walking:
                isWalking = value;
                break;
            case State.Running:
                isRunning = value;
                break;
            case State.Shooting:
                isShooting = value;
                break;
            case State.Jumping:
                isJumping = value;
                break;
            default:
                break;
        }
    }

    #endregion
}
