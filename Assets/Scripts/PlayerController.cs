using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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
    public int Id;
    public float RateOfFire;
    public float AnimationTimePreparation;
    public int MaxAmmo;
    public int CurrentAmmo;
    public int NumberBulletPerShoot = 1;
    public float BulletSpeed = 50.0f;
    public int Damages;
    public float DestroyBulletAfterSeconds = 2.0f;
    public Transform FirePosition;
    public Transform WeaponPositionned;
    public Transform Bullet;
    public bool UnlockedByDefault;
}

public class PlayerController : NetworkBehaviour
{
    public List<Weapon> WeaponsAvailable;
    private List<Weapon> currentWeapons;
    private int weaponIndex = 0;
    private int nextWeaponWanted = 0;
    private float deltaTimeShooting = 0.0f;

    public Transform Camera;
    public float JumpSpeed = 10.0f;
    public float WalkSpeed = 5.0f;
    public float RunSpeed = 10.0f;
    public float XSensitivity = 2.0f;
    public float YSensitivity = 2.0f;
    public float GravityMultiplier = 2.0f;

    private CharacterController characterController;
    private Animator animator;
    private LifeBehaviour lifeScript;

    private Text UiHealth;
    private Text UiAmmo;

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
        head = transform.GetChild(0).GetChild(0).GetChild(1); // Crado mais fonctionne
        rightArm = transform.GetChild(0).GetChild(0).GetChild(4); // Crado mais fonctionne
        leftArm = transform.GetChild(0).GetChild(0).GetChild(5); // Crado mais fonctionne

        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        lifeScript = GetComponent<LifeBehaviour>();

        currentWeapons = new List<Weapon>();
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

        weaponIndex = -1;
        nextWeaponWanted = 0;

        if (!isLocalPlayer)
        {
            Camera.gameObject.SetActive(false);
        }
        else
        {
            GameInfoHandler.PlayerUi.SetActive(true);

            UiAmmo = GameObject.Find("Ammo_Text").GetComponent<Text>();
            UiHealth = GameObject.Find("Life_Text").GetComponent<Text>();
        }

        for (int i = 0; i < WeaponsAvailable.Count; i++)
        {
            WeaponsAvailable[i].CurrentAmmo = WeaponsAvailable[i].MaxAmmo;
            WeaponsAvailable[i].WeaponPositionned.gameObject.SetActive(false);
        }

        currentWeapons.AddRange(WeaponsAvailable.Where(w => w.UnlockedByDefault));
    }

    // Update is called once per frame
    void Update()
    {
        Animate();

        if (!isLocalPlayer)
            return; // If the player isn't the player of the current client, we don't update his position

        ChangeWeapon();
        UpdateUi();

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
            isShooting = true;
            CmdSetState(State.Shooting, true);

            if (currentWeapons.Any() && deltaTimeShooting >= currentWeapons[weaponIndex].RateOfFire)
            {
                //animator.SetBool("IsShooting", true);
                if (isShooting && !wasPreviouslyShooting)
                    isFirstShoot = true;

                if ((isFirstShoot && deltaTimeShooting >= currentWeapons[weaponIndex].AnimationTimePreparation) || !isFirstShoot)
                {
                    if (currentWeapons[weaponIndex].CurrentAmmo > 0 || currentWeapons[weaponIndex].MaxAmmo == -1) // NB : -1 maxammo = infinite ammo
                    {
                        currentWeapons[weaponIndex].CurrentAmmo--;
                        CmdFire(weaponIndex, currentWeapons[weaponIndex].FirePosition.position);
                    }
                    deltaTimeShooting -= isFirstShoot ? currentWeapons[weaponIndex].AnimationTimePreparation : currentWeapons[weaponIndex].RateOfFire;
                    isFirstShoot = false;
                }
            }
        }

        if (!Input.GetButton("Fire1") && isShooting) // just stop shooting
        {
            deltaTimeShooting = 0;
            isShooting = false;
            CmdSetState(State.Shooting, false);
            //animator.SetBool("IsShooting", false);
        }

        wasPreviouslyShooting = isShooting;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetKeyUp(KeyCode.O)) // Scroll forward
            nextWeaponWanted = weaponIndex + 1;
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f || Input.GetKeyUp(KeyCode.P)) // scroll backward
            nextWeaponWanted = weaponIndex - 1;

        // normalize input if it exceeds 1 in combined length:
        if (input.sqrMagnitude > 1)
        {
            input.Normalize();
        }
    }

    private void Animate()
    {
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsWalking", isWalking || isRunning);
        animator.SetBool("IsShooting", isShooting);


    }

    private void UpdateUi()
    {
        if (!isLocalPlayer || lifeScript == null)
            return;

        if (UiHealth != null)
            UiHealth.text = lifeScript.Health.ToString();

        if (UiAmmo != null)
            UiAmmo.text = currentWeapons.Any() ? currentWeapons[weaponIndex].CurrentAmmo.ToString() : "ERROR";

    }

    private void ChangeWeapon()
    {

        if (weaponIndex != nextWeaponWanted && currentWeapons.Any())
        {
            if (nextWeaponWanted >= currentWeapons.Count)
                nextWeaponWanted = 0;

            if (nextWeaponWanted < 0)
                nextWeaponWanted = currentWeapons.Count - 1;

            if (nextWeaponWanted != weaponIndex)
            {
                if (weaponIndex >= 0) // NB -> Init : weapon index = -1
                    currentWeapons[weaponIndex].WeaponPositionned.gameObject.SetActive(false); // disable current weapon 

                currentWeapons[nextWeaponWanted].WeaponPositionned.gameObject.SetActive(true); // active wanted weapon

                if (currentWeapons[nextWeaponWanted].CurrentAmmo < 0)
                {
                    GameInfoHandler.InfiniteAmmoImage.SetActive(true);
                    UiAmmo.gameObject.SetActive(false);
                }
                else
                {
                    GameInfoHandler.InfiniteAmmoImage.SetActive(false);
                    UiAmmo.gameObject.SetActive(true);
                }
            }

            CmdChangeWeapon(nextWeaponWanted, weaponIndex);
            weaponIndex = nextWeaponWanted;
            animator.SetInteger("CurrentWeapon", currentWeapons[weaponIndex].Id);
        }
    }

    public void NewWeapon(int idWeapon)
    {
        if (!isServer)
            return;

        RpcNewWeapon(idWeapon);
    }

    [ClientRpc]
    public void RpcNewWeapon(int idWeapon)
    {
        if (idWeapon < 0 || idWeapon >= WeaponsAvailable.Count)
            return;

        Weapon weaponToUnlock = WeaponsAvailable.FirstOrDefault(w => w.Id == idWeapon);
        if (weaponToUnlock == null)
            return;

        Weapon current = currentWeapons.FirstOrDefault(w => w.Id == idWeapon);

        if (current == null)
        {
            current = weaponToUnlock;
            currentWeapons.Add(current);
            currentWeapons = currentWeapons.OrderBy(w => w.Id).ToList();
        }

        current.CurrentAmmo = current.MaxAmmo;
    }

    public void AddAmmo(int idWeapon)
    {
        if (!isServer)
            return;

        RpcAddAmmo(idWeapon);
    }

    [ClientRpc]
    public void RpcAddAmmo(int idWeapon)
    {
        if (idWeapon < 0 || idWeapon >= WeaponsAvailable.Count)
            return;

        Weapon current = currentWeapons.FirstOrDefault(w => w.Id == idWeapon);

        if (current != null)
            current.CurrentAmmo = current.MaxAmmo;
    }


    #region Commands & RPC Methods
    // Command function is called from the client, but invoked on the server
    [Command]
    void CmdFire(int weaponIndex, Vector3 firePosition)
    {
        var weapon = currentWeapons[weaponIndex];

        var bullet = Instantiate(weapon.Bullet, firePosition, Quaternion.identity);

        bullet.GetComponent<Rigidbody>().velocity = Camera.transform.forward * weapon.BulletSpeed;
        bullet.GetComponent<BulletBehaviour>().SetDamage(weapon.Damages);
        NetworkServer.Spawn(bullet.gameObject);

        Destroy(bullet.gameObject, weapon.DestroyBulletAfterSeconds);
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

    [Command]
    void CmdChangeWeapon(int newIndexWeapon, int previousIndexWeapon)
    {
        RpcChangeWeapon(newIndexWeapon, previousIndexWeapon);
    }

    [ClientRpc]
    void RpcChangeWeapon(int newIndexWeapon, int previousIndexWeapon)
    {
        if (isLocalPlayer)
            return;

        if (previousIndexWeapon >= 0) // NB -> Init : weapon index = -1
            currentWeapons[previousIndexWeapon].WeaponPositionned.gameObject.SetActive(false); // disable current weapon 

        currentWeapons[newIndexWeapon].WeaponPositionned.gameObject.SetActive(true); // active wanted weapon

        animator.SetInteger("CurrentWeapon", newIndexWeapon);
        weaponIndex = newIndexWeapon;

    }
    #endregion

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public override void OnStartLocalPlayer()
    {
        GameInfoHandler.GameStarted = true;

        //Debug.Log(GameInfoHandler.GameStarted);
    }
}
