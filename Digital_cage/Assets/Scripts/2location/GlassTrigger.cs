using UnityEngine;
using System.Collections.Generic;

public class GlassTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("References")]
    public ManagerDialogue2 dialogueManager;
    public TodoUIManager todoManager;
    public TVGlitchEffect tvGlitchEffect;

    [Header("2D Sprite Settings")]
    public SpriteRenderer targetSprite;
    public float fadeInDuration = 2f;
    public bool waitForTask2 = true; // Ждать выполнения задачи 2 для показа спрайта

    private Collider triggerCollider;
    private bool hasBeenActivated = false;
    private bool playerInTrigger = false;
    private bool spriteShown = false;
    private bool canActivateDialogue = false; // Новый флаг - можно ли активировать диалог
    private Coroutine fadeCoroutine;

    void Start()
    {
        // Получаем коллайдер
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogError("GlassTrigger: Не найден коллайдер на объекте!");
            return;
        }

        // Выключаем коллайдер при старте
        triggerCollider.enabled = false;
        canActivateDialogue = false; // Диалог нельзя активировать
        Debug.Log("GlassTrigger: Коллайдер выключен при старте");

        // Настраиваем спрайт - скрываем его
        if (targetSprite != null)
        {
            Color spriteColor = targetSprite.color;
            spriteColor.a = 0f;
            targetSprite.color = spriteColor;
            targetSprite.gameObject.SetActive(true); // Оставляем активным для анимации
        }

        // Автоматически находим менеджеры если не назначены
        if (todoManager == null)
            todoManager = FindObjectOfType<TodoUIManager>();

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<ManagerDialogue2>();
    }

    void Update()
    {
        // Проверяем условие активации только если коллайдер еще выключен
        if (!triggerCollider.enabled)
        {
            CheckActivationCondition();
        }

        // Проверяем условие для показа спрайта
        if (!spriteShown && waitForTask2)
        {
            CheckSpriteActivationCondition();
        }

        // Если игрок в триггере и можно активировать диалог (и диалог еще не активирован)
        if (playerInTrigger && canActivateDialogue && !hasBeenActivated)
        {
            // Автоматически активируем диалог при нахождении в триггере
            ActivateDialogue();
        }
    }

    /// <summary>
    /// Проверяет условие для активации коллайдера (индекс 2 в туду выполнен)
    /// </summary>
    private void CheckActivationCondition()
    {
        if (todoManager != null && todoManager.todoItems != null)
        {
            // Проверяем, выполнена ли задача с индексом 2
            if (todoManager.todoItems.Length > 2 &&
                todoManager.todoItems[2] != null &&
                todoManager.todoItems[2].text.StartsWith("<s>"))
            {
                // Включаем коллайдер когда условие выполнено
                triggerCollider.enabled = true;
                canActivateDialogue = true; // Теперь можно активировать диалог
                Debug.Log("GlassTrigger: Задача с индексом 2 выполнена! Коллайдер активирован. Диалог можно активировать при входе в триггер.");
            }
        }
        else
        {
            Debug.LogWarning("GlassTrigger: TodoManager или todoItems не назначены!");
        }
    }

    /// <summary>
    /// Проверяет условие для показа спрайта (индекс 2 в туду выполнен)
    /// </summary>
    private void CheckSpriteActivationCondition()
    {
        if (targetSprite != null && todoManager != null && todoManager.todoItems != null)
        {
            // Проверяем, выполнена ли задача с индексом 2
            if (todoManager.todoItems.Length > 2 &&
                todoManager.todoItems[2] != null &&
                todoManager.todoItems[2].text.StartsWith("<s>"))
            {
                // Запускаем плавное появление спрайта
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine);

                fadeCoroutine = StartCoroutine(FadeInSprite());
                spriteShown = true;
                Debug.Log("GlassTrigger: Задача с индексом 2 выполнена! Запускаем появление спрайта.");
            }
        }
    }

    /// <summary>
    /// Корутина плавного появления спрайта
    /// </summary>
    private System.Collections.IEnumerator FadeInSprite()
    {
        if (targetSprite == null) yield break;

        float timer = 0f;
        Color startColor = targetSprite.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        // Активируем спрайт если он был выключен
        targetSprite.gameObject.SetActive(true);

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeInDuration;
            targetSprite.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }

        // Убеждаемся, что спрайт полностью видим
        targetSprite.color = targetColor;
        Debug.Log("GlassTrigger: Спрайт полностью показан");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            Debug.Log("GlassTrigger: Игрок вошел в триггер");

            // Диалог активируется автоматически в Update, если canActivateDialogue = true
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            Debug.Log("GlassTrigger: Игрок вышел из триггера");

            // Если игрок вышел из триггера до активации диалога, сбрасываем флаги
            if (!hasBeenActivated)
            {
                // Ничего не делаем, ждем следующего входа в триггер
            }
        }
    }

    /// <summary>
    /// Активирует диалог
    /// </summary>
    private void ActivateDialogue()
    {
        if (hasBeenActivated || dialogueManager == null || dialogueLines.Count == 0)
            return;

        // Запускаем диалог
        dialogueManager.StartDialogue(dialogueLines, OnDialogueEnd);
        hasBeenActivated = true;

        // Отключаем коллайдер после активации диалога
        triggerCollider.enabled = false;
        canActivateDialogue = false;

        Debug.Log("GlassTrigger: Диалог активирован");
    }

    /// <summary>
    /// Вызывается когда диалог завершен
    /// </summary>
    private void OnDialogueEnd()
    {
        // Запускаем глитч-эффект на телевизоре
        if (tvGlitchEffect != null)
        {
            tvGlitchEffect.StartGlitchEffect();
        }
        else
        {
            Debug.LogWarning("GlassTrigger: TVGlitchEffect не назначен!");
        }
    }

    /// <summary>
    /// Принудительно показывает спрайт (для тестирования)
    /// </summary>
    public void ForceShowSprite()
    {
        if (targetSprite != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeInSprite());
            spriteShown = true;
        }
    }

    /// <summary>
    /// Принудительно скрывает спрайт (для тестирования)
    /// </summary>
    public void ForceHideSprite()
    {
        if (targetSprite != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            Color spriteColor = targetSprite.color;
            spriteColor.a = 0f;
            targetSprite.color = spriteColor;
            spriteShown = false;
        }
    }

    /// <summary>
    /// Метод для принудительной активации триггера (для тестирования)
    /// </summary>
    public void ForceActivate()
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
            canActivateDialogue = true;
            Debug.Log("GlassTrigger: Принудительная активация коллайдера");
        }
    }

    /// <summary>
    /// Метод для принудительной деактивации триггера
    /// </summary>
    public void ForceDeactivate()
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
            canActivateDialogue = false;
            hasBeenActivated = false;
            playerInTrigger = false;
            Debug.Log("GlassTrigger: Принудительная деактивация коллайдера");
        }
    }

    /// <summary>
    /// Сбрасывает состояние диалога (можно использовать если нужно перезапустить)
    /// </summary>
    public void ResetDialogue()
    {
        hasBeenActivated = false;
        playerInTrigger = false;

        if (triggerCollider != null && canActivateDialogue)
        {
            triggerCollider.enabled = true;
        }

        Debug.Log("GlassTrigger: Состояние диалога сброшено");
    }
}