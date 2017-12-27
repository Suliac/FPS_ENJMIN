using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameBar : MonoBehaviour {

    public Camera PlayerCamera;
    PlayerController player;

    void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    void OnGUI()
    {
        Vector3 posBar = PlayerCamera.WorldToScreenPoint(transform.position);

        GUI.color = Color.blue;
        //GUI.backgroundColor = new Color(0, 0, 0, 0);
        GUI.TextArea(new Rect(posBar.x - 25, Screen.height - posBar.y - 100, 30, 5), "coucou");
    }
}
