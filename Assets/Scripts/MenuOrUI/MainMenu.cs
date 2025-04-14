using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Method to start the game
    public void StartGame()
    {
        Debug.Log("Starting Game..."); // Log message for starting the game
        SceneManager.LoadScene(1);
    }

    // Method to quit the game
    public void QuitGame()
    {
        Application.Quit(); // Quit the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
