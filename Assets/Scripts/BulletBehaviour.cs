using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour {
    private int count = 0;

    private int damages = 10;
	void OnCollisionEnter(Collision col)
    {
        var hit = col.gameObject;
        var lifeB = hit.GetComponent<LifeBehaviour>();

        if(lifeB != null && count == 0) // If the bullet touch an object with a LifeBehaviour script, the bullet is destroyed after damaging the target
        {
            count++;
            //Debug.Log("Hit ! "+count);
            lifeB.TakeDamage(damages);
            Destroy(gameObject);
        }

        if (count > 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamage(int damage)
    {
        damages = damage;
    }
}
