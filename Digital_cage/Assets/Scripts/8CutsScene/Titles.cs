using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // Добавляем для управления сценами

public class Titles : MonoBehaviour
{
    [Header("Позиции камеры")]
    public Transform doorPosition;           // 1. Общий план двери
    public Transform cumPosition;            // 2. План на CUM
    public Transform zoomCumPosition;        // 3. Крупный план CUM

    [Header("Тайминги движения")]
    public float doorHoldTime = 1f;           // Задержка на общей двери в начале
    public float doorToCumTime = 3f;          // Время от двери к CUM
    public float cumToZoomTime = 2f;          // Время от CUM к зуму CUM
    public float finalHoldTime = 2f;          // Задержка в конце на зуме CUM

    [Header("Спрайт титров")]
    public SpriteRenderer titleSprite;        // SpriteRenderer с титрами
    public float spriteFadeInTime = 2f;       // Время появления спрайта
    public float spriteHoldTime = 3f;         // Время показа спрайта
    public float spriteFadeOutTime = 2f;      // Время исчезновения спрайта
    public float spriteDelayAfterCamera = 0.5f; // Задержка после прибытия камеры

    [Header("Настройки спрайта")]
    public Color spriteColor = Color.white;   // Цвет спрайта
    public int sortingOrder = 100;            // Sorting Order для отображения поверх всего

    [Header("Черный экран")]
    public Image blackScreen;                // UI Image для черного экрана
    public float blackScreenFadeInTime = 4f; // Время появления черного экрана
    public float blackScreenHoldTime = 3f;   // Время удержания черного экрана (не исчезает!)

    [Header("Световые эффекты")]
    public Light8[] lightsToTurnOff;          // Все Light8 которые нужно выключить
    public float turnOffLightsDelay = 0.5f;   // Задержка после исчезновения спрайта

    [Header("Аудио")]
    public AudioClip titleMusic;             // Аудиоклип для титров
    public AudioSource audioSource;          // AudioSource для воспроизведения
    public float audioFadeInTime = 2f;       // Время нарастания громкости аудио

    [Header("Переход в меню")]
    public string mainMenuSceneName = "MainMenu"; // Название сцены главного меню
    public float menuTransitionDelay = 3f;    // Задержка перед переходом в меню

