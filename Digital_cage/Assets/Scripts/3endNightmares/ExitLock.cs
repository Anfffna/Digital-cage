using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExitLock : MonoBehaviour, IInteractable
{
    [Header("Lock Settings")]
    public string correctCode = "123456";
    public float digitDisplayDelay = 0.5f;

    [Header("Digit Objects")]
    public GameObject[] digitObjects;

    [Header("Blink Settings")]
    public float blinkInterval = 0.3f;
    public int blinkCount = 6;
    public bool blinkInSequence = false;

    [Header("Dialogue Settings")]
    public ManagerDialogue3 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Todo Settings")]
    public LightTodoUIManager todoManager;
    public string exitTaskText = "Exit text";

    [Header("Apocalypse Settings")]
    public ApocalypseController apocalypseController;

    [Header("Exit Door Reference")]
    public ExitDoor exitDoor;

    [Header("Audio Settings")]
    public AudioClip digitAppearSound;
    public AudioClip blinkSound;
    public AudioSource audioSource;

    [Header("Door Interaction Effects")]
    public GameObject uiSprite;
    public float redBlinkInterval = 0.5f;
    public Color redColor = Color.red;

    [Header("Dependency Settings")]
    public ExitBasement exitBasement; // Ссылка на выход из подвала

    private bool hasBeenUsed = false;
    private bool isShowingCode = false;
    private bool isBlinking = false;
    private bool isInteractable = false; // Добавлен флаг доступности
    private Coroutine showCodeCoroutine;
    private Coroutine blinkCoroutine;
    private Coroutine blinkEffectCoroutine;
    private Coroutine digitSoundCoroutine;
    private Coroutine checkExitBasementCoroutine; // Корутина для проверки выхода из подвала

    public event System.Action<int> OnExitLockDialogueIndex;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Default"); // Изначально неинтерактивен
        HideAllDigits();

        if (uiSprite != null)
        {
            uiSprite.SetActive(false);
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Находим ExitBasement если не назначен в инспекторе
        if (exitBasement == null)
        {
            exitBasement = FindObjectOfType<ExitBasement>();
            Debug.Log("ExitLock: ExitBasement найден автоматически: " + (exitBasement != null));
        }

        // Запускаем проверку завершения ExitBasement
        checkExitBasementCoroutine = StartCoroutine(CheckExitBasementCompletion());
    }

    private IEnumerator CheckExitBasementCompletion()
    {
        Debug.Log("ExitLock: Ожидание завершения ExitBasement...");

        // Ждем пока ExitBasement не будет найден
        while (exitBasement == null)
        {
            yield return new WaitForSeconds(0.5f);
            exitBasement = FindObjectOfType<ExitBasement>();
        }

        Debug.Log("ExitLock: ExitBasement найден, ожидаем завершения...");

        // Постоянно проверяем, был ли использован ExitBasement
        while (!isInteractable && !hasBeenUsed)
        {
            if (exitBasement != null)
            {
                // Используем рефлексию для проверки приватного поля hasBeenUsed
                var exitBasementType = exitBasement.GetType();
                var hasBeenUsedField = exitBasementType.GetField("hasBeenUsed",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (hasBeenUsedField != null)
                {
                    bool exitBasementCompleted = (bool)hasBeenUsedField.GetValue(exitBasement);
                    if (exitBasementCompleted)
                    {
                        UnlockExitLock();
                        Debug.Log("ExitLock: ExitBasement завершен! LockDoor теперь интерактивен.");
                        yield break;
                    }
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void UnlockExitLock()
    {
        isInteractable = true;
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        Debug.Log("ExitLock: LockDoor теперь интерактивен после выхода из подвала!");
    }

    private void HideAllDigits()
    {
        foreach (GameObject digitObject in digitObjects)
        {
            if (digitObject != null)
            {
                digitObject.SetActive(false);
            }
        }
    }

    public string GetInteractionText()
    {
        if (!isInteractable || hasBeenUsed || isShowingCode)
            return "";

        return "Нажмите E";
    }

    public void Interact()
    {
        if (!isInteractable || hasBeenUsed || isShowingCode) return;

        gameObject.layer = LayerMask.NameToLayer("Default");
        hasBeenUsed = true;

        showCodeCoroutine = StartCoroutine(ShowCodeSequence());
    }

    // Остальной код без изменений...
    private IEnumerator ShowCodeSequence()
    {
        isShowingCode = true;

        Debug.Log($"ExitLock: Показываем код: {correctCode}");

        // Запускаем звук при появлении первой цифры
        if (digitAppearSound != null && audioSource != null)
        {
            digitSoundCoroutine = StartCoroutine(PlayDigitSoundLoop());
        }

        for (int i = 0; i < correctCode.Length; i++)
        {
            if (i < digitObjects.Length && digitObjects[i] != null)
            {
                digitObjects[i].SetActive(true);
                Debug.Log($"ExitLock: Показана цифра на позиции {i}");
                yield return new WaitForSeconds(digitDisplayDelay);
            }
        }

        // Останавливаем звук после показа всех цифр
        if (digitSoundCoroutine != null)
        {
            StopCoroutine(digitSoundCoroutine);
            digitSoundCoroutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (apocalypseController != null)
        {
            apocalypseController.StartApocalypse();
            Debug.Log("ExitLock: Запущен апокалипсис!");
        }

        StartExitLockDialogue();

        Debug.Log("ExitLock: Диалог запущен, ожидаем разблокировки двери...");
    }

    private IEnumerator PlayDigitSoundLoop()
    {
        while (isShowingCode)
        {
            audioSource.PlayOneShot(digitAppearSound);
            yield return new WaitForSeconds(digitDisplayDelay);
        }
    }

    private void StartExitLockDialogue()
    {
        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            dialogueManager.OnDialogueIndexReached += OnDialogueIndexReached;
            dialogueManager.StartDialogue(dialogueLines);
            Debug.Log("ExitLock: Диалог запущен после ввода кода");
        }
        else
        {
            Debug.LogWarning("ExitLock: DialogueManager или dialogueLines не назначены!");
        }
    }

    private void OnDialogueIndexReached(int lineIndex)
    {
        Debug.Log($"ExitLock: Получен индекс диалога {lineIndex}");

        OnExitLockDialogueIndex?.Invoke(lineIndex);

        if (lineIndex == 3 && todoManager != null)
        {
            todoManager.ReplaceCurrentTaskWithExitText(exitTaskText);
        }

        if (lineIndex >= dialogueLines.Count - 1)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueIndexReached;
        }
    }

    private IEnumerator BlinkDigits()
    {
        PlaySound(blinkSound);

        if (blinkInSequence)
        {
            for (int blink = 0; blink < blinkCount; blink++)
            {
                for (int i = 0; i < digitObjects.Length; i++)
                {
                    if (digitObjects[i] != null)
                    {
                        digitObjects[i].SetActive(false);
                        yield return new WaitForSeconds(blinkInterval / digitObjects.Length);
                        digitObjects[i].SetActive(true);
                    }
                }
                yield return new WaitForSeconds(blinkInterval);
            }
        }
        else
        {
            for (int blink = 0; blink < blinkCount; blink++)
            {
                foreach (GameObject digit in digitObjects)
                {
                    if (digit != null) digit.SetActive(false);
                }
                yield return new WaitForSeconds(blinkInterval);

                foreach (GameObject digit in digitObjects)
                {
                    if (digit != null) digit.SetActive(true);
                }
                yield return new WaitForSeconds(blinkInterval);
            }
        }
    }

    public void StartDoorInteractionEffects()
    {
        Debug.Log("ExitLock: Запускаем эффекты взаимодействия с дверью");

        if (uiSprite != null)
        {
            uiSprite.SetActive(true);
            StartCoroutine(BlinkUISprite());
        }
        else
        {
            Debug.LogWarning("ExitLock: UI Sprite не назначен!");
        }

        MakeDigitsRed();
    }

    private IEnumerator BlinkUISprite()
    {
        if (uiSprite == null) yield break;

        isBlinking = true;

        while (isBlinking)
        {
            uiSprite.SetActive(!uiSprite.activeInHierarchy);
            yield return new WaitForSeconds(redBlinkInterval);
        }
    }

    private void MakeDigitsRed()
    {
        for (int i = 0; i < digitObjects.Length; i++)
        {
            if (digitObjects[i] != null)
            {
                var renderer = digitObjects[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = redColor;
                }

                var image = digitObjects[i].GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.color = redColor;
                }

                var spriteRenderer = digitObjects[i].GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = redColor;
                }

                var textMesh = digitObjects[i].GetComponent<TMPro.TextMeshProUGUI>();
                if (textMesh != null)
                {
                    textMesh.color = redColor;
                }
            }
        }

        Debug.Log("ExitLock: Цифры стали статично красными");
    }

    public void StopDoorInteractionEffects()
    {
        isBlinking = false;

        if (uiSprite != null)
        {
            uiSprite.SetActive(false);
        }

        if (blinkEffectCoroutine != null)
        {
            StopCoroutine(blinkEffectCoroutine);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void OnDestroy()
    {
        if (showCodeCoroutine != null)
        {
            StopCoroutine(showCodeCoroutine);
        }
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        if (blinkEffectCoroutine != null)
        {
            StopCoroutine(blinkEffectCoroutine);
        }
        if (digitSoundCoroutine != null)
        {
            StopCoroutine(digitSoundCoroutine);
        }
        if (checkExitBasementCoroutine != null)
        {
            StopCoroutine(checkExitBasementCoroutine);
        }

        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueIndexReached -= OnDialogueIndexReached;
        }
    }
}