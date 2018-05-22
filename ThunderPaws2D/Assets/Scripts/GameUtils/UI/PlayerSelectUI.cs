using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSelectUI : MonoBehaviour {

	public void Select1Player() {
        // Just start the game
        SceneManager.LoadScene("PreAlphaDemoMainMenu");
    }

    public void Select2Player() {
        // Hide this UI and show the controller assignment screen
        SceneManager.LoadScene("PreAlphaDemoControllerAssignment");
    }
}
