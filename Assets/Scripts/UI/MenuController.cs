using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private Button resumeButton;
    
    #region Public Button Methods
    //Methods Invoked by the buttons' click event 

    public void Start()
    {
        //Todo: make a simple "HasGameState" method that does a bit more sofisticated checking
        this.resumeButton.interactable = SaveGameSystem.LoadGameState(out var data1, out var data2);
    }

    public void SetCubeSizeDefault(int size) {
        PerSessionData.CubeSize = size;
    }

    public void OnButtonResume() {
        Debug.Log($"Resuming game");
        PerSessionData.newGameMode = PerSessionData.GameModes.Resume;
        SceneManager.LoadScene("Game");
    }

    public void OnButtonNewGame() {
        Debug.Log($"Starting new game with size {PerSessionData.CubeSize}");
        PerSessionData.newGameMode = PerSessionData.GameModes.NewGame;
        SceneManager.LoadScene("Game");
    }

    public void OnButtonQuit() {
        Application.Quit();
    }
    #endregion
}
