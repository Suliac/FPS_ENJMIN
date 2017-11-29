﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class InGameManager : NetworkBehaviour {
    public int FragToWin = 20;
    public static Dictionary<string, int> fragPerPlayer = new Dictionary<string, int>();
    public static List<PlayerController> toNotify; // TODO : créer une classe générique et la faire hériter aux classe pouvant s'abonner

	// Use this for initialization
	void Start () {
        toNotify = new List<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!isServer) // Only the server has the right to detect gameover
            return;

        if (fragPerPlayer.Any(pair => pair.Value >= FragToWin))
        {
            string winner = fragPerPlayer.FirstOrDefault(pair => pair.Value >= FragToWin).Key;
            GameInfoHandler.WinnerName = winner;
            GameInfoHandler.GameOver = true;
        }
	}


    public static void NewPlayer(string playerId)
    {
        if (!fragPerPlayer.ContainsKey(playerId))
            fragPerPlayer.Add(playerId, 0);

        NotifyAll();
    }

    public static void QuitPlayer(string playerId)
    {
        if (fragPerPlayer.ContainsKey(playerId))
            fragPerPlayer.Remove(playerId);
        

        //Debug.Log("'" + playerId + "' disconnected, frag count line :"+fragPerPlayer.Count);
        NotifyDisconnect(playerId);
    }

    public static void NewFrag(string playerScoringId)
    {
        //Debug.Log("New frag for : " + playerScoringId);
        if (fragPerPlayer.ContainsKey(playerScoringId))
            fragPerPlayer[playerScoringId]++;

        NotifyAll();
    }

    public static void SubscribeToScoreUpdates(PlayerController controller)
    {
        toNotify.Add(controller);
    }

    public static void UnsubscribeToScoreUpdates(PlayerController controller)
    {
        toNotify.Remove(controller);
        //Debug.Log("Unsub, count now = " + toNotify.Count);
    }

    static void NotifyAll()
    {
        foreach (var playerController in toNotify)
        {
            playerController.CmdUpdateScores();
        }
    }

    static void NotifyDisconnect(string playerName)
    {
        foreach (var playerController in toNotify)
        {
            playerController.CmdDeleteScore(playerName);
        }
    }
    
}