using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfoHandler : MonoBehaviour
{
    private static GameInfoHandler instance;

    public bool _GameStarted = false;
    public static bool GameStarted { get { return instance._GameStarted; } set { instance._GameStarted = value; } }

    #region Pickup
    public int _NumberPickup = 0;
    public static int NumberPickup { get { return instance._NumberPickup; } }
    public static void DelPickup() { instance._NumberPickup--; }
    public static void AddPickup() { instance._NumberPickup++; }
    #endregion

    #region Frags
    public static Dictionary<string, int> fragPerPlayer = new Dictionary<string, int>();

    public void NewPlayer(string playerId)
    {
        if (!fragPerPlayer.ContainsKey(playerId))
            fragPerPlayer.Add(playerId, 0);
    }

    public void NewFrag(string playerScoringId)
    {
        if (fragPerPlayer.ContainsKey(playerScoringId))
            fragPerPlayer[playerScoringId]++;
    }
    #endregion

    #region UI
    public GameObject _PlayerUi;
    public static GameObject PlayerUi { get { return instance._PlayerUi; } }

    public GameObject _InfiniteAmmoImage;
    public static GameObject InfiniteAmmoImage { get { return instance._InfiniteAmmoImage; } }
    #endregion

    void Awake()
    {
        if (instance == null)
        {
            instance = gameObject.GetComponent<GameInfoHandler>();
            //DontDestroyOnLoad(gameObject);
        }
    }
}
