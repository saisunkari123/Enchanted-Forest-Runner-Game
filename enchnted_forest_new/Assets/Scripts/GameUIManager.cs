using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sylvan Rush - In-Game UI Manager (Pre-Game Intro Card & Screen-Recording Ready)
/// Renders crisp Pre-Game Title Card, Gameplay HUD, Touch/D-Pad Action Buttons,
/// Pause Screen, and Game Over Screen automatically via native OnGUI.
/// </summary>
public class GameUIManager : MonoBehaviour
{
    private GUIStyle headerStyle;
    private GUIStyle titleHeaderStyle;
    private GUIStyle titleSubStyle;
    private GUIStyle sectionHeaderStyle;
    private GUIStyle infoTextStyle;
    private GUIStyle subStyle;
    private GUIStyle scoreStyle;
    private GUIStyle buttonStyle;
    private GUIStyle playButtonStyle;
    private GUIStyle cardStyle;
    private GUIStyle dpadButtonStyle;

    private Texture2D cardBgTex;
    private Texture2D barTex;
    private Texture2D playBtnTex;

    void OnGUI()
    {
        InitStyles();

        if (GameManager.Instance == null) return;

        if (GameManager.Instance.currentState == GameState.Title)
        {
            DrawTitleScreen();
        }
        else if (GameManager.Instance.currentState == GameState.Playing)
        {
            DrawGameplayHUD();
            DrawOnScreenControls();
        }
        else if (GameManager.Instance.currentState == GameState.Paused)
        {
            DrawPauseScreen();
        }
        else if (GameManager.Instance.currentState == GameState.GameOver)
        {
            DrawGameOverScreen();
        }
    }

    void InitStyles()
    {
        if (cardBgTex == null)
        {
            cardBgTex = MakeTex(2, 2, new Color(0.04f, 0.08f, 0.12f, 0.94f));
        }

        if (barTex == null)
        {
            barTex = MakeTex(2, 2, new Color(0f, 0.96f, 0.83f, 0.9f));
        }

        if (playBtnTex == null)
        {
            playBtnTex = MakeTex(2, 2, new Color(0f, 0.88f, 0.74f, 1f));
        }

        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = new Color(0f, 0.96f, 0.83f);
        }

        if (titleHeaderStyle == null)
        {
            titleHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleHeaderStyle.normal.textColor = new Color(0f, 0.98f, 0.85f);
        }

        if (titleSubStyle == null)
        {
            titleSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            titleSubStyle.normal.textColor = new Color(0.6f, 0.88f, 0.88f);
        }

        if (sectionHeaderStyle == null)
        {
            sectionHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            sectionHeaderStyle.normal.textColor = new Color(1.0f, 0.82f, 0.22f); // Vibrant Gold
        }

