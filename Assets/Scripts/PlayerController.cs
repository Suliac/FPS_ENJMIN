using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{

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

    private bool isPreviouslyGrounded = false;
    private bool isJumping = false;
    private bool wantToJump = false;

    private bool isWalking = false;
    private bool isRunning = false;

    private Vector3 moveDirection;
    private Vector2 input;

    private Quaternion camTargetRot;
    private Quaternion charTargetRot;


    // Use this for initialization
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        camTargetRot = Camera.localRotation;
        charTargetRot = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return; // If the player isn't the player of the current client, we don't update his position

        //var x = Input.GetAxis("Horizontal") * 0.1f;
        //var z = Input.GetAxis("Vertical") * 0.1f;
        //transform.Translate(x, 0, z);

        if (!isPreviouslyGrounded && characterController.isGrounded)
        {
            moveDirection.y = 0f;
            animator.SetBool("IsJumping", false);
            isJumping = false;
        }
        if (!characterController.isGrounded && !isJumping && isPreviouslyGrounded)
        {
            moveDirection.y = 0f;
        }

        isPreviouslyGrounded = characterController.isGrounded;

        if (Input.GetButton("Fire1"))
        {
            CmdFire();
        }

    }

    private void FixedUpdate()
    {
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
                animator.SetBool("IsJumping", true);
                wantToJump = false;
                isJumping = true;

            }
        }
        else
        {
            moveDirection += Physics.gravity * GravityMultiplier * Time.fixedDeltaTime;
        }

        var m_CollisionFlags = characterController.Move(moveDirection * Time.fixedDeltaTime); // move the player
        transform.localRotation = charTargetRot; // move the cam of the player
        Camera.localRotation = camTargetRot; // move the cam
    }

    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float yRot = Input.GetAxis("Mouse X") * XSensitivity;
        float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

        camTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);
        charTargetRot *= Quaternion.Euler(0f, yRot, 0f);

        //bool waswalking = isWalking;

        isWalking = !Input.GetKey(KeyCode.LeftShift) && (horizontal != 0.0f || vertical != 0.0f);
        isRunning = Input.GetKey(KeyCode.LeftShift) && (horizontal != 0.0f || vertical != 0.0f);

        // set the desired speed tobe walking or running
        speed = isWalking ? WalkSpeed : RunSpeed;
        input = new Vector2(horizontal, vertical);

        animator.SetBool("IsWalking", isWalking || isRunning);

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

    // Command function is called from the client, but invoked on the server
    [Command]
    void CmdFire()
    {
        var bullet = Instantiate(Bullet, transform.position + transform.forward, Quaternion.identity);

        bullet.GetComponent<Rigidbody>().AddForce(Camera.transform.forward * 40, ForceMode.Impulse);

        NetworkServer.Spawn(bullet.gameObject);

        Destroy(bullet.gameObject, 2.0f); // destroy after 2 seconds
    }
}
