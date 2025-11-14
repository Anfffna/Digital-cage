using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using TMPro;

public class AdventDoor : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public List<GameObject> doorObjects = new List<GameObject>();
    public GameObject specificFirstDoor;
    public GameObject excludedDoor; // Дверь которую исключаем из плашки "Заперто"

    [Header("Decoration Objects")]
    public List<GameObject> decorationObjects = new List<GameObject>(); // Декоративные объекты без взаимодействия

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;

    [Header("Interaction Settings")]
    public GameObject lockedTextPanel; // UI плашка с текстом "Заперто"
    public TextMeshProUGUI lockedText; // Текст "Заперто"

    [Header("Loading Screen")]
    public LoadingScreen loadingScreen; // Перетащи LoadingScreen в инспекторе

    [Header("Excluded Door Dialogue")]
    public ManagerDialogue2 dialogueManager;
    [TextArea(2, 5)]
    public List<string> excludedDoorDialogue;

    private bool doorsSpawned = false;
    public bool DoorsSpawned => doorsSpawned;
    private Coroutine spawnCoroutine;
    private List<GameObject> spawnedDoors = new List<GameObject>();
    private HashSet<GameObject> usedDoors = new HashSet<GameObject>();
    private int totalRegularDoors; // Общее количество обычных дверей (без excludedDoor)

    void Start()
    {
        HideAllDoors();

        if (lockedTextPanel != null)
        {
            lockedTextPanel.SetActive(false);
        }

        // Считаем сколько обычных дверей (без excludedDoor)
        totalRegularDoors = doorObjects.Count(door => door != excludedDoor);
    }

    public void SpawnRandomDoors()
    {
        if (doorsSpawned) return;
        if (doorObjects.Count == 0) return;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnDoorsWithInterval());
    }

    private IEnumerator SpawnDoorsWithInterval()
    {
        doorsSpawned = true;

        List<GameObject> doorsToSpawn = new List<GameObject>(doorObjects);

        // Убираем конкретную дверь из общего списка если она там есть
        if (specificFirstDoor != null && doorsToSpawn.Contains(specificFirstDoor))
        {
            doorsToSpawn.Remove(specificFirstDoor);
        }

        // Убираем исключенную дверь из общего списка если она там есть
        if (excludedDoor != null && doorsToSpawn.Contains(excludedDoor))
        {
            doorsToSpawn.Remove(excludedDoor);
        }

        // Перемешиваем оставшиеся двери
        var shuffledDoors = doorsToSpawn.OrderBy(x => Random.value).ToList();

        // ПОКАЗЫВАЕМ КОНКРЕТНУЮ ДВЕРЬ ПЕРВОЙ
        if (specificFirstDoor != null)
        {
            SetupDoorInteraction(specificFirstDoor);
            specificFirstDoor.SetActive(true);
            spawnedDoors.Add(specificFirstDoor);
            yield return new WaitForSeconds(spawnInterval);
        }

        // ПОКАЗЫВАЕМ ИСКЛЮЧЕННУЮ ДВЕРЬ В СЛУЧАЙНОМ ПОРЯДКЕ
        if (excludedDoor != null)
        {
            // Добавляем исключенную дверь в случайное место среди остальных
            int randomIndex = Random.Range(0, shuffledDoors.Count + 1);
            shuffledDoors.Insert(randomIndex, excludedDoor);
        }

        // ПОКАЗЫВАЕМ ВСЕ ДВЕРИ ПО ОДНОЙ С ИНТЕРВАЛОМ
        foreach (var door in shuffledDoors)
        {
            if (door != null)
            {
                SetupDoorInteraction(door);
                door.SetActive(true);
                spawnedDoors.Add(door);
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // ПОКАЗЫВАЕМ ДЕКОРАТИВНЫЕ ОБЪЕКТЫ (без взаимодействия)
        yield return StartCoroutine(SpawnDecorationObjects());
    }

    /// <summary>
    /// Показывает декоративные объекты без взаимодействия
    /// </summary>
    private IEnumerator SpawnDecorationObjects()
    {
        foreach (var decoration in decorationObjects)
        {
            if (decoration != null)
            {
                // Устанавливаем слой Default чтобы не было взаимодействия
                decoration.layer = LayerMask.NameToLayer("Default");
                decoration.SetActive(true);
                yield return new WaitForSeconds(spawnInterval * 0.5f); // Более быстрый интервал для декораций
            }
        }
    }

    /// <summary>
    /// Настраивает взаимодействие для двери
    /// </summary>
    private void SetupDoorInteraction(GameObject door)
    {
        // Устанавливаем слой Interactable только для интерактивных дверей
        door.layer = LayerMask.NameToLayer("Interactable");

        // Добавляем компонент InteractableDoor если его нет
        InteractableDoor interactable = door.GetComponent<InteractableDoor>();
        if (interactable == null)
        {
            interactable = door.AddComponent<InteractableDoor>();
        }

        // Настраиваем компонент
        interactable.SetAdventDoor(this);
        interactable.SetIsExcludedDoor(door == excludedDoor);
    }

    /// <summary>
    /// Вызывается когда игрок взаимодействует с дверью
    /// </summary>
    public void OnDoorInteract(GameObject door, bool isExcludedDoor)
    {
        if (usedDoors.Contains(door)) return;

        // СРАЗУ делаем дверь неинтерактивной и добавляем в usedDoors
        usedDoors.Add(door);

        // Получаем компонент InteractableDoor и отключаем взаимодействие
        InteractableDoor interactable = door.GetComponent<InteractableDoor>();
        if (interactable != null)
        {
            interactable.SetCanInteract(false);
        }

        door.layer = LayerMask.NameToLayer("Default");

        // Если это excluded door - запускаем диалог
        if (isExcludedDoor && dialogueManager != null && excludedDoorDialogue != null && excludedDoorDialogue.Count > 0)
        {
            dialogueManager.StartDialogue(excludedDoorDialogue);

            // ЗАПУСКАЕМ LOADING SCREEN ЧЕРЕЗ 4 СЕКУНДЫ ПОСЛЕ НАЧАЛА ДИАЛОГА
            StartCoroutine(StartLoadingAfterDelay(6f));
        }
        else
        {
            // Обычное поведение для других дверей - показываем "Заперто"
            ShowLockedText();

            // Проверяем, были ли использованы ВСЕ обычные двери (кроме excludedDoor)
            CheckAllRegularDoorsUsed();
        }
    }

    /// <summary>
    /// Запускает loading screen через заданное время и скрывает диалог
    /// </summary>
    private IEnumerator StartLoadingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Скрываем диалог
        if (dialogueManager != null && dialogueManager.dialoguePanel != null)
        {
            dialogueManager.dialoguePanel.SetActive(false);
        }

        // Запускаем loading screen
        LoadingScreen loadingScreen = FindObjectOfType<LoadingScreen>();
        if (loadingScreen != null)
        {
            loadingScreen.StartLoadingScreen();
        }
    }

    /// <summary>
    /// Проверяет, были ли использованы все обычные двери
    /// </summary>
    private void CheckAllRegularDoorsUsed()
    {
        // Считаем сколько обычных дверей уже использовано
        int usedRegularDoors = usedDoors.Count(door => door != excludedDoor);

        // Если использованы ВСЕ обычные двери - скрываем specificFirstDoor
        if (usedRegularDoors >= totalRegularDoors && specificFirstDoor != null)
        {
            specificFirstDoor.SetActive(false);
            Debug.Log("Все обычные двери использованы - скрываем specificFirstDoor");
        }
    }

    /// <summary>
    /// Показывает плашку с текстом "Заперто"
    /// </summary>
    private void ShowLockedText()
    {
        if (lockedTextPanel != null)
        {
            lockedTextPanel.SetActive(true);

            // Скрываем плашку через 2 секунды
            StartCoroutine(HideLockedTextAfterDelay(2f));
        }
    }

    private IEnumerator HideLockedTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (lockedTextPanel != null)
        {
            lockedTextPanel.SetActive(false);
        }
    }

    [ContextMenu("Spawn Doors Now")]
    public void ForceSpawnDoors()
    {
        SpawnRandomDoors();
    }

    [ContextMenu("Hide All Doors")]
    public void HideAllDoors()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        // Скрываем ВСЕ двери включая исключенную
        foreach (var door in doorObjects)
        {
            if (door != null)
                door.SetActive(false);
        }

        // Скрываем декоративные объекты
        foreach (var decoration in decorationObjects)
        {
            if (decoration != null)
                decoration.SetActive(false);
        }

        if (specificFirstDoor != null && !doorObjects.Contains(specificFirstDoor))
        {
            specificFirstDoor.SetActive(false);
        }

        if (excludedDoor != null && !doorObjects.Contains(excludedDoor))
        {
            excludedDoor.SetActive(false);
        }

        // Очищаем списки
        spawnedDoors.Clear();
        usedDoors.Clear();
        doorsSpawned = false;

        // Скрываем плашку
        if (lockedTextPanel != null)
        {
            lockedTextPanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    // Реализация IInteractable (для совместимости, но основной функционал в InteractableDoor)
    public string GetInteractionText()
    {
        return "";
    }

    public void Interact()
    {
        // Не используется здесь
    }
}

