using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ExitDoor5 : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public GameObject doorObject; // сама дверь
    public string targetSceneName = "6Work"; // сцена для перехода

    [Header("Sleep Condition")]
    public Sleep sleepSystem; // ссылка на скрипт Sleep
    public bool requireSleepCompletion = true; // требовать завершение сна

    [Header("Fade Settings")]
    public Image fadeImage; // черный UI Image для затемнения
    public float fadeDuration = 1f; // длительность затемнения

    private bool isInteractable = false;
    private bool isTransitioning = false;
    private bool playerHasSlept = false; // игрок поспал и проснулся

    void Awake()
    {
        // В Awake чтобы сделать это как можно раньше
        if (doorObject == null)
            doorObject = this.gameObject;

        // СРАЗУ МЕНЯЕМ СЛОЙ НА DEFAULT!
        doorObject.layer = LayerMask.NameToLayer("Default");
    }

    void Start()
    {
        // Настраиваем fade image если он существует
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            // Устанавливаем полностью прозрачным в начале
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
        }

        // Двойная проверка что слой Default
        if (doorObject.layer != LayerMask.NameToLayer("Default"))
        {
            doorObject.layer = LayerMask.NameToLayer("Default");
        }

        // Устанавливаем флаг НЕ интерактивности
        isInteractable = false;

        // Начинаем отслеживание сна
        StartCoroutine(WaitForSleepCompletion());
    }

    void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта
        if (sleepSystem != null)
        {
            sleepSystem.OnPlayerWokeUp -= OnPlayerWokeUpHandler;
        }
    }

    private IEnumerator WaitForSleepCompletion()
    {
        // Если сон не требуется - сразу открываем дверь
        if (!requireSleepCompletion)
        {
            MakeDoorInteractable();
            yield break;
        }

        // Ждем пока Sleep система будет назначена
        if (sleepSystem == null)
        {
            sleepSystem = FindObjectOfType<Sleep>();
            yield return new WaitForSeconds(0.1f);

            if (sleepSystem == null)
            {
                sleepSystem = FindObjectOfType<Sleep>();
            }
        }

        // Подписываемся на событие пробуждения
        if (sleepSystem != null)
        {
            sleepSystem.OnPlayerWokeUp += OnPlayerWokeUpHandler;
        }
    }

    /// <summary>
    /// Обработчик события пробуждения игрока
    /// </summary>
    private void OnPlayerWokeUpHandler()
    {
        playerHasSlept = true;
        MakeDoorInteractable();
    }

    /// <summary>
    /// Делаем дверь интерактивной (включаем слой Interactable)
    /// ТОЛЬКО КОГДА ИГРОК ПРОСНУЛСЯ
    /// </summary>
    private void MakeDoorInteractable()
    {
        if (isInteractable) return;

        isInteractable = true;

        // Меняем слой ТОЛЬКО ЗДЕСЬ, после пробуждения
        doorObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public void Interact()
    {
        // Проверка на isInteractable ПЕРВОЕ ДЕЛО
        if (!isInteractable || isTransitioning) return;

        // Скрываем курсор
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Начинаем переход
        StartCoroutine(TransitionToScene());
    }

    public string GetInteractionText()
    {
        // Если дверь не интерактивна, не показываем текст вообще
        if (!isInteractable)
        {
            return ""; // Пустая строка - текст не будет показан
        }
        return "Выйти на работу (E)";
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

    /// <summary>
    /// Принудительно открыть дверь (для тестирования)
    /// </summary>
    public void ForceOpenDoor()
    {
        if (!isInteractable)
        {
            playerHasSlept = true;
            MakeDoorInteractable();
        }
    }

    void OnValidate()
    {
        // Автоматическое назначение Sleep системы в редакторе
        if (requireSleepCompletion && sleepSystem == null)
        {
            sleepSystem = FindObjectOfType<Sleep>();
        }
    }

    void OnDrawGizmos()
    {
        // Визуализация состояния двери в редакторе
        if (doorObject != null)
        {
            Gizmos.color = isInteractable ? Color.green : (playerHasSlept ? Color.yellow : Color.red);
            Gizmos.DrawWireSphere(doorObject.transform.position, 0.5f);
        }
    }
}