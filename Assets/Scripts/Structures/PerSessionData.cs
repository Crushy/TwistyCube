using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A static class that holds state data only relevant to the current game
/// session (ie, between program launch and exit).
/// </summary>
//Alternatively this could have been a persistant GO in the title/preload scene
public static class PerSessionData
{
    public static int CubeSize = 4;
    public static bool ShowTimer;



    public enum GameModes
    {
        Resume,
        NewGame
    }
    public static GameModes newGameMode;
    public enum GameStates
    {
        Paused,
        Running
    }
    public static event System.Action<GameStates> GameStateChanged;

    private static GameStates gameState;
    public static GameStates GameState {
        set
        {
            gameState = value;
            GameStateChanged?.Invoke(gameState);
        }
        get
        {
            return gameState;
        }
    }

}
