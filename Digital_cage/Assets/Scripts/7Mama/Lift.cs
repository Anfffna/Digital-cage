using UnityEngine;
using System.Collections;

public class Lift : MonoBehaviour
{
    [Header("Door Animators")]
    public Animator liftDoor1;      // Первая дверь лифта
    public Animator liftDoor2;      // Вторая дверь лифта

    [Header("Timing Settings")]
    public float openDelay = 4f;    // Задержка перед открытием после старта сцены
    public float openDuration = 2f; // Длительность анимации открытия
    public float autoCloseDelay = 10f; // Автозакрытие если игрок не вошел (опционально)

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip liftStartSound;    // Звук при запуске сцены (4 секунды)
    public AudioClip doorOpenSound;     // Звук открытия дверей
    public AudioClip doorCloseSound;    // Звук закрытия дверей

    [Header("Trigger Settings")]
    public DialogueTrigger7 dialogueTrigger; // Ссылка на триггер диалога 7

    private bool isOpen = false;
    private bool isClosing = false;
    private Coroutine autoCloseCoroutine;

    void Start()
    {
        // Проверяем наличие аниматоров
        if (liftDoor1 == null || liftDoor2 == null)
        {
            Debug.LogError("Lift: Не назначены оба аниматора дверей!");
            enabled = false;
            return;
        }

        // Сразу закрываем двери на старте
        CloseDoorsImmediately();

        // Начинаем последовательность открытия
        StartCoroutine(StartLiftSequence());
    }

    void OnEnable()
    {
        // Подписываемся на событие завершения диалога
        if (dialogueTrigger != null)
        {
            DialogueTrigger7.OnDialogue7Finished += OnDialogueFinished;
        }
    }

    void OnDisable()
    {
        // Отписываемся от события
        if (dialogueTrigger != null)
        {
            DialogueTrigger7.OnDialogue7Finished -= OnDialogueFinished;
        }
    }

    IEnumerator StartLiftSequence()
    {
        Debug.Log("Lift: Начинаю последовательность лифта...");

        // 1. Запускаем звук лифта на 4 секунды
        if (audioSource != null && liftStartSound != null)
        {
            audioSource.clip = liftStartSound;
            audioSource.Play();
            Debug.Log("Lift: Воспроизводится звук лифта (4 сек)");
        }

        // 2. Ждем 4 секунды
        yield return new WaitForSeconds(openDelay);

        // 3. Останавливаем звук лифта (если он еще играет)
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // 4. Открываем двери
        OpenDoors();
    }

    void OpenDoors()
    {
        if (isOpen) return;

        Debug.Log("Lift: Открываю двери...");

        // Запускаем анимацию открытия на обоих аниматорах
        liftDoor1.SetTrigger("Open");
        liftDoor2.SetTrigger("Open");

        // Проигрываем звук открытия
        if (audioSource != null && doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }

        isOpen = true;

        // Запускаем таймер автозакрытия (если нужно)
        if (autoCloseDelay > 0)
        {
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);

            autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
        }
    }

    void CloseDoors()
    {
        if (!isOpen || isClosing) return;

        Debug.Log("Lift: Закрываю двери...");

        isClosing = true;

        // Запускаем анимацию закрытия на обоих аниматорах
        liftDoor1.SetTrigger("Close");
        liftDoor2.SetTrigger("Close");

        // Проигрываем звук закрытия
        if (audioSource != null && doorCloseSound != null)
        {
            audioSource.PlayOneShot(doorCloseSound);
        }

        // Через секунду сбрасываем флаг
        StartCoroutine(ResetClosingFlag());
    }

    void CloseDoorsImmediately()
    {
        // Сразу закрываем двери без анимации (для начального состояния)
        liftDoor1.Play("Close", 0, 1f); // Переходим в конец анимации закрытия
        liftDoor2.Play("Close", 0, 1f);

        liftDoor1.SetBool("Open", false);
        liftDoor2.SetBool("Open", false);

        isOpen = false;
        isClosing = false;
    }

    IEnumerator AutoCloseCoroutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        // Автозакрытие только если двери еще открыты
        if (isOpen && !isClosing)
        {
            Debug.Log("Lift: Автозакрытие дверей (игрок не вошел)");
            CloseDoors();
        }
    }

    IEnumerator ResetClosingFlag()
    {
        // Ждем окончания анимации закрытия (примерно 1-2 секунды)
        yield return new WaitForSeconds(2f);

        isOpen = false;
        isClosing = false;
        Debug.Log("Lift: Двери полностью закрыты");
    }

    void OnDialogueFinished()
    {
        Debug.Log("Lift: Диалог 7 завершен, закрываю двери...");
        CloseDoors();
    }

    // Методы для ручного управления (для тестов)
    [ContextMenu("Тест: Открыть двери")]
    public void TestOpenDoors()
    {
        OpenDoors();
    }

    [ContextMenu("Тест: Закрыть двери")]
    public void TestCloseDoors()
    {
        CloseDoors();
    }

    [ContextMenu("Тест: Запустить последовательность")]
    public void TestStartSequence()
    {
        StartCoroutine(StartLiftSequence());
    }

    [ContextMenu("Сбросить состояние")]
    public void ResetState()
    {
        StopAllCoroutines();
        CloseDoorsImmediately();
        isOpen = false;
        isClosing = false;

        Debug.Log("Lift: Состояние сброшено");
    }

    void OnValidate()
    {
        // Автоматическое назначение триггера диалога
        if (dialogueTrigger == null)
        {
            dialogueTrigger = FindObjectOfType<DialogueTrigger7>();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (liftDoor1 != null && liftDoor2 != null)
        {
            // Визуализация связи между дверьми
            Gizmos.color = isOpen ? Color.green : (isClosing ? Color.yellow : Color.red);
            Gizmos.DrawLine(liftDoor1.transform.position, liftDoor2.transform.position);

            // Отметки на дверях
            Gizmos.DrawWireSphere(liftDoor1.transform.position, 0.3f);
            Gizmos.DrawWireSphere(liftDoor2.transform.position, 0.3f);

            // Текст состояния
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
            Vector3 centerPos = (liftDoor1.transform.position + liftDoor2.transform.position) / 2f;
            string stateText = isOpen ? "ОТКРЫТО" : (isClosing ? "ЗАКРЫВАЕТСЯ" : "ЗАКРЫТО");
            UnityEditor.Handles.Label(centerPos + Vector3.up * 0.5f, $"Лифт: {stateText}", style);
#endif
        }
    }
}