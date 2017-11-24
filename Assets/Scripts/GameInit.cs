using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInit : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameInfoHandler.PlayerUi.SetActive(false); // We don't want our UI to appear when we are in the lobby screen
	}
	
	
}
