using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EvilController : MonoBehaviour
{

    private GameObject[] players;
    private List<GameObject> playersReachable;
    private bool run = false;

    public float UpdateIA = 0.0f;
    public const float UpdateIAPeriod = 0.3f;

    //public float AttackDistance = 1.5f;
    public float DetectDistance = 15.0f;

    public float AttackSpeed = 1.0f;
    public int Damages = 50;

    public Transform DeathEffect;
    // Use this for initialization
    void Start()
    {
        //players = GameObject.FindGameObjectsWithTag("Player");
        playersReachable = new List<GameObject>();

        UpdateIA = Random.value;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateIA += Time.deltaTime;

        if (UpdateIA > UpdateIAPeriod)
        {
            UpdateIA = 0.0f;

            // Check if there is players in the detection zone
            //foreach (var player in players)
            //{
            //    float distance = (player.transform.position - transform.position).magnitude;
            //    if (distance <= DetectDistance)
            //    {
            //        if (!playersReachable.Contains(player))
            //        {
            //            playersReachable.Add(player);
            //        }
            //    }
            //    else
            //    {
            //        if (playersReachable.Contains(player))
            //        {
            //            playersReachable.Remove(player);
            //        }
            //    }
            //}

            Vector3 nearestPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            float distance = DetectDistance;
            foreach (var player in playersReachable)
            {
                if ((player.transform.position - transform.position).magnitude < distance)
                {
                    nearestPosition = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
                }
            }

            NavMeshAgent na = GetComponent<NavMeshAgent>();
            if (na.enabled && transform.position != nearestPosition)
            {
                Debug.Log("set destination");
                    na.SetDestination(nearestPosition);
            }

            run = na.desiredVelocity.magnitude > 0.5f;
            //GetComponentInChildren<Animator>().SetBool("Walk", run);

            //float distanceToPlayer = (player.position - transform.position).magnitude;

            //if (distanceToPlayer < AttackDistance)
            //{
            //    // start punching
            //    transform.LookAt(player);
            //    GetComponentInChildren<Animator>().SetTrigger("Punch");
            //}

            if (!na.enabled) // Si il n'y a plus de NavMeshAgent
            {
                if (GetComponent<Rigidbody>().velocity.magnitude < 0.5f) // Si la magnitude de la vitesse < 0.5 = si immobile
                {
                    GetComponent<NavMeshAgent>().enabled = true;
                    GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        }


    }

    public void OnCollisionEnter(Collision col)
    {
        Debug.Log("collision");
        LifeBehaviour lifeB = col.transform.GetComponent<LifeBehaviour>();
        if (lifeB != null)
        {
            lifeB.TakeDamage(Damages, "zombie");

            //GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;

            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().AddForce(col.relativeVelocity / 3, ForceMode.Impulse);
        }
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            if (!playersReachable.Contains(col.gameObject))
            {
                Debug.Log("Player enter trigger");
                playersReachable.Add(col.gameObject);
            }
        }
    }

    public void OnTriggerLeave(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            if (playersReachable.Contains(col.gameObject))
            {
                Debug.Log("Player leave trigger");
                playersReachable.Remove(col.gameObject);
            }
        }
    }
}
