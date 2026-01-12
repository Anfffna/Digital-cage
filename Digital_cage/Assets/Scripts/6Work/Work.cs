using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Work : MonoBehaviour
{
    [Header("Canvas Reference")]
    public Canvas workCanvas; // Ссылка на Canvas

    [Header("Cursor Settings")]
    public CursorUI cursorManager; // Ссылка на менеджер курсора
    public bool showCursorWithUI = true; // Показывать курсор вместе с UI

    [Header("Timing Settings")]
    public float showDelay = 2f; // Задержка после смены материала окон
    public float fadeDuration = 1.5f; // Длительность появления

    [Header("Document Images")]
    public List<DocumentImage> documents = new List<DocumentImage>(); // Список всех документов-картинок
    private int currentDocumentIndex = 0; // Индекс текущего документа

    [Header("UI References - Images")]
    public Image currentDocumentImage; // Image для отображения текущего документа

    [Header("Button References - FULL BUTTONS")]
    public Button urgentButton; // ПОЛНАЯ кнопка СРОЧНО (с Image внутри)
    public Button planButton; // ПОЛНАЯ кнопка В ПЛАН (с Image внутри)
    public Button archiveButton; // ПОЛНАЯ кнопка АРХИВ (с Image внутри)

    [Header("Button Images (для визуальных эффектов)")]
    public Image urgentButtonImage; // Image внутри кнопки СРОЧНО
    public Image planButtonImage; // Image внутри кнопки В ПЛАН  
    public Image archiveButtonImage; // Image внутри кнопки АРХИВ

    [Header("Error Message")]
    public Image errorMessageImage; // Image сообщения об ошибке
    public float errorDisplayTime = 3f; // Время показа ошибки
    private Coroutine errorCoroutine;

    [Header("External Scripts")]
    public ErrorTalk errorTalkScript;
    public SoundScary soundScaryScript;

    private CanvasGroup canvasGroup;
    private bool isShown = false;
    private Windows windowsScript;

    // Флаг выполнения работы в ТЕКУЩЕЙ сессии игры
    private bool isWorkCompletedInThisSession = false;

    // Структура для хранения данных картинки-документа
    [System.Serializable]
    public class DocumentImage
    {
        public Sprite documentSprite; // Спрайт документа
        public CorrectButton correctButton; // Правильная кнопка для этого документа
    }

    // Перечисление правильных кнопок
    public enum CorrectButton
    {
        Urgent,
        Plan,
        Archive
    }

    void Start()
    {
        // Каждый новый запуск игры - работа доступна
        isWorkCompletedInThisSession = false;
        Debug.Log("Work: Новая игровая сессия - работа доступна");

        // Инициализация UI
        InitializeUI();
    }

    void InitializeUI()
    {
        // Ищем скрипт окон
        windowsScript = FindObjectOfType<Windows>();

        // Настраиваем Canvas
        if (workCanvas != null)
        {
            canvasGroup = workCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = workCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            workCanvas.enabled = true;
        }
        else
        {
            Debug.LogError("Work: Не назначен workCanvas!");
        }

        // Ищем CursorUI
        if (cursorManager == null)
        {
            cursorManager = FindObjectOfType<CursorUI>();
        }

        // Настраиваем кнопки
        SetupButtons();

        // Автоматически находим Image внутри кнопок, если не назначены
        FindButtonImages();

        // Скрываем сообщение об ошибке
        if (errorMessageImage != null)
        {
            errorMessageImage.gameObject.SetActive(false);
        }

        // Проверяем, все ли документы настроены
        if (documents.Count == 0)
        {
            Debug.LogWarning("Work: Список документов пуст! Добавьте документы-картинки в инспекторе.");
        }
    }

    void FindButtonImages()
    {
        // Автоматически находим Image внутри кнопок, если не назначены вручную
        if (urgentButton != null && urgentButtonImage == null)
            urgentButtonImage = urgentButton.GetComponent<Image>();

        if (planButton != null && planButtonImage == null)
            planButtonImage = planButton.GetComponent<Image>();

        if (archiveButton != null && archiveButtonImage == null)
            archiveButtonImage = archiveButton.GetComponent<Image>();
    }

    void SetupButtons()
    {
        // Назначаем обработчики нажатий для ПОЛНЫХ КНОПОК
        if (urgentButton != null)
            urgentButton.onClick.AddListener(() => OnButtonClicked(CorrectButton.Urgent));
        else
            Debug.LogError("Work: Не назначена кнопка Urgent!");

        if (planButton != null)
            planButton.onClick.AddListener(() => OnButtonClicked(CorrectButton.Plan));
        else
            Debug.LogError("Work: Не назначена кнопка Plan!");

        if (archiveButton != null)
            archiveButton.onClick.AddListener(() => OnButtonClicked(CorrectButton.Archive));
        else
            Debug.LogError("Work: Не назначена кнопка Archive!");
    }

    void Update()
    {
        // Если работа уже выполнена в этой сессии, не проверяем ничего
        if (isWorkCompletedInThisSession) return;

        // Проверяем, можно ли показывать UI
        if (!isShown && windowsScript != null)
        {
            CheckForWindowsMaterialChange();
        }
    }

    void CheckForWindowsMaterialChange()
    {
        // Если работа уже выполнена в этой сессии или UI уже показан, не проверяем
        if (isWorkCompletedInThisSession || isShown) return;

        // Проверяем смену материала окон через рефлексию
        System.Reflection.FieldInfo field = windowsScript.GetType().GetField("materialChanged",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            bool materialChanged = (bool)field.GetValue(windowsScript);

            if (materialChanged)
            {
                Debug.Log("Work: Обнаружена смена материала окон, запуск UI");
                StartCoroutine(ShowCanvasWithDelay());
                isShown = true;
            }
        }
    }

    IEnumerator ShowCanvasWithDelay()
    {
        yield return new WaitForSeconds(showDelay);

        // Показываем курсор
        if (showCursorWithUI && cursorManager != null)
        {
            cursorManager.ShowCursor();
            Debug.Log("Work: Курсор показан");
        }

        // Плавное появление Canvas
        if (workCanvas != null && canvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;

            // Показываем первый документ
            ShowDocument(0);
            Debug.Log("Work: Canvas показан, документ загружен");
        }
    }

    void ShowDocument(int index)
    {
        if (index >= 0 && index < documents.Count)
        {
            currentDocumentIndex = index;
            DocumentImage doc = documents[index];

            // Устанавливаем картинку документа
            if (currentDocumentImage != null && doc.documentSprite != null)
            {
                currentDocumentImage.sprite = doc.documentSprite;
                currentDocumentImage.SetNativeSize(); // Авторазмер под спрайт

                // Активируем Image, если он был выключен
                if (!currentDocumentImage.gameObject.activeSelf)
                    currentDocumentImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError($"Work: Нет картинки для документа {index}");
            }

            Debug.Log($"Work: Показан документ {index + 1}/{documents.Count}. Правильный ответ: {doc.correctButton}");
        }
        else
        {
            Debug.LogError($"Work: Неверный индекс документа: {index}");
        }
    }

    void OnButtonClicked(CorrectButton clickedButton)
    {
        // Если работа уже выполнена в этой сессии, игнорируем клики
        if (isWorkCompletedInThisSession) return;

        if (currentDocumentIndex >= documents.Count)
        {
            Debug.Log("Work: Все документы обработаны, клики игнорируются");
            return;
        }

        DocumentImage currentDoc = documents[currentDocumentIndex];

        // Проверяем, правильная ли кнопка нажата
        if (clickedButton == currentDoc.correctButton)
        {
            Debug.Log($"Work: Правильно! Документ отправлен в {clickedButton}");

            // Визуальная обратная связь
            HighlightCorrectButton(clickedButton, true);

            // Воспроизводим звук и показываем диалог через SoundScary
            // Звук запустится через 6 секунд сам по себе
            if (soundScaryScript != null)
            {
                soundScaryScript.PlaySoundAndDialogue();
            }
            else
            {
                Debug.LogWarning("Work: SoundScary не назначен!");
            }

            // СРАЗУ переходим к следующему документу - НЕ ЖДЕМ!
            StartCoroutine(MoveToNextDocumentWithDelay(0.5f)); // Только небольшая задержка для визуального эффекта
        }
        else
        {
            Debug.Log($"Work: Ошибка! Нажата {clickedButton}, нужно {currentDoc.correctButton}");

            // Показываем диалог ошибки через ErrorTalk
            if (errorTalkScript != null)
            {
                errorTalkScript.TriggerErrorDialogue();
            }
            else
            {
                Debug.LogWarning("Work: ErrorTalk не назначен!");
            }

            // Показываем сообщение об ошибке
            ShowErrorMessage();

            // Подсвечиваем правильную кнопку (красным для ошибки)
            HighlightCorrectButton(currentDoc.correctButton, false);
        }
    }

    void HighlightCorrectButton(CorrectButton correctButton, bool isCorrect)
    {
        // Сбрасываем все кнопки к обычному состоянию
        ResetButtonColors();

        // Подсвечиваем кнопку
        Image targetButton = null;
        switch (correctButton)
        {
            case CorrectButton.Urgent:
                targetButton = urgentButtonImage;
                break;
            case CorrectButton.Plan:
                targetButton = planButtonImage;
                break;
            case CorrectButton.Archive:
                targetButton = archiveButtonImage;
                break;
        }

        if (targetButton != null)
        {
            // Сохраняем оригинальный цвет
            Color originalColor = targetButton.color;
            Color highlightColor = isCorrect ? Color.green : Color.red;

            // Подсвечиваем
            StartCoroutine(FlashButtonColor(targetButton, highlightColor, originalColor, 0.5f));
        }
    }

    void ResetButtonColors()
    {
        // Восстанавливаем оригинальные цвета кнопок
        if (urgentButtonImage != null) urgentButtonImage.color = Color.white;
        if (planButtonImage != null) planButtonImage.color = Color.white;
        if (archiveButtonImage != null) archiveButtonImage.color = Color.white;
    }

    IEnumerator FlashButtonColor(Image button, Color flashColor, Color originalColor, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.PingPong(timer * 4f, 1f); // Быстрое мигание
            button.color = Color.Lerp(originalColor, flashColor, t);
            yield return null;
        }
        button.color = originalColor;
    }

    IEnumerator MoveToNextDocumentWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        MoveToNextDocument();
    }

    void MoveToNextDocument()
    {
        currentDocumentIndex++;

        if (currentDocumentIndex < documents.Count)
        {
            // Показываем следующий документ
            ShowDocument(currentDocumentIndex);
            ResetButtonColors();
        }
        else
        {
            // Все документы обработаны
            Debug.Log("Work: Все документы обработаны! Завершение работы в этой сессии.");

            // Завершение работы
            StartCoroutine(CompleteWork());
        }
    }

    IEnumerator CompleteWork()
    {
        Debug.Log("Work: Завершение работы в этой сессии...");

        // Мгновенно скрываем документ
        if (currentDocumentImage != null)
        {
            currentDocumentImage.gameObject.SetActive(false);
        }

        // Плавно скрываем весь Canvas
        if (canvasGroup != null)
        {
            float timer = 0f;
            float startAlpha = canvasGroup.alpha;

            // Плавное исчезновение за 1 секунду
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        // Скрываем курсор
        if (cursorManager != null && showCursorWithUI)
        {
            cursorManager.HideCursor();
            Debug.Log("Work: Курсор скрыт");
        }

        isShown = false;

        // УСТАНАВЛИВАЕМ ФЛАГ, ЧТО РАБОТА ВЫПОЛНЕНА В ЭТОЙ СЕССИИ
        isWorkCompletedInThisSession = true;

        // Отключаем Canvas
        if (workCanvas != null)
        {
            workCanvas.enabled = false;
        }

        Debug.Log("Work: Работа завершена в этой сессии. Больше не появится до перезапуска игры.");
    }

    void ShowErrorMessage()
    {
        if (errorMessageImage == null)
        {
            Debug.LogError("Work: Не назначен errorMessageImage!");
            return;
        }

        // Останавливаем предыдущую корутину, если она есть
        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine);
        }

        // Запускаем новую корутину для мигания ошибки
        errorCoroutine = StartCoroutine(FlashErrorMessage());
    }

    IEnumerator FlashErrorMessage()
    {
        // Показываем сообщение об ошибке
        GameObject errorObject = errorMessageImage.gameObject;
        errorObject.SetActive(true);

        // Мигание сообщения
        float timer = 0f;
        float flashDuration = errorDisplayTime;

        while (timer < flashDuration)
        {
            // Быстрое мигание красным
            for (int i = 0; i < 3; i++)
            {
                errorMessageImage.color = Color.red; // Красный
                yield return new WaitForSeconds(0.1f);
                errorMessageImage.color = new Color(1, 0.5f, 0.5f, 0.5f); // Светло-красный, полупрозрачный
                yield return new WaitForSeconds(0.1f);
            }

            timer += 0.6f;
            yield return new WaitForSeconds(0.4f);
        }

        // Скрываем сообщение
        errorObject.SetActive(false);
        errorMessageImage.color = Color.white; // Восстанавливаем цвет
        errorCoroutine = null;
    }

    // Публичный метод для принудительного показа
    public void ShowCanvas()
    {
        // Если работа уже выполнена в этой сессии, не показываем
        if (isWorkCompletedInThisSession) return;

        if (!isShown)
        {
            StopAllCoroutines();

            if (showCursorWithUI && cursorManager != null)
            {
                cursorManager.ShowCursor();
            }

            StartCoroutine(ShowCanvasWithDelay());
        }
    }

    // Метод для скрытия Canvas и курсора
    public void HideCanvas()
    {
        if (isShown)
        {
            StopAllCoroutines();

            if (cursorManager != null)
            {
                cursorManager.HideCursor();
                Debug.Log("Work: Курсор скрыт");
            }

            StartCoroutine(FadeOutCanvas());
        }
    }

    IEnumerator FadeOutCanvas()
    {
        float timer = 0f;
        float startAlpha = canvasGroup.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        isShown = false;
    }

    void OnDestroy()
    {
        if (cursorManager != null && isShown)
        {
            cursorManager.HideCursor();
        }
    }
}