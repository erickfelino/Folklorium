using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public static void LoadHowToPlay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("HowToPlay");
    }

    public static void LoadDeckEditor()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("DeckEditor");
    }

    public static void LoadGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public static void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}