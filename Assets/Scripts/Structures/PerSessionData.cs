using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Alternatively this could have been a persistant GO in the title/preload scene
public static class PerSessionData
{
    public static int CubeSize = 3;
    public static bool ShowTimer;

    public enum GameMode
    {
        Resume,
        NewGame
    }

    //public enum GameState
    //{
    //    Paused,
    //    Running
    //}
    //public static GameState gameState;

}
