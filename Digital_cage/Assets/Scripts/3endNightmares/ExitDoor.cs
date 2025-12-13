using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ExitDoor : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public bool isLocked = true;

    [Header("Dependencies")]
    public ExitLock exitLock;

    [Header("Animation")]
    public Animator doorAnimator;
    public string openAnimationName = "Open";
    public string closeAnimationName = "Close";

    [Header("Dialogue Settings")]
    public ManagerDialogue3 dialogueManager;
    [TextArea(2, 5)]
    public List<string> afterCloseDialogueLines;

    [Header("Todo Settings")]
    public LightTodoUIManager lightTodoManager;
    public ScaryToDo scaryTodoManager;

    [Header("Shadow Settings")]
    public ShadowExit shadowExit;

    [Header("Camera Settings")]
    public float cameraDropHeight = 1.2f;
    public float cameraDropDuration = 2f;

    [Header("UI Settings")]
    public Image blackScreen;
    public float blackScreenFadeDuration = 2f;

    [Header("Audio Settings")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip mainBackgroundMusic;
    public AudioSource audioSource;
    public AudioSource backgroundMusicSource;

    [Header("Test Mode")]
    public bool testMode = false; // Включить режим тестирования
    public float testModeDelay = 1f; // Задержка перед запуском теста

    [Header("Scene Transition")]
    public float waitAfterDialogues = 3f; // Ждать 3 секунды после диалогов
    public string nextSceneName = "4Office"; // Имя следующей сцены

    private bool isOpen = false;
    private bool doorAutoOpened = false;
    private bool hasBeenInteracted = false;
    private bool shadowsStarted = false;
    private bool isExitDoorDialogueActive = false;
    private bool cameraDropped = false;
    private bool allDialoguesCompleted = false;
    private Transform playerCamera;
    private Vector3 originalCameraLocalPosition;

    void Start()
    {
        if (isLocked)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }

        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (backgroundMusicSource == null)
        {
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            backgroundMusicSource.loop = true;
            backgroundMusicSource.volume = 0.7f;
        }

        SetupBlackScreen();
        FindPlayerCamera();

        if (exitLock != null)
        {
            exitLock.OnExitLockDialogueIndex += OnExitLockDialogueIndex;
            Debug.Log("ExitDoor: Подписался на событие ExitLock");
        }

        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached += OnGlobalDialogueIndexReached;
        }

        // === ТЕСТОВЫЙ РЕЖИМ ===
        if (testMode)
        {
            Debug.Log("=== ТЕСТОВЫЙ РЕЖИМ АКТИВЕН ===");
            Debug.Log("Дверь будет автоматически открыта и сразу начнется финальная последовательность");
            StartCoroutine(TestModeSequence());
        }
    }

    private void SetupBlackScreen()
    {
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.color = new Color(0, 0, 0, 0);
        }
        else
        {
            Debug.LogWarning("ExitDoor: BlackScreen не назначен!");
        }
    }

    private void FindPlayerCamera()
    {
        GameObject cameraObject = GameObject.Find("PlayerCamera");
        if (cameraObject != null)
        {
            playerCamera = cameraObject.transform;
            originalCameraLocalPosition = playerCamera.localPosition;
            Debug.Log($"ExitDoor: Камера найдена! Локальная позиция: {originalCameraLocalPosition}");
        }
        else
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerCamera = mainCamera.transform;
                originalCameraLocalPosition = playerCamera.localPosition;
                Debug.Log($"ExitDoor: Камера найдена через Main Camera. Локальная позиция: {originalCameraLocalPosition}");
            }
            else
            {
                Debug.LogWarning("ExitDoor: Камера PlayerCamera не найдена!");
            }
        }
    }

    private void OnExitLockDialogueIndex(int lineIndex)
    {
        Debug.Log($"ExitDoor: Получен индекс диалога ExitLock: {lineIndex}");

        if (lineIndex == 3 && !doorAutoOpened)
        {
            AutoOpenDoor();
        }
    }

    private void OnGlobalDialogueIndexReached(int lineIndex)
    {
        if (isExitDoorDialogueActive)
        {
            Debug.Log($"ExitDoor: Получен индекс нашего диалога: {lineIndex}");

            // Запускаем тени после 3 индекса нашего диалога
            if (lineIndex == 3 && !shadowsStarted && shadowExit != null)
            {
                StartShadows();
            }

            // Опускаем камеру после предпоследней реплики (индекс 6)
            if (lineIndex == 6 && !cameraDropped)
            {
                StartCameraDrop();
            }

            // Проверяем, завершился ли диалог (последняя строка)
            if (lineIndex >= afterCloseDialogueLines.Count)
            {
                Debug.Log($"ExitDoor: Достигнут конец диалога (строка {lineIndex} из {afterCloseDialogueLines.Count})");
                OnAllDialoguesFinished();
            }
        }
    }

    private void StartCameraDrop()
    {
        if (playerCamera != null)
        {
            StartCoroutine(DropCameraCoroutine());
            Debug.Log("ExitDoor: Начинаем опускание камеры...");
        }
        else
        {
            Debug.LogWarning("ExitDoor: Камера не найдена для опускания!");
        }
    }

    private IEnumerator DropCameraCoroutine()
    {
        float startHeight = playerCamera.localPosition.y;
        float targetHeight = cameraDropHeight;
        float timer = 0f;

        Debug.Log($"ExitDoor: Опускаем камеру с {startHeight} до {targetHeight}");

        while (timer < cameraDropDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / cameraDropDuration;

            float currentHeight = Mathf.Lerp(startHeight, targetHeight, progress);

            Vector3 newLocalPosition = playerCamera.localPosition;
            newLocalPosition.y = currentHeight;
            playerCamera.localPosition = newLocalPosition;

            yield return null;
        }

        Vector3 finalLocalPosition = playerCamera.localPosition;
        finalLocalPosition.y = targetHeight;
        playerCamera.localPosition = finalLocalPosition;

        cameraDropped = true;
        Debug.Log("ExitDoor: Камера опущена!");
    }

    private void StartBlackScreen()
    {
        if (blackScreen != null)
        {
            StartCoroutine(FadeBlackScreen());
            Debug.Log("ExitDoor: Запускаем черный экран после диалогов!");
        }
        else
        {
            Debug.LogWarning("ExitDoor: BlackScreen не назначен!");
        }
    }

    private IEnumerator FadeBlackScreen()
    {
        if (blackScreen == null)
        {
            yield break;
        }

        // БЛОКИРОВКА ДВИЖЕНИЯ ИГРОКА
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
            Debug.Log("ExitDoor: Движение игрока заблокировано");
        }
        else
        {
            Debug.LogWarning("ExitDoor: PlayerController не найден!");
        }

        blackScreen.gameObject.SetActive(true);
        float timer = 0.0f;

        Debug.Log("ExitDoor: Начинаем плавное появление черного экрана");

        float initialVolume = audioSource != null ? audioSource.volume : 0.0f;
        float targetVolume = 0.1f;

        while (timer < blackScreenFadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / blackScreenFadeDuration;

            float alpha = Mathf.Lerp(0.0f, 1.0f, progress);
            blackScreen.color = new Color(0, 0, 0, alpha);

            if (audioSource != null)
            {
                float currentVolume = Mathf.Lerp(initialVolume, targetVolume, progress);
                audioSource.volume = currentVolume;
            }

            yield return null;
        }

        blackScreen.color = new Color(0, 0, 0, 1.0f);

        if (audioSource != null)
        {
            audioSource.volume = targetVolume;
            Debug.Log("ExitDoor: Громкость аудио источника плавно установлена на " + targetVolume);
        }

        Debug.Log("ExitDoor: Черный экран полностью показан!");
        OnFinalSequenceComplete();
    }

    private void OnFinalSequenceComplete()
    {
        Debug.Log("ExitDoor: ФИНАЛЬНАЯ ПОСЛЕДОВАТЕЛЬНОСТЬ ЗАВЕРШЕНА!");

        // Запускаем переход на сцену через 2 секунды
        StartCoroutine(LoadNextSceneAfterDelay(2f));
    }

    private IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        Debug.Log($"ExitDoor: Ждем {delay} секунд перед переходом на сцену '{nextSceneName}'");

        yield return new WaitForSeconds(delay);

        Debug.Log($"ExitDoor: Переход на сцену '{nextSceneName}'...");

        // Загружаем сцену
        SceneManager.LoadScene(nextSceneName);
    }

    private void StartShadows()
    {
        shadowsStarted = true;
        shadowExit.StartShadows();
        Debug.Log("ExitDoor: Запущены тени после 3 индекса диалога ExitDoor!");
    }

    private void AutoOpenDoor()
    {
        doorAutoOpened = true;
        isOpen = true;
        isLocked = false;

        PlaySound(openSound);

        if (doorAnimator != null)
        {
            doorAnimator.Play(openAnimationName);
            Debug.Log($"ExitDoor: Дверь автоматически открыта анимацией {openAnimationName}");
        }

        gameObject.layer = LayerMask.NameToLayer("Interactable");
        Debug.Log("ExitDoor: Дверь автоматически открыта на 3 индексе диалога!");
    }

    public string GetInteractionText()
    {
        if (isLocked || hasBeenInteracted)
            return "";

        return "Выход";
    }

    public void Interact()
    {
        if (isLocked || hasBeenInteracted) return;

        hasBeenInteracted = true;
        gameObject.layer = LayerMask.NameToLayer("Default");

        if (exitLock != null)
        {
            exitLock.StartDoorInteractionEffects();
            Debug.Log("ExitDoor: Вызваны эффекты взаимодействия с дверью");
        }

        CloseDoor();

        SwitchToScaryTodo();

        StartBackgroundMusic();

        StartAfterCloseDialogue();

        Debug.Log("ExitDoor: Игрок взаимодействует с открытой дверью");
    }

    private void CloseDoor()
    {
        isOpen = false;

        PlaySound(closeSound);

        if (doorAnimator != null)
        {
            doorAnimator.Play(closeAnimationName);
            Debug.Log($"ExitDoor: Дверь захлопнута анимацией {closeAnimationName}");
        }

        Debug.Log("ExitDoor: Дверь захлопнута после взаимодействия");
    }

    private void StartBackgroundMusic()
    {
        if (mainBackgroundMusic != null && backgroundMusicSource != null)
        {
            backgroundMusicSource.clip = mainBackgroundMusic;
            backgroundMusicSource.Play();
            Debug.Log("ExitDoor: Запущена фоновая страшная музыка!");
        }
        else
        {
            if (mainBackgroundMusic == null)
                Debug.LogWarning("ExitDoor: MainBackgroundMusic не назначен!");
            if (backgroundMusicSource == null)
                Debug.LogWarning("ExitDoor: BackgroundMusicSource не найден!");
        }
    }

    private void SwitchToScaryTodo()
    {
        if (lightTodoManager != null)
        {
            lightTodoManager.HideTodoList();
            Debug.Log("ExitDoor: Обычный туду скрыт");
        }

        if (scaryTodoManager != null)
        {
            scaryTodoManager.ShowTaskList();
            Debug.Log("ExitDoor: Страшный туду показан");
        }
        else
        {
            Debug.LogWarning("ExitDoor: ScaryTodoManager не назначен!");
        }
    }

    private void StartAfterCloseDialogue()
    {
        if (dialogueManager != null && afterCloseDialogueLines != null && afterCloseDialogueLines.Count > 0)
        {
            isExitDoorDialogueActive = true;
            // Запускаем диалог с callback на завершение
            dialogueManager.StartDialogue(afterCloseDialogueLines, OnDialogueManagerComplete);
            Debug.Log("ExitDoor: Запущен диалог после захлопывания двери");
        }
        else
        {
            Debug.LogWarning("ExitDoor: DialogueManager или afterCloseDialogueLines не назначены!");
            // Если нет диалогов, сразу переходим к финальной последовательности
            OnAllDialoguesFinished();
        }
    }

    private void OnDialogueManagerComplete()
    {
        Debug.Log("ExitDoor: DialogueManager сообщил о завершении диалога");
        OnAllDialoguesFinished();
    }

    private void OnAllDialoguesFinished()
    {
        if (allDialoguesCompleted) return; // Уже вызывали

        allDialoguesCompleted = true;
        isExitDoorDialogueActive = false;

        Debug.Log($"ExitDoor: ВСЕ диалоги завершены! Ждем {waitAfterDialogues} секунд...");

        // Ждем указанное время после диалогов
        StartCoroutine(WaitAfterDialogues());
    }

    private IEnumerator WaitAfterDialogues()
    {
        yield return new WaitForSeconds(waitAfterDialogues);

        Debug.Log("ExitDoor: Время ожидания после диалогов прошло, запускаем финальную последовательность");

        // Теперь запускаем черный экран и финальную последовательность
        StartBlackScreen();
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
            Debug.Log("ExitDoor: Фоновая музыка остановлена");
        }
    }

    void OnDestroy()
    {
        if (exitLock != null)
        {
            exitLock.OnExitLockDialogueIndex -= OnExitLockDialogueIndex;
        }

        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnGlobalDialogueIndexReached;
        }
    }

    private IEnumerator TestModeSequence()
    {
        yield return new WaitForSeconds(testModeDelay);

        // 1. Автоматически открываем дверь
        doorAutoOpened = true;
        isOpen = true;
        isLocked = false;

        if (doorAnimator != null)
        {
            doorAnimator.Play(openAnimationName);
        }
        Debug.Log("ТЕСТ: Дверь автоматически открыта");

        // 2. Пропускаем диалоги и сразу запускаем финальную последовательность
        hasBeenInteracted = true;

        // 3. Запускаем фоную музыку
        StartBackgroundMusic();

        // 4. Показываем страшный туду
        SwitchToScaryTodo();

        // 5. Пропускаем все и сразу запускаем финальные эффекты
        if (dialogueManager != null && afterCloseDialogueLines.Count > 0)
        {
            // Запускаем диалог, но сразу переходим к концу
            isExitDoorDialogueActive = true;
            dialogueManager.StartDialogue(afterCloseDialogueLines);

            // Форсируем завершение диалогов
            allDialoguesCompleted = true;

            // Ждем немного
            yield return new WaitForSeconds(1f);

            // Запускаем тени
            if (shadowExit != null && !shadowsStarted)
            {
                shadowsStarted = true;
                shadowExit.StartShadows();
            }

            // Опускаем камеру
            if (playerCamera != null && !cameraDropped)
            {
                StartCameraDrop();
            }

            // Ждем время после диалогов и запускаем черный экран
            yield return new WaitForSeconds(waitAfterDialogues);

            // Включаем черный экран
            if (blackScreen != null)
            {
                StartCoroutine(FadeBlackScreen());
            }
        }
        else
        {
            // Если нет диалога, просто запускаем финальные эффекты
            if (playerCamera != null)
            {
                StartCameraDrop();
            }

            // Ждем время после диалогов
            yield return new WaitForSeconds(waitAfterDialogues);

            if (blackScreen != null)
            {
                StartCoroutine(FadeBlackScreen());
            }
        }

        Debug.Log("ТЕСТ: Запущена финальная последовательность!");
    }
}