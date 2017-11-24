using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfoHandler : MonoBehaviour
{
    private static GameInfoHandler instance;

    public GameObject _PlayerUi;
    public static GameObject PlayerUi { get { return instance._PlayerUi; } }

    public GameObject _InfiniteAmmoImage;
    public static GameObject InfiniteAmmoImage { get { return instance._InfiniteAmmoImage; } }

    public bool _GameStarted = false;
    public static bool GameStarted { get { return instance._GameStarted; } set { instance._GameStarted = value; } }

    public int _NumberPickup = 0;
    public static int NumberPickup { get { return instance._NumberPickup; } }
    public static void DelPickup() { instance._NumberPickup--; }
    public static void AddPickup() { instance._NumberPickup++; }

    void Awake()
    {
        if (instance == null)
        {
            instance = gameObject.GetComponent<GameInfoHandler>();
            //DontDestroyOnLoad(gameObject);
        }
    }
}
