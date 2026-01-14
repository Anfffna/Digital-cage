using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SmsMama : MonoBehaviour
{
    [Header("HoursLater Reference")]
    public HoursLater hoursLaterScript;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip ringtoneClip;

    [Header("Animation")]
    public Animator handAnimator;
    public string handUpTrigger = "HandUp";
    public string handDownTrigger = "HandDown";

    [Header("Camera Zoom Settings")]
    public Camera playerCamera;
    public float zoomAmount = 2f;
    public float zoomDuration = 1f;
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Dialogue Settings")]
    public ManagerDialogue6 dialogueManager;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Timing Settings")]
    public float delayAfterHoursDialogue = 3f;
    public float delayBeforeHandUp = 0.5f;
    public float delayAfterHandUp = 0.5f;
    public int handDownLineIndex = 2; // Опустить руку ПОСЛЕ этой строки
    public float handDownDelay = 0.5f;
    public bool disableTodoPanel = true;

    private bool hasTriggered = false;
    private bool handUpTriggered = false;
    private bool handDownTriggered = false;
    private Coroutine activationCoroutine;
    private Coroutine zoomCoroutine;
    private float originalFOV;
    private Transform cameraTransform;
    private Vector3 originalCameraPosition;

    void Start()
    {
        // Находим ссылки если не назначены
        if (hoursLaterScript == null)
            hoursLaterScript = FindObjectOfType<HoursLater>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        if (handAnimator == null)
            Debug.LogWarning("SmsMama: Не назначен Animator для руки!");

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<ManagerDialogue6>();

        if (dialogueLines == null || dialogueLines.Count == 0)
            Debug.LogWarning("SmsMama: Не настроены строки диалога!");

        // Настраиваем камеру
        SetupCamera();

        // Сброс состояния
        hasTriggered = false;
        handUpTriggered = false;
        handDownTriggered = false;

        Debug.Log("SmsMama: Инициализирован");
    }

    void SetupCamera()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
                Debug.LogWarning("SmsMama: Камера не найдена автоматически!");
            }
        }

        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
            cameraTransform = playerCamera.transform;
            originalCameraPosition = cameraTransform.localPosition;
            Debug.Log($"SmsMama: Камера настроена. FOV: {originalFOV}");
        }
    }

    public void StartSmsSequence()
    {
        if (hasTriggered)
        {
            Debug.Log("SmsMama: Уже срабатывал ранее, пропускаем");
            return;
        }

        Debug.Log("=== SmsMama.StartSmsSequence() ВЫЗВАН ===");
        hasTriggered = true;

        if (activationCoroutine != null)
            StopCoroutine(activationCoroutine);

        activationCoroutine = StartCoroutine(SmsSequence());
    }

    private IEnumerator SmsSequence()
    {
        Debug.Log("SmsMama: Начинаю SMS последовательность...");

        // 1. Ждем паузу после предыдущего диалога
        yield return new WaitForSeconds(delayAfterHoursDialogue);

        // 2. Начинаем звонок телефона
        StartPhoneRinging();

        // 3. Ждем перед взятием трубки
        yield return new WaitForSeconds(2f);

        // 4. Поднимаем руку (берем трубку)
        Debug.Log("SmsMama: Поднимаем руку (берем трубку)...");

        if (delayBeforeHandUp > 0)
            yield return new WaitForSeconds(delayBeforeHandUp);

        if (handAnimator != null && !string.IsNullOrEmpty(handUpTrigger))
        {
            Debug.Log($"SmsMama: Триггерим анимацию '{handUpTrigger}'");
            handAnimator.SetTrigger(handUpTrigger);
            handUpTriggered = true;

            // Запускаем плавный зум камеры
            StartCameraZoomIn();
        }

        if (delayAfterHandUp > 0)
            yield return new WaitForSeconds(delayAfterHandUp);

        // 5. Запускаем диалог
        if (dialogueLines != null && dialogueLines.Count > 0 && dialogueManager != null)
        {
            Debug.Log($"SmsMama: Запускаем диалог ({dialogueLines.Count} строк, опускание ПОСЛЕ строки {handDownLineIndex})");

            // Отключаем TODO панель
            if (disableTodoPanel)
                DisableTodoPanel();

            // Подписываемся на события диалога
            SubscribeToDialogueEvents();

            // Запускаем диалог
            dialogueManager.StartDialogue(dialogueLines);

            // Запускаем корутину для слежения за завершением диалога
            StartCoroutine(WaitForDialogueCompletion());
        }
        else
        {
            Debug.LogWarning("SmsMama: Не могу запустить диалог!");
            yield return new WaitForSeconds(3f);
            StartCoroutine(TriggerHandDown());
        }
    }

    void SubscribeToDialogueEvents()
    {
        Debug.Log("SmsMama: Подписываюсь на OnDialogueIndexReached");
        dialogueManager.OnDialogueIndexReached += OnDialogueLineChanged;
    }

    void UnsubscribeFromDialogueEvents()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueLineChanged;
            Debug.Log("SmsMama: Отписался от OnDialogueIndexReached");
        }
    }

    void OnDialogueLineChanged(int lineIndex)
    {
        Debug.Log($"SmsMama: OnDialogueLineChanged вызван! Строка: {lineIndex}");

        // Проверяем, достигли ли мы строки для опускания руки
        if (lineIndex >= handDownLineIndex && !handDownTriggered && handUpTriggered)
        {
            Debug.Log($"SmsMama: Опускаю руку! Текущая строка: {lineIndex}, цель: {handDownLineIndex}");
            StartCoroutine(TriggerHandDown());
            UnsubscribeFromDialogueEvents();
        }
    }

    IEnumerator WaitForDialogueCompletion()
    {
        Debug.Log("SmsMama: Жду завершения диалога...");

        while (IsDialogueActive())
            yield return null;

        Debug.Log("SmsMama: Диалог завершен");

        if (!handDownTriggered && handUpTriggered)
        {
            Debug.Log("SmsMama: Диалог завершился, а рука не опущена. Опускаю...");
            StartCoroutine(TriggerHandDown());
        }

        UnsubscribeFromDialogueEvents();
    }

    bool IsDialogueActive()
    {
        if (dialogueManager == null || dialogueManager.dialoguePanel == null)
            return false;

        return dialogueManager.dialoguePanel.activeSelf;
    }

    void DisableTodoPanel()
    {
        try
        {
            dialogueManager.showTodoAfterLine = 999;
            Debug.Log("SmsMama: Todo панель отключена (showTodoAfterLine = 999)");
        }
        catch
        {
            Debug.LogWarning("SmsMama: Не удалось отключить Todo панель");
        }
    }

    private void StartPhoneRinging()
    {
        if (ringtoneClip != null && audioSource != null)
        {
            Debug.Log("SmsMama: Телефон начинает звонить...");
            audioSource.clip = ringtoneClip;
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    private void StopPhoneRinging()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    IEnumerator TriggerHandDown()
    {
        if (handDownTriggered) yield break;

        handDownTriggered = true;
        Debug.Log("SmsMama: Начинаю опускание руки...");

        // Задержка перед опусканием
        if (handDownDelay > 0)
        {
            Debug.Log($"SmsMama: Жду {handDownDelay} секунд перед опусканием");
            yield return new WaitForSeconds(handDownDelay);
        }

        // Опускаем руку
        if (handAnimator != null && !string.IsNullOrEmpty(handDownTrigger) && handUpTriggered)
        {
            Debug.Log($"SmsMama: Триггерим анимацию '{handDownTrigger}'");
            handAnimator.SetTrigger(handDownTrigger);

            // Запускаем отмену зума камеры
            StartCameraZoomOut();

            // Останавливаем звонок
            StopPhoneRinging();

            // ПОКАЗЫВАЕМ ВТОРОЙ ПУНКТ TODO
            ShowTodoTask2();

            Debug.Log("SmsMama: Рука опущена");
            yield return new WaitForSeconds(1f);

            Debug.Log("SmsMama: Последовательность завершена");
        }
        else
        {
            Debug.LogWarning("SmsMama: Не могу опустить руку!");
        }
    }

    // Новый метод для показа второго пункта Todo
    private void ShowTodoTask2()
    {
        // Находим TodoUI6 в сцене
        TodoUI6 todoUI = FindObjectOfType<TodoUI6>();
        if (todoUI != null)
        {
            Debug.Log("SmsMama: Показываю второй пункт Todo");
            todoUI.ShowTask2Only(); // Метод остается прежним
        }
        else
        {
            Debug.LogWarning("SmsMama: Не найден TodoUI6 для показа второго пункта!");
        }
    }

    // Плавный зум камеры (приближение)
    void StartCameraZoomIn()
    {
        if (playerCamera == null || zoomCoroutine != null)
            return;

        zoomCoroutine = StartCoroutine(ZoomCamera(true));
    }

    // Плавный возврат камеры в исходное состояние
    void StartCameraZoomOut()
    {
        if (playerCamera == null)
            return;

        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);

        zoomCoroutine = StartCoroutine(ZoomCamera(false));
    }

    IEnumerator ZoomCamera(bool zoomIn)
    {
        if (playerCamera == null) yield break;

        float startTime = Time.time;
        float endTime = startTime + zoomDuration;

        float startFOV = playerCamera.fieldOfView;
        Vector3 startPosition = cameraTransform.localPosition;

        // Рассчитываем целевую позицию и FOV
        float targetFOV;
        Vector3 targetPosition;

        if (zoomIn)
        {
            // Уменьшаем FOV для эффекта зума
            targetFOV = originalFOV / zoomAmount;

            // Слегка сдвигаем камеру вперед в направлении взгляда
            targetPosition = originalCameraPosition + cameraTransform.forward * 0.3f;
        }
        else
        {
            // Возвращаем к исходным значениям
            targetFOV = originalFOV;
            targetPosition = originalCameraPosition;
        }

        Debug.Log($"SmsMama: Начинаю {(zoomIn ? "приближение" : "отдаление")} камеры. FOV: {startFOV} -> {targetFOV}");

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / zoomDuration;
            float curvedT = zoomCurve.Evaluate(t);

            // Плавно изменяем FOV
            playerCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, curvedT);

            // Плавно изменяем позицию камеры
            cameraTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, curvedT);

            yield return null;
        }

        // Гарантируем точные конечные значения
        playerCamera.fieldOfView = targetFOV;
        cameraTransform.localPosition = targetPosition;

        Debug.Log($"SmsMama: Зум камеры завершен. Текущий FOV: {playerCamera.fieldOfView}");
        zoomCoroutine = null;
    }

    [ContextMenu("Тест: Запустить SMS")]
    public void TestStartSmsSequence()
    {
        if (!hasTriggered)
            StartSmsSequence();
    }

    [ContextMenu("Тест: Опустить руку")]
    public void TestHandDown()
    {
        if (!handDownTriggered)
        {
            handUpTriggered = true;
            StartCoroutine(TriggerHandDown());
        }
    }

    [ContextMenu("Сбросить")]
    public void ResetState()
    {
        hasTriggered = false;
        handUpTriggered = false;
        handDownTriggered = false;

        if (activationCoroutine != null)
            StopCoroutine(activationCoroutine);

        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            zoomCoroutine = null;
        }

        // Восстанавливаем камеру
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = originalFOV;
            cameraTransform.localPosition = originalCameraPosition;
        }

        UnsubscribeFromDialogueEvents();
        StopPhoneRinging();

        Debug.Log("SmsMama: Состояние сброшено");
    }

    void OnDestroy()
    {
        StopPhoneRinging();
        UnsubscribeFromDialogueEvents();

        if (activationCoroutine != null)
            StopCoroutine(activationCoroutine);

        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);
    }
}