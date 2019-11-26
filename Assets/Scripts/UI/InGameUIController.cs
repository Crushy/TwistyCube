using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIController : MonoBehaviour
{
    [SerializeField]
    private Text timerIndicator;

    public void SetTimerText(System.TimeSpan text) {
        timerIndicator.text = text.ToShortForm();
    }

    #region Button Actions
    public void ToggleTimer()
    {
        PerSessionData.ShowTimer = !PerSessionData.ShowTimer;
        this.timerIndicator.gameObject.SetActive(PerSessionData.ShowTimer);
    }

    public void ShowPauseMenu()
    {

    }

    public void HidePauseMenu()
    {

    }
    #endregion
}
