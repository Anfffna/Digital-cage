using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundScary : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip correctAnswerSound;
    public float soundVolume = 1f;

    [Header("Dialogue Settings")]
    public ManagerDialogue6 dialogueManager;

    [Header("Post-Sound Dialogue")]
    [TextArea(2, 5)]
    public List<string> afterSoundDialogue = new List<string>();

    [Header("Settings")]
    public float delayAfterSound = 0.2f; // Задержка после звука перед диалогом
    public float initialDelay = 6f; // Задержка ПЕРЕД запуском звука (6 секунд)
    public bool onlyOnce = true; // Срабатывать только один раз

    private bool isPlaying = false;
    private bool hasTriggered = false; // Флаг, что уже сработал
    private Coroutine soundCoroutine;

    void Start()
    {
        // Настраиваем AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("SoundScary: AudioSource не найден, добавляю новый");
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Настраиваем громкость
        if (audioSource != null)
        {
            audioSource.volume = soundVolume;
        }

        // Ищем ManagerDialogue6 если не назначен
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<ManagerDialogue6>();
            if (dialogueManager == null)
            {
                Debug.LogWarning("SoundScary: ManagerDialogue6 не найден в сцене!");
            }
        }

        // Проверяем наличие звука
        if (correctAnswerSound == null)
        {
            Debug.LogWarning("SoundScary: Не назначен AudioClip для правильного ответа!");
        }

        // Проверяем наличие диалогов
        if (afterSoundDialogue == null || afterSoundDialogue.Count == 0)
        {
            Debug.LogWarning("SoundScary: Не настроены строки диалога после звука!");
        }

        isPlaying = false;
        hasTriggered = false;
        Debug.Log("SoundScary: Инициализирован (onlyOnce = " + onlyOnce + ", initialDelay = " + initialDelay + "s)");
    }

    // Метод для вызова из Work при правильном ответе
    public void PlaySoundAndDialogue()
    {
        // Если уже срабатывал и включен режим "только один раз"
        if (onlyOnce && hasTriggered)
        {
            Debug.Log("SoundScary: Уже срабатывал ранее, пропускаем (onlyOnce = true)");
            return;
        }

        // Если уже проигрывается, не запускаем новый
        if (isPlaying)
        {
            Debug.Log("SoundScary: Уже проигрывается, пропускаем");
            return;
        }

        // Запускаем цепочку: ЗАДЕРЖКА 6с ? звук ? ожидание ? диалог
        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
        }

        soundCoroutine = StartCoroutine(SoundAndDialogueSequence());
    }

    IEnumerator SoundAndDialogueSequence()
    {
        isPlaying = true;

        // 0. ЖДЕМ 6 СЕКУНД ПЕРЕД ЗАПУСКОМ
        Debug.Log($"SoundScary: Жду {initialDelay} секунд перед запуском звука...");
        yield return new WaitForSeconds(initialDelay);

        // Отмечаем, что сработал (если включен режим "только один раз")
        if (onlyOnce)
        {
            hasTriggered = true;
        }

        // 1. Воспроизводим звук
        if (audioSource != null && correctAnswerSound != null)
        {
            Debug.Log("SoundScary: Воспроизвожу звук правильного ответа");
            audioSource.PlayOneShot(correctAnswerSound, soundVolume);

            // Ждем полного окончания звука
            yield return new WaitForSeconds(correctAnswerSound.length);
        }
        else
        {
            Debug.LogWarning("SoundScary: Не могу воспроизвести звук");
        }

        // 2. Небольшая задержка после звука
        yield return new WaitForSeconds(delayAfterSound);

        // 3. Показываем диалог
        if (dialogueManager != null && afterSoundDialogue != null && afterSoundDialogue.Count > 0)
        {
            Debug.Log($"SoundScary: Показываю диалог после звука ({afterSoundDialogue.Count} строк)");
            dialogueManager.StartDialogue(afterSoundDialogue);
        }
        else
        {
            Debug.LogWarning("SoundScary: Не могу показать диалог - нет менеджера или строк");
        }

        // 4. Ждем немного, чтобы диалог успел начаться
        yield return new WaitForSeconds(0.5f);

        isPlaying = false;
        soundCoroutine = null;
    }

    // Метод для сброса состояния
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
            soundCoroutine = null;
        }
        isPlaying = false;
        Debug.Log("SoundScary: Состояние сброшено, будет срабатывать снова");
    }

    // Метод для остановки (если нужно прервать)
    public void StopAll()
    {
        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
            soundCoroutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        isPlaying = false;
        Debug.Log("SoundScary: Все остановлено");
    }

    // Метод для проверки, идет ли сейчас воспроизведение
    public bool IsPlaying()
    {
        return isPlaying;
    }

    // Метод для проверки, срабатывал ли уже
    public bool HasTriggered()
    {
        return hasTriggered;
    }
}