using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfoHandler : MonoBehaviour {

    private static GameInfoHandler instance;

    public GameObject _PlayerUi;
    public static GameObject PlayerUi { get { return instance._PlayerUi; } }

    public GameObject _InfiniteAmmoImage;
    public static GameObject InfiniteAmmoImage { get { return instance._InfiniteAmmoImage; } }

    void Awake()
    {
        if (instance == null)
        {
            instance = gameObject.GetComponent<GameInfoHandler>();
            //DontDestroyOnLoad(gameObject);
        }
    }
}
