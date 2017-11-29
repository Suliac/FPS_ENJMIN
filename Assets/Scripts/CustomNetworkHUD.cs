using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkHUD : MonoBehaviour
{

    public string IpAddress;
    public string Port;
    public string PlayerName;

    public int ButtonWidth = 100;
    public int ButtonHeight = 30;
    public int TextBoxWidth = 200;
    public int TextBoxHeight = 25;
    public int LabelWidth = 275;
    public int LabelHeight = 25;

    public int MaxPlayerScoreDisplay = 5;

    private bool _started;
    private GUIStyle labelStyle;

    public void Start()
    {
        _started = false;
    }
    public void OnGUI()
    {
        InitStyle();
        //GUILayout.Space(GuiOffset);
        if (!_started)
        {
            GameInfoHandler.WantToDisconnect = false;
            GameInfoHandler.ReadyToDisconnect = false;
            GameInfoHandler.GameOver = false;
            GameInfoHandler.GameStarted = false;
            GameInfoHandler.GamePaused = false;

            HostButton();

            IpAddress = GUI.TextField(new Rect((Screen.width - TextBoxWidth) / 2, Screen.height / 2 - 1.5f * TextBoxHeight, TextBoxWidth, TextBoxHeight), IpAddress);
            Port = GUI.TextField(new Rect((Screen.width - TextBoxWidth) / 2, Screen.height / 2 - 2.5f * TextBoxHeight, TextBoxWidth, TextBoxHeight), Port, 5);
            PlayerName = GUI.TextField(new Rect((Screen.width - TextBoxWidth) / 2, Screen.height / 2 - 3.5f * TextBoxHeight, TextBoxWidth, TextBoxHeight), PlayerName, 25);

            JoinButton();
        }
        else
        {
            GUI.color = Color.red;

            if (!NetworkManager.singleton.isNetworkActive)
                GUI.Label(new Rect((Screen.width - LabelWidth) / 2, Screen.height / 2 + 1.5f * LabelHeight, LabelWidth, LabelHeight), "Impossible to reach the server " + IpAddress + ":" + Port);


            GUI.color = Color.white;

            if (GameInfoHandler.GameOver)
            {
                DisconnectButton();
            }
            else if (GameInfoHandler.GamePaused)
            {
                DisconnectButton();
                ResumeButton();
            }

            if (GameInfoHandler.DisplayScores)
                LadderBoard();

            if (GameInfoHandler.WantToDisconnect && GameInfoHandler.ReadyToDisconnect)
            {
                _started = false;
                NetworkManager.singleton.StopHost();
            }
        }
    }

    void HostButton()
    {
        if (GUI.Button(new Rect(Screen.width / 2 - ButtonWidth, (Screen.height - ButtonHeight) / 2, ButtonWidth, ButtonHeight), "Host"))
        {
            _started = true;
            NetworkManager.singleton.networkPort = int.Parse(Port);
            NetworkManager.singleton.StartHost();
            GameInfoHandler.InitFrags();
            GameInfoHandler.PlayerName = PlayerName;
        }
    }

    void JoinButton()
    {
        if (GUI.Button(new Rect(Screen.width / 2, (Screen.height - ButtonHeight) / 2, ButtonWidth, ButtonHeight), "Join"))
        {
            _started = true;
            NetworkManager.singleton.networkAddress = IpAddress;
            NetworkManager.singleton.networkPort = int.Parse(Port);

            NetworkManager.singleton.StartClient();
            GameInfoHandler.InitFrags();
            GameInfoHandler.PlayerName = PlayerName;
        }
    }

    void DisconnectButton()
    {
        if (GUI.Button(new Rect(Screen.width / 2, (Screen.height - ButtonHeight) / 2, ButtonWidth, ButtonHeight), "Disconnect"))
        {
            
            GameInfoHandler.GameOver = false;
            GameInfoHandler.GameStarted = false;
            GameInfoHandler.GamePaused = false;

            GameInfoHandler.AmmoText.SetActive(true);
            GameInfoHandler.InfiniteAmmoImage.SetActive(true);
            GameInfoHandler.PlayerUi.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var allPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in allPlayers)
            {
                PlayerController controller = player.GetComponent<PlayerController>();
                if (controller != null && controller.isLocalPlayer)
                {
                    controller.Disconnect();
                }
            }

            GameInfoHandler.WantToDisconnect = true;
        }
    }

    void ResumeButton()
    {
        if (GUI.Button(new Rect(Screen.width / 2 - ButtonWidth, (Screen.height - ButtonHeight) / 2, ButtonWidth, ButtonHeight), "Resume"))
        {
            GameInfoHandler.GamePaused = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void LadderBoard()
    {
        var frags = GameInfoHandler.Frags; // TODO : Voir pour pas appeler ca tout le temps | idée : notifier comme player controller ?
        float size = Mathf.Min((float)frags.Count, (float)MaxPlayerScoreDisplay);
        
        //float posY = Screen.height / 2.0f - (size / 2.0f - rank) * LabelHeight;
        float posY = Screen.height / 2.0f - (LabelHeight * (size + 1)) / 2;
        float posX = Screen.width / 2.0f - LabelWidth / 2;

        string scores = string.Join("\n", frags.Take(5).Select(pair => pair.Key + " : " + pair.Value.ToString() + " kill(s)").ToArray());
        GUI.Label(new Rect(posX, posY, LabelWidth, LabelHeight * size), scores, labelStyle);
    }

    void InitStyle()
    {
        labelStyle = new GUIStyle(GUI.skin.box);

        //Color[] pix = new Color[4];

        //for (int i = 0; i < pix.Length; i++)
        //    pix[i] = Color.black;

        //Texture2D result = new Texture2D(2, 2);
        //result.SetPixels(pix);
        //result.Apply();

        //labelStyle.normal.background = result;
    }
}
