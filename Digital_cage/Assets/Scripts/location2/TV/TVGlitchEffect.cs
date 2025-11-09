using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TVGlitchEffect : MonoBehaviour
{
    [Header("Glitch Settings")]
    public GameObject glitchSprite; // 2D спрайт с глитч-картинкой
    public float glitchDuration = 5f; // Длительность показа глитча

    [Header("Post-Glitch Sprite")]
    public GameObject spriteObject; // Спрайт объект из сцены
    public float spriteDuration = 3f; // Длительность показа спрайта

    [Header("Post-Glitch Dialogue")]
    public ManagerDialogue2 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Flashing Lights")]
    public FlashingLights flashingLights;

    [Header("Audio")]
    public AudioSource audioSource; // Единственный AudioSource на телевизоре
    public AudioClip glitchAudioClip; // MP3 аудио для глитча
    public AudioClip pictureAudioClip; // MP3 аудио для картинки

    [Header("Volume Settings")]
    public float initialGlitchVolume = 0.09f; // Начальная громкость глитча
    public float finalGlitchVolume = 0.01f; // Конечная громкость глитча
    public float volumeFadeDuration = 2f; // Длительность плавного уменьшения громкости

    private Coroutine glitchCoroutine;
    private bool isGlitching = false;

    void Start()
    {
        // Автоматически находим менеджер диалога если не назначен
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<ManagerDialogue2>();

        // Скрываем все спрайты при старте
        if (glitchSprite != null)
        {
            glitchSprite.SetActive(false);
        }
        if (spriteObject != null)
        {
            spriteObject.SetActive(false);
        }

        // Настраиваем AudioSource
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// Запуск глитч-эффекта
    /// </summary>
    public void StartGlitchEffect()
    {
        if (glitchSprite == null)
        {
            Debug.LogError("TVGlitchEffect: Glitch Sprite не назначен!");
            return;
        }

        if (isGlitching)
        {
            StopCoroutine(glitchCoroutine);
        }

        glitchCoroutine = StartCoroutine(GlitchRoutine());
    }

    /// <summary>
    /// Корутина глитч-эффекта
    /// </summary>
    private IEnumerator GlitchRoutine()
    {
        isGlitching = true;
        Debug.Log("TVGlitchEffect: Запуск глитч-эффекта");

        // ПОКАЗЫВАЕМ ГЛИТЧ-СПРАЙТ
        if (glitchSprite != null)
        {
            glitchSprite.SetActive(true);
            Debug.Log("TVGlitchEffect: Глитч-спрайт показан");
        }

        // ЗАПУСКАЕМ ЗВУК ГЛИТЧА С НАЧАЛЬНОЙ ГРОМКОСТЬЮ
        if (audioSource != null && glitchAudioClip != null)
        {
            audioSource.clip = glitchAudioClip;
            audioSource.volume = initialGlitchVolume; // Устанавливаем начальную громкость
            audioSource.Play();
            Debug.Log($"TVGlitchEffect: Звук глитча запущен с громкостью {initialGlitchVolume}");
        }

        // ЖДЕМ ДЛИТЕЛЬНОСТЬ ГЛИТЧА (5 секунд)
        yield return new WaitForSeconds(glitchDuration);

        // СКРЫВАЕМ ГЛИТЧ-СПРАЙТ
        if (glitchSprite != null)
        {
            glitchSprite.SetActive(false);
            Debug.Log("TVGlitchEffect: Глитч-спрайт скрыт");
        }

        // ОСТАНАВЛИВАЕМ ЗВУК ГЛИТЧА ПРИ СМЕНЕ НА КАРТИНКУ
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("TVGlitchEffect: Звук глитча остановлен");
        }

        // СРАЗУ ПОКАЗЫВАЕМ ДОПОЛНИТЕЛЬНЫЙ СПРАЙТ
        if (spriteObject != null)
        {
            spriteObject.SetActive(true);
            Debug.Log("TVGlitchEffect: Дополнительный спрайт показан");
        }

        // ЗАПУСКАЕМ ЗВУК ДЛЯ КАРТИНКИ
        if (audioSource != null && pictureAudioClip != null)
        {
            audioSource.clip = pictureAudioClip;
            audioSource.volume = initialGlitchVolume; // Сбрасываем громкость для картинки
            audioSource.Play();
            Debug.Log("TVGlitchEffect: Звук картинки запущен");
        }

        // ЖДЕМ 1 СЕКУНДУ ПОСЛЕ СКРЫТИЯ ГЛИТЧ-СПРАЙТА
        yield return new WaitForSeconds(1f);

        // ЗАПУСКАЕМ ДИАЛОГ (дополнительный спрайт все еще виден)
        StartPostGlitchDialogue();

        // ЖДЕМ ОСТАВШЕЕСЯ ВРЕМЯ И СКРЫВАЕМ ДОПОЛНИТЕЛЬНЫЙ СПРАЙТ
        yield return new WaitForSeconds(spriteDuration - 1f);
        if (spriteObject != null)
        {
            spriteObject.SetActive(false);
            Debug.Log("TVGlitchEffect: Дополнительный спрайт скрыт");
        }

        // ОСТАНАВЛИВАЕМ ЗВУК КАРТИНКИ
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("TVGlitchEffect: Звук картинки остановлен");
        }

        if (flashingLights != null)
        {
            flashingLights.StartFlashing();
        }

        // ВОЗОБНОВЛЯЕМ ГЛИТЧ-СПРАЙТ НАВСЕГДА!
        if (glitchSprite != null)
        {
            glitchSprite.SetActive(true);
            Debug.Log("TVGlitchEffect: Глитч-спрайт возобновлен навсегда");
        }

        // ЗАПУСКАЕМ ЗВУК ГЛИТЧА СНОВА ПРИ ВОЗОБНОВЛЕНИИ
        if (audioSource != null && glitchAudioClip != null)
        {
            audioSource.clip = glitchAudioClip;
            audioSource.volume = initialGlitchVolume; // Начинаем с начальной громкости
            audioSource.Play();
            Debug.Log($"TVGlitchEffect: Звук глитча возобновлен с громкостью {initialGlitchVolume}");

            // ПЛАВНО УМЕНЬШАЕМ ГРОМКОСТЬ ГЛИТЧА ПОСЛЕ ВОЗОБНОВЛЕНИЯ
            yield return StartCoroutine(FadeGlitchVolume());
        }

        isGlitching = false;
    }

    /// <summary>
    /// Плавное уменьшение громкости глитча
    /// </summary>
    private IEnumerator FadeGlitchVolume()
    {
        if (audioSource == null) yield break;

        float startVolume = audioSource.volume;
        float timer = 0f;

        Debug.Log($"TVGlitchEffect: Начало плавного уменьшения громкости с {startVolume} до {finalGlitchVolume}");

        while (timer < volumeFadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / volumeFadeDuration;
            audioSource.volume = Mathf.Lerp(startVolume, finalGlitchVolume, progress);
            yield return null;
        }

        // Убеждаемся, что громкость установлена точно в конечное значение
        audioSource.volume = finalGlitchVolume;
        Debug.Log($"TVGlitchEffect: Громкость глитча уменьшена до {finalGlitchVolume}");
    }

    /// <summary>
    /// Запуск диалога после глитч-эффекта
    /// </summary>
    private void StartPostGlitchDialogue()
    {
        if (dialogueLines != null && dialogueLines.Count > 0)
        {
            if (dialogueManager != null)
            {
                Debug.Log($"TVGlitchEffect: Запуск диалога после глитча, строк: {dialogueLines.Count}");
                dialogueManager.StartDialogue(dialogueLines, OnPostGlitchDialogueEnd);
            }
            else
            {
                Debug.LogError("TVGlitchEffect: Dialogue Manager не назначен!");
            }
        }
        else
        {
            Debug.LogWarning("TVGlitchEffect: Нет строк для диалога после глитча");
        }
    }

    /// <summary>
    /// Вызывается когда завершен диалог после глитча
    /// </summary>
    private void OnPostGlitchDialogueEnd()
    {
        Debug.Log("TVGlitchEffect: Диалог после глитча завершен");
        // Здесь можно добавить дополнительную логику
    }

    /// <summary>
    /// Принудительно остановить глитч-эффект
    /// </summary>
    public void StopGlitchEffect()
    {
        if (isGlitching && glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
        }

        // Скрываем все спрайты
        if (glitchSprite != null)
        {
            glitchSprite.SetActive(false);
        }
        if (spriteObject != null)
        {
            spriteObject.SetActive(false);
        }

        // ОСТАНАВЛИВАЕМ ЗВУК
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("TVGlitchEffect: Звук остановлен");
        }

        isGlitching = false;
        Debug.Log("TVGlitchEffect: Глитч-эффект принудительно остановлен");
    }

    [ContextMenu("Test Glitch Effect")]
    public void TestGlitch()
    {
        StartGlitchEffect();
    }

    void OnDestroy()
    {
        if (isGlitching)
        {
            StopGlitchEffect();
        }
    }
}