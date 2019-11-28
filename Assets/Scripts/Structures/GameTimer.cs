using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer
{
    private System.DateTime timerStart;

    private bool stoppedTimer = false; 

    //Time from serialized runs
    private System.TimeSpan accumulatedTime = System.TimeSpan.Zero;

    public GameTimer(System.TimeSpan accumulatedTime)
    {
        this.accumulatedTime = accumulatedTime;
        timerStart = System.DateTime.UtcNow;
    }

    public System.TimeSpan GetCurrentGameDuration()
    {
        if (!this.stoppedTimer)
        {
            return this.accumulatedTime + (System.DateTime.UtcNow - timerStart);
        }
        else
        {
            return accumulatedTime;
        }
        
    }

    public void StopTimer()
    {
        if (this.stoppedTimer)
        {
            Debug.LogError("Timer was already stopped");
            return;
        }
        this.stoppedTimer = true;
        this.accumulatedTime = this.accumulatedTime + (System.DateTime.UtcNow - timerStart);
    }
}
