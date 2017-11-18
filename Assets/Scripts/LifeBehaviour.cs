using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LifeBehaviour : NetworkBehaviour
{

    public const int MaxHealth = 100;
    public const float RespawnTimeSeconds = 2.0f;
    public bool DestroyOnDeath;

    private GameObject startPosition;

    [SyncVar]
    public int Health = MaxHealth;

    void Awake()
    {
        startPosition = GameObject.Find("StartPosition");
    }

    public void TakeDamage(int amount)
    {
        if (!isServer)
            return;

        Health -= amount;
        if (Health <= 0)
        {
            Health = MaxHealth;

            if (DestroyOnDeath)
                Destroy(gameObject);
            else
                RpcRespawn();
        }
    }

    [ClientRpc]
    void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            transform.position = startPosition.transform.position;

            // Respawn the player after 'RespawnTime' seconds
            //StartCoroutine(Respawn(RespawnTimeSeconds));
        }
    }

    //IEnumerator Respawn(float timeRespawn)
    //{
    //    gameObject.SetActive(false);
    //    yield return new WaitForSeconds(timeRespawn);
    //    Debug.Log("Yo !");
    //    gameObject.SetActive(true);
    //    transform.position = Vector3.zero;
    //}
}
