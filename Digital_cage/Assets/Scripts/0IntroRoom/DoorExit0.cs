using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;

public class DoorExit0 : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public GameObject doorObject; // сама дверь
    public string targetSceneName = "1locationOffice"; // сцена для перехода

    [Header("Mama Call Condition")]
    public MamaCall mamaCall; // ссылка на скрипт MamaCall

    [Header("Fade Settings")]
    public Image fadeImage; // черный UI Image для затемнения
    public float fadeDuration = 1f; // длительность затемнения

    private bool isInteractable = false;
    private bool isTransitioning = false;

    void Start()
    {
        if (doorObject == null)
            doorObject = this.gameObject;

        // Настраиваем fade image если он существует
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            // Устанавливаем полностью прозрачным в начале
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("DoorExit0: fadeImage не назначен!");
        }

        // Ставим дверь неинтерактивной
        doorObject.layer = LayerMask.NameToLayer("Default");

        // Начинаем отслеживание завершения MamaCall
        StartCoroutine(WaitForMamaCallFullCompletion());
    }

    private IEnumerator WaitForMamaCallFullCompletion()
    {
        Debug.Log("DoorExit0: Ждем полного завершения MamaCall...");

        // Шаг 1: Ждем пока MamaCall сработает (рука поднялась)
        if (mamaCall == null)
        {
            mamaCall = FindObjectOfType<MamaCall>();
            if (mamaCall == null)
            {
                Debug.LogError("DoorExit0: MamaCall не найден в сцене!");
                yield break;
            }
        }

        // Ждем начала MamaCall
        while (!mamaCall.HasTriggered())
        {
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("DoorExit0: MamaCall начался (рука поднялась)");

        // Шаг 2: Теперь ждем пока рука опустится
        yield return StartCoroutine(WaitForHandDown());

        // Шаг 3: Дополнительная задержка для плавности
        yield return new WaitForSeconds(0.5f);

        // Открываем дверь
        OpenDoor();
    }

    private IEnumerator WaitForHandDown()
    {
        Debug.Log("DoorExit0: Ждем когда рука опустится...");

        // Способ: Проверяем через рефлексию
        bool handDown = false;
        int maxWaitTime = 30; // максимум 30 секунд ожидания
        float elapsedTime = 0f;

        while (!handDown && elapsedTime < maxWaitTime)
        {
            // Пытаемся проверить статус руки
            handDown = CheckIfHandDown();

            if (!handDown)
            {
                elapsedTime += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }
        }

        if (elapsedTime >= maxWaitTime)
        {
            Debug.LogWarning("DoorExit0: Таймаут ожидания опускания руки!");
        }
        else
        {
            Debug.Log("DoorExit0: Рука опущена!");
        }
    }

    private bool CheckIfHandDown()
    {
        try
        {
            // Пытаемся получить доступ к приватному полю handDownTriggered через рефлексию
            FieldInfo handDownField = typeof(MamaCall).GetField("handDownTriggered",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (handDownField != null)
            {
                return (bool)handDownField.GetValue(mamaCall);
            }

            // Если не нашли поле, ищем свойство
            PropertyInfo handDownProperty = typeof(MamaCall).GetProperty("handDownTriggered",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (handDownProperty != null)
            {
                return (bool)handDownProperty.GetValue(mamaCall);
            }

            // Если ничего не нашли, используем запасной вариант
            Debug.LogWarning("DoorExit0: Не удалось найти handDownTriggered, используем запасной метод");
            return CheckHandDownFallback();
        }
        catch
        {
            // Если произошла ошибка, используем запасной вариант
            return CheckHandDownFallback();
        }
    }

    private bool CheckHandDownFallback()
    {
        // Запасной вариант: ждем фиксированное время после начала звонка
        // В реальном проекте лучше добавить публичный метод в MamaCall
        return false; // или логика на основе прошедшего времени
    }

    private void OpenDoor()
    {
        isInteractable = true;
        doorObject.layer = LayerMask.NameToLayer("Interactable");
        Debug.Log("DoorExit0: Дверь теперь интерактивна!");

        // Можно добавить визуальный/звуковой эффект
        StartCoroutine(ShowDoorGlowEffect());
    }

    private IEnumerator ShowDoorGlowEffect()
    {
        // Простой визуальный эффект - можно настроить как нужно
        Renderer doorRenderer = doorObject.GetComponent<Renderer>();
        if (doorRenderer != null)
        {
            Material material = doorRenderer.material;
            Color originalColor = material.color;
            Color glowColor = new Color(0.5f, 1f, 0.5f, 1f); // Легкий зеленый оттенок

            // Мерцание 3 раза
            for (int i = 0; i < 3; i++)
            {
                material.color = glowColor;
                yield return new WaitForSeconds(0.3f);
                material.color = originalColor;
                yield return new WaitForSeconds(0.3f);
            }

            // Возвращаем оригинальный цвет
            material.color = originalColor;
        }
    }

    public void Interact()
    {
        if (!isInteractable || isTransitioning) return;

        Debug.Log("DoorExit0: Игрок нажал на дверь, переходим в " + targetSceneName);

        // Скрываем курсор
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Начинаем переход
        StartCoroutine(TransitionToScene());
    }

    public string GetInteractionText()
    {
        return isInteractable ? "Нажмите E, чтобы выйти" : "";
    }

    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;

        // Затемняем экран
        if (fadeImage != null)
        {
            yield return StartCoroutine(FadeToBlack());
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // Загружаем сцену
        SceneManager.LoadScene(targetSceneName);
    }

    private IEnumerator FadeToBlack()
    {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    // Для тестирования
    public void ForceOpenDoor()
    {
        if (!isInteractable)
        {
            OpenDoor();
        }
    }
}