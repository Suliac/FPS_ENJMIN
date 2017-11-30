using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class InGameManager : NetworkBehaviour
{
    private static object mutex = new object();
    public int FragToWin = 20;
    public static Dictionary<string, int> fragPerPlayer = new Dictionary<string, int>();
    public static Dictionary<string, PlayerController> toNotify; // TODO : créer une classe générique et la faire hériter aux classe pouvant s'abonner
    public float TimeBetweenTest = 1.0f;
    private float dtTestPlayerDisconnect = 0.0f;
    // Use this for initialization
    void Start()
    {
        toNotify = new Dictionary<string, PlayerController>();
        fragPerPlayer = new Dictionary<string, int>();
        dtTestPlayerDisconnect = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) // Only the server has the right to detect gameover
            return;

        if (fragPerPlayer.Any(pair => pair.Value >= FragToWin))
        {
            string winner = fragPerPlayer.FirstOrDefault(pair => pair.Value >= FragToWin).Key;
            GameInfoHandler.WinnerName = winner;
            GameInfoHandler.GameOver = true;

            NotifyAllGameOver();
        }

        if (GameInfoHandler.NbRdyGameOver >= (toNotify.Count - 1))
            GameInfoHandler.ReadyToDisconnect = true; // we need to wait until all the clients are notified of the gameover

        dtTestPlayerDisconnect += Time.deltaTime;
        if (dtTestPlayerDisconnect >= TimeBetweenTest)
        {
            var playerToNotify = new Dictionary<string, PlayerController>(toNotify);
            foreach (var player in playerToNotify)
            {
                if (player.Value == null)
                {
                    Unsubscribe(player.Key);
                    QuitPlayer(player.Key);
                }
            }

            dtTestPlayerDisconnect -= TimeBetweenTest;
        }

    }

    public static void Init()
    {
        toNotify = new Dictionary<string, PlayerController>();
        fragPerPlayer = new Dictionary<string, int>();
    }

    public static bool IsExistingPlayer(string playerName)
    {
        return fragPerPlayer.ContainsKey(playerName);
    }

    public static void NewPlayer(string playerId)
    {
        if (!fragPerPlayer.ContainsKey(playerId))
            fragPerPlayer.Add(playerId, 0);

        NotifyAllNewScore();
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

        NotifyAllNewScore();
    }

    public static void Subscribe(string playerName, PlayerController controller)
    {
        lock (mutex)
            toNotify.Add(playerName, controller);
    }

    public static void Unsubscribe(string playerName)
    {
        lock (mutex)
            toNotify.Remove(playerName);
        //Debug.Log("Unsub, count now = " + toNotify.Count);
    }

    static void NotifyAllNewScore()
    {
        var playerToNotify = new Dictionary<string, PlayerController>(toNotify);
        foreach (var playerController in playerToNotify)
        {
            if (playerController.Value != null)
                playerController.Value.CmdUpdateScores();
        }
    }

    static void NotifyDisconnect(string playerName)
    {
        var playerToNotify = new Dictionary<string, PlayerController>(toNotify);
        foreach (var playerController in playerToNotify)
        {
            if (playerController.Value != null)
                playerController.Value.CmdDeleteScore(playerName);
        }
    }

    static void NotifyAllGameOver()
    {
        var playerToNotify = new Dictionary<string, PlayerController>(toNotify);
        foreach (var playerController in playerToNotify)
        {
            if (playerController.Value != null)
                playerController.Value.CmdSetGameOver(GameInfoHandler.WinnerName);
        }

    }
}
