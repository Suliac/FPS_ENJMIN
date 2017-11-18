using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateHandler : MonoBehaviour
{

    public enum GameState
    {
        Menu,
        Lobby,
        Game,
        Lose,
        Victory
    }

    private static GameStateHandler instance;

    public GameState _state = GameState.Lobby;
    public static GameState state
    {
        get { return instance._state; }
        set { instance._state = value; }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
}
