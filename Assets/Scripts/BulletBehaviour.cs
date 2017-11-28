using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    private bool collision = false;
    private int damages = 10;
    void OnCollisionEnter(Collision col)
    {
        var hit = col.gameObject;
        var lifeB = hit.GetComponent<LifeBehaviour>();

        if (lifeB != null && !collision) // If the bullet touch an object with a LifeBehaviour script, the bullet is destroyed after damaging the target
        {
            collision = true;
            lifeB.TakeDamage(damages);
        }

        Destroy(gameObject);
    }

    public void SetDamage(int damage)
    {
        damages = damage;
    }
}
