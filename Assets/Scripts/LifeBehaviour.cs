using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LifeBehaviour : NetworkBehaviour
{
    public bool isZombie = false;
    public const int MaxHealth = 100;
    public const float RespawnTimeSeconds = 2.0f;
    public bool DestroyOnDeath;

    private Animator animator;
    private GameObject startPosition;

    [SyncVar]
    private bool dying = false;
    public bool Dying { get { return dying; } }
    //private PlayerController playerController;

    [SyncVar]
    public int Health = MaxHealth;

    void Awake()
    {
        startPosition = GameObject.Find("StartPosition");
        animator = GetComponentInChildren<Animator>();
        //playerController = gameObject.GetComponent<PlayerController>();
    }

    public void Suicide(string playerId)
    {
        if (!isServer)
            return;

        if (!dying)
        {
            dying = true;
            Health = 0;
            if (Health <= 0)
            {
                if (!isZombie)
                {
                    InGameManager.RemoveFrag(playerId);
                }

                Health = MaxHealth;
                if (animator)
                    animator.SetTrigger("Death");
                else
                    Death();
            }
        }
    }

    public void TakeDamage(int amount, string shootingPlayerName)
    {
        if (!isServer)
            return;

        if (!dying)
        {
            Health -= amount;
            if (Health <= 0)
            {
                dying = true;
                //Frag
                if (!isZombie && !shootingPlayerName.Equals("zombie"))
                {
                    InGameManager.NewFrag(shootingPlayerName);
                }

                if (animator)
                    RpcDeathAnim();
                else
                    Death();
            } 
        }
    }

    public void AddLife(int amount)
    {
        if (!isServer)
            return;

        Health += amount;

    }

    [ClientRpc]
    void RpcDeathAnim()
    {
        animator.SetTrigger("Death");
    }

    [ClientRpc]
    void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            transform.position = startPosition.transform.position;
            //PlayerController controller = GetComponent<PlayerController>();
            //if(controller)
            //{
            //    controller.InitWeapon();
            //}
        }
    }

    public void OnEndDeathAnim()
    {
        print("OnEndDeathAnim");

        if (!isServer)
            return;

        Death();
    }

    private void Death()
    {
        print("death");
        Health = MaxHealth;
        dying = false;
        if (DestroyOnDeath)
            Destroy(gameObject);
        else
            RpcRespawn();
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
