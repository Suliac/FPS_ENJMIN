using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour {

    public int WeaponId;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerController controller = col.gameObject.GetComponent<PlayerController>();

            if (controller != null)
            {
                controller.NewWeapon(WeaponId);
            }
        }
    }
}
