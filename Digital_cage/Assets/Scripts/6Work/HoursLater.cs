using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HoursLater : MonoBehaviour
{
    [Header("UI Settings")]
    public Image hoursLaterImage; // Image который будет появляться
    public CanvasGroup imageCanvasGroup; // CanvasGroup для плавного появления

    [Header("Timing Settings")]
    public float showDelay = 1f; // Задержка после завершения работы перед показом
    public float fadeInDuration = 1.5f; // Длительность появления
    public float displayDuration = 4f; // Время показа (4 секунды)
    public float fadeOutDuration = 1.5f; // Длительность исчезновения
    public float delayAfterFadeOut = 2f; // Задержка ПОСЛЕ исчезновения картинки перед диалогом

    [Header("Work Reference")]
    public Work workScript; // Ссылка на скрипт Work

    [Header("Dialogue Settings")]
    public ManagerDialogue6 dialogueManager;
    [TextArea(2, 5)]
    public List<string> afterHoursDialogue = new List<string>();

    private bool hasShown = false; // Флаг, что уже показали

    void Start()
    {
        // Ищем Work если не назначен
        if (workScript == null)
        {
            workScript = GetComponent<Work>();
            if (workScript == null)
            {
                Debug.LogWarning("HoursLater: Не найден скрипт Work на этом объекте!");
            }
        }

        // Ищем ManagerDialogue6 если не назначен
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<ManagerDialogue6>();
            if (dialogueManager == null)
            {
                Debug.LogWarning("HoursLater: ManagerDialogue6 не найден в сцене!");
            }
        }

        // Проверяем наличие диалогов
        if (afterHoursDialogue == null || afterHoursDialogue.Count == 0)
        {
            Debug.LogWarning("HoursLater: Не настроены строки диалога после 'Hours Later'!");
        }

        // Скрываем Image при старте
        if (hoursLaterImage != null)
        {
            hoursLaterImage.gameObject.SetActive(false);

            // Если есть CanvasGroup, настраиваем его
            if (imageCanvasGroup == null)
            {
                imageCanvasGroup = hoursLaterImage.GetComponent<CanvasGroup>();
                if (imageCanvasGroup == null)
                {
                    imageCanvasGroup = hoursLaterImage.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (imageCanvasGroup != null)
            {
                imageCanvasGroup.alpha = 0f;
            }
        }
        else
        {
            Debug.LogError("HoursLater: Не назначен hoursLaterImage!");
        }

        hasShown = false;
        Debug.Log("HoursLater: Инициализирован");
    }

    void Update()
    {
        // Проверяем завершение работы, если еще не показывали
        if (!hasShown && workScript != null)
        {
            CheckWorkCompletion();
        }
    }

    void CheckWorkCompletion()
    {
        // Проверяем через рефлексию, завершена ли работа
        System.Reflection.FieldInfo field = workScript.GetType().GetField("isWorkCompletedInThisSession",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            bool isWorkCompleted = (bool)field.GetValue(workScript);

            if (isWorkCompleted && !hasShown)
            {
                StartCoroutine(ShowHoursLaterSequence());
                hasShown = true;
            }
        }
        else
        {
            Debug.LogWarning("HoursLater: Не могу получить доступ к isWorkCompletedInThisSession");
        }
    }

    IEnumerator ShowHoursLaterSequence()
    {
        Debug.Log("HoursLater: Начинаю показ 'Hours Later'");

        // 1. Ждем небольшую задержку
        yield return new WaitForSeconds(showDelay);

        // 2. Активируем Image
        if (hoursLaterImage != null)
        {
            hoursLaterImage.gameObject.SetActive(true);

            // 3. Плавное появление (fade in)
            if (imageCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(imageCanvasGroup, 0f, 1f, fadeInDuration));
            }
            else
            {
                hoursLaterImage.color = new Color(1, 1, 1, 1);
            }

            // 4. Ждем 4 секунды
            yield return new WaitForSeconds(displayDuration);

            // 5. Плавное исчезновение (fade out)
            if (imageCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(imageCanvasGroup, 1f, 0f, fadeOutDuration));
            }
            else
            {
                float timer = 0f;
                Color startColor = hoursLaterImage.color;
                Color endColor = startColor;
                endColor.a = 0f;

                while (timer < fadeOutDuration)
                {
                    timer += Time.deltaTime;
                    hoursLaterImage.color = Color.Lerp(startColor, endColor, timer / fadeOutDuration);
                    yield return null;
                }
            }

            // 6. Скрываем Image
            hoursLaterImage.gameObject.SetActive(false);

            Debug.Log("HoursLater: Картинка полностью исчезла");

            // 7. ЖДЕМ 2 СЕКУНДЫ после исчезновения
            Debug.Log($"HoursLater: Жду {delayAfterFadeOut} секунд после исчезновения картинки");
            yield return new WaitForSeconds(delayAfterFadeOut);

            // 8. ЗАПУСКАЕМ ДИАЛОГ
            StartAfterHoursDialogue();

            Debug.Log("HoursLater: Показ 'Hours Later' завершен, диалог запущен");
        }
    }

    void StartAfterHoursDialogue()
    {
        if (dialogueManager != null && afterHoursDialogue != null && afterHoursDialogue.Count > 0)
        {
            Debug.Log($"HoursLater: Запускаю диалог после 'Hours Later' ({afterHoursDialogue.Count} строк)");

            // Запускаем диалог с коллбэком
            dialogueManager.StartDialogue(afterHoursDialogue, StartSmsMamaSequence);
        }
    }

    void StartSmsMamaSequence()
    {
        // Ищем SmsMama в сцене
        SmsMama smsMama = FindObjectOfType<SmsMama>();
        if (smsMama != null)
        {
            Debug.Log("HoursLater: Запускаю SmsMama последовательность");
            smsMama.StartSmsSequence();
        }
        else
        {
            Debug.LogWarning("HoursLater: Не найден SmsMama в сцене!");
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float fromAlpha, float toAlpha, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            group.alpha = Mathf.Lerp(fromAlpha, toAlpha, progress);
            yield return null;
        }

        group.alpha = toAlpha;
    }

    // Публичный метод для принудительного запуска (для тестирования)
    [ContextMenu("Тест: Показать Hours Later")]
    public void TestShowHoursLater()
    {
        if (!hasShown)
        {
            StartCoroutine(ShowHoursLaterSequence());
            hasShown = true;
        }
    }

    // Публичный метод для сброса состояния (для тестирования)
    [ContextMenu("Сбросить состояние")]
    public void ResetState()
    {
        hasShown = false;

        if (hoursLaterImage != null)
        {
            hoursLaterImage.gameObject.SetActive(false);

            if (imageCanvasGroup != null)
            {
                imageCanvasGroup.alpha = 0f;
            }
        }

        Debug.Log("HoursLater: Состояние сброшено");
    }
}