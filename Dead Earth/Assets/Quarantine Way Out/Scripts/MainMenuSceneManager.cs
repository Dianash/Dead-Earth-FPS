using UnityEngine;

public class MainMenuSceneManager : MonoBehaviour
{
    public void LoadGame()
    {
        if (ApplicationManager.Instance)
            ApplicationManager.Instance.LoadGame();
    }

    public void QuitGame()
    {
        if (ApplicationManager.Instance)
            ApplicationManager.Instance.QuitGame();
    }
}
