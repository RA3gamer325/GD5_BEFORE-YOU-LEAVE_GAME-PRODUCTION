using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;

    [Header("Settings")]
    public string menuSceneName = "MainMenu";
    public bool isPaused = false;

    private static PauseMenu instance;

    void Awake()
    {
        // 🔥 Make persistent (DON'T destroy between scenes)
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        // ❌ Disable pause in Main Menu
        if (SceneManager.GetActiveScene().name == menuSceneName)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    // ▶ RESUME
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ⏸ PAUSE
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 🔁 RESTART
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 🏠 MAIN MENU
    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

    }

    // ❌ QUIT
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}