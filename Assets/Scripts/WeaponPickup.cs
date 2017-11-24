using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WeaponPickup : NetworkBehaviour
{
    public int WeaponId;
    public float fallingSpeed = 10.0f;
    public Vector3 endPosition;

    private bool used = false;
    private bool goodPos = false;
    
    void FixedUpdate()
    {
        if (transform.position.y > endPosition.y)
            transform.Translate(-transform.up * fallingSpeed * Time.fixedDeltaTime);
        else if (!goodPos)
        {
            transform.position = endPosition;
            goodPos = true; // avoid to update at each frame
        }
    }

    [ClientRpc]
    public void RpcSetEndPosition(Vector3 endPos)
    {
        endPosition = endPos;
    }

    void OnTriggerEnter(Collider col)
    {
        if (!used)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                PlayerController controller = col.gameObject.GetComponent<PlayerController>();

                if (controller != null)
                {
                    controller.NewWeapon(WeaponId);
                    GameInfoHandler.DelPickup();
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // We need to do the coroutine on each client otherwise, the pickup seems to "stutter"
    //[ClientRpc]
    //public void RpcPickupFalling(Vector3 startPosition, Vector3 endPosition, float timePickupFalling)
    //{
    //    StartCoroutine(SmoothFall(startPosition, endPosition, timePickupFalling));
    //}

    //public IEnumerator SmoothFall(Vector3 startPosition, Vector3 endPosition, float time = 1.0f)
    //{
    //    float myTime = 0.0f;

    //    while (myTime < time)
    //    {
    //        myTime += Time.deltaTime;
    //        float partOfTotalTime = myTime / time;
    //        transform.position = Vector3.Lerp(startPosition, endPosition, Mathf.Min(1, partOfTotalTime));
    //        yield return null;
    //    }
    //}
}