    private Camera mainCamera;
    private bool isMoving = false;
    private Coroutine spriteCoroutine;
    private float originalVolume = 1f;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Titles: Не найдена основная камера!");
            return;
        }

        // Инициализируем аудио
        InitializeAudio();

        // Инициализируем спрайт
        InitializeSprite();

        // Инициализируем черный экран
        InitializeBlackScreen();
    }

    void InitializeAudio()
    {
        // Если AudioSource не назначен, создаем его
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (titleMusic != null)
        {
            audioSource.clip = titleMusic;
            audioSource.playOnAwake = false;
            audioSource.loop = true; // Если нужно зациклить музыку

            // Сохраняем оригинальную громкость
            originalVolume = audioSource.volume;

            // Начинаем с нулевой громкости для плавного появления
            audioSource.volume = 0f;

            Debug.Log("Titles: Аудио инициализировано");
        }
        else
        {
            Debug.LogWarning("Titles: titleMusic не назначен!");
        }
    }

    void InitializeSprite()
    {
        if (titleSprite != null)
        {
            titleSprite.gameObject.SetActive(true);
            Color color = spriteColor;
            color.a = 0f;
            titleSprite.color = color;
            titleSprite.sortingOrder = sortingOrder;
            Debug.Log("Titles: Спрайт инициализирован");
        }
        else
        {
            Debug.LogWarning("Titles: titleSprite не назначен!");
        }
    }

    void InitializeBlackScreen()
    {
        if (blackScreen != null)
        {
            // Скрываем черный экран в начале
            blackScreen.gameObject.SetActive(true);
            Color color = blackScreen.color;
            color.a = 1f; // Начинаем с черного экрана
            color.r = color.g = color.b = 0f; // Чисто черный
            blackScreen.color = color;
            Debug.Log("Titles: Черный экран инициализирован (черный)");
        }
        else
        {
            Debug.LogWarning("Titles: blackScreen не назначен!");
        }
    }

    IEnumerator TitlesSequence()
    {
        isMoving = true;

        // 0. Сначала убираем черный экран и запускаем музыку
        Debug.Log($"Titles: Исчезновение черного экрана и запуск музыки...");
        yield return StartCoroutine(FadeBlackScreen(1f, 0f, 1f)); // Исчезает за 1 секунду

        // Запускаем музыку плавно
        if (titleMusic != null)
        {
            audioSource.Play();
            StartCoroutine(FadeAudio(0f, originalVolume, audioFadeInTime));
        }

        // Камера УЖЕ телепортирована к doorPosition из SmoothCutscene

        // 1. Задержка на общей двери
        Debug.Log($"Titles: Задержка на общей двери ({doorHoldTime} сек)...");
        yield return new WaitForSeconds(doorHoldTime);

        // 2. Дверь ? CUM
        Debug.Log($"Titles: Дверь ? CUM ({doorToCumTime} сек)...");
        yield return StartCoroutine(MoveCameraTo(cumPosition.position, cumPosition.rotation, doorToCumTime));

        // 3. Запускаем спрайт с небольшой задержкой
        if (titleSprite != null)
        {
            Debug.Log($"Titles: Запускаю спрайт через {spriteDelayAfterCamera} сек...");
            yield return new WaitForSeconds(spriteDelayAfterCamera);
            spriteCoroutine = StartCoroutine(SpriteSequence());
        }

        // 4. CUM ? Зум CUM (во время показа спрайта)
        Debug.Log($"Titles: CUM ? Зум CUM ({cumToZoomTime} сек) во время спрайта...");
        yield return StartCoroutine(MoveCameraTo(zoomCumPosition.position, zoomCumPosition.rotation, cumToZoomTime));

        // 5. Ждем окончания спрайта (если он еще не закончился)
        float spriteTotalTime = spriteFadeInTime + spriteHoldTime + spriteFadeOutTime;
        float movementTime = spriteDelayAfterCamera + cumToZoomTime;
        float remainingSpriteTime = spriteTotalTime - movementTime;

        if (remainingSpriteTime > 0)
        {
            Debug.Log($"Titles: Жду окончания спрайта ({remainingSpriteTime:F1} сек)...");
            yield return new WaitForSeconds(remainingSpriteTime);
        }

        // 6. ВЫКЛЮЧАЕМ ВСЕ СВЕТА после исчезновения спрайта
        Debug.Log($"Titles: Выключаю все света через {turnOffLightsDelay} сек...");
        yield return new WaitForSeconds(turnOffLightsDelay);
        TurnOffAllLights();

        // 7. ЗАПУСКАЕМ ЧЕРНЫЙ ЭКРАН сразу после выключения света
        Debug.Log($"Titles: Запускаю черный экран ({blackScreenFadeInTime} сек)...");
        yield return StartCoroutine(FadeBlackScreen(0f, 1f, blackScreenFadeInTime));

        // 8. Плавно выключаем музыку
        if (audioSource.isPlaying)
        {
            StartCoroutine(FadeAudio(originalVolume, 0f, 1f));
        }

        // 9. Удерживаем черный экран (НЕ ВЫКЛЮЧАЕМ!)
        Debug.Log($"Titles: Удерживаю черный экран ({blackScreenHoldTime} сек)...");
        yield return new WaitForSeconds(blackScreenHoldTime);

        // 10. Переход в главное меню
        Debug.Log($"Titles: Переход в главное меню через {menuTransitionDelay} сек...");
        yield return new WaitForSeconds(menuTransitionDelay);

        // Загружаем главное меню
        LoadMainMenu();

        Debug.Log("Titles: Последовательность завершена!");
        isMoving = false;
    }

    IEnumerator SpriteSequence()
    {
        Debug.Log("Titles: Начинаю показ спрайта...");

        // 1. Появление
        Debug.Log($"Titles: Появление спрайта ({spriteFadeInTime} сек)...");
        yield return StartCoroutine(FadeSprite(0f, 1f, spriteFadeInTime));

        // 2. Держим
        Debug.Log($"Titles: Демонстрация спрайта ({spriteHoldTime} сек)...");
        yield return new WaitForSeconds(spriteHoldTime);

        // 3. Исчезновение
        Debug.Log($"Titles: Исчезновение спрайта ({spriteFadeOutTime} сек)...");
        yield return StartCoroutine(FadeSprite(1f, 0f, spriteFadeOutTime));

        Debug.Log("Titles: Спрайт завершен");
    }

    void TurnOffAllLights()
    {
        // Находим все Light8 в сцене если не назначены вручную
        if (lightsToTurnOff == null || lightsToTurnOff.Length == 0)
        {
            lightsToTurnOff = FindObjectsOfType<Light8>();
        }

        foreach (var lightSystem in lightsToTurnOff)
        {
            if (lightSystem != null)
            {
                // Временно отключаем neverStop чтобы можно было выключить
                bool originalNeverStop = lightSystem.neverStop;
                lightSystem.neverStop = false; // Разрешаем остановку

                lightSystem.StopBlinking();
                Debug.Log($"Titles: Выключен Light8 на объекте {lightSystem.gameObject.name}");
            }
        }

        // Также выключаем все отдельные Light компоненты
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (var lightComp in allLights)
        {
            lightComp.enabled = false;
        }

        Debug.Log($"Titles: Выключено {lightsToTurnOff.Length} Light8 систем и {allLights.Length} отдельных Light компонентов");
    }

    IEnumerator FadeBlackScreen(float fromAlpha, float toAlpha, float duration)
    {
        if (blackScreen == null) yield break;

        float timer = 0f;
        Color color = blackScreen.color;
        color.a = fromAlpha;
        blackScreen.color = color;

        // Убеждаемся что черный экран активен
        blackScreen.gameObject.SetActive(true);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            color.a = Mathf.Lerp(fromAlpha, toAlpha, smoothProgress);
            blackScreen.color = color;
            yield return null;
        }

        color.a = toAlpha;
        blackScreen.color = color;

        Debug.Log($"Titles: Черный экран установлен на альфу {toAlpha}");
    }

    IEnumerator FadeAudio(float fromVolume, float toVolume, float duration)
    {
        if (audioSource == null) yield break;

        float timer = 0f;
        audioSource.volume = fromVolume;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            audioSource.volume = Mathf.Lerp(fromVolume, toVolume, smoothProgress);
            yield return null;
        }

        audioSource.volume = toVolume;

        // Если громкость стала 0, останавливаем воспроизведение
        if (toVolume <= 0.01f)
        {
            audioSource.Stop();
        }

        Debug.Log($"Titles: Аудио установлено на громкость {toVolume}");
    }

    IEnumerator MoveCameraTo(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothProgress);

            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
    }

    IEnumerator FadeSprite(float fromAlpha, float toAlpha, float duration)
    {
        if (titleSprite == null) yield break;

        float timer = 0f;
        Color color = titleSprite.color;
        color.a = fromAlpha;
        titleSprite.color = color;

        titleSprite.gameObject.SetActive(true);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            color.a = Mathf.Lerp(fromAlpha, toAlpha, smoothProgress);
            titleSprite.color = color;

            yield return null;
        }

        color.a = toAlpha;
        titleSprite.color = color;

        if (toAlpha <= 0.01f)
        {
            titleSprite.gameObject.SetActive(false);
        }
    }

    void LoadMainMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.Log($"Titles: Загружаю главное меню: {mainMenuSceneName}");
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("Titles: Не указано название сцены главного меню!");
        }
    }

    // Этот метод будет вызван из SmoothCutscene
    public void StartTitles()
    {
        if (isMoving) return;

        Debug.Log("Titles: Начинаю последовательность...");
        StartCoroutine(TitlesSequence());
    }

    [ContextMenu("Тест: Весь сценарий")]
    public void TestFullSequence()
    {
        StopAllCoroutines();
        StartCoroutine(TitlesSequence());
    }

    [ContextMenu("Тест: Выключить все света")]
    public void TestTurnOffLights()
    {
        TurnOffAllLights();
    }

    [ContextMenu("Тест: Показать черный экран")]
    public void TestShowBlackScreen()
    {
        StartCoroutine(FadeBlackScreen(0f, 1f, 2f));
    }

    [ContextMenu("Тест: Скрыть черный экран")]
    public void TestHideBlackScreen()
    {
        if (blackScreen != null)
        {
            Color color = blackScreen.color;
            color.a = 0f;
            blackScreen.color = color;
            Debug.Log("Titles: Черный экран скрыт");
        }
    }

    [ContextMenu("Тест: Включить музыку")]
    public void TestPlayMusic()
    {
        if (titleMusic != null)
        {
            audioSource.Play();
            audioSource.volume = originalVolume;
            Debug.Log("Titles: Музыка включена");
        }
    }

    [ContextMenu("Тест: Выключить музыку")]
    public void TestStopMusic()
    {
        if (audioSource.isPlaying)
        {
            StartCoroutine(FadeAudio(audioSource.volume, 0f, 1f));
            Debug.Log("Titles: Музыка выключается");
        }
    }

    [ContextMenu("Тест: К общей двери")]
    public void TestMoveToDoor()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        StartCoroutine(MoveCameraTo(doorPosition.position, doorPosition.rotation, 2f));
    }

    [ContextMenu("Тест: К CUM")]
    public void TestMoveToCum()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        StartCoroutine(MoveCameraTo(cumPosition.position, cumPosition.rotation, 2f));
    }

    [ContextMenu("Тест: Зум на CUM")]
    public void TestZoomCum()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        StartCoroutine(MoveCameraTo(zoomCumPosition.position, zoomCumPosition.rotation, 2f));
    }

    [ContextMenu("Тест: Показать спрайт")]
    public void TestShowSprite()
    {
        if (spriteCoroutine != null) StopCoroutine(spriteCoroutine);
        spriteCoroutine = StartCoroutine(SpriteSequence());
    }

    [ContextMenu("Тест: Скрыть спрайт")]
    public void TestHideSprite()
    {
        if (titleSprite != null)
        {
            Color color = titleSprite.color;
            color.a = 0f;
            titleSprite.color = color;
            titleSprite.gameObject.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        DrawCameraPosition(doorPosition, Color.green, "Дверь");
        DrawCameraPosition(cumPosition, Color.blue, "CUM");
        DrawCameraPosition(zoomCumPosition, Color.red, "ЗумCUM");

        DrawLineBetween(doorPosition, cumPosition, Color.green);
        DrawLineBetween(cumPosition, zoomCumPosition, Color.blue);
    }

    void DrawCameraPosition(Transform point, Color color, string label)
    {
        if (point != null)
        {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(point.position, 0.3f);
            Gizmos.DrawLine(point.position, point.position + point.forward * 1f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(point.position + Vector3.up * 0.5f, label);
#endif
        }
    }

    void DrawLineBetween(Transform from, Transform to, Color color)
    {
        if (from != null && to != null)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(from.position, to.position);
        }
    }
}