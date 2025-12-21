using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightSwitch5 : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public DialogueManager5 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Todo Settings")]
    public TodoUI5 todoManager;

    [Header("Light Settings")]
    public Light pointLight; // Ссылка на Point Light
    public float lightFadeOutDuration = 1.5f; // Время плавного выключения света
    public bool useLightFade = true; // Плавное выключение или мгновенное

    [Header("Audio Settings")]
    public AudioClip switchOffSound; // Звук выключения выключателя
    public AudioClip lightOffSound; // Дополнительный звук выключения света
    public AudioSource audioSource;

    private bool isInteractable = false;
    private bool hasBeenUsed = false;
    private bool dialogueTriggered = false;
    private Coroutine lightFadeCoroutine;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        SetInteractable(false);

        // Настраиваем свет (если назначен) - ВКЛЮЧЕН изначально
        InitializeLight();

        // Настраиваем аудио
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("LightSwitch5: AudioSource создан автоматически");
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        StartCoroutine(CheckTodoUIAvailability());
    }

    private void InitializeLight()
    {
        if (pointLight != null)
        {
            // Сохраняем изначальную интенсивность, свет ВКЛЮЧЕН
            pointLight.enabled = true;

            // Запоминаем оригинальную интенсивность (она должна быть настроена в инспекторе)
            if (pointLight.intensity <= 0)
            {
                pointLight.intensity = 1f; // Дефолтное значение если не настроено
                Debug.LogWarning("LightSwitch5: Интенсивность света была 0, установлено 1f");
            }

            // Если в инспекторе не настроено, устанавливаем настройки по умолчанию
            if (pointLight.type != LightType.Point)
            {
                Debug.LogWarning("LightSwitch5: Рекомендуется использовать Point Light!");
            }

            Debug.Log("LightSwitch5: Свет изначально включен");
        }
        else
        {
            Debug.LogWarning("LightSwitch5: Point Light не назначен!");
        }
    }

    private IEnumerator CheckTodoUIAvailability()
    {
        // Ждем пока todoManager не будет назначен
        while (todoManager == null)
        {
            yield return new WaitForSeconds(0.5f);
            todoManager = FindObjectOfType<TodoUI5>();
        }

        Debug.Log("LightSwitch5: TodoUI5 найден, ожидаем появления панели...");

        // Ждем пока Todo панель не станет активной
        while (!isInteractable && !hasBeenUsed)
        {
            if (todoManager != null && todoManager.IsPanelShowing())
            {
                SetInteractable(true);
                Debug.Log("LightSwitch5: Теперь интерактивен!");
                break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    public string GetInteractionText()
    {
        if (!isInteractable || hasBeenUsed)
        {
            return "";
        }

        return "Нажмите E";
    }

    public void Interact()
    {
        if (!isInteractable || hasBeenUsed || dialogueTriggered)
        {
            return;
        }

        Debug.Log("LightSwitch5: Взаимодействие с выключателем");

        // 1. СРАЗУ зачеркиваем пункт в TodoUI5!
        if (todoManager != null)
        {
            todoManager.CompleteLightTask(); // ? ЗДЕСЬ ПУНКТ ЗАЧЕРКИВАЕТСЯ!
        }

        // 2. ВЫКЛЮЧАЕМ свет
        TurnOffLight();

        // 3. Воспроизводим звук выключателя
        if (switchOffSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(switchOffSound);
        }

        // 4. Воспроизводим звук выключения света (если есть)
        if (lightOffSound != null && audioSource != null)
        {
            // Запускаем с небольшой задержкой для натуральности
            StartCoroutine(PlayLightSoundDelayed(0.3f));
        }

        SetInteractable(false);
        hasBeenUsed = true;
        dialogueTriggered = true;

        // 5. Запускаем диалог если есть (УЖЕ ПОСЛЕ ЗАЧЕРКИВАНИЯ!)
        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            dialogueManager.StartDialogue(dialogueLines, OnDialogueEnd);
        }
        else
        {
            // Если диалога нет, просто завершаем
            dialogueTriggered = false;
        }
    }

    private void TurnOffLight()
    {
        if (pointLight == null) return;

        if (useLightFade && lightFadeOutDuration > 0)
        {
            // Плавное выключение света
            if (lightFadeCoroutine != null)
                StopCoroutine(lightFadeCoroutine);

            lightFadeCoroutine = StartCoroutine(FadeOutLight());
        }
        else
        {
            // Мгновенное выключение
            pointLight.enabled = false;
        }

        Debug.Log("LightSwitch5: Свет выключен");
    }

    private IEnumerator FadeOutLight()
    {
        float startIntensity = pointLight.intensity;

        float timer = 0f;
        while (timer < lightFadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / lightFadeOutDuration;
            pointLight.intensity = Mathf.Lerp(startIntensity, 0f, progress);
            yield return null;
        }

        // После плавного уменьшения интенсивности выключаем свет полностью
        pointLight.intensity = 0f;
        pointLight.enabled = false;
    }

    private IEnumerator PlayLightSoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(lightOffSound);
    }

    private void OnDialogueEnd()
    {
        // Диалог завершен, но пункт УЖЕ зачеркнут!
        dialogueTriggered = false;
        Debug.Log("LightSwitch5: Диалог завершен");
    }

    private void SetInteractable(bool interactable)
    {
        isInteractable = interactable;

        if (interactable)
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    void OnDestroy()
    {
        StopAllCoroutines();

        if (lightFadeCoroutine != null)
        {
            StopCoroutine(lightFadeCoroutine);
        }
    }

    void OnDrawGizmos()
    {
        if (isInteractable && !hasBeenUsed)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.5f);

            // Если есть свет, показываем его радиус (желтый - свет включен)
            if (pointLight != null && pointLight.type == LightType.Point)
            {
                Gizmos.color = pointLight.enabled ? Color.yellow : Color.gray;
                Gizmos.DrawWireSphere(pointLight.transform.position, pointLight.range);
            }
        }
        else if (hasBeenUsed)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.3f);

            // Если свет выключен после использования
            if (pointLight != null && pointLight.type == LightType.Point)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(pointLight.transform.position, pointLight.range * 0.5f); // Меньший радиус
            }
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.1f);

            // Если свет включен но выключатель еще не активен
            if (pointLight != null && pointLight.enabled)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f); // Оранжевый
                Gizmos.DrawWireSphere(pointLight.transform.position, pointLight.range * 0.7f);
            }
        }
    }

    // Опционально: метод для включения света (если понадобится)
    public void TurnOnLight()
    {
        if (pointLight != null)
        {
            if (lightFadeCoroutine != null)
                StopCoroutine(lightFadeCoroutine);

            pointLight.enabled = true;
            pointLight.intensity = 1f; // Восстанавливаем интенсивность
        }
    }
}