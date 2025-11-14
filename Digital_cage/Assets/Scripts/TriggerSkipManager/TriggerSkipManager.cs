using UnityEngine;
using System.Collections;

public class TriggerSkipManager : MonoBehaviour
{
    [Header("Тестируемый AdventDoor")]
    public AdventDoor targetAdventDoor;

    [Header("Настройки")]
    public float waitAfterSpawn = 3f; // Ждем 3 секунды после спавна

    void Start()
    {
        if (targetAdventDoor != null)
        {
            StartCoroutine(TestAdventDoorCoroutine());
        }
    }

    private IEnumerator TestAdventDoorCoroutine()
    {
        Debug.Log("=== НАЧИНАЕМ ТЕСТ ADVENTDOOR ===");

        // 1. Запускаем спавн дверей
        targetAdventDoor.SpawnRandomDoors();
        Debug.Log("Вызван SpawnRandomDoors()");

        // 2. ЖДЕМ пока корутина в AdventDoor выполнится
        yield return new WaitForSeconds(waitAfterSpawn);

        // 3. Проверяем результат
        if (targetAdventDoor.DoorsSpawned)
        {
            Debug.Log("? Двери успешно созданы!");
        }
        else
        {
            Debug.Log("? Двери не создались. Пробуем ForceSpawnDoors...");

            // Пробуем альтернативный метод
            targetAdventDoor.ForceSpawnDoors();
            yield return new WaitForSeconds(1f);

            if (targetAdventDoor.DoorsSpawned)
            {
                Debug.Log("? Двери созданы через ForceSpawnDoors!");
            }
            else
            {
                Debug.Log("? Двери все равно не создались");
            }
        }
    }

    void Update()
    {
        // Простые тесты по клавишам
        if (Input.GetKeyDown(KeyCode.F1) && targetAdventDoor != null)
        {
            targetAdventDoor.ForceSpawnDoors();
            Debug.Log("ForceSpawnDoors вызван");
        }

        if (Input.GetKeyDown(KeyCode.F2) && targetAdventDoor != null)
        {
            targetAdventDoor.HideAllDoors();
            Debug.Log("Все двери скрыты");
        }
    }
}