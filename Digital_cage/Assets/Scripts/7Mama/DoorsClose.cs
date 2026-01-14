using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DoorsClose : MonoBehaviour
{
    [Header("Doors Settings")]
    public List<GameObject> doorObjects = new List<GameObject>(); // Все обычные двери
    public GameObject specialDoor; // ОСОБЕННАЯ дверь (отдельно от списка)

    [Header("UI Settings")]
    public GameObject sharedPromptPanel;       // ОБЩАЯ плашка для всех обычных дверей
    public float promptShowDuration = 2f;      // Время показа плашки

    [Header("Audio")]
    public AudioSource audioSource;            // Для звуков
    public AudioClip doorInteractSound;        // Общий звук для всех дверей
    public AudioClip specialDoorSound;         // Звук для особенной двери

    [Header("Special Door Dialogue")]
    public ManagerDialogue7 dialogueManager;   // Диалог для особенной двери
    [TextArea(2, 5)]
    public List<string> excludedDoorDialogue;  // Реплики диалога

    [Header("Teleport Settings")]
    public float delayAfterDialogue = 3f;      // Задержка после диалога (3 секунды)
    public float fadeDuration = 2f;            // Длительность появления/исчезновения черного экрана
    public Image blackScreen;                  // Черный экран (UI Image)
    public Transform teleportDestination;      // Точка телепортации

    private HashSet<GameObject> usedDoors = new HashSet<GameObject>(); // Использованные двери
    private bool isTeleporting = false;

    void Start()
    {
        InitializeDoors();

        // Скрываем плашку при старте
        if (sharedPromptPanel != null)
        {
            sharedPromptPanel.SetActive(false);
        }

        // Скрываем черный экран
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            Color color = blackScreen.color;
            color.a = 0f;
            blackScreen.color = color;
        }
    }

    void InitializeDoors()
    {
        // Инициализируем обычные двери
        foreach (var door in doorObjects)
        {
            if (door == null)
            {
                Debug.LogWarning("DoorsClose: Найдена дверь без назначенного объекта!");
                continue;
            }

            // Добавляем компонент DoorInteractionComponent если его нет
            DoorInteractionComponent interactable = door.GetComponent<DoorInteractionComponent>();
            if (interactable == null)
            {
                interactable = door.AddComponent<DoorInteractionComponent>();
            }

            // Настраиваем компонент (обычная дверь)
            interactable.SetDoorsClose(this);
            interactable.SetIsSpecialDoor(false);
        }

        // Инициализируем особенную дверь если она есть
        if (specialDoor != null)
        {
            // Добавляем компонент DoorInteractionComponent если его нет
            DoorInteractionComponent interactable = specialDoor.GetComponent<DoorInteractionComponent>();
            if (interactable == null)
            {
                interactable = specialDoor.AddComponent<DoorInteractionComponent>();
            }

            // Настраиваем компонент (особенная дверь)
            interactable.SetDoorsClose(this);
            interactable.SetIsSpecialDoor(true);

            Debug.Log($"DoorsClose: Особенная дверь '{specialDoor.name}' инициализирована");
        }

        Debug.Log($"DoorsClose: Инициализировано {doorObjects.Count} обычных дверей + {(specialDoor != null ? 1 : 0)} особенная дверь");
    }

    /// <summary>
    /// Вызывается когда игрок взаимодействует с дверью
    /// </summary>
    public void OnDoorInteract(GameObject door, bool isSpecialDoor)
    {
        if (usedDoors.Contains(door))
        {
            Debug.Log($"DoorsClose: Дверь {door.name} уже была использована");
            return;
        }

        Debug.Log($"DoorsClose: Взаимодействие с дверью {door.name} (особенная: {isSpecialDoor})");

        // Отмечаем дверь как использованную
        usedDoors.Add(door);

        // Делаем дверь неинтерактивной
        DoorInteractionComponent interactable = door.GetComponent<DoorInteractionComponent>();
        if (interactable != null)
        {
            interactable.SetCanInteract(false);
        }

        // Меняем слой двери
        door.layer = LayerMask.NameToLayer("Default");

        if (isSpecialDoor)
        {
            // ОСОБЕННАЯ ДВЕРЬ: запускаем диалог и телепортацию
            HandleSpecialDoorInteraction(door);
        }
        else
        {
            // ОБЫЧНАЯ ДВЕРЬ: показываем плашку
            HandleRegularDoorInteraction(door);
        }
    }

    void HandleRegularDoorInteraction(GameObject door)
    {
        // Проигрываем звук если есть
        if (audioSource != null && doorInteractSound != null)
        {
            audioSource.PlayOneShot(doorInteractSound);
        }

        // Показываем ОБЩУЮ плашку
        ShowSharedPrompt();
    }

    void HandleSpecialDoorInteraction(GameObject door)
    {
        // Проигрываем особый звук если есть
        if (audioSource != null && specialDoorSound != null)
        {
            audioSource.PlayOneShot(specialDoorSound);
        }

        // Запускаем диалог если есть
        if (dialogueManager != null && excludedDoorDialogue != null && excludedDoorDialogue.Count > 0)
        {
            dialogueManager.StartDialogue(excludedDoorDialogue, OnDialogueComplete);
        }
        else
        {
            // Если диалога нет, сразу начинаем телепортацию
            StartCoroutine(TeleportSequence());
        }
    }

    void OnDialogueComplete()
    {
        Debug.Log("DoorsClose: Диалог завершен, начинаю телепортацию...");
        StartCoroutine(TeleportSequence());
    }

    IEnumerator TeleportSequence()
    {
        if (isTeleporting) yield break;

        isTeleporting = true;

        // Ждем задержку после диалога
        Debug.Log($"DoorsClose: Жду {delayAfterDialogue} секунд после диалога...");
        yield return new WaitForSeconds(delayAfterDialogue);

        // Плавно появляется черный экран
        Debug.Log("DoorsClose: Появление черного экрана...");
        yield return StartCoroutine(FadeBlackScreen(0f, 1f, fadeDuration));

        // ТЕЛЕПОРТАЦИЯ когда экран полностью черный
        Debug.Log("DoorsClose: Телепортирую игрока...");
        TeleportPlayer();

        // Ждем 2 секунды на черном экране
        yield return new WaitForSeconds(2f);

        // Плавно исчезает черный экран
        Debug.Log("DoorsClose: Исчезновение черного экрана...");
        yield return StartCoroutine(FadeBlackScreen(1f, 0f, fadeDuration));

        isTeleporting = false;
        Debug.Log("DoorsClose: Телепортация завершена");
    }

    IEnumerator FadeBlackScreen(float fromAlpha, float toAlpha, float duration)
    {
        if (blackScreen == null) yield break;

        float timer = 0f;
        Color color = blackScreen.color;
        color.a = fromAlpha;
        blackScreen.color = color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, progress);
            blackScreen.color = color;
            yield return null;
        }

        color.a = toAlpha;
        blackScreen.color = color;
    }

    void TeleportPlayer()
    {
        if (teleportDestination == null)
        {
            Debug.LogError("DoorsClose: Не назначена точка телепортации!");
            return;
        }

        // Находим игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("DoorsClose: Не найден объект игрока с тегом 'Player'!");
            return;
        }

        // Отключаем управление игроком на время телепортации
        MonoBehaviour[] playerComponents = player.GetComponents<MonoBehaviour>();
        foreach (var component in playerComponents)
        {
            if (component != null && component.enabled)
            {
                component.enabled = false;
            }
        }

        // Телепортируем
        player.transform.position = teleportDestination.position;
        player.transform.rotation = teleportDestination.rotation;

        Debug.Log($"DoorsClose: Игрок телепортирован в {teleportDestination.position}");

        // Включаем управление обратно
        StartCoroutine(ReenablePlayerComponents(player, playerComponents));
    }

    IEnumerator ReenablePlayerComponents(GameObject player, MonoBehaviour[] components)
    {
        yield return new WaitForSeconds(0.1f);

        foreach (var component in components)
        {
            if (component != null)
            {
                component.enabled = true;
            }
        }

        Debug.Log("DoorsClose: Управление игроком восстановлено");
    }

    void ShowSharedPrompt()
    {
        if (sharedPromptPanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowPromptCoroutine());
        }
    }

    IEnumerator ShowPromptCoroutine()
    {
        // Показываем плашку
        sharedPromptPanel.SetActive(true);

        // Плавное появление
        yield return StartCoroutine(FadePrompt(sharedPromptPanel, 0f, 1f, 0.3f));

        // Ждем указанное время
        yield return new WaitForSeconds(promptShowDuration);

        // Плавное исчезновение
        yield return StartCoroutine(FadePrompt(sharedPromptPanel, 1f, 0f, 0.3f));

        // Скрываем плашку
        sharedPromptPanel.SetActive(false);
    }

    IEnumerator FadePrompt(GameObject prompt, float fromAlpha, float toAlpha, float duration)
    {
        CanvasGroup canvasGroup = prompt.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            // Если нет CanvasGroup, просто ждем
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, progress);
            yield return null;
        }

        canvasGroup.alpha = toAlpha;
    }

    /// <summary>
    /// Сбросить конкретную дверь
    /// </summary>
    public void ResetDoor(GameObject door)
    {
        if (door == specialDoor || doorObjects.Contains(door))
        {
            usedDoors.Remove(door);

            // Возвращаем компонент взаимодействия
            DoorInteractionComponent interactable = door.GetComponent<DoorInteractionComponent>();
            if (interactable != null)
            {
                interactable.SetCanInteract(true);
            }

            // Возвращаем слой Interactable
            door.layer = LayerMask.NameToLayer("Interactable");

            Debug.Log($"DoorsClose: Дверь {door.name} сброшена");
        }
        else
        {
            Debug.LogError($"DoorsClose: Дверь {door.name} не найдена!");
        }
    }

    /// <summary>
    /// Сбросить все двери
    /// </summary>
    public void ResetAllDoors()
    {
        foreach (var door in doorObjects)
        {
            if (door != null)
            {
                usedDoors.Remove(door);

                DoorInteractionComponent interactable = door.GetComponent<DoorInteractionComponent>();
                if (interactable != null)
                {
                    interactable.SetCanInteract(true);
                }

                door.layer = LayerMask.NameToLayer("Interactable");
            }
        }

        // Сбрасываем особенную дверь
        if (specialDoor != null)
        {
            usedDoors.Remove(specialDoor);

            DoorInteractionComponent interactable = specialDoor.GetComponent<DoorInteractionComponent>();
            if (interactable != null)
            {
                interactable.SetCanInteract(true);
            }

            specialDoor.layer = LayerMask.NameToLayer("Interactable");
        }

        Debug.Log("DoorsClose: Все двери сброшены");
    }

    // Тестовые методы
    [ContextMenu("Тест: Взаимодействие с первой обычной дверью")]
    public void TestInteractFirstDoor()
    {
        if (doorObjects.Count > 0 && doorObjects[0] != null)
        {
            OnDoorInteract(doorObjects[0], false);
        }
    }

    [ContextMenu("Тест: Взаимодействие с особенной дверью")]
    public void TestInteractSpecialDoor()
    {
        if (specialDoor != null)
        {
            OnDoorInteract(specialDoor, true);
        }
    }

    [ContextMenu("Тест: Телепортация без диалога")]
    public void TestTeleport()
    {
        StartCoroutine(TeleportSequence());
    }

    [ContextMenu("Тест: Сбросить все двери")]
    public void TestResetAll()
    {
        ResetAllDoors();
    }

    [ContextMenu("Тест: Информация о дверях")]
    public void TestPrintDoorInfo()
    {
        Debug.Log($"DoorsClose: Обычных дверей: {doorObjects.Count}, Использовано: {usedDoors.Count}");

        foreach (var door in doorObjects)
        {
            if (door != null)
            {
                string usedStatus = usedDoors.Contains(door) ? "ИСПОЛЬЗОВАНА" : "ДОСТУПНА";
                Debug.Log($"Обычная дверь {door.name}: {usedStatus}");
            }
        }

        if (specialDoor != null)
        {
            string specialStatus = usedDoors.Contains(specialDoor) ? "ИСПОЛЬЗОВАНА" : "ДОСТУПНА";
            Debug.Log($"Особенная дверь {specialDoor.name}: {specialStatus}");
        }
    }

    void OnValidate()
    {
        // Автоподключение AudioSource если не назначен
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Рисуем обычные двери
        foreach (var door in doorObjects)
        {
            if (door != null)
            {
                Gizmos.color = usedDoors.Contains(door) ? Color.red : Color.green;
                Gizmos.DrawWireSphere(door.transform.position, 0.5f);

                // Текст состояния
                GUIStyle style = new GUIStyle();
                style.normal.textColor = usedDoors.Contains(door) ? Color.red : Color.green;
                style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
                string stateText = usedDoors.Contains(door) ? "ИСП" : "ОБЫЧ";
                UnityEditor.Handles.Label(door.transform.position + Vector3.up * 0.7f, stateText, style);
#endif
            }
        }

        // Рисуем особенную дверь
        if (specialDoor != null)
        {
            Gizmos.color = usedDoors.Contains(specialDoor) ? Color.magenta : Color.cyan;
            Gizmos.DrawWireSphere(specialDoor.transform.position, 0.7f);

            // Линия к точке телепортации
            if (teleportDestination != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(specialDoor.transform.position, teleportDestination.position);
                Gizmos.DrawWireSphere(teleportDestination.position, 0.5f);
            }

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.cyan;
            style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
            string stateText = usedDoors.Contains(specialDoor) ? "ОСОБ ИСП" : "ОСОБЕННАЯ";
            UnityEditor.Handles.Label(specialDoor.transform.position + Vector3.up * 1f, stateText, style);
            
            if (teleportDestination != null)
            {
                UnityEditor.Handles.Label(teleportDestination.position + Vector3.up * 0.5f, "ТЕЛЕПОРТ", style);
            }
#endif
        }
    }
}

public class DoorInteractionComponent : MonoBehaviour, IInteractable
{
    private DoorsClose doorsClose;
    private bool canInteract = true;
    private bool isSpecialDoor = false;

    public void SetDoorsClose(DoorsClose manager)
    {
        doorsClose = manager;
    }

    public void SetIsSpecialDoor(bool special)
    {
        isSpecialDoor = special;
    }

    public void SetCanInteract(bool interactable)
    {
        canInteract = interactable;
    }

    public string GetInteractionText()
    {
        if (!canInteract) return "";
        return isSpecialDoor ? "Особая дверь (E)" : "Нажмите E";
    }

    public void Interact()
    {
        if (!canInteract || doorsClose == null) return;

        canInteract = false;
        doorsClose.OnDoorInteract(this.gameObject, isSpecialDoor);
    }
}