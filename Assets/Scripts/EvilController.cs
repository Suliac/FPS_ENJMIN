using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class EvilController : NetworkBehaviour
{

    private GameObject[] players;
    private List<GameObject> playersReachable;
    private bool run = false;

    public float UpdateIA = 0.0f;
    public const float UpdateIAPeriod = 0.3f;

    public float AttackDistance = 1.5f;
    public float DetectDistance = 15.0f;

    public float AttackSpeed = 1.0f;
    public int Damages = 50;

    public Transform DeathEffect;
    private Animator animator;

    [SyncVar]
    private Vector3 initPosition;
    [SyncVar]
    private Vector3 nearestPosition;
    [SyncVar]
    private bool returnToInitPosition;

    private GameObject target;

    private float currentTimePunch = 0.0f;
    private bool isInit = false;
    private bool wasWalkingBefore = false;
    // Use this for initialization
    void Start()
    {
        //initPosition = transform.position;
        //players = GameObject.FindGameObjectsWithTag("Player");
        playersReachable = new List<GameObject>();
        animator = GetComponentInChildren<Animator>();
        UpdateIA = Random.value;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInit)
            return;

        UpdateIA += Time.deltaTime;

        if (UpdateIA > UpdateIAPeriod)
        {
            UpdateIA = 0.0f;

            if (isServer) // On veut que la destination ne soit choisie que sur le serveur mais que ca soit les clients qui gèrent le déplacement
            {
                CmdFindNextPosition();
            }
            
            if(returnToInitPosition)
            {
                float distanceToInitPos = (initPosition - transform.position).magnitude;
                if (distanceToInitPos < 0.5f)
                    returnToInitPosition = false;
            }

            NavMeshAgent na = GetComponent<NavMeshAgent>();

            if (!na.enabled) // Si il n'y a plus de NavMeshAgent
            {
                if (GetComponent<Rigidbody>().velocity.magnitude < 0.5f) // Si la magnitude de la vitesse < 0.5 = si immobile
                {
                    GetComponent<NavMeshAgent>().enabled = true;
                    GetComponent<Rigidbody>().isKinematic = true;
                }
            }

            float distanceToPlayer = (nearestPosition - transform.position).magnitude;
            if (na && na.enabled)
            {
                currentTimePunch = 0;
                na.SetDestination(nearestPosition);

                if (distanceToPlayer <= AttackDistance && target != null)
                {
                    na.isStopped = true;
                    transform.LookAt(new Vector3(nearestPosition.x, transform.position.y, nearestPosition.z));

                    //Ennemi OS le joueur
                    if (isServer)
                    {
                        print("Should damage");
                        LifeBehaviour lifeB = target.transform.GetComponent<LifeBehaviour>(); // NB : Collision fonctionne par car joueur kinematic
                        if (lifeB != null)
                        {
                            //print("damage from zombie");
                            lifeB.TakeDamage(Damages, "zombie");
                        }

                        returnToInitPosition = true;
                    }
                }
                else
                {
                    na.isStopped = false;
                }
            }


            run = na.desiredVelocity.magnitude > 0.5f;
            if (wasWalkingBefore != run)
            {
                animator.SetBool("IsWalking", run);
            }


            wasWalkingBefore = run;
        }


    }

    [ClientRpc]
    public void RpcInit(Vector3 objectInitPosition)
    {
        initPosition = objectInitPosition;
        NavMeshAgent na = GetComponent<NavMeshAgent>();

        if (!na.enabled) // Si il n'y a plus de NavMeshAgent
        {
            GetComponent<NavMeshAgent>().enabled = true;
            GetComponent<Rigidbody>().isKinematic = true;
        }

        na.Warp(initPosition);
        isInit = true;
    }
    

    [Command]
    public void CmdFindNextPosition()
    {
        if (!isServer)
            return; // normalement useless, si commande on est deja sur le serveur

        Vector3 tmpNearestPosition = initPosition;
        if (returnToInitPosition) // Quand le zombie doit retourner a sa position, on ne veut pas qu'il continue de cherhcher des cibles pendant ce temps
        {
            target = null;
            foreach (var player in playersReachable)
            {
                if ((player.transform.position - transform.position).magnitude < DetectDistance)
                {
                    target = player;
                    tmpNearestPosition = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
                }
            } 
        }

        nearestPosition = tmpNearestPosition;
        //RpcSetDestinationAndTarget(tmpNearestPosition);
    }

    //public void OnCollisionEnter(Collision col)
    //{
    //    LifeBehaviour lifeB = col.transform.GetComponent<LifeBehaviour>();
    //    if (lifeB != null)
    //    {
    //        lifeB.TakeDamage(Damages, "zombie");

    //        //GetComponent<NavMeshAgent>().enabled = false;

    //        //GetComponent<Rigidbody>().isKinematic = false;
    //        //GetComponent<Rigidbody>().AddForce(col.relativeVelocity / 3, ForceMode.Impulse);
    //    }
    //}

    public void OnTriggerEnter(Collider col)
    {
        if (!isServer)
            return;

        if (col.gameObject.CompareTag("Player"))
        {
            if (!playersReachable.Contains(col.gameObject))
            {
                //Debug.Log("Player enter trigger");
                playersReachable.Add(col.gameObject);
            }
        }
    }

    public void OnTriggerLeave(Collider col)
    {
        if (!isServer)
            return;
        if (col.gameObject.CompareTag("Player"))
        {
            if (playersReachable.Contains(col.gameObject))
            {
                //Debug.Log("Player leave trigger");
                playersReachable.Remove(col.gameObject);
            }
        }
    }
}
