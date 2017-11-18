using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInit : MonoBehaviour {
    
	void Start () {
        Debug.Log("start");
        GameInfoHandler.PlayerUi.SetActive(false);
        GameInfoHandler.InfiniteAmmoImage.SetActive(false);
	}
	
}
