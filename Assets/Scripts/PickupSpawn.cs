using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PickupSpawn : NetworkBehaviour {

    public Transform Pickup;
    public const float WaterY = -2.0f;
    public const int MaxNumberContainer = 10;
    public float TimePickupFalling = 5.0f;

    //private int numberContainer = 0;
    private const float maxDistanceRayCast = 300.0f;
    private const float topY = 200.0f;

    private float delta = 0.0f;
    public float TimeBetweenSpawn = 30.0f;

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(GameInfoHandler.GameStarted);
        if (GameInfoHandler.GameStarted)
        {
            //Debug.Log("FOO");
            if (!isServer) // we want that only the server manage this kind of stuff
                return;

            delta += Time.deltaTime;
            if (delta >= TimeBetweenSpawn && GameInfoHandler.NumberPickup < MaxNumberContainer)
            {
                bool foundGoodPos = false;

                float randomX = 0.0f;
                float randomZ = 0.0f;
                float finalY = 0.0f;
                
                Vector3 newPickupPos = new Vector3(randomX, topY, randomZ);

                while (!foundGoodPos)
                {
                    randomZ = UnityEngine.Random.Range(-150.0f, 150.0f);
                    randomX = UnityEngine.Random.Range(-150.0f, 150.0f);
                    newPickupPos = new Vector3(randomX, topY, randomZ);

                    RaycastHit hit;

                    if (Physics.Raycast(newPickupPos, -Vector3.up, out hit, maxDistanceRayCast))
                    {
                        if (hit.collider.name == "Terrain") // Pas sur que ca soit très propre
                        {
                           finalY = topY - hit.distance;

                            if (finalY > WaterY)
                                foundGoodPos = true; 
                        }
                    }
                }

                Vector3 startPos = newPickupPos;
                Vector3 endPos = new Vector3(startPos.x, finalY, startPos.z);
                
                Transform newPickup = Instantiate(Pickup, startPos, Quaternion.identity); 

                // We need an id to find the pickups on all clients
                WeaponPickup pickupScript = newPickup.GetComponent<WeaponPickup>();
                NetworkServer.Spawn(newPickup.gameObject);
                pickupScript.RpcSetEndPosition(endPos);
                
                //pickupScript.RpcPickupFalling(startPos, endPos, TimePickupFalling);
                
                GameInfoHandler.AddPickup();
                delta -= TimeBetweenSpawn;
            } 
        }
    }

    
}
