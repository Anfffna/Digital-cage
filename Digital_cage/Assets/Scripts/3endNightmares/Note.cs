using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Note : MonoBehaviour, IInteractable
{
    [Header("Note Settings")]
    public Image noteImageUI; // UI Image с изображением записки

    [Header("Dialogue Settings")]
    public ManagerDialogue3 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;

    [Header("Todo Manager")]
    public LightTodoUIManager todoManager;

    private bool hasBeenUsed = false;
    private bool isNoteOpen = false;
    private CanvasGroup noteCanvasGroup;
    private bool isClosing = false;

    void Start()
    {
        // Устанавливаем слой Interactable
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        // Настраиваем UI записки
        if (noteImageUI != null)
        {
            noteImageUI.gameObject.SetActive(false);
            noteCanvasGroup = noteImageUI.GetComponent<CanvasGroup>();
            if (noteCanvasGroup == null)
            {
                noteCanvasGroup = noteImageUI.gameObject.AddComponent<CanvasGroup>();
            }
            noteCanvasGroup.alpha = 0f;
        }
    }

    public string GetInteractionText()
    {
        if (hasBeenUsed || isClosing)
            return "";

        return "Нажмите E";
    }

    public void Interact()
    {
        if (hasBeenUsed || isClosing) return;

        if (!isNoteOpen)
        {
            OpenNote();
        }
        else
        {
            CloseNote();
        }
    }

    private void OpenNote()
    {
        isNoteOpen = true;

        // Показываем UI записки
        if (noteImageUI != null)
        {
            noteImageUI.gameObject.SetActive(true);
            StartCoroutine(FadeNote(0f, 1f, fadeInDuration));
        }

        // Блокируем движение игрока
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.canMove = false;
        }

        Debug.Log("Note: Записка открыта");
    }

    private void CloseNote()
    {
        if (!isNoteOpen || isClosing) return;

        isClosing = true;
        isNoteOpen = false;

        // Плавно скрываем UI записки
        if (noteImageUI != null)
        {
            StartCoroutine(FadeNote(1f, 0f, fadeOutDuration, true));
        }

        // Разблокируем движение игрока
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.canMove = true;
        }

        StartCoroutine(UpdateTodoAfterDelay(fadeOutDuration));
    }

    private IEnumerator UpdateTodoAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Обновляем туду-лист в зависимости от состояния
        if (todoManager != null)
        {
            // Если второй пункт активен - переходим к третьему
            if (todoManager.IsNewTaskActive())
            {
                todoManager.CompleteThirdTask();
                Debug.Log("Note: Активирован третий пункт туду-листа");
            }
            // Если второй пункт не активен - активируем его
            else if (!todoManager.IsThirdTaskActive())
            {
                todoManager.CompleteLightTask();
                Debug.Log("Note: Активирован второй пункт туду-листа");
            }
        }

        // Запускаем диалог
        StartCoroutine(StartDialogueAfterDelay(0.5f));
    }

    private IEnumerator StartDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Скрываем объект в мире только после всего
        hasBeenUsed = true;
        gameObject.SetActive(false);

        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            dialogueManager.StartDialogue(dialogueLines);
            Debug.Log("Note: Диалог запущен!");
        }
    }

    private IEnumerator FadeNote(float fromAlpha, float toAlpha, float duration, bool disableAfter = false)
    {
        if (noteCanvasGroup == null) yield break;

        float timer = 0f;
        noteCanvasGroup.alpha = fromAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            noteCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, progress);
            yield return null;
        }

        noteCanvasGroup.alpha = toAlpha;

        if (disableAfter && noteImageUI != null)
        {
            noteImageUI.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Закрытие записки по нажатию Escape или правой кнопки мыши
        if (isNoteOpen && !isClosing && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse1)))
        {
            CloseNote();
        }
    }
}