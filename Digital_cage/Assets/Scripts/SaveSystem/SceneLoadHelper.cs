using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadHelper : MonoBehaviour
{
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Если это не главное меню, применяем загруженные данные
        if (scene.name != "MainMenu")
        {
            // Ждем конца кадра чтобы все объекты успели загрузиться
            Invoke("ApplySaveData", 0.1f);
        }
    }

    private void ApplySaveData()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ApplyLoadedData();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}