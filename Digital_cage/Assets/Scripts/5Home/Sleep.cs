using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Sleep : MonoBehaviour, IInteractable
{
    [Header("Bed Setup")]
    public Transform sleepPoint; // Точка, где будет голова игрока на кровати
    public KeyCode sleepKey = KeyCode.E;
    public float getUpForwardOffset = 1f; // Смещение вперед при вставании

    [Header("Camera Settings")]
    public Transform cameraPivot; // Ссылка на камеру игрока
    public float cameraSleepX = -20f; // Наклон камеры в лежачем положении
    public float cameraStandX = 0f; // Наклон камеры в стоячем положении
    public Vector3 cameraSleepOffset = new Vector3(0f, -0.5f, 0f); // Смещение камеры при лежании
    public float cameraLerpSpeed = 4f; // Скорость плавного перемещения камеры

    [Header("Player Settings")]
    public float rotationLerpSpeed = 4f; // Скорость плавного поворота

    [Header("Black Screen Settings")]
    public Image blackScreenImage; // Ваш UI Image для черного экрана
    public float fadeToBlackDuration = 1.5f; // Длительность затемнения
    public float fadeFromBlackDuration = 1.5f; // Длительность прояснения

    [Header("Dialogue Settings")]
    public DialogueManager5 dialogueManager; // Ссылка на DialogueManager5
    [TextArea(2, 5)]
    public List<string> dialogueLines; // Список реплик для диалога
    public float delayAfterDialogue = 5f; // Задержка после диалога перед прояснением

    [Header("Todo Settings")]
    public float todoReplacementDuration = 2f; // Длительность замены пунктов Todo (используется для расчета времени)
    public TodoUI5 todoManager; // Ссылка на TodoUI5 для проверки выполнения первого пункта

    [Header("Audio Settings")]
    public AudioSource audioSource; // Аудиоисточник для будильника
    public AudioClip alarmClockSound; // Звук будильника (должен быть коротким, 1-3 секунды)
    [Range(0f, 1f)]
    public float alarmVolume = 0.7f; // Громкость будильника
    public bool playAlarmBeforeWake = true; // Включить/выключить будильник
    public float alarmDelayBeforeWake = 2f; // ЗАДЕРЖКА ПЕРЕД ПРОБУЖДЕНИЕМ (после начала будильника)

    private GameObject player;
    private CharacterController charController;
    private PlayerController playerController;
    private bool playerNearby = false;
    private bool isSleeping = false;
    private bool hasBeenUsed = false; // Флаг: использовали ли кровать
    public bool IsPlayerSleeping => isSleeping;

    private Vector3 cameraOriginalLocalPos;
    private float targetX;
    private float targetY;
    private bool isManualRotating = false;
    private bool sleepLocked = false; // Блокировка во время сна и диалога

    // События
    public System.Action OnPlayerLayDown;
    public System.Action OnPlayerWokeUp;
    public System.Action OnDialogueComplete;
    public System.Action OnAlarmRang; // Событие когда зазвонил будильник

    void Start()
    {
        // Устанавливаем начальный слой - Default (не интерактивен)
        gameObject.layer = LayerMask.NameToLayer("Default");

        player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Sleep: Player с тегом 'Player' не найден!");
            return;
        }

        charController = player.GetComponent<CharacterController>();
        playerController = player.GetComponent<PlayerController>();

        // Автоматически находим камеру игрока если не назначена
        if (cameraPivot == null)
        {
            if (playerController != null && playerController.playerCamera != null)
            {
                cameraPivot = playerController.playerCamera;
            }
            else
            {
                Camera playerCamera = player.GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    cameraPivot = playerCamera.transform;
                }
                else
                {
                    Debug.LogWarning("Sleep: Не найдена камера игрока!");
                }
            }
        }

        if (cameraPivot != null)
        {
            cameraOriginalLocalPos = cameraPivot.localPosition;
        }
        else
        {
            Debug.LogError("Sleep: Camera Pivot не назначен и не найден!");
        }

        // Настраиваем черный экран
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(false);
            Color color = blackScreenImage.color;
            color.a = 0f;
            blackScreenImage.color = color;
        }
        else
        {
            Debug.LogWarning("Sleep: Black Screen Image не назначен!");
        }

        // Проверяем наличие DialogueManager5
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager5>();
            if (dialogueManager == null)
            {
                Debug.LogWarning("Sleep: DialogueManager5 не найден!");
            }
        }

        // Автоматически находим TodoUI5 если не назначен
        if (todoManager == null)
        {
            todoManager = FindObjectOfType<TodoUI5>();
        }

        // Создаем AudioSource если не назначен
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = alarmVolume;
            Debug.Log("Sleep: Создан AudioSource для будильника");
        }

        // Запускаем проверку доступности кровати
        StartCoroutine(CheckBedAvailability());
    }

    /// <summary>
    /// Проверяем, можно ли использовать кровать (после выполнения первого пункта)
    /// </summary>
    private IEnumerator CheckBedAvailability()
    {
        // Ждем пока todoManager не будет назначен
        while (todoManager == null)
        {
            yield return new WaitForSeconds(0.5f);
            todoManager = FindObjectOfType<TodoUI5>();
        }

        Debug.Log("Sleep: TodoUI5 найден, проверяем выполнение первого пункта...");

        // Ждем пока первый пункт не будет выполнен
        while (!hasBeenUsed && !IsFirstTaskCompleted())
        {
            yield return new WaitForSeconds(0.3f);
        }

        // Если первый пункт выполнен и кровать еще не использовалась
        if (IsFirstTaskCompleted() && !hasBeenUsed)
        {
            // Делаем кровать интерактивной
            SetBedInteractable(true);
            Debug.Log("Sleep: Кровать теперь интерактивна (первый пункт выполнен)");
        }
    }

    /// <summary>
    /// Проверяем, выполнен ли первый пункт в TodoUI5
    /// </summary>
    private bool IsFirstTaskCompleted()
    {
        if (todoManager != null)
        {
            return todoManager.IsLightTaskCompleted();
        }
        return false;
    }

    /// <summary>
    /// Включить/выключить интерактивность кровати
    /// </summary>
    private void SetBedInteractable(bool interactable)
    {
        if (interactable && !hasBeenUsed)
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            Debug.Log("Sleep: Кровать установлена на слой Interactable");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            Debug.Log("Sleep: Кровать установлена на слой Default");
        }
    }

    void Update()
    {
        // Взаимодействие с кроватью (только если интерактивна)
        if (playerNearby && !isSleeping && Input.GetKeyDown(sleepKey) && !hasBeenUsed)
        {
            LayDown();
        }
        else if (isSleeping && Input.GetKeyDown(sleepKey) && !sleepLocked)
        {
            WakeUp();
        }

        // Плавное смещение камеры
        if (cameraPivot != null)
        {
            Vector3 targetPos = isSleeping
                ? cameraOriginalLocalPos + cameraSleepOffset
                : cameraOriginalLocalPos;

            cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, targetPos, Time.deltaTime * cameraLerpSpeed);
        }

        // Плавный поворот игрока по Y
        if (isSleeping && player != null && !isManualRotating)
        {
            Vector3 euler = player.transform.eulerAngles;
            float y = Mathf.LerpAngle(euler.y, targetY, Time.deltaTime * rotationLerpSpeed);
            player.transform.rotation = Quaternion.Euler(euler.x, y, euler.z);
        }

        // Плавное изменение вертикального угла камеры (X)
        if (playerController != null)
        {
            if (isSleeping)
            {
                playerController.CameraXRotation = Mathf.Lerp(playerController.CameraXRotation, targetX, Time.deltaTime * rotationLerpSpeed);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Sleep: Игрок рядом с кроватью");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("Sleep: Игрок отошел от кровати");
        }
    }

    /// <summary>
    /// Уложить игрока на кровать
    /// </summary>
    private void LayDown()
    {
        // Проверяем, можно ли использовать кровать
        if (player == null || sleepPoint == null || hasBeenUsed) return;

        Debug.Log("Sleep: Игрок ложится на кровать");
        hasBeenUsed = true; // Помечаем как использованную

        // Отключаем интерактивность кровати
        SetBedInteractable(false);

        // Отключаем CharacterController для точного позиционирования
        if (charController != null)
        {
            charController.enabled = false;
        }

        // Позиционируем игрока на кровати
        player.transform.position = sleepPoint.position;

        // Поворачиваем игрока в направлении кровати
        Vector3 lookDirection = sleepPoint.forward;
        lookDirection.y = 0; // Сохраняем только горизонтальное направление
        if (lookDirection != Vector3.zero)
        {
            player.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // Устанавливаем целевые углы камеры
        targetX = cameraSleepX;
        targetY = player.transform.eulerAngles.y;

        // Блокируем управление камерой при лежании
        if (playerController != null)
        {
            playerController.OnSitDown(); // Используем существующий метод для блокировки

            // Ограничиваем вертикальный обзор
            playerController.restrictVerticalLook = true;
            playerController.minSitX = cameraSleepX - 10f;
            playerController.maxSitX = cameraSleepX + 10f;

            // Ограничиваем горизонтальный обзор
            playerController.restrictHorizontalLook = true;
            playerController.minSitY = targetY - 45f;
            playerController.maxSitY = targetY + 45f;
        }

        isSleeping = true;
        playerNearby = false;

        // Вызываем событие
        OnPlayerLayDown?.Invoke();

        // Запускаем процесс сна с диалогом
        StartCoroutine(SleepAndDialogueSequence());
    }

    /// <summary>
    /// Разбудить игрока
    /// </summary>
    private void WakeUp()
    {
        if (player == null) return;

        Debug.Log("Sleep: Игрок просыпается");

        // Снимаем блокировку
        sleepLocked = false;
        isManualRotating = false;

        if (playerController != null)
        {
            playerController.restrictHorizontalLook = false;
            playerController.restrictVerticalLook = false;
        }

        // Включаем CharacterController
        if (charController != null)
        {
            charController.enabled = true;

            // Сдвигаем игрока немного вперед от кровати
            Vector3 forward = player.transform.forward * getUpForwardOffset;
            charController.Move(forward);
        }

        // Восстанавливаем управление
        if (playerController != null)
        {
            playerController.OnStandUp();
            playerController.restrictVerticalLook = false;
            playerController.restrictHorizontalLook = false;
        }

        isSleeping = false;

        // Вызываем событие
        OnPlayerWokeUp?.Invoke();
    }

    /// <summary>
    /// Проиграть звук будильника
    /// </summary>
    private void PlayAlarmSound()
    {
        if (audioSource != null && alarmClockSound != null && playAlarmBeforeWake)
        {
            audioSource.clip = alarmClockSound;
            audioSource.volume = alarmVolume;
            audioSource.Play();
            Debug.Log($"Sleep: Звук будильника проигрывается (длительность: {alarmClockSound.length:F2} сек)");

            // Вызываем событие
            OnAlarmRang?.Invoke();
        }
        else if (playAlarmBeforeWake)
        {
            Debug.LogWarning("Sleep: Не могу проиграть будильник - нет AudioSource или AudioClip!");
        }
    }

    /// <summary>
    /// Последовательность: черный экран ? диалог ? замена пункта ? будильник ? убрать черный экран ? пробуждение
    /// </summary>
    private System.Collections.IEnumerator SleepAndDialogueSequence()
    {
        // Блокируем возможность встать
        sleepLocked = true;

        // 1. Плавное появление черного экрана
        if (blackScreenImage != null)
        {
            yield return StartCoroutine(FadeBlackScreen(1f, fadeToBlackDuration));
        }
        else
        {
            yield return new WaitForSeconds(fadeToBlackDuration);
        }

        // 2. Запускаем диалог (если есть DialogueManager5 и реплики)
        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            Debug.Log($"Sleep: Запуск диалога, строк: {dialogueLines.Count}");

            // Ждем немного перед диалогом
            yield return new WaitForSeconds(0.5f);

            // Запускаем диалог и ждем его завершения
            bool dialogueFinished = false;
            dialogueManager.StartDialogue(dialogueLines, () => {
                dialogueFinished = true;
                OnDialogueComplete?.Invoke();
                Debug.Log("Sleep: Диалог завершен");
            });

            // Ждем завершения диалога
            yield return new WaitWhile(() => !dialogueFinished);
        }
        else
        {
            Debug.LogWarning("Sleep: Нет DialogueManager5 или реплик для диалога");
        }

        // 3. ЗАМЕНА ВТОРОГО ПУНКТА НА ТРЕТИЙ ВО ВРЕМЯ ЧЕРНОГО ЭКРАНА
        Debug.Log("Sleep: Заменяем второй пункт Todo на третий (во время черного экрана)...");

        // Ждем немного после диалога
        yield return new WaitForSeconds(0.5f);

        if (todoManager != null)
        {
            // Проверяем, что второй пункт активен (после замены первого)
            if (todoManager.IsSecondTaskActive() && !todoManager.IsThirdTaskActive())
            {
                Debug.Log("Sleep: Начинаем замену второго пункта на третий...");

                // ЗАМЕНЯЕМ ВТОРОЙ ПУНКТ НА ТРЕТИЙ
                // (все происходит "за кадром" - игрок не видит, т.к. черный экран)
                todoManager.CompleteSecondTask();

                // Ждем завершения всей анимации замены
                // Используем todoReplacementDuration вместо replacementFadeDuration
                yield return new WaitForSeconds(todoReplacementDuration);

                Debug.Log("Sleep: Замена пунктов завершена (игрок не видел анимацию)");
            }
            else if (todoManager.IsThirdTaskActive())
            {
                Debug.Log("Sleep: Третий пункт уже активен");
            }
            else
            {
                Debug.LogWarning("Sleep: Второй пункт не активен для замены");
            }
        }
        else
        {
            Debug.LogWarning("Sleep: TodoUI5 не найден!");
        }

        // 4. РАССЧЕТ ВРЕМЕНИ ДЛЯ БУДИЛЬНИКА
        // Время которое уже прошло: диалог + ожидание + замена Todo
        float timeSpentSoFar = 0.5f + 0.5f + todoReplacementDuration; // 0.5 перед диалогом + 0.5 после + замена

        // Рассчитываем сколько нужно ждать до момента когда нужно проиграть будильник
        // Будильник должен начаться за alarmDelayBeforeWake секунд ДО того как убрать черный экран
        float timeToStartAlarm = delayAfterDialogue - timeSpentSoFar - alarmDelayBeforeWake;

        if (timeToStartAlarm > 0)
        {
            Debug.Log($"Sleep: Ждем {timeToStartAlarm:F2} секунд до будильника...");
            yield return new WaitForSeconds(timeToStartAlarm);
        }
        else if (timeToStartAlarm < 0)
        {
            Debug.LogWarning($"Sleep: Время на будильник отрицательное ({timeToStartAlarm:F2} сек)! Увеличьте delayAfterDialogue.");
        }

        // 5. ПРОИГРЫВАЕМ БУДИЛЬНИК (он остановится сам по окончании аудиоклипа)
        Debug.Log($"Sleep: ЗВОНИТ БУДИЛЬНИК! Ждем {alarmDelayBeforeWake} секунд перед открытием глаз...");
        PlayAlarmSound();

        // 6. ЖДЕМ alarmDelayBeforeWake секунд ПОСЛЕ НАЧАЛА будильника
        // Звук будильника продолжит играть в фоне
        yield return new WaitForSeconds(alarmDelayBeforeWake);

        // 7. Плавное исчезновение черного экрана
        // Игрок откроет глаза, звук будильника все еще может играть (если аудио длиннее чем alarmDelayBeforeWake)
        Debug.Log("Sleep: Игрок открывает глаза...");
        if (blackScreenImage != null)
        {
            yield return StartCoroutine(FadeBlackScreen(0f, fadeFromBlackDuration));
        }
        else
        {
            yield return new WaitForSeconds(fadeFromBlackDuration);
        }

        // 8. Разблокируем возможность встать
        sleepLocked = false;

        Debug.Log("Sleep: Последовательность сна завершена. Игрок проснулся от будильника.");
    }

    /// <summary>
    /// Плавное изменение прозрачности черного экрана (UI Image)
    /// </summary>
    private System.Collections.IEnumerator FadeBlackScreen(float targetAlpha, float duration)
    {
        if (blackScreenImage == null) yield break;

        // Включаем Image если выключен
        if (!blackScreenImage.gameObject.activeSelf)
        {
            blackScreenImage.gameObject.SetActive(true);
        }

        Color startColor = blackScreenImage.color;
        Color endColor = startColor;
        endColor.a = targetAlpha;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            blackScreenImage.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        blackScreenImage.color = endColor;

        // Выключаем Image если полностью прозрачный
        if (targetAlpha <= 0.01f)
        {
            blackScreenImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Принудительно разбудить игрока
    /// </summary>
    public void ForceWakeUp()
    {
        if (isSleeping)
        {
            StopAllCoroutines(); // Останавливаем все корутины

            // Останавливаем звук будильника если играет
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("Sleep: Звук будильника принудительно остановлен");
            }

            // Немедленно убираем черный экран
            if (blackScreenImage != null)
            {
                blackScreenImage.gameObject.SetActive(false);
            }

            // Останавливаем диалог если он идет
            if (dialogueManager != null)
            {
                dialogueManager.ForceEndDialogue();
            }

            WakeUp();
        }
    }

    /// <summary>
    /// Установить новые реплики для диалога
    /// </summary>
    public void SetDialogueLines(List<string> newLines)
    {
        if (newLines != null)
        {
            dialogueLines = newLines;
            Debug.Log($"Sleep: Установлено {newLines.Count} реплик");
        }
    }

    /// <summary>
    /// Добавить реплику в конец списка
    /// </summary>
    public void AddDialogueLine(string line)
    {
        if (dialogueLines == null)
        {
            dialogueLines = new List<string>();
        }

        dialogueLines.Add(line);
        Debug.Log($"Sleep: Добавлена реплика: '{line}'");
    }

    // Реализация IInteractable
    public string GetInteractionText()
    {
        // Если кровать уже использована - не показываем текст
        if (hasBeenUsed)
        {
            return "";
        }

        // Если первый пункт не выполнен - не показываем текст
        if (!IsFirstTaskCompleted())
        {
            return "";
        }

        if (isSleeping)
        {
            return sleepLocked ? "..." : "Встать (E)";
        }
        return "Лечь спать (E)";
    }

    public void Interact()
    {
        // Не взаимодействуем если уже использовано или первый пункт не выполнен
        if (hasBeenUsed || !IsFirstTaskCompleted()) return;

        if (isSleeping && !sleepLocked)
        {
            WakeUp();
        }
        else if (!isSleeping)
        {
            LayDown();
        }
    }

    void OnValidate()
    {
        // Автоматическое назначение камеры в редакторе
        if (cameraPivot == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                PlayerController pc = playerObj.GetComponent<PlayerController>();
                if (pc != null && pc.playerCamera != null)
                {
                    cameraPivot = pc.playerCamera;
                }
            }
        }

        // Автоматическое назначение DialogueManager5
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager5>();
        }

        // Автоматическое назначение TodoUI5
        if (todoManager == null)
        {
            todoManager = FindObjectOfType<TodoUI5>();
        }

        // Проверяем длительность аудио будильника
        if (alarmClockSound != null)
        {
            Debug.Log($"Sleep: Будильник длится {alarmClockSound.length:F2} секунд");
            if (alarmClockSound.length > 5f)
            {
                Debug.LogWarning("Sleep: Аудио будильника довольно длинное! Рекомендуется использовать короткий звук (1-3 сек).");
            }
        }
    }

    void OnDrawGizmos()
    {
        if (sleepPoint != null)
        {
            // Цвет меняется в зависимости от состояния
            if (hasBeenUsed)
            {
                Gizmos.color = Color.gray; // Использована
            }
            else if (IsFirstTaskCompleted())
            {
                Gizmos.color = Color.green; // Готова к использованию
            }
            else
            {
                Gizmos.color = Color.red; // Недоступна
            }

            Gizmos.DrawWireSphere(sleepPoint.position, 0.2f);
            Gizmos.DrawLine(sleepPoint.position, sleepPoint.position + sleepPoint.forward * 0.5f);

            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
            Gizmos.DrawSphere(sleepPoint.position, 0.1f);
        }
    }
}