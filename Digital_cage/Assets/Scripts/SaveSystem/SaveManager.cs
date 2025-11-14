using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string savePath;
    private SaveData currentSaveData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "game_save.dat");
            Debug.Log("SaveManager создан. Путь: " + savePath);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool HasSave
    {
        get { return Instance != null && File.Exists(Instance.savePath); }
    }

    public void SaveGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Получаем позицию игрока (если есть)
        Vector3 playerPosition = GetPlayerPosition();

        // Создаем данные для сохранения
        currentSaveData = new SaveData
        {
            sceneName = currentScene,
            PlayerPosition = playerPosition, // Используем свойство
            saveTime = System.DateTime.Now.ToString()
        };

        // Сохраняем в файл
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);

        formatter.Serialize(stream, currentSaveData);
        stream.Close();

        Debug.Log($"Игра сохранена! Сцена: {currentScene}, Позиция: {playerPosition}");
    }

    public void LoadGame()
    {
        if (!HasSave)
        {
            Debug.LogWarning("Нет сохраненной игры!");
            return;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Open);

        currentSaveData = formatter.Deserialize(stream) as SaveData;
        stream.Close();

        // Загружаем сцену
        SceneManager.LoadScene(currentSaveData.sceneName);

        Debug.Log($"Игра загружена! Сцена: {currentSaveData.sceneName}");
    }

    // Применяем загруженные данные после загрузки сцены
    public void ApplyLoadedData()
    {
        if (currentSaveData == null) return;

        // Восстанавливаем позицию игрока
        SetPlayerPosition(currentSaveData.PlayerPosition);

        Debug.Log($"Позиция игрока восстановлена: {currentSaveData.PlayerPosition}");
    }

    private Vector3 GetPlayerPosition()
    {
        // Найди игрока в сцене (настрой под свои теги/имена)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.transform.position;
        }

        return Vector3.zero;
    }

    private void SetPlayerPosition(Vector3 position)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = position;
        }
    }

    public void DeleteSave()
    {
        if (HasSave)
        {
            File.Delete(savePath);
            currentSaveData = null;
            Debug.Log("Сохранение удалено!");
        }
    }

    public SaveData GetSaveData()
    {
        return currentSaveData;
    }
}