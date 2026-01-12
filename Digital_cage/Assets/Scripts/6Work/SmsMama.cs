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

    void Start()
    {
        if (hoursLaterScript == null)
        {
            hoursLaterScript = FindObjectOfType<HoursLater>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
        }

        if (handAnimator == null)
        {
            Debug.LogWarning("SmsMama: Не назначен Animator для руки!");
        }

        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<ManagerDialogue6>();
        }

        if (dialogueLines == null || dialogueLines.Count == 0)
        {
            Debug.LogWarning("SmsMama: Не настроены строки диалога!");
        }

        hasTriggered = false;
        handUpTriggered = false;
        handDownTriggered = false;

        Debug.Log("SmsMama: Инициализирован");
    }

    public void StartSmsSequence()
    {
        Debug.Log("=== SmsMama.StartSmsSequence() ВЫЗВАН ===");

        if (hasTriggered)
        {
            Debug.Log("SmsMama: Уже срабатывал ранее, пропускаем");
            return;
        }

        hasTriggered = true;

        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
        }

        activationCoroutine = StartCoroutine(SmsSequence());
    }

    private IEnumerator SmsSequence()
    {
        Debug.Log("SmsMama: Начинаю SMS последовательность...");

        // 1. Ждем 3 секунды после завершения диалога HoursLater
        yield return new WaitForSeconds(delayAfterHoursDialogue);

        // 2. НАЧИНАЕМ ЗВОНОК ТЕЛЕФОНА
        StartPhoneRinging();

        // 3. Ждем 2 секунды перед тем как взять трубку
        yield return new WaitForSeconds(2f);

        // 4. ПОДНИМАЕМ РУКУ (БЕРЕМ ТРУБКУ)
        Debug.Log("SmsMama: Поднимаем руку (берем трубку)...");

        if (delayBeforeHandUp > 0)
        {
            yield return new WaitForSeconds(delayBeforeHandUp);
        }

        if (handAnimator != null && !string.IsNullOrEmpty(handUpTrigger))
        {
            Debug.Log($"SmsMama: Триггерим анимацию '{handUpTrigger}'");
            handAnimator.SetTrigger(handUpTrigger);
            handUpTriggered = true;
        }

        if (delayAfterHandUp > 0)
        {
            yield return new WaitForSeconds(delayAfterHandUp);
        }

        // 5. ЗАПУСКАЕМ ДИАЛОГ
        if (dialogueLines != null && dialogueLines.Count > 0 && dialogueManager != null)
        {
            Debug.Log($"SmsMama: Запускаем диалог ({dialogueLines.Count} строк, опускание ПОСЛЕ строки {handDownLineIndex})");

            // ОТКЛЮЧАЕМ TODO ПАНЕЛЬ
            if (disableTodoPanel)
            {
                DisableTodoPanel();
            }

            // ПОДПИСЫВАЕМСЯ НА СОБЫТИЕ ПЕРЕД ЗАПУСКОМ ДИАЛОГА
            SubscribeToDialogueEvents();

            // Запускаем диалог
            dialogueManager.StartDialogue(dialogueLines);

            // ЗАПУСКАЕМ КОРУТИНУ ДЛЯ СЛЕДКИ ЗАВЕРШЕНИЯ ДИАЛОГА
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

        // ПРЯМАЯ ПОДПИСКА НА СОБЫТИЕ
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

        // В ManagerDialogue6 currentLineIndex начинается с 1
        // Проверяем, достигли ли мы строки для опускания руки
        if (lineIndex >= handDownLineIndex && !handDownTriggered && handUpTriggered)
        {
            Debug.Log($"SmsMama: Опускаю руку! Текущая строка: {lineIndex}, цель: {handDownLineIndex}");
            StartCoroutine(TriggerHandDown());

            // Отписываемся от события
            UnsubscribeFromDialogueEvents();
        }
    }

    IEnumerator WaitForDialogueCompletion()
    {
        Debug.Log("SmsMama: Жду завершения диалога...");

        // Ждем пока диалог активен
        while (IsDialogueActive())
        {
            yield return null;
        }

        Debug.Log("SmsMama: Диалог завершен");

        // Если рука все еще не опущена - опускаем
        if (!handDownTriggered && handUpTriggered)
        {
            Debug.Log("SmsMama: Диалог завершился, а рука не опущена. Опускаю...");
            StartCoroutine(TriggerHandDown());
        }

        // Отписываемся на всякий случай
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
            // Просто устанавливаем showTodoAfterLine в очень большое число
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
        {
            audioSource.Stop();
        }
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

            // Останавливаем звонок
            StopPhoneRinging();

            Debug.Log("SmsMama: Рука опущена");
            yield return new WaitForSeconds(1f);

            Debug.Log("SmsMama: Последовательность завершена");
        }
        else
        {
            Debug.LogWarning("SmsMama: Не могу опустить руку!");
        }
    }

    [ContextMenu("Тест: Запустить SMS")]
    public void TestStartSmsSequence()
    {
        if (!hasTriggered)
        {
            StartSmsSequence();
        }
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
        {
            StopCoroutine(activationCoroutine);
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
        {
            StopCoroutine(activationCoroutine);
        }
    }
}