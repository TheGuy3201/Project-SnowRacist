using System;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] String mainMenuSceneName = "MainMenu";

    public void OnPaused()
    {
        // Pause the game
        Time.timeScale = 0f;
        this.gameObject.SetActive(true);
    }

    public void OnResumeButtonClicked()
    {
        // Resume the game
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void OnMainMenuButtonClicked()
    {
        // Load the main menu scene
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }
}
