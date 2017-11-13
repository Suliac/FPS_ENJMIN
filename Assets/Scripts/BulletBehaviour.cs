using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour {

	void OnCollisionEnter(Collision col)
    {
        var hit = col.gameObject;
        var lifeB = hit.GetComponent<LifeBehaviour>();

        if(lifeB != null) // If the bullet touch an object with a LifeBehaviour script, the bullet is destroyed after damaging the target
        {
            lifeB.TakeDamage(10);
            Destroy(gameObject);
        }
    }
}
