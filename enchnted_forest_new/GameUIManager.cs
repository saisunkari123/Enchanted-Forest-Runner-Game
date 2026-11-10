using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sylvan Rush - In-Game UI Manager
/// Renders crisp Score, Distance, Orbs, Buffs, and Game Over Screen automatically via OnGUI.
/// Works out-of-the-box in any Unity project without requiring Canvas or TextMeshPro setup!
/// </summary>
public class GameUIManager : MonoBehaviour
{
    private GUIStyle headerStyle;
    private GUIStyle subStyle;
    private GUIStyle scoreStyle;
    private GUIStyle buttonStyle;
    private GUIStyle cardStyle;

    void OnGUI()
    {
        InitStyles();

        if (GameManager.Instance == null) return;

        if (GameManager.Instance.currentState == GameState.Playing)
        {
            DrawGameplayHUD();
        }
        else if (GameManager.Instance.currentState == GameState.GameOver)
        {
            DrawGameOverScreen();
        }
    }

    void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = new Color(0f, 0.96f, 0.83f); // Cyan
        }

        if (subStyle == null)
        {
            subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft
            };
            subStyle.normal.textColor = new Color(0.6f, 0.8f, 0.75f);
        }

        if (scoreStyle == null)
        {
            scoreStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            scoreStyle.normal.textColor = Color.white;
        }

        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            buttonStyle.normal.textColor = new Color(0.02f, 0.08f, 0.1f);
        }

        if (cardStyle == null)
        {
            cardStyle = new GUIStyle(GUI.skin.box);
        }
    }

    void DrawGameplayHUD()
    {
        // Top-Left Stats Box
        Rect statsRect = new Rect(20, 20, 310, 65);
        GUI.Box(statsRect, GUIContent.none, cardStyle);

        GUILayout.BeginArea(statsRect);
        GUILayout.BeginHorizontal();

        // Score
        GUILayout.BeginVertical();
        GUILayout.Label("SCORE", subStyle);
        GUILayout.Label(GameManager.Instance.score.ToString("N0"), scoreStyle);
        GUILayout.EndVertical();

        // Distance
        GUILayout.BeginVertical();
        GUILayout.Label("DISTANCE", subStyle);
        GUILayout.Label(Mathf.FloorToInt(GameManager.Instance.distance) + "m", scoreStyle);
        GUILayout.EndVertical();

        // Combo Multiplier
        GUILayout.BeginVertical();
        GUILayout.Label("COMBO", subStyle);
        GUILayout.Label(GameManager.Instance.comboMultiplier + "x", scoreStyle);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // Top-Right Orbs Box
        Rect orbsRect = new Rect(Screen.width - 140, 20, 120, 45);
        GUI.Box(orbsRect, GUIContent.none, cardStyle);
        GUI.Label(new Rect(Screen.width - 130, 28, 100, 30), "💎 " + GameManager.Instance.orbsCollected, scoreStyle);

        // Active Buff Indicators
        int buffY = 95;
        if (PowerUpManager.Instance != null)
        {
            if (PlayerController.Instance != null && PlayerController.Instance.hasShield)
            {
                DrawBuffBadge(new Rect(20, buffY, 150, 26), "🛡️ BARK SHIELD", new Color(0.22f, 0.94f, 0.49f));
                buffY += 32;
            }
            if (PlayerController.Instance != null && PlayerController.Instance.hasDash)
            {
                DrawBuffBadge(new Rect(20, buffY, 150, 26), "⚡ WIND DASH", new Color(0f, 0.96f, 0.83f));
                buffY += 32;
            }
            if (PlayerController.Instance != null && PlayerController.Instance.hasMagnet)
            {
                DrawBuffBadge(new Rect(20, buffY, 150, 26), "🧲 SPIRIT MAGNET", new Color(0.78f, 0.49f, 1f));
                buffY += 32;
            }
        }

        // Bottom Controls Hint
        GUI.Label(new Rect(20, Screen.height - 35, 450, 25), "A/D: Move Lanes | W / Space: Jump", subStyle);
    }

    void DrawBuffBadge(Rect rect, string label, Color col)
    {
        Color oldColor = GUI.color;
        GUI.color = col;
        GUI.Box(rect, label, cardStyle);
        GUI.color = oldColor;
    }

    void DrawGameOverScreen()
    {
        // Dark Dim Background
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);

        float panelWidth = 360;
        float panelHeight = 280;
        Rect panelRect = new Rect((Screen.width - panelWidth) * 0.5f, (Screen.height - panelHeight) * 0.5f, panelWidth, panelHeight);
        GUI.Box(panelRect, GUIContent.none, cardStyle);

        GUILayout.BeginArea(panelRect);
        GUILayout.Space(20);
        GUILayout.Label("RUN COMPLETE", headerStyle);
        GUILayout.Space(15);

        GUILayout.Label("Final Score: " + GameManager.Instance.score.ToString("N0"), scoreStyle);
        GUILayout.Label("Distance Run: " + Mathf.FloorToInt(GameManager.Instance.distance) + " meters", subStyle);
        GUILayout.Label("Mana Orbs Gathered: " + GameManager.Instance.orbsCollected, subStyle);
        GUILayout.Space(10);
        GUILayout.Label("Personal Best Score: " + GameManager.Instance.GetPersonalBestScore().ToString("N0"), subStyle);
        GUILayout.Label("Personal Best Distance: " + Mathf.FloorToInt(GameManager.Instance.GetPersonalBestDistance()) + "m", subStyle);

        GUILayout.Space(25);
        if (GUILayout.Button("PLAY AGAIN", buttonStyle, GUILayout.Height(42)))
        {
            GameManager.Instance.StartNewRun();
        }
        GUILayout.EndArea();
    }
}
