using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MamaCall : MonoBehaviour
{
    [Header("Trigger Settings")]
    public Collider triggerCollider;
    public string playerTag = "Player";

    [Header("Signature Condition")]
    public EntryDialogue entryDialogue;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip ringtoneClip;
    public float audioDelay = 0.2f;

    [Header("Animation")]
    public Animator handAnimator;
    public string handUpTrigger = "HandUp";
    public string handDownTrigger = "HandDown";

    [Header("Dialogue Settings")]
    public DialogueManager0 dialogueManager;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Timing Settings")]
    public bool destroyAfterTrigger = true;
    public float delayBeforeHandUp = 0.5f;
    public float delayAfterHandUp = 0.5f;
    public int handDownLineIndex = 2;
    public float handDownDelay = 0.5f;

    private bool hasTriggered = false;
    private bool signatureCompleted = false;
    private bool playerInside = false;
    private bool handUpTriggered = false;
    private bool handDownTriggered = false;
    private int currentDialogueIndex = 0;

    public bool IsHandDown() { return handDownTriggered; }

    void Start()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider == null)
            {
                Debug.LogError("MamaCall: Не найден триггерный коллайдер!");
                return;
            }
        }

        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning("MamaCall: Коллайдер не настроен как триггер!");
        }

        if (entryDialogue == null)
        {
            Debug.LogError("MamaCall: Не назначен EntryDialogue!");
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true; // Зацикливание включено!
        }

        if (handAnimator == null)
        {
            Debug.LogWarning("MamaCall: Не назначен Animator для руки!");
        }

        if (dialogueManager != null)
        {
            SubscribeToDialogueEvents();
        }

        StartCoroutine(CheckSignatureCompletion());
    }

    private void SubscribeToDialogueEvents()
    {
        dialogueManager.OnDialogueIndexReached += OnDialogueLineChanged;
    }

    private void UnsubscribeFromDialogueEvents()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueLineChanged;
        }
    }

    private IEnumerator CheckSignatureCompletion()
    {
        while (!entryDialogue.SignatureCompleted)
            yield return null;

        signatureCompleted = true;
        Debug.Log("MamaCall: Подпись завершена, триггер готов!");

        if (playerInside && !hasTriggered)
        {
            TriggerMamaCall();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = true;

        if (signatureCompleted && !hasTriggered)
        {
            TriggerMamaCall();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInside = false;
    }

    private void TriggerMamaCall()
    {
        if (hasTriggered) return;

        hasTriggered = true;
        StartCoroutine(ActivateMamaCall());
    }

    private IEnumerator ActivateMamaCall()
    {
        Debug.Log("MamaCall: Активируем событие...");

        // === ЭТАП 1: НАЧАЛО ЗВОНКА ТЕЛЕФОНА ===
        StartPhoneRinging();

        // Ждем перед тем как взять трубку
        yield return new WaitForSeconds(2f);

        // === ЭТАП 2: ПОДНЯТИЕ РУКИ (БЕРЕМ ТРУБКУ) ===
        Debug.Log("MamaCall: Поднимаем руку (берем трубку)...");

        // Задержка перед поднятием руки
        if (delayBeforeHandUp > 0)
        {
            yield return new WaitForSeconds(delayBeforeHandUp);
        }

        // Поднимаем руку (звонок ПРОДОЛЖАЕТСЯ!)
        if (handAnimator != null && !string.IsNullOrEmpty(handUpTrigger))
        {
            Debug.Log($"MamaCall: Триггерим анимацию '{handUpTrigger}'");
            handAnimator.SetTrigger(handUpTrigger);
            handUpTriggered = true;
        }

        // ЗВОНОК ПРОДОЛЖАЕТСЯ! Не останавливаем его здесь!

        // Задержка после поднятия руки
        if (delayAfterHandUp > 0)
        {
            yield return new WaitForSeconds(delayAfterHandUp);
        }

        // === ЭТАП 3: ЗАПУСК ДИАЛОГА ===
        if (dialogueLines != null && dialogueLines.Count > 0 && dialogueManager != null)
        {
            Debug.Log("MamaCall: Запускаем диалог...");
            dialogueManager.StartDialogue(dialogueLines);
        }
        else if (dialogueLines != null && dialogueLines.Count > 0 && dialogueManager == null)
        {
            Debug.LogWarning("MamaCall: Нет DialogueManager0!");
        }
    }

    // Метод для начала звонка телефона
    private void StartPhoneRinging()
    {
        if (ringtoneClip != null && audioSource != null)
        {
            Debug.Log("MamaCall: Телефон начинает звонить...");

            // Настраиваем аудио
            audioSource.clip = ringtoneClip;
            audioSource.loop = true; // Звонок на повторе
            audioSource.Play();

            // Визуальные эффекты звонка
            StartCoroutine(RingingVisualEffects());
        }
        else if (ringtoneClip == null)
        {
            Debug.LogWarning("MamaCall: Нет рингтона!");
        }
    }

    // Метод для остановки звонка - вызывается когда кладут трубку
    private void StopPhoneRinging()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            Debug.Log("MamaCall: Звонок остановлен (положили трубку)");
            audioSource.Stop();

            // Останавливаем визуальные эффекты
            StopAllCoroutines();
        }
    }

    // Визуальные эффекты во время звонка
    private IEnumerator RingingVisualEffects()
    {
        // Здесь можно добавить мерцание света, вибрацию телефона и т.д.
        // Например, если у вас есть объект телефона с аниматором:
        /*
        Animator phoneAnimator = GetComponent<Animator>();
        if (phoneAnimator != null)
        {
            phoneAnimator.SetBool("Ringing", true);
            
            // Ждем пока звонок не остановится
            while (audioSource != null && audioSource.isPlaying)
            {
                yield return null;
            }
            
            phoneAnimator.SetBool("Ringing", false);
        }
        */

        yield return null;
    }

    // Этот метод вызывается при смене строки диалога
    private void OnDialogueLineChanged(int lineIndex)
    {
        currentDialogueIndex = lineIndex;
        Debug.Log($"MamaCall: Текущая строка диалога: {lineIndex}");

        // Проверяем, нужно ли опустить руку
        if (!handDownTriggered && lineIndex == handDownLineIndex + 1)
        {
            StartCoroutine(TriggerHandDown());
        }
    }

    private IEnumerator TriggerHandDown()
    {
        // Задержка перед опусканием руки
        if (handDownDelay > 0)
        {
            yield return new WaitForSeconds(handDownDelay);
        }

        // Опускаем руку (КЛАДЕМ ТРУБКУ)
        if (handAnimator != null && !string.IsNullOrEmpty(handDownTrigger) && handUpTriggered)
        {
            Debug.Log($"MamaCall: Триггерим анимацию '{handDownTrigger}' на строке {handDownLineIndex}");
            handAnimator.SetTrigger(handDownTrigger);
            handDownTriggered = true;

            // === ВОТ ТУТ ОСТАНАВЛИВАЕМ ЗВОНОК! ===
            StopPhoneRinging();

            // Дополнительный звук "клацанья" трубки
            PlayHangUpSound();

            // Ждем немного
            yield return new WaitForSeconds(1f);

            // Уничтожаем объект если нужно
            if (destroyAfterTrigger)
            {
                Debug.Log("MamaCall: Событие завершено, уничтожаем объект...");
                UnsubscribeFromDialogueEvents();
                Destroy(gameObject);
            }
            else
            {
                triggerCollider.enabled = false;
                UnsubscribeFromDialogueEvents();
            }
        }
    }

    // Звук когда кладут трубку
    private void PlayHangUpSound()
    {
        // Можно добавить короткий звук "клик"
        Debug.Log("MamaCall: Трубка положена");

        // Пример:
        // AudioSource.PlayClipAtPoint(hangUpClip, transform.position, 0.5f);
    }

    // Метод для ручной активации
    public void ForceActivate()
    {
        if (!hasTriggered && signatureCompleted)
        {
            StartCoroutine(ActivateMamaCall());
        }
        else if (!signatureCompleted)
        {
            Debug.Log("MamaCall: Нельзя активировать - подпись не завершена!");
        }
    }

    public bool IsReady()
    {
        return signatureCompleted && !hasTriggered;
    }

    public bool HasTriggered()
    {
        return hasTriggered;
    }

    void OnDestroy()
    {
        UnsubscribeFromDialogueEvents();

        // Гарантируем остановку звука
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void OnDrawGizmos()
    {
        if (triggerCollider != null)
        {
            Gizmos.color = signatureCompleted ? Color.green : Color.yellow;

            if (triggerCollider is BoxCollider boxCollider)
            {
                Gizmos.DrawWireCube(transform.position + boxCollider.center, boxCollider.size);
            }
            else if (triggerCollider is SphereCollider sphereCollider)
            {
                Gizmos.DrawWireSphere(transform.position + sphereCollider.center, sphereCollider.radius);
            }
        }
    }
}