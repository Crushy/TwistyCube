using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer
{
    private System.DateTime timerStart;

    //Time from serialized runs
    private System.TimeSpan accumulatedTime = System.TimeSpan.Zero;

    public GameTimer(System.TimeSpan accumulatedTime)
    {
        this.accumulatedTime = accumulatedTime;
        timerStart = System.DateTime.UtcNow;
    }

    public System.TimeSpan GetCurrentGameDuration()
    {
        return this.accumulatedTime + (System.DateTime.UtcNow - timerStart);
    }
}
