using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ExitDoorMama6 : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public GameObject doorObject; // сама дверь
    public string targetSceneName = "NextScene"; // сцена для перехода

    [Header("Todo Condition")]
    public bool requireTodoCompletion = true; // требовать второй пункт Todo

    [Header("Fade Settings")]
    public Image fadeImage; // черный UI Image для затемнения
    public float fadeDuration = 1f; // длительность затемнения

    private bool isInteractable = false;
    private bool isTransitioning = false;
    private bool todoCompleted = false; // второй пункт Todo показан
    private Coroutine checkTodoCoroutine;

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

        // Начинаем отслеживание Todo
        StartCheckingTodo();
    }

    void StartCheckingTodo()
    {
        if (checkTodoCoroutine != null)
            StopCoroutine(checkTodoCoroutine);

        checkTodoCoroutine = StartCoroutine(CheckTodoRoutine());
    }

    private IEnumerator CheckTodoRoutine()
    {
        // Если Todo не требуется - сразу открываем дверь
        if (!requireTodoCompletion)
        {
            MakeDoorInteractable();
            yield break;
        }

        Debug.Log("ExitDoorMama6: Начинаю проверку второго пункта Todo...");

        while (!todoCompleted)
        {
            // Ищем TodoUI6 в сцене
            TodoUI6 todoUI = FindObjectOfType<TodoUI6>();

            if (todoUI != null && todoUI.IsTask2Shown())
            {
                todoCompleted = true;
                Debug.Log("ExitDoorMama6: Второй пункт Todo появился! Открываю дверь...");
                MakeDoorInteractable();
                break;
            }

            // Проверяем раз в полсекунды
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Делаем дверь интерактивной (включаем слой Interactable)
    /// ТОЛЬКО КОГДА ВТОРОЙ ПУНКТ TODO ПОКАЗАН
    /// </summary>
    private void MakeDoorInteractable()
    {
        if (isInteractable) return;

        isInteractable = true;

        // Меняем слой на Interactable
        doorObject.layer = LayerMask.NameToLayer("Interactable");

        Debug.Log("ExitDoorMama6: Дверь теперь интерактивна!");
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
        // Если дверь не интерактивна, показываем подсказку
        if (!isInteractable)
        {
            if (requireTodoCompletion && !todoCompleted)
            {
                return "Выход заблокирован..."; // Или любая другая подсказка
            }
            return ""; // Пустая строка
        }
        return "Выйти (E)";
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
            todoCompleted = true;
            MakeDoorInteractable();
            Debug.Log("ExitDoorMama6: Дверь принудительно открыта");
        }
    }

    /// <summary>
    /// Принудительно проверить Todo (для тестирования)
    /// </summary>
    [ContextMenu("Тест: Проверить Todo")]
    public void TestCheckTodo()
    {
        TodoUI6 todoUI = FindObjectOfType<TodoUI6>();
        if (todoUI != null)
        {
            Debug.Log($"ExitDoorMama6: Todo найден. IsTask2Shown = {todoUI.IsTask2Shown()}");
            if (todoUI.IsTask2Shown() && !isInteractable)
            {
                ForceOpenDoor();
            }
        }
        else
        {
            Debug.LogWarning("ExitDoorMama6: TodoUI не найден в сцене!");
        }
    }

    [ContextMenu("Тест: Открыть дверь")]
    public void TestOpenDoor()
    {
        ForceOpenDoor();
    }

    [ContextMenu("Сбросить дверь")]
    public void ResetDoor()
    {
        if (checkTodoCoroutine != null)
        {
            StopCoroutine(checkTodoCoroutine);
            checkTodoCoroutine = null;
        }

        isInteractable = false;
        isTransitioning = false;
        todoCompleted = false;

        // Возвращаем слой на Default
        if (doorObject != null)
        {
            doorObject.layer = LayerMask.NameToLayer("Default");
        }

        // Снова начинаем проверять
        StartCheckingTodo();

        Debug.Log("ExitDoorMama6: Дверь сброшена");
    }

    void OnValidate()
    {
        // В редакторе показываем состояние
        if (doorObject != null)
        {
            // Можно добавить любую логику валидации
        }
    }

    void OnDrawGizmos()
    {
        // Визуализация состояния двери в редакторе
        if (doorObject != null)
        {
            Gizmos.color = isInteractable ? Color.green : (todoCompleted ? Color.yellow : Color.red);
            Gizmos.DrawWireSphere(doorObject.transform.position, 0.5f);

            // Дополнительная иконка
            if (!isInteractable && requireTodoCompletion)
            {
                Gizmos.DrawIcon(doorObject.transform.position + Vector3.up * 0.7f, "d_TreeEditor.Trash@2x", true);
            }
        }
    }

    void OnDestroy()
    {
        // Останавливаем корутину при уничтожении
        if (checkTodoCoroutine != null)
        {
            StopCoroutine(checkTodoCoroutine);
        }
    }
}