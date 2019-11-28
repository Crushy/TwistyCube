using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIController : MonoBehaviour
{
    #pragma warning disable 649
    [SerializeField] private Text timerIndicator;

    [SerializeField] private Button undoButton;

    [SerializeField] private RectTransform pauseMenu;

    [SerializeField] private VictoryPopupController winDialog;

    [SerializeField] private CanvasGroup fadeCanvas;

    //[SerializeField] private Button pauseButton;
    #pragma warning restore 649

    public void SetTimerText(System.TimeSpan text) {
        timerIndicator.text = text.ToShortForm();
    }

    public void FadeIn()
    {
        fadeCanvas.gameObject.SetActive(true);
        StartCoroutine(
            InterpolationUtils.InterpolateAction(
                (t) =>
                {
                    //Apply an easing function to t so the animation looks smoother
                    float easedT = Mathf.SmoothStep(0, 1, t);
                    fadeCanvas.alpha = 1-t;
                },
                .5f,
                (t)=>fadeCanvas.gameObject.SetActive(false)
            )
        );
    }

    public void FadeOut()
    {
        fadeCanvas.gameObject.SetActive(false);
        StartCoroutine(
            InterpolationUtils.InterpolateAction(
                (t) =>
                {
                    //Apply an easing function to t so the animation looks smoother
                    float easedT = Mathf.SmoothStep(0, 1, t);
                    fadeCanvas.alpha = t;
                },
                .5f,
                (t) => fadeCanvas.gameObject.SetActive(true)
            )
        );
    }

    public void ShowPauseMenu()
    {
        this.pauseMenu.gameObject.SetActive(true);
    }

    public void ClosePauseMenu()
    {
        this.pauseMenu.gameObject.SetActive(false);
    }

    public void ShowWinScreen(System.TimeSpan timeTaken) {
        this.winDialog.gameObject.SetActive(true);
        this.winDialog.SetTimeTaken(timeTaken);
    }

    public void CloseWinScreen()
    {
        this.winDialog.gameObject.SetActive(false);
    }

    public void ShowUndoButton()
    {
        this.undoButton.gameObject.SetActive(true);
    }

    public void HideUndoButton()
    {
        this.undoButton.gameObject.SetActive(false);
    }

    #region Button Actions
    public void UI_ToggleTimer()
    {
        PerSessionData.ShowTimer = !PerSessionData.ShowTimer;
        this.timerIndicator.gameObject.SetActive(PerSessionData.ShowTimer);
    }

    
    #endregion
}
