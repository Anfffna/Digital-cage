using UnityEngine;

public class SaveManagerLoader : MonoBehaviour
{
    [SerializeField] private GameObject saveManagerPrefab;

    void Start()
    {
        // Если SaveManager еще не существует в сцене
        if (SaveManager.Instance == null && saveManagerPrefab != null)
        {
            Instantiate(saveManagerPrefab);
        }
    }
}