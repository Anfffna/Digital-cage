using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shadow6 : MonoBehaviour
{
    [Header("Shadow Settings")]
    public GameObject shadowObject; // Объект тени

    [Header("Movement Settings")]
    public float moveSpeed = 30f;
    public float moveDistance = 12f;
    public Vector3 moveDirection = Vector3.right;

    [Header("Timing")]
    public float appearDuration = 0.3f;
    public float disappearDuration = 0.2f;
    public float waitBeforeMove = 0.4f;

    [Header("Dialogue Settings")]
    public ManagerDialogue6 dialogueManager; // Перетащи сюда ManagerDialogue6 из сцены

    [TextArea(2, 5)]
    public List<string> dialogueLines; // Здесь будут строки диалога для тени

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip whooshSound;

    [Header("Trigger Settings")]
    public bool oneTimeOnly = true; // Сработает только один раз
    public BoxCollider triggerCollider; // Ссылка на триггер (можно не назначать - возьмется автоматически)

    private bool isActivated = false;
    private bool hasTriggered = false;
    private bool playerInTrigger = false;
    private bool todoUpdated = false;
    private MeshRenderer shadowRenderer;
    private Material shadowMaterial;
    private Color originalColor;
    private Vector3 startPosition;
    private Coroutine checkCoroutine;

    void Start()
    {
        if (shadowObject == null)
        {
            Debug.LogError("Shadow6: Не назначен объект тени!");
            enabled = false;
            return;
        }

        // Находим триггер если не назначен
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<BoxCollider>();
            if (triggerCollider == null)
            {
                Debug.LogError("Shadow6: Нет BoxCollider на этом GameObject!");
                enabled = false;
                return;
            }

            // Делаем его триггером если не триггер
            triggerCollider.isTrigger = true;
        }

        // Сохраняем стартовую позицию
        startPosition = shadowObject.transform.position;

        // Нормализуем направление
        moveDirection = moveDirection.normalized;

        // ВЫКЛЮЧАЕМ ТЕНЬ ПРИ СТАРТЕ
        shadowObject.SetActive(false);

        Debug.Log("Shadow6: Инициализирован. Тень выключена. Жду игрока в триггере и второй пункт Todo.");

        // Начинаем проверять Todo
        StartCheckingTodo();
    }

    void StartCheckingTodo()
    {
        if (checkCoroutine != null)
            StopCoroutine(checkCoroutine);

        checkCoroutine = StartCoroutine(CheckTodoRoutine());
    }

    IEnumerator CheckTodoRoutine()
    {
        Debug.Log("Shadow6: Начинаю проверку второго пункта Todo...");

        while (!hasTriggered)
        {
            TodoUI6 todoUI = FindObjectOfType<TodoUI6>();

            if (todoUI != null && todoUI.IsTask2Shown())
            {
                todoUpdated = true;
                Debug.Log("Shadow6: Второй пункт Todo обновлен! Теперь жду игрока в триггере...");

                // Проверяем, может игрок уже в триггере
                if (playerInTrigger && !hasTriggered)
                {
                    Debug.Log("Shadow6: Игрок уже в триггере! Запускаю тень...");
                    ActivateShadow();
                    break;
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void StartDialogue()
    {
        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            Debug.Log("Shadow6: Запускаю диалог...");
            dialogueManager.StartDialogue(dialogueLines);
        }
        else
        {
            Debug.LogWarning("Shadow6: Не настроен диалог!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            Debug.Log("Shadow6: Игрок вошел в триггер");

            // Если Todo уже обновлен, запускаем тень
            if (todoUpdated && !hasTriggered && !isActivated)
            {
                Debug.Log("Shadow6: Условия выполнены! Запускаю тень...");
                ActivateShadow();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            Debug.Log("Shadow6: Игрок вышел из триггера");
        }
    }

    void ActivateShadow()
    {
        if (isActivated || hasTriggered) return;

        isActivated = true;
        StartCoroutine(ShadowSequence());
    }

    IEnumerator ShadowSequence()
    {
        Debug.Log("Shadow6: Запускаю последовательность тени...");

        // 0. ЗАПУСКАЕМ ДИАЛОГ ПЕРЕД ПОЯВЛЕНИЕМ ТЕНИ
        StartDialogue();

        // 1. ВКЛЮЧАЕМ ТЕНЬ
        shadowObject.SetActive(true);

        // 2. Плавно появляемся (если нужно плавное появление)
        if (appearDuration > 0)
        {
            // Получаем рендерер если еще не получили
            if (shadowRenderer == null)
                shadowRenderer = shadowObject.GetComponent<MeshRenderer>();

            if (shadowRenderer != null && shadowMaterial == null)
            {
                shadowMaterial = shadowRenderer.material;
                originalColor = shadowMaterial.color;
            }

            if (shadowMaterial != null)
            {
                // Делаем прозрачной сначала
                Color startColor = originalColor;
                startColor.a = 0f;
                shadowMaterial.color = startColor;

                yield return StartCoroutine(FadeShadow(0f, originalColor.a, appearDuration));
            }
        }

        // 3. Пауза перед движением
        yield return new WaitForSeconds(waitBeforeMove);

        // 4. Звук
        if (audioSource != null && whooshSound != null)
        {
            audioSource.PlayOneShot(whooshSound);
        }

        // 5. Движение
        float distanceMoved = 0f;
        Vector3 currentPos = startPosition;

        while (distanceMoved < moveDistance)
        {
            float step = moveSpeed * Time.deltaTime;
            currentPos += moveDirection * step;
            shadowObject.transform.position = currentPos;
            distanceMoved += step;
            yield return null;
        }

        // 6. Исчезаем (если нужно плавное исчезновение)
        if (disappearDuration > 0 && shadowMaterial != null)
        {
            yield return StartCoroutine(FadeShadow(originalColor.a, 0f, disappearDuration));
        }

        // 7. ВЫКЛЮЧАЕМ ТЕНЬ
        shadowObject.SetActive(false);

        // 8. Возвращаем на место
        shadowObject.transform.position = startPosition;

        hasTriggered = oneTimeOnly; // Если oneTimeOnly = true, больше не сработает
        isActivated = false;

        Debug.Log("Shadow6: Тень завершила движение");
    }

    IEnumerator FadeShadow(float fromAlpha, float toAlpha, float duration)
    {
        if (shadowMaterial == null || duration <= 0) yield break;

        float timer = 0f;
        Color startColor = shadowMaterial.color;
        Color endColor = startColor;
        endColor.a = toAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            shadowMaterial.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        shadowMaterial.color = endColor;
    }

    [ContextMenu("Тест: Активировать тень")]
    public void TestActivate()
    {
        if (!isActivated && !hasTriggered)
        {
            // Для теста имитируем оба условия
            todoUpdated = true;
            playerInTrigger = true;
            ActivateShadow();
        }
    }

    [ContextMenu("Тест: Проверить состояние")]
    public void TestState()
    {
        TodoUI6 todoUI = FindObjectOfType<TodoUI6>();
        if (todoUI != null)
        {
            Debug.Log($"Shadow6: Todo найден. IsTask2Shown = {todoUI.IsTask2Shown()}");
            todoUpdated = todoUI.IsTask2Shown();
        }

        Debug.Log($"Shadow6: Состояние - playerInTrigger: {playerInTrigger}, todoUpdated: {todoUpdated}, hasTriggered: {hasTriggered}");
        Debug.Log($"Shadow6: Тень активна = {shadowObject.activeSelf}");
    }

    [ContextMenu("Симулировать вход игрока")]
    public void SimulatePlayerEnter()
    {
        playerInTrigger = true;
        Debug.Log("Shadow6: Имитация входа игрока в триггер");

        if (todoUpdated && !hasTriggered)
        {
            Debug.Log("Shadow6: Запускаю тень (симуляция)");
            ActivateShadow();
        }
    }

    [ContextMenu("Симулировать обновление Todo")]
    public void SimulateTodoUpdate()
    {
        todoUpdated = true;
        Debug.Log("Shadow6: Имитация обновления Todo");

        if (playerInTrigger && !hasTriggered)
        {
            Debug.Log("Shadow6: Запускаю тень (симуляция)");
            ActivateShadow();
        }
    }

    [ContextMenu("Сбросить")]
    public void ResetShadow()
    {
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
            checkCoroutine = null;
        }

        StopAllCoroutines();

        isActivated = false;
        hasTriggered = false;
        todoUpdated = false;
        playerInTrigger = false;

        // Возвращаем тень на место и выключаем
        if (shadowObject != null)
        {
            shadowObject.transform.position = startPosition;
            shadowObject.SetActive(false);
        }

        // Снова начинаем проверять
        StartCheckingTodo();

        Debug.Log("Shadow6: Полностью сброшено");
    }

    void OnDrawGizmos()
    {
        // Всегда показываем триггер в редакторе
        if (triggerCollider != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Оранжевый прозрачный
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(triggerCollider.center, triggerCollider.size);

            // Контур
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            Gizmos.DrawWireCube(triggerCollider.center, triggerCollider.size);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (shadowObject != null)
        {
            // Показываем траекторию движения
            Gizmos.color = Color.red;
            Vector3 start = shadowObject.transform.position;
            Vector3 end = start + moveDirection.normalized * moveDistance;
            Gizmos.DrawLine(start, end);

            // Стрелка
            Vector3 right = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, 160, 0) * Vector3.forward * 0.5f;
            Vector3 left = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, 200, 0) * Vector3.forward * 0.5f;
            Gizmos.DrawLine(end, end + right);
            Gizmos.DrawLine(end, end + left);

            // Точка старта
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(start, 0.2f);

            // Информация
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow;
#if UNITY_EDITOR
            UnityEditor.Handles.Label(start + Vector3.up * 0.5f, "Тень: " + (shadowObject.activeSelf ? "ВКЛ" : "ВЫКЛ"), style);
#endif
        }
    }
}