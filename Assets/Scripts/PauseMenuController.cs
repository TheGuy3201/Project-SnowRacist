using System;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] String mainMenuSceneName = "MainMenu";
    [SerializeField] GameObject pauseMenuUI;

    public void OnPaused()
    {
        // Pause the game
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
    }

    public void OnResumeButtonClicked()
    {
        // Resume the game
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
    }

    public void OnMainMenuButtonClicked()
    {
        // Load the main menu scene
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnQuitButtonClicked()
    {
        // Quit the application
        Application.Quit();
    }
}
