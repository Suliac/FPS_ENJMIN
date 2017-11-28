using System.Collections;
using System.Collections.Generic;
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

    private bool _started;

    public void Start()
    {
        _started = false;
    }
    public void OnGUI()
    {
        //GUILayout.Space(GuiOffset);
        if (!_started)
        {
            if (GUI.Button(new Rect(Screen.width / 2 - ButtonWidth, (Screen.height - ButtonHeight) / 2, ButtonWidth, ButtonHeight), "Host"))
            {
                _started = true;
                NetworkManager.singleton.networkPort = int.Parse(Port);
                NetworkManager.singleton.StartHost();
                GameInfoHandler.PlayerName = PlayerName;
            }

            IpAddress = GUI.TextField(new Rect((Screen.width - TextBoxWidth) / 2, Screen.height / 2 - 1.5f * TextBoxHeight, TextBoxWidth, TextBoxHeight), IpAddress);
            Port = GUI.TextField(new Rect((Screen.width - TextBoxWidth) / 2, Screen.height / 2 - 2.5f * TextBoxHeight, TextBoxWidth, TextBoxHeight), Port, 5);
            PlayerName = GUI.TextField(new Rect((Screen.width - TextBoxWidth) / 2, Screen.height / 2 - 3.5f * TextBoxHeight, TextBoxWidth, TextBoxHeight), PlayerName, 25);

            if (GUI.Button(new Rect(Screen.width / 2, (Screen.height - ButtonHeight) / 2, ButtonWidth, ButtonHeight), "Join"))
            {
                _started = true;
                NetworkManager.singleton.networkAddress = IpAddress;
                NetworkManager.singleton.networkPort = int.Parse(Port);

                NetworkManager.singleton.StartClient();
                GameInfoHandler.PlayerName = PlayerName;
            }
        }
        else
        {
            GUI.color = Color.red;

            if (!NetworkManager.singleton.isNetworkActive)
                GUI.Label(new Rect((Screen.width - LabelWidth) / 2, Screen.height / 2 + 1.5f * LabelHeight, LabelWidth, LabelHeight), "Impossible to reach the server "+IpAddress+":"+Port);


            GUI.color = Color.white;
            if (GameInfoHandler.GamePaused)
            {
                if (GUI.Button(new Rect(Screen.width / 2, (Screen.height - ButtonHeight) / 2, ButtonWidth, ButtonHeight), "Disconnect"))
                {
                    _started = false;
                    GameInfoHandler.AmmoText.SetActive(true);
                    GameInfoHandler.InfiniteAmmoImage.SetActive(true);
                    GameInfoHandler.PlayerUi.SetActive(false);

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    NetworkManager.singleton.StopHost();
                }

                if (GUI.Button(new Rect(Screen.width / 2 - ButtonWidth, (Screen.height - ButtonHeight) / 2, ButtonWidth, ButtonHeight), "Resume"))
                {
                    GameInfoHandler.GamePaused = false;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    void OnClientError(NetworkConnection conn, int errorCode)
    {
        Debug.Log(conn.lastError.ToString());
    }

    void OnServerError(NetworkConnection conn, int errorCode)
    {
        Debug.Log(conn.lastError.ToString());
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        Debug.Log("Could not connect to server: " + error);
    }
}
