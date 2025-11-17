using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip mainMenuMusic;
    public AudioSource audioSource;

    private string currentSceneName;

    void Start()
    {
        // Получаем текущую сцену
        currentSceneName = SceneManager.GetActiveScene().name;

        // Если мы на главном меню - запускаем музыку
        if (currentSceneName == "MainMenu")
        {
            PlayMainMenuMusic();
        }

        // Подписываемся на событие смены сцены
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnSceneChanged(Scene previousScene, Scene newScene)
    {
        // Останавливаем музыку при уходе с главного меню
        if (previousScene.name == "MainMenu" && audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Запускаем музыку при входе в главное меню
        if (newScene.name == "MainMenu")
        {
            PlayMainMenuMusic();
        }

        currentSceneName = newScene.name;
    }

    private void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null && audioSource != null)
        {
            audioSource.clip = mainMenuMusic;
            audioSource.loop = true; // Зацикливаем музыку
            audioSource.Play();
            Debug.Log("Запущена музыка главного меню");
        }
        else
        {
            if (mainMenuMusic == null)
                Debug.LogWarning("MainMenuMusic не назначен!");
            if (audioSource == null)
                Debug.LogWarning("AudioSource не найден!");
        }
    }

    // Остальные методы без изменений
    public void LoadGameScene()
    {
        SceneManager.LoadScene("Game");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Выход из игры!");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }
}