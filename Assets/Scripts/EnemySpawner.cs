using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour {

    public GameObject Enemy;
    public int NumberEnemies;

    public override void OnStartServer()
    {
        for (int i = 0; i < NumberEnemies; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-8.0f, 8.0f), 0.2f, Random.Range(-8.0f, 8.0f));

            Quaternion rotation = Quaternion.Euler(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180));

            GameObject enemy = Instantiate(Enemy, pos, rotation);
            NetworkServer.Spawn(enemy);
        }
    }
}
