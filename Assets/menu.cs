using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    public string level1SceneName = "Level 1";
    public string gymSceneName = "GYM";

    // ▶ START GAME (LEVEL 1)
    public void StartGame()
    {
        SceneManager.LoadScene(level1SceneName);
    }

    // 🏋 GYM
    public void OpenGym()
    {
        SceneManager.LoadScene(gymSceneName);
    }

    // ❌ QUIT GAME
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}