using System;
using System.IO;
using AZUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [Header("Scene Names")]
    public string mainMenuScene = "Lobby";
    public string mainLevelScene = "SampleLevel";

    [Header("Settings")]
    public Vector2 mouseSensitivity = Vector2.one;
    [HideInInspector] public Vector2 originalMouseSensitivity = Vector2.one;

    public InputDevice lastDetectedDevice = null;

    private Vector2 lastScreenSize;

    public event Action onScreenSizeChanged;

    new void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        InputSystem.onEvent += (ptr, device) => { lastDetectedDevice = device; };

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        originalMouseSensitivity = mouseSensitivity;
    }

    void Update()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (lastScreenSize != screenSize)
        {
            lastScreenSize = screenSize;
            onScreenSizeChanged?.Invoke();
        }
    }

    public void QuitGame()
    {
        SceneFader.FadeOutThen(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif

            Application.Quit();
        });
    }

    public void GoToMainMenu()
    {
        GoToScene(mainMenuScene);
    }

    public void GoToMainLevel()
    {
        GoToScene(mainLevelScene);
    }

    public void RestartCurrentScene()
    {
        GoToScene(SceneManager.GetActiveScene().name);
    }

    public void GoToScene(string sceneName)
    {
        SceneFader.FadeOutThen(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    public void DeleteAllSaveData()
    {
        string dir = Application.persistentDataPath;
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
    }
}
