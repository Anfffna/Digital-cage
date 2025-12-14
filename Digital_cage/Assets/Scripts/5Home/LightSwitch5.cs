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
    public float lightFadeInDuration = 1.5f; // Время плавного включения света
    public bool useLightFade = true; // Плавное включение или мгновенное

    [Header("Audio Settings")]
    public AudioClip switchOnSound;
    public AudioClip lightHumSound; // Дополнительный звук включения света
    public AudioSource audioSource;

    private bool isInteractable = false;
    private bool hasBeenUsed = false;
    private bool dialogueTriggered = false;
    private Coroutine lightFadeCoroutine;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        SetInteractable(false);

        // Настраиваем свет (если назначен)
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
            // Сохраняем изначальную интенсивность, но выключаем свет
            pointLight.enabled = false;
            pointLight.intensity = 0f;

            // Если в инспекторе не настроено, устанавливаем настройки по умолчанию
            if (pointLight.type != LightType.Point)
            {
                Debug.LogWarning("LightSwitch5: Рекомендуется использовать Point Light!");
            }
        }
        else
        {
            Debug.LogWarning("LightSwitch5: Point Light не назначен! Свет не будет включаться.");
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

        // Включаем свет
        TurnOnLight();

        // Воспроизводим звук выключателя
        if (switchOnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(switchOnSound);
        }

        // Воспроизводим звук включения света (если есть)
        if (lightHumSound != null && audioSource != null)
        {
            // Запускаем с небольшой задержкой для натуральности
            StartCoroutine(PlayLightSoundDelayed(0.3f));
        }

        SetInteractable(false);
        hasBeenUsed = true;
        dialogueTriggered = true;

        // Запускаем диалог если есть
        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            dialogueManager.StartDialogue(dialogueLines, OnDialogueEnd);
        }
        else
        {
            CompleteTask();
        }
    }

    private void TurnOnLight()
    {
        if (pointLight == null) return;

        if (useLightFade && lightFadeInDuration > 0)
        {
            // Плавное включение света
            if (lightFadeCoroutine != null)
                StopCoroutine(lightFadeCoroutine);

            lightFadeCoroutine = StartCoroutine(FadeInLight());
        }
        else
        {
            // Мгновенное включение
            pointLight.enabled = true;

            // Если интенсивность была 0, устанавливаем разумное значение
            if (pointLight.intensity <= 0)
                pointLight.intensity = 1f;
        }

        Debug.Log("LightSwitch5: Свет включен");
    }

    private IEnumerator FadeInLight()
    {
        pointLight.enabled = true;

        float originalIntensity = pointLight.intensity;
        if (originalIntensity <= 0) originalIntensity = 1f; // Дефолтное значение

        float timer = 0f;
        while (timer < lightFadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / lightFadeInDuration;
            pointLight.intensity = Mathf.Lerp(0f, originalIntensity, progress);
            yield return null;
        }

        pointLight.intensity = originalIntensity;
    }

    private IEnumerator PlayLightSoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(lightHumSound);
    }

    private void OnDialogueEnd()
    {
        CompleteTask();
        dialogueTriggered = false;
    }

    private void CompleteTask()
    {
        if (todoManager != null)
        {
            todoManager.CompleteLightTask();
            Debug.Log("LightSwitch5: Задача завершена в TodoUI5");
        }
        else
        {
            Debug.LogWarning("LightSwitch5: TodoUI5 не назначен!");
        }
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

            // Если есть свет, показываем его радиус
            if (pointLight != null && pointLight.type == LightType.Point)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(pointLight.transform.position, pointLight.range);
            }
        }
        else if (hasBeenUsed)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.3f);

            // Подсветка включенного света
            if (pointLight != null && pointLight.enabled)
            {
                Gizmos.color = new Color(1f, 1f, 0.3f, 0.5f);
                Gizmos.DrawWireSphere(pointLight.transform.position, pointLight.range);
            }
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.1f);
        }
    }

    // Опционально: метод для выключения света (если понадобится)
    public void TurnOffLight()
    {
        if (pointLight != null)
        {
            if (lightFadeCoroutine != null)
                StopCoroutine(lightFadeCoroutine);

            pointLight.enabled = false;
        }
    }
}