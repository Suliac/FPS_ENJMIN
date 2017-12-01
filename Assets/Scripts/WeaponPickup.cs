using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum PickupType
{
    Weapon,
    Ammo,
    Life,
    Evil
}

public class WeaponPickup : NetworkBehaviour
{
    public GameObject Zombie;
    public float FallingSpeed = 100.0f;
    public int LifeAmount = 25;

    [SyncVar]
    public PickupType Type;
    [SyncVar]
    private int WeaponId = -1;
    [SyncVar]
    private Vector3 endPosition;

    private Transform RifleMini;
    private Transform LaserMini;
    private Transform AmmoMini;
    private Transform LifeMini;

    private bool used = false;

    private bool goodPos = false;

    private bool chestOpenned = false;
    private bool animLaunched = false;
    private Animator anim;
    private float dtOpenChest = 0.0f;

    void Awake()
    {
        anim = transform.GetComponentInChildren<Animator>();
    }

    void Start()
    {
        AmmoMini = transform.GetChild(2);
        RifleMini = transform.GetChild(3).GetChild(0);
        LaserMini = transform.GetChild(3).GetChild(1);
        LifeMini = transform.GetChild(3).GetChild(2);

        AmmoMini.gameObject.SetActive(false);
        RifleMini.gameObject.SetActive(false);
        LaserMini.gameObject.SetActive(false);
        LifeMini.gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (transform.position.y > endPosition.y)
            transform.Translate(-transform.up * FallingSpeed * Time.fixedDeltaTime);
        else if (!goodPos)
        {
            dtOpenChest = 0;
            transform.position = endPosition;
            goodPos = true; // avoid to update at each frame                 
        }

        if (goodPos && !chestOpenned)
        {
            if(!animLaunched)
            {
                anim.SetTrigger("OpenChest");
                animLaunched = true;
            }

            dtOpenChest += Time.fixedDeltaTime;
            
            if (dtOpenChest > 1.0f)
            {
                //Debug.Log("Cool");

                switch (Type)
                {
                    case PickupType.Weapon:
                        if (WeaponId == 1)
                            RifleMini.gameObject.SetActive(true);
                        else if (WeaponId == 2)
                            LaserMini.gameObject.SetActive(true);
                        break;
                    case PickupType.Ammo:
                        AmmoMini.gameObject.SetActive(true);

                        if (WeaponId == 1)
                            RifleMini.gameObject.SetActive(true);
                        else if (WeaponId == 2)
                            LaserMini.gameObject.SetActive(true);
                        break;
                    case PickupType.Life:
                        LifeMini.gameObject.SetActive(true);
                        break;
                    case PickupType.Evil:
                        break;
                    default:
                        break;
                }

                chestOpenned = true;

                //if (isServer)
                //{
                //    GameObject newPickup = (GameObject)Instantiate(Zombie, transform.position, Quaternion.identity);
                //    NetworkServer.Spawn(Zombie);
                //}
            }
        }

    }
    
    [ClientRpc]
    public void RpcInit(Vector3 initEndPosition, PickupType initType, int initWeaponId)
    {
        endPosition = initEndPosition;
        Type = initType;
        WeaponId = initWeaponId;
    }
    
    [Command]
    public void CmdSpawnZombie()
    {

    }

    void OnTriggerEnter(Collider col)
    {
        if (!used)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                PlayerController controller = col.gameObject.GetComponent<PlayerController>();

                if (controller != null)
                {
                    switch (Type)
                    {
                        case PickupType.Weapon:
                            controller.NewWeapon(WeaponId);
                            break;
                        case PickupType.Ammo:
                            controller.AddAmmo(WeaponId);
                            break;
                        case PickupType.Evil:
                            break;
                        case PickupType.Life:
                            LifeBehaviour lifeB = col.gameObject.GetComponent<LifeBehaviour>();
                            if (lifeB != null)
                                lifeB.AddLife(LifeAmount);
                            break;
                        default:
                            break;
                    }

                    GameInfoHandler.DelPickup();
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // We need to do the coroutine on each client otherwise, the pickup seems to "stutter"
    //[ClientRpc]
    //public void RpcPickupFalling(Vector3 startPosition, Vector3 endPosition, float timePickupFalling)
    //{
    //    StartCoroutine(SmoothFall(startPosition, endPosition, timePickupFalling));
    //}

    //public IEnumerator SmoothFall(Vector3 startPosition, Vector3 endPosition, float time = 1.0f)
    //{
    //    float myTime = 0.0f;

    //    while (myTime < time)
    //    {
    //        myTime += Time.deltaTime;
    //        float partOfTotalTime = myTime / time;
    //        transform.position = Vector3.Lerp(startPosition, endPosition, Mathf.Min(1, partOfTotalTime));
    //        yield return null;
    //    }
    //}
}