// Отдельный компонент для взаимодействия с дверями
public class InteractableDoor : MonoBehaviour, IInteractable
{
    private AdventDoor adventDoor;
    private bool canInteract = true;
    private bool isExcludedDoor = false;
    private Animator doorAnimator;

    void Start()
    {
        // Получаем компонент аниматора
        doorAnimator = GetComponent<Animator>();
    }

    public void SetAdventDoor(AdventDoor doorManager)
    {
        adventDoor = doorManager;
    }

    public void SetIsExcludedDoor(bool excluded)
    {
        isExcludedDoor = excluded;
    }

    public void SetCanInteract(bool interactable)
    {
        canInteract = interactable;
    }

    public string GetInteractionText()
    {
        if (!canInteract) return "";
        return "Нажмите E";
    }

    public void Interact()
    {
        if (!canInteract) return;

        canInteract = false;

        // ЗАПУСКАЕМ АНИМАЦИЮ ОТКРЫТИЯ ДВЕРИ
        if (doorAnimator != null)
        {
            doorAnimator.Play("open_door");
        }

        // СРАЗУ делаем объект неинтерактивным
        gameObject.layer = LayerMask.NameToLayer("Default");

        adventDoor?.OnDoorInteract(this.gameObject, isExcludedDoor);
    }
}