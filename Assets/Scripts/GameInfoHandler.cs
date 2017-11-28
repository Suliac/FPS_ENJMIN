using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfoHandler : MonoBehaviour
{
    private static GameInfoHandler instance;

    public bool _GameStarted = false;
    public static bool GameStarted { get { return instance._GameStarted; } set { instance._GameStarted = value; } }

    public bool _GamePaused = false;
    public static bool GamePaused { get { return instance._GamePaused; } set { instance._GamePaused = value; } }

    public string _PlayerName;
    public static string PlayerName { get { return instance._PlayerName; } set { instance._PlayerName = value; } }

    #region Pickup
    public int _NumberPickup = 0;
    public static int NumberPickup { get { return instance._NumberPickup; } }
    public static void DelPickup() { instance._NumberPickup--; }
    public static void AddPickup() { instance._NumberPickup++; }
    #endregion
        
    #region UI
    public GameObject _PlayerUi;
    public static GameObject PlayerUi { get { return instance._PlayerUi; } }

    public GameObject _InfiniteAmmoImage;
    public static GameObject InfiniteAmmoImage { get { return instance._InfiniteAmmoImage; } }

    public GameObject _AmmoText;
    public static GameObject AmmoText { get { return instance._AmmoText; } }
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
