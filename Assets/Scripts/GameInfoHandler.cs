using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameInfoHandler : MonoBehaviour
{
    private static object _lock = new object();
    private static GameInfoHandler instance;

    public static void Init()
    {
        instance._WantToDisconnect = false;
        instance._ReadyToDisconnect = false;
        instance._GameOver = false;
        instance._GameStarted = false;
        instance._GamePaused = false;
        instance._Frags = new Dictionary<string, int>();
        instance._NbRdyGameOver = 0;
    }

    #region GameStates
    public bool _GameStarted = false;
    public static bool GameStarted { get { return instance._GameStarted; } set { instance._GameStarted = value; } }

    public bool _GamePaused = false;
    public static bool GamePaused { get { return instance._GamePaused; } set { instance._GamePaused = value; } }

    public bool _GameOver = false;
    public static bool GameOver { get { return instance._GameOver; } set { instance._GameOver = value; } }

    public bool _DisplayScores = false;
    public static bool DisplayScores { get { return instance._DisplayScores; } set { instance._DisplayScores = value; } }

    public bool _WantToDisconnect = false;
    public static bool WantToDisconnect { get { return instance._WantToDisconnect; } set { instance._WantToDisconnect = value; } }

    public bool _ReadyToDisconnect = false;
    public static bool ReadyToDisconnect { get { return instance._ReadyToDisconnect; } set { instance._ReadyToDisconnect = value; } }
    #endregion

    #region Names
    public string _WinnerName;
    public static string WinnerName { get { return instance._WinnerName; } set { instance._WinnerName = value; } }

    public string _PlayerName;
    public static string PlayerName { get { return instance._PlayerName; } set { instance._PlayerName = value; } } 
    #endregion

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

    public GameObject _GameOverText;
    public static GameObject GameOverText { get { return instance._GameOverText; } }

    public GameObject _RankingText;
    public static GameObject RankingText { get { return instance._RankingText; } }
    #endregion

    #region Frags
    public Dictionary<string, int> _Frags = new Dictionary<string, int>();
    public static Dictionary<string, int> Frags { get { return instance._Frags; } }
    public static void UpdateFrags(string playerName, int score)
    {
        if (instance._Frags.ContainsKey(playerName))
            instance._Frags[playerName] = score;
        else
            instance._Frags.Add(playerName, score);

        //instance._Frags.ToList().Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
    }

    public static void DeleteFrag(string playerName)
    {
        if (instance._Frags.ContainsKey(playerName))
            instance._Frags.Remove(playerName);

        //instance._Frags.ToList().Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
    }

    public static void InitFrags()
    {
        instance._Frags = new Dictionary<string, int>();
    }
    #endregion

    public int _NbRdyGameOver;
    public static int NbRdyGameOver { get { return instance._NbRdyGameOver; } }

    public static void NewClientRdyForGameOver()
    {
        lock(_lock)
        {
            instance._NbRdyGameOver++;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = gameObject.GetComponent<GameInfoHandler>();
            //DontDestroyOnLoad(gameObject);
        }
    }
}
