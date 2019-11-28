using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StagedUI : MonoBehaviour
{
    #pragma warning disable 649

    [SerializeField]
    private RectTransform[] stages;

    [SerializeField]
    private UnityEvent onStagesDone;

    private int currentStage = 0;

    #pragma warning restore 649

    private void OnEnable() {
        this.currentStage = 0;
        ChangeActiveStage(this.stages[this.currentStage]);
    }

    private void OnDisable() {
        this.currentStage = 0;
    }

    private void ChangeActiveStage(RectTransform newStage) {
        
        //Could be optimized away by using another variable
        System.Array.ForEach(this.stages,x=>x.gameObject.SetActive(false));

        newStage.gameObject.SetActive(true);
    }

    public void NextStage() {
        this.currentStage++;
        
        if (this.currentStage>=stages.Length) {
            onStagesDone?.Invoke();
            return;
        }
        else {
            ChangeActiveStage(this.stages[this.currentStage]);
        }

        
    }

    public void PrevStage() {
        this.currentStage--;
        
        if (this.currentStage>=0) {
            ChangeActiveStage(this.stages[this.currentStage]);
        }
        else {
            this.currentStage = 0;
        }
    }

    public void ResetStages()
    {
        this.currentStage = 0;
        ChangeActiveStage(this.stages[0]);
    }

    
}
