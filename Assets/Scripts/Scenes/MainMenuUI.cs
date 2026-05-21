using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void Play()
    {
        SceneLoader.LoadGame();
    }

    public void EditDeck()
    {
        SceneLoader.LoadDeckEditor();
    }

    public void HowToPlay()
    {
        SceneLoader.LoadHowToPlay();
    }

    public void Quit()
    {
        SceneLoader.QuitGame();
    }
}