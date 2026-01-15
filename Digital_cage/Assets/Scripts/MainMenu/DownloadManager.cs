using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DownloadManager : MonoBehaviour
{
    [Header("Download Panel Settings")]
    public GameObject downloadPanel; // Ссылка на панель загрузок
    public float animationDuration = 0.3f; // Длительность анимации
    public Button closeButton; // Кнопка закрытия (крестик)

    [Header("Download Plates")]
    public DownloadPlate[] downloadPlates; // Массив плашек с информацией о загрузках

    [Header("Fade Settings")]
    public Image fadeImage; // Изображение для затемнения (черный экран)
    public float fadeDuration = 1.0f; // Длительность затемнения

    private bool isDownloadOpen = false;
    private bool isAnimating = false;
    private CanvasGroup downloadCanvasGroup;

    [System.Serializable]
    public class DownloadPlate
    {
        public string plateName; // Название для отладки
        public Button plateButton; // Ссылка на кнопку плашки
        public string sceneToLoad; // Имя сцены для загрузки (заполняется в инспекторе)
        public int sceneIndex = -1; // ИЛИ индекс сцены (если используете индексы)
    }

    // === ПУБЛИЧНЫЕ СВОЙСТВА ДЛЯ ДОСТУПА ИЗ ДРУГИХ СКРИПТОВ ===
    public bool IsDownloadOpen => isDownloadOpen;
    public bool IsAnimating => isAnimating;

    void Start()
    {
        // Настраиваем панель загрузок
        if (downloadPanel != null)
        {
            // Добавляем CanvasGroup если его нет
            downloadCanvasGroup = downloadPanel.GetComponent<CanvasGroup>();
            if (downloadCanvasGroup == null)
            {
                downloadCanvasGroup = downloadPanel.AddComponent<CanvasGroup>();
            }

            // Сразу делаем панель неактивной и невидимой
            downloadPanel.SetActive(false);
            downloadCanvasGroup.alpha = 0f;
            downloadCanvasGroup.interactable = false;
            downloadCanvasGroup.blocksRaycasts = false;
        }

        // Настраиваем затемнение
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
            fadeImage.color = new Color(0, 0, 0, 0);
        }
        else
        {
            Debug.LogWarning("FadeImage не назначен! Создайте UI Image в Canvas для затемнения.");
        }

        // Настраиваем кнопку закрытия
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseDownloadPanel);
        }
        else
        {
            Debug.LogWarning("CloseButton не назначен в DownloadManager!");
        }

        // Настраиваем кнопки плашек
        SetupDownloadPlates();
    }

    void Update()
    {
        // Обработка закрытия по ESC (опционально)
        if (isDownloadOpen && Input.GetKeyDown(KeyCode.Escape) && !isAnimating)
        {
            CloseDownloadPanel();
        }
    }

    private void SetupDownloadPlates()
    {
        // Проверяем массив плашек
        if (downloadPlates == null || downloadPlates.Length == 0)
        {
            Debug.LogWarning("DownloadPlates массив не настроен!");
            return;
        }

        // Настраиваем каждую плашку
        for (int i = 0; i < downloadPlates.Length; i++)
        {
            int index = i; // Важно для замыкания в лямбда-выражении

            if (downloadPlates[index].plateButton != null)
            {
                // Добавляем слушатель события нажатия
                downloadPlates[index].plateButton.onClick.AddListener(() => LoadSceneFromPlate(index));

                Debug.Log($"Настроена плашка {index}: {downloadPlates[index].plateName}");
            }
            else
            {
                Debug.LogWarning($"Плашка {index} не имеет кнопки!");
            }
        }
    }

    private void LoadSceneFromPlate(int plateIndex)
    {
        if (plateIndex < 0 || plateIndex >= downloadPlates.Length)
        {
            Debug.LogError($"Неверный индекс плашки: {plateIndex}");
            return;
        }

        var plate = downloadPlates[plateIndex];

        // НЕ закрываем панель! Просто загружаем сцену с затемнением
        if (!isAnimating)
        {
            StartCoroutine(LoadSceneWithFadeCoroutine(plate));
        }
    }

    private IEnumerator LoadSceneWithFadeCoroutine(DownloadPlate plate)
    {
        // Отключаем взаимодействие с панелью во время загрузки
        if (downloadCanvasGroup != null)
        {
            downloadCanvasGroup.interactable = false;
            downloadCanvasGroup.blocksRaycasts = false;
        }

        // Активируем затемнение (поверх панели)
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = true; // Блокируем взаимодействие
        }

        // Затемнение в черный
        yield return StartCoroutine(FadeScreen(0f, 1f, fadeDuration));

        Time.timeScale = 1f; // Восстанавливаем время

        // Загружаем сцену
        if (!string.IsNullOrEmpty(plate.sceneToLoad))
        {
            Debug.Log($"Загружаем сцену по имени: {plate.sceneToLoad}");

            // Запускаем асинхронную загрузку
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(plate.sceneToLoad);
            asyncLoad.allowSceneActivation = false;

            // Ждем загрузки сцены
            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            // Даем завершить затемнение (еще немного ждем)
            yield return new WaitForSecondsRealtime(0.1f);

            // Активируем сцену
            asyncLoad.allowSceneActivation = true;
        }
        else if (plate.sceneIndex >= 0)
        {
            Debug.Log($"Загружаем сцену по индексу: {plate.sceneIndex}");

            // Запускаем асинхронную загрузку
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(plate.sceneIndex);
            asyncLoad.allowSceneActivation = false;

            // Ждем загрузки сцены
            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            // Даем завершить затемнение
            yield return new WaitForSecondsRealtime(0.1f);

            // Активируем сцену
            asyncLoad.allowSceneActivation = true;
        }
        else
        {
            Debug.LogError($"Плашка '{plate.plateName}' не имеет настроенной сцены для загрузки!");

            // Если ошибка - убираем затемнение и восстанавливаем панель
            if (fadeImage != null)
            {
                yield return StartCoroutine(FadeScreen(1f, 0f, fadeDuration));
                fadeImage.gameObject.SetActive(false);
                fadeImage.raycastTarget = false;
            }

            // Восстанавливаем взаимодействие с панелью
            if (downloadCanvasGroup != null && isDownloadOpen)
            {
                downloadCanvasGroup.interactable = true;
                downloadCanvasGroup.blocksRaycasts = true;
            }
        }
    }

    // Метод для плавного затемнения/осветления экрана
    private IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;

            color.a = Mathf.Lerp(startAlpha, endAlpha, progress);
            fadeImage.color = color;

            yield return null;
        }

        // Финальное значение
        color.a = endAlpha;
        fadeImage.color = color;
    }

    // ========== МЕТОДЫ ДЛЯ ОТКРЫТИЯ/ЗАКРЫТИЯ ПАНЕЛИ ==========

    public void OpenDownloadPanel()
    {
        if (downloadPanel != null && !isAnimating && !isDownloadOpen)
        {
            StartCoroutine(OpenDownloadPanelCoroutine());
        }
        else
        {
            Debug.LogWarning("DownloadPanel не назначен или анимация уже идет!");
        }
    }

    public void CloseDownloadPanel()
    {
        if (downloadPanel != null && !isAnimating && isDownloadOpen)
        {
            StartCoroutine(CloseDownloadPanelCoroutine());
        }
    }

    private IEnumerator OpenDownloadPanelCoroutine()
    {
        isAnimating = true;

        // Активируем панель перед анимацией
        downloadPanel.SetActive(true);
        downloadCanvasGroup.interactable = false;
        downloadCanvasGroup.blocksRaycasts = false;

        // Приостанавливаем игру, если мы не в главном меню
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != "MainMenu")
        {
            Time.timeScale = 0f;
        }

        // Анимация появления
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationDuration;

            // Плавное увеличение прозрачности
            downloadCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, progress);

            // Легкое масштабирование для эффекта "появления"
            downloadPanel.transform.localScale = Vector3.one * Mathf.SmoothStep(0.8f, 1f, progress);

            yield return null;
        }

        // Устанавливаем финальные значения
        downloadCanvasGroup.alpha = 1f;
        downloadPanel.transform.localScale = Vector3.one;
        downloadCanvasGroup.interactable = true;
        downloadCanvasGroup.blocksRaycasts = true;

        isDownloadOpen = true;
        isAnimating = false;

        Debug.Log("Открыта панель загрузок");
    }

    private IEnumerator CloseDownloadPanelCoroutine()
    {
        isAnimating = true;

        // Отключаем взаимодействие во время анимации
        downloadCanvasGroup.interactable = false;
        downloadCanvasGroup.blocksRaycasts = false;

        // Анимация исчезновения
        float elapsedTime = 0f;
        Vector3 startScale = downloadPanel.transform.localScale;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationDuration;

            // Плавное уменьшение прозрачности
            downloadCanvasGroup.alpha = Mathf.SmoothStep(1f, 0f, progress);

            // Легкое масштабирование для эффекта "исчезновения"
            downloadPanel.transform.localScale = startScale * Mathf.SmoothStep(1f, 0.8f, progress);

            yield return null;
        }

        // Устанавливаем финальные значения
        downloadCanvasGroup.alpha = 0f;
        downloadPanel.transform.localScale = startScale;
        downloadPanel.SetActive(false);

        isDownloadOpen = false;
        isAnimating = false;

        // Возобновляем игру, если мы не в главном меню
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != "MainMenu")
        {
            Time.timeScale = 1f;
        }

        Debug.Log("Панель загрузок закрыта");
    }

    public void ToggleDownloadPanel()
    {
        if (isAnimating) return;

        if (isDownloadOpen)
        {
            CloseDownloadPanel();
        }
        else
        {
            OpenDownloadPanel();
        }
    }

    // Метод для кнопки "Скачать" в главном меню
    public void OnDownloadButtonClicked()
    {
        OpenDownloadPanel();
    }
}