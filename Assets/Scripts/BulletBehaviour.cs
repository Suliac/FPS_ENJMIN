using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    private bool collision = false;
    private int damages = 10;
    private string playerName;
    void OnCollisionEnter(Collision col)
    {
        var hit = col.gameObject;
        var lifeB = hit.GetComponent<LifeBehaviour>();

        if (lifeB != null && !collision) // If the bullet touch an object with a LifeBehaviour script, the bullet is destroyed after damaging the target
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if ((player && player.PlayerId != playerName) || !player) // on vérifie que le joueurs ne s'attaque pas lui meme (si bug latence)
            {
                //print("Hit !");
                collision = true;
                lifeB.TakeDamage(damages, playerName);

                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void Init(int damage, string shootingPlayerName)
    {
        damages = damage;
        playerName = shootingPlayerName;
    }
}
