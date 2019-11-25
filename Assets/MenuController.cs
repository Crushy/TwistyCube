using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    #region Public Button Methods
    //Methods Invoked by the buttons' click event 

    // TODO: serialize this value somewhere so we can actually remeber the player's favourite cube size.
    public void SetCubeSizeDefault(int size) {
        this.cubeSize = size;
    }

    public void OnButtonResume() {

    }

    /* TODO:
      This looks horrible. Ideally it should be reworked in such a way that we can also save
      the player's favourite cube size elsewhere and have the new game sequence simply write over the previous
      default. The MenuController would simply intercept that change. Or we can use some sort of editor event extension
    */

    private int cubeSize = 2;

    public void OnButtonNewGame() {
        Debug.Log($"Starting new game with size {this.cubeSize}");
    }

    public void OnButtonQuit() {
        Application.Quit();
    }
    #endregion
}
