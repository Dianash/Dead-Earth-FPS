using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] private List<GameState> startingGameStates = new List<GameState>();

    private Dictionary<string, string> gameStateDictionary = new Dictionary<string, string>();
    private static ApplicationManager instance = null;

    public static ApplicationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (ApplicationManager)FindObjectOfType(typeof(ApplicationManager));
            }

            return instance;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        ResetGamesStates();
    }

    /// <summary>
    /// Returns the value of a game state
    /// </summary>
    public string GetGameState(string key)
    {
        string result = null;
        gameStateDictionary.TryGetValue(key, out result);
        return result;
    }

    /// <summary>
    /// Sets a Game State
    /// </summary>
    public bool SetGameState(string key, string value)
    {
        if (key == null || value == null)
            return false;

        gameStateDictionary[key] = value;

        return true;
    }

    public void LoadMainMenu()
    {
        ResetGamesStates();
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("Way Out");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ResetGamesStates()
    {
        // Copy starting game states into game state dictionary
        for (int i = 0; i < startingGameStates.Count; i++)
        {
            GameState gameState = startingGameStates[i];
            gameStateDictionary[gameState.Key] = gameState.Value;
        }
    }
}