        if (infoTextStyle == null)
        {
            infoTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft
            };
            infoTextStyle.normal.textColor = new Color(0.90f, 0.96f, 0.98f); // High-contrast crisp white/cyan
        }

        if (subStyle == null)
        {
            subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft
            };
            subStyle.normal.textColor = new Color(0.6f, 0.8f, 0.82f);
        }

        if (scoreStyle == null)
        {
            scoreStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 19,
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

        if (playButtonStyle == null)
        {
            playButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            playButtonStyle.normal.background = playBtnTex;
            playButtonStyle.normal.textColor = new Color(0.01f, 0.06f, 0.08f);
        }

        if (dpadButtonStyle == null)
        {
            dpadButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            dpadButtonStyle.normal.textColor = Color.white;
        }

        if (cardStyle == null)
        {
            cardStyle = new GUIStyle(GUI.skin.box);
            cardStyle.normal.background = cardBgTex;
        }
    }

    Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    /// <summary>
    /// Renders the polished Pre-Game Title Card for university presentation and screen recording.
    /// Clean layout with generous left padding, larger typography, and Obstacles section.
    /// </summary>
    void DrawTitleScreen()
    {
        float panelW = 530f;
        float panelH = 490f;
        Rect panelRect = new Rect((Screen.width - panelW) * 0.5f, (Screen.height - panelH) * 0.5f, panelW, panelH);

        // Draw card background
        GUI.Box(panelRect, GUIContent.none, cardStyle);

        // Top cyan decorative line
        GUI.color = new Color(0f, 0.96f, 0.83f, 0.9f);
        GUI.DrawTexture(new Rect(panelRect.x + 24, panelRect.y + 4, panelW - 48, 3), barTex);
        GUI.color = Color.white;

        // Content area with generous 38px left and right padding and proper bottom clearance
        float padX = 38f;
        Rect contentRect = new Rect(panelRect.x + padX, panelRect.y + 16f, panelRect.width - (padX * 2), panelRect.height - 24f);

        GUILayout.BeginArea(contentRect);
        GUILayout.Space(4);

        // Title Header
        GUILayout.Label("SYLVAN RUSH", titleHeaderStyle);
        GUILayout.Label("Enchanted Forest Endless Runner", titleSubStyle);
        GUILayout.Space(12);

        // 1. Controls Section (Larger text & clear spacing)
        GUILayout.Label("CONTROLS", sectionHeaderStyle);
        GUILayout.Label("• A / D  or  ◀ ▶   : Shift Lanes Left / Right\n• W / Space  or  ▲ : Jump over Obstacles\n• S / Down  or  ▼  : Duck / Fast Fall", infoTextStyle);
        GUILayout.Space(10);

        // 2. Obstacles Section (Requested by user)
        GUILayout.Label("OBSTACLES", sectionHeaderStyle);
        GUILayout.Label("• 🪵 Woodland Tree Stump  : Vault / Leap cleanly over\n• 🪨 Ancient Stone Boulder : Dodge sideways or Leap over", infoTextStyle);
        GUILayout.Space(10);

        // 3. Spirit Blessings & Powers Section
        GUILayout.Label("SPIRIT BLESSINGS & POWERS", sectionHeaderStyle);
        GUILayout.Label("• 🧲 Spirit Magnet : Pulls Mana Orbs across all lanes\n• 🛡️ Bark Shield    : 3 Orbiting shields absorb 1 fatal collision\n• ⚡ Wind Dash       : 5s Invulnerable supersonic turbo sprint", infoTextStyle);
        GUILayout.Space(16);

        // 4. Big Call-To-Action Play Button
        if (GUILayout.Button("▶   START GAME   (or Press SPACE)", playButtonStyle, GUILayout.Height(46)))
        {
            GameManager.Instance.StartNewRun();
        }

        GUILayout.EndArea();
    }

    void DrawGameplayHUD()
    {
        // Top-Left Stats Box
        Rect statsRect = new Rect(20, 20, 320, 72);
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

        // Combo timer decay bar
        if (GameManager.Instance.comboMultiplier > 1)
        {
            float fillRatio = Mathf.Clamp01(GameManager.Instance.comboTimer / 2.8f);
            GUI.color = new Color(0f, 0.96f, 0.83f, 0.85f);
            GUI.DrawTexture(new Rect(10, 64, 300 * fillRatio, 4), barTex);
            GUI.color = Color.white;
        }

        GUILayout.EndArea();

        // Top-Right Orbs Box
        Rect orbsRect = new Rect(Screen.width - 150, 20, 130, 48);
        GUI.Box(orbsRect, GUIContent.none, cardStyle);
        GUI.Label(new Rect(Screen.width - 140, 28, 120, 32), "💎 " + GameManager.Instance.orbsCollected, scoreStyle);

        // Pause Button (Top-Right under Orbs)
        if (GUI.Button(new Rect(Screen.width - 95, 75, 75, 28), "PAUSE", buttonStyle))
        {
            GameManager.Instance.PauseGame();
        }

        // Active Buff Indicators
        int buffY = 104;
        if (PowerUpManager.Instance != null && PlayerController.Instance != null)
        {
            if (PlayerController.Instance.hasShield)
            {
                float shieldRatio = Mathf.Clamp01(PowerUpManager.Instance.shieldTimer / 14f);
                DrawBuffBadge(new Rect(20, buffY, 160, 28), "🛡️ BARK SHIELD", new Color(0.22f, 0.94f, 0.49f), shieldRatio);
                buffY += 34;
            }
            if (PlayerController.Instance.hasDash)
            {
                float dashRatio = Mathf.Clamp01(PowerUpManager.Instance.dashTimer / 5.0f);
                DrawBuffBadge(new Rect(20, buffY, 160, 28), "⚡ WIND DASH", new Color(0f, 0.96f, 0.83f), dashRatio);
                buffY += 34;
            }
            if (PlayerController.Instance.hasMagnet)
            {
                float magnetRatio = Mathf.Clamp01(PowerUpManager.Instance.magnetTimer / 8.5f);
                DrawBuffBadge(new Rect(20, buffY, 160, 28), "🧲 SPIRIT MAGNET", new Color(0.78f, 0.49f, 1f), magnetRatio);
                buffY += 34;
            }
        }

        // Bottom Controls Hint
        GUI.Label(new Rect(20, Screen.height - 35, 500, 25), "A/D: Move Lanes | W/Space: Jump | S: Slide/Duck", subStyle);
    }

    void DrawBuffBadge(Rect rect, string label, Color col, float progress)
    {
        Color oldColor = GUI.color;
        GUI.color = col;
        GUI.Box(rect, label, cardStyle);

        // Mini progress bar under badge
        GUI.DrawTexture(new Rect(rect.x + 4, rect.y + rect.height - 3, (rect.width - 8) * progress, 2), barTex);
        GUI.color = oldColor;
    }

    void DrawOnScreenControls()
    {
        // Bottom-Left D-Pad: Lane Left & Lane Right
        float bSize = 64;
        float margin = 24;

        if (GUI.Button(new Rect(margin, Screen.height - margin - bSize, bSize, bSize), "◀", dpadButtonStyle))
        {
            if (PlayerController.Instance != null) PlayerController.Instance.MoveLane(-1);
        }

        if (GUI.Button(new Rect(margin + bSize + 12, Screen.height - margin - bSize, bSize, bSize), "▶", dpadButtonStyle))
        {
            if (PlayerController.Instance != null) PlayerController.Instance.MoveLane(1);
        }

        // Bottom-Right Action Buttons: Jump & Slide
        if (GUI.Button(new Rect(Screen.width - margin - bSize * 2 - 12, Screen.height - margin - bSize, bSize, bSize), "▲\nJUMP", dpadButtonStyle))
        {
            if (PlayerController.Instance != null) PlayerController.Instance.RequestJump();
        }

        if (GUI.Button(new Rect(Screen.width - margin - bSize, Screen.height - margin - bSize, bSize, bSize), "▼\nSLIDE", dpadButtonStyle))
        {
            if (PlayerController.Instance != null) PlayerController.Instance.RequestSlide();
        }
    }

    void DrawPauseScreen()
    {
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);

        float panelWidth = 320;
        float panelHeight = 220;
        Rect panelRect = new Rect((Screen.width - panelWidth) * 0.5f, (Screen.height - panelHeight) * 0.5f, panelWidth, panelHeight);
        GUI.Box(panelRect, GUIContent.none, cardStyle);

        GUILayout.BeginArea(panelRect);
        GUILayout.Space(20);
        GUILayout.Label("GAME PAUSED", headerStyle);
        GUILayout.Space(25);

        if (GUILayout.Button("RESUME", buttonStyle, GUILayout.Height(40)))
        {
            GameManager.Instance.ResumeGame();
        }

        GUILayout.Space(12);
        if (GUILayout.Button("RESTART RUN", buttonStyle, GUILayout.Height(40)))
        {
            GameManager.Instance.StartNewRun();
        }

        GUILayout.EndArea();
    }

    void DrawGameOverScreen()
    {
        // Dark Dim Background
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);

        float panelWidth = 380;
        float panelHeight = 310;
        Rect panelRect = new Rect((Screen.width - panelWidth) * 0.5f, (Screen.height - panelHeight) * 0.5f, panelWidth, panelHeight);
        GUI.Box(panelRect, GUIContent.none, cardStyle);

        GUILayout.BeginArea(panelRect);
        GUILayout.Space(20);
        GUILayout.Label("RUN COMPLETE", headerStyle);
        GUILayout.Space(15);

        GUILayout.Label("Final Score: " + GameManager.Instance.score.ToString("N0"), scoreStyle);
        GUILayout.Label("Distance Run: " + Mathf.FloorToInt(GameManager.Instance.distance) + " meters", subStyle);
        GUILayout.Label("Mana Orbs Gathered: " + GameManager.Instance.orbsCollected, subStyle);
        GUILayout.Space(12);
        GUILayout.Label("Personal Best Score: " + GameManager.Instance.GetPersonalBestScore().ToString("N0"), subStyle);
        GUILayout.Label("Personal Best Distance: " + Mathf.FloorToInt(GameManager.Instance.GetPersonalBestDistance()) + "m", subStyle);

        GUILayout.Space(25);
        if (GUILayout.Button("PLAY AGAIN", buttonStyle, GUILayout.Height(44)))
        {
            GameManager.Instance.StartNewRun();
        }
        GUILayout.EndArea();
    }
}
