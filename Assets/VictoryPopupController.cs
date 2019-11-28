using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryPopupController : MonoBehaviour
{
    [SerializeField] private Text timeTakenTextField;

    public void SetTimeTaken(TimeSpan timeTaken)
    {
        timeTakenTextField.text = $"It took you: {timeTaken.ToShortForm()}";
    }
}
