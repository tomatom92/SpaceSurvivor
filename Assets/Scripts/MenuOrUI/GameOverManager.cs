using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] GameObject gameOverPanel; // Reference to the Game Over panel
    public static GameOverManager Instance { get; private set; } // Singleton instance

    private void Awake()
    {
        // Ensure only one instance of GameOverManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        gameOverPanel.SetActive(false); // Hide the Game Over panel at the start
    }

    // Method to show the Game Over panel
    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }
    public void HideGameOverPanel()
    {
        gameOverPanel.SetActive(false);
    }

    // Method to retry the game
    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
        HideGameOverPanel(); // Hide the Game Over panel
        PlayerMovement.instance.isDead = false; // Reset player state
        PlayerHealth.instance.lives = 1; // Reset player lives
        PlayerShoot.instance.GetComponent<PlayerShoot>().enabled = true; // Reactivate player shooting
        PlayerMovement.instance.GetComponent<PlayerMovement>().enabled = true; // Reactivate player movement
        EnemySpawnManager.Instance.ResetWaves(); // Reset enemy spawn manager
        //reactivate player movement and shooting.
    }

    // Method to go to the main menu
    public void GoToMainMenu()
    {
        HideGameOverPanel(); // Hide the Game Over panel
        SceneManager.LoadScene(0); // Load the Main Menu scene
    }
}
