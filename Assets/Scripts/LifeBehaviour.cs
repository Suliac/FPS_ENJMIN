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
    //private PlayerController playerController;

    [SyncVar]
    public int Health = MaxHealth;

    void Awake()
    {
        startPosition = GameObject.Find("StartPosition");
        //playerController = gameObject.GetComponent<PlayerController>();
    }

    public void TakeDamage(int amount, string shootingPlayerName)
    {
        if (!isServer)
            return;

        Health -= amount;
        if (Health <= 0)
        {
            //Frag
            InGameManager.NewFrag(shootingPlayerName);

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
