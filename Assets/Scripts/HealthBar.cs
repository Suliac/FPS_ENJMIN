using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{

    GUIStyle healthStyle;
    GUIStyle backStyle;
    LifeBehaviour lifeB;

    void Awake()
    {
        lifeB = GetComponent<LifeBehaviour>();
    }

    void OnGUI()
    {
        InitStyles();

        //////////////////  Draw a HealthBar
        Vector3 posBar = Camera.main.WorldToScreenPoint(transform.position);

        // Draw background
        GUI.color = Color.grey;
        GUI.backgroundColor = Color.grey;
        GUI.Box(new Rect(posBar.x - 26, Screen.height - posBar.y + 20, LifeBehaviour.MaxHealth / 2, 7), ".", backStyle);

        // draw 'real' health bar
        GUI.color = Color.green;
        GUI.backgroundColor = Color.green;
        GUI.Box(new Rect(posBar.x - 25, Screen.height - posBar.y + 21, Math.Max(0, lifeB.Health / 2), 5), ".", healthStyle);
    }

    void InitStyles()
    {
        if (healthStyle == null)
        {
            healthStyle = new GUIStyle(GUI.skin.box);
            healthStyle.normal.background = MakeTex(2, 2, new Color(0.0f, 1.0f, 0.0f, 1.0f));
        }

        if (backStyle == null)
        {
            backStyle = new GUIStyle(GUI.skin.box);
            backStyle.normal.background = MakeTex(2, 2, new Color(0.0f, 1.0f, 0.0f, 1.0f));
        }
    }

    private Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = color;
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply()
            ;
        return result;
    }
}
