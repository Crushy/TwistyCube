using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{

    
    #region Public Button Methods
    //Methods Invoked by the buttons' click event 

    public void SetCubeSizeDefault(int size) {
        PerSessionData.CubeSize = size;
    }

    public void OnButtonResume() {
        Debug.Log($"Resuming game");
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void OnButtonNewGame() {
        Debug.Log($"Starting new game with size {PerSessionData.CubeSize}");
        SceneManager.LoadScene("Game",LoadSceneMode.Single);
    }

    public void OnButtonQuit() {
        Application.Quit();
    }
    #endregion
}
