using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    public Transform Bullet;
    public Transform Camera;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer)
            return; // If the player isn't the player of the current client, we don't update his position

        var x = Input.GetAxis("Horizontal") * 0.1f;
        var z = Input.GetAxis("Vertical") * 0.1f;

        transform.Translate(x, 0, z);

        if (Input.GetButton("Fire1"))
        {
            CmdFire();
        }

    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    // Command function is called from the client, but invoked on the server
    [Command]
    void CmdFire()
    {
        var bullet = Instantiate(Bullet, transform.position + transform.forward, Quaternion.identity);

        bullet.GetComponent<Rigidbody>().AddForce(Camera.transform.forward * 40, ForceMode.Impulse);

        NetworkServer.Spawn(bullet.gameObject);

        Destroy(bullet.gameObject, 2.0f); // destroy after 2 seconds
    }
}
