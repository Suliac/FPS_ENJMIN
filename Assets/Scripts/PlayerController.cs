using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class Weapon
{
    public string Name;
    public float RateOfFire;
    public float AnimationTimePreparation;
    public int MaxAmmo;
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

    [SyncVar]
    private bool isJumping = false;

    [SyncVar]
    private bool isWalking = false;

    [SyncVar]
    private bool isRunning = false;

    [SyncVar]
    public bool isShooting = false;

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
        rightArm = GameObject.Find("RightArmDummy").transform;
        leftArm = GameObject.Find("LeftArmDummy").transform;
        head = GameObject.Find("HeadDummy").transform;



    }

    // Use this for initialization
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        camTargetRot = Camera.localRotation;
        shootArmRot = Camera.localRotation;
        shootLeftArmRot = Camera.localRotation;
        charTargetRot = transform.localRotation;

        Weapons[weaponIndex].WeaponPositionned.gameObject.SetActive(true);

        if (!isLocalPlayer)
        {
            Camera.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return; // If the player isn't the player of the current client, we don't update his position

        if (!isPreviouslyGrounded && characterController.isGrounded)
        {
            moveDirection.y = 0f;
            //animator.SetBool("IsJumping", false);
            isJumping = false;
        }
        if (!characterController.isGrounded && !isJumping && isPreviouslyGrounded)
        {
            moveDirection.y = 0f;
        }

        isPreviouslyGrounded = characterController.isGrounded;

    }

    private void FixedUpdate()
    {
        Animate();

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
                //PlayJumpSound();
                //animator.SetBool("IsJumping", true);
                isJumping = true;

            }
        }
        else
        {
            moveDirection += Physics.gravity * GravityMultiplier * Time.fixedDeltaTime;
        }

        //var m_CollisionFlags = characterController.Move(moveDirection * Time.fixedDeltaTime); // move the player
        CmdMove(moveDirection, Time.fixedDeltaTime, camTargetRot, charTargetRot);

       
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer)
            return; // If the player isn't the player of the current client, we don't update his position

        head.localRotation = camTargetRot;
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
                //animator.SetBool("IsShooting", true);

                if (isShooting && !wasPreviouslyShooting)
                    isFirstShoot = true;

                if ((isFirstShoot && deltaTimeShooting >= Weapons[weaponIndex].AnimationTimePreparation) || !isFirstShoot)
                {
                    CmdFire();

                    deltaTimeShooting -= Weapons[weaponIndex].RateOfFire;
                    isFirstShoot = false;
                }
            }
        }

        if (!Input.GetButton("Fire1") && isShooting) // juste stop shooting
        {
            isShooting = false;
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

    private void Animate()
    {
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsWalking", isWalking || isRunning);
        animator.SetBool("IsShooting", isShooting);
    }

    // Command function is called from the client, but invoked on the server
    [Command]
    void CmdFire()
    {
        var bullet = Instantiate(Bullet, Weapons[weaponIndex].FirePosition.position, Quaternion.identity);

        bullet.GetComponent<Rigidbody>().AddForce(Camera.transform.forward * 40, ForceMode.Impulse);

        NetworkServer.Spawn(bullet.gameObject);

        Destroy(bullet.gameObject, 2.0f); // destroy after 2 seconds
    }

    [Command]
    void CmdMove(Vector3 move, float dt, Quaternion camRotation, Quaternion playerRotation)
    {
        RpcMove(move, dt, camRotation, playerRotation);
    }

    [ClientRpc]
    void RpcMove(Vector3 move, float dt, Quaternion camRotation, Quaternion playerRotation)
    {
        //if (isLocalPlayer)
        //    return;

        characterController.Move(move * dt); // move the player   
        transform.localRotation = camRotation; // rotate the player 
        Camera.parent.localRotation = playerRotation; // rotate the cam
    }
}
