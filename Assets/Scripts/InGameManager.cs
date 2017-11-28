using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class InGameManager : NetworkBehaviour {
    public int FragToWin = 20;
    public static Dictionary<string, int> fragPerPlayer = new Dictionary<string, int>();
    public static List<PlayerController> toNotify;

	// Use this for initialization
	void Start () {
        toNotify = new List<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!isServer)
            return;

        if (fragPerPlayer.Any(pair => pair.Value > FragToWin))
        {
            string winner = fragPerPlayer.FirstOrDefault(pair => pair.Value > FragToWin).Key;
            Debug.Log("Winner :" + winner);
        }
	}


    public static void NewPlayer(string playerId)
    {
        if (!fragPerPlayer.ContainsKey(playerId))
            fragPerPlayer.Add(playerId, 0);
    }

    public static void NewFrag(string playerScoringId)
    {
        Debug.Log("New frag for : " + playerScoringId);
        if (fragPerPlayer.ContainsKey(playerScoringId))
            fragPerPlayer[playerScoringId]++;

        NotifyAll();
    }

    public static void SubscribeToScoreUpdates(PlayerController controller)
    {
        toNotify.Add(controller);
    }

    public static void NotifyAll()
    {
        foreach (var playerController in toNotify)
        {
            playerController.CmdUpdateScores();
        }
    }
    
}
