using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryPopupController : MonoBehaviour
{
    #pragma warning disable 649
    [SerializeField] private Text timeTakenTextField;
    #pragma warning restore 649

    public void SetTimeTaken(TimeSpan timeTaken)
    {
        timeTakenTextField.text = $"It took you: {timeTaken.ToShortForm()}";
    }
}
