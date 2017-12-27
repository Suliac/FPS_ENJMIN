using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class EvilController : NetworkBehaviour
{
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

    public AudioClip AttackSound;
    public AudioClip HurtSound;

    private AudioSource audioSource;
    private AudioSource hurtAudioSource;

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

    private LifeBehaviour life;
    // Use this for initialization
    void Start()
    {
        //initPosition = transform.position;
        life = GetComponent<LifeBehaviour>();
        playersReachable = new List<GameObject>();
        animator = GetComponentInChildren<Animator>();
        UpdateIA = Random.value;
        GetComponent<Rigidbody>().isKinematic = true;

        var audioSources = GetComponents<AudioSource>();
        audioSource = audioSources[0];
        hurtAudioSource = audioSources[1];

        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players) // detection des joueurs qui sont déja dans le champs de détection (mais qui n'ont donc pas déclenché le trigger)
        {
            var dist = (player.transform.position - transform.position).magnitude;
            if (dist <= DetectDistance)
            {
                playersReachable.Add(player);
            }
        }

        if (initPosition != null && initPosition != Vector3.zero && !isInit)
        {
            print("init mano car player arrive apres spawn");
            NavMeshAgent na = GetComponent<NavMeshAgent>();

            if (!na.enabled) // Si il n'y a plus de NavMeshAgent
            {
                GetComponent<NavMeshAgent>().enabled = true;
                GetComponent<Rigidbody>().isKinematic = true;
            }

            na.Warp(initPosition);
            isInit = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInit)
            return;

        if (!life.Dying)
        {
            UpdateIA += Time.deltaTime;

            if (UpdateIA > UpdateIAPeriod)
            {
                UpdateIA = 0.0f;

                if (isServer) // On veut que la destination ne soit choisie que sur le serveur mais que ca soit les clients qui gèrent le déplacement
                {
                    CmdFindNextPosition();
                }

                if (returnToInitPosition)
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

                if (na && na.enabled)
                {
                    currentTimePunch = 0;
                    na.SetDestination(nearestPosition);
                    float distanceToPlayer = (nearestPosition - transform.position).magnitude;

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

                            playersReachable.Remove(target);
                            na.SetDestination(initPosition);
                            returnToInitPosition = true;
                            target = null;
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
                    if (run)
                    {
                        CmdPlaySound(false);
                    }
                }


                wasWalkingBefore = run;
            }
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
        if (!returnToInitPosition) // Quand le zombie doit retourner a sa position, on ne veut pas qu'il continue de cherhcher des cibles pendant ce temps
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
            if (playersReachable == null)
                playersReachable = new List<GameObject>();

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

    [Command]
    public void CmdPlaySound(bool playHurt)
    {
        if (playHurt)
            RpcPlayHurtSound();
        else
            RpcPlaySound();
    }

    [ClientRpc]
    private void RpcPlaySound()
    {
        audioSource.clip = AttackSound;
        audioSource.Play();
    }

    [ClientRpc]
    private void RpcPlayHurtSound()
    {
        hurtAudioSource.clip = HurtSound;
        hurtAudioSource.Play();
    }

}
