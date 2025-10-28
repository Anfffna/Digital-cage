using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PhotoCapturePoint : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    public GameObject cameraUIScreen; // Canvas UI телефона
    public AudioClip cameraSound;   // звук щелчка
    public int todoIndex = 1;         // индекс пункта в ToDoUI (1 — терминал, 2 — чип)
    public ToDoUI toDoUI;             // ссылка на UI задач (ToDo)

    [Header("Timing")]
    public float photoDuration = 2.5f;
    public float flashDuration = 0.4f;
    public float uiFadeInTime = 0.5f;

    [HideInInspector] public bool isUsed = false;

    private bool taken = false;
    private bool todoVisible => toDoUI != null && toDoUI.gameObject.activeSelf && toDoUI.panel != null && toDoUI.panel.alpha >= 1f;

    public string GetInteractionText()
    {
        // Не показываем подсказку, если ToDo панель ещё не появилась
        if (!todoVisible)
            return "";

        // ? Если задание 0 ("взять телефон") не выполнено — не показываем текст
        if (toDoUI != null && !toDoUI.CanCompleteTask(todoIndex))
            return "";

        return taken ? "" : "Нажмите E, чтобы сделать фото";
    }

    public void Interact()
    {
        // Не даём взаимодействовать, пока ToDo UI не появился
        if (!todoVisible)
            return;

        // ? Если нельзя выполнить этот пункт — выходим
        if (toDoUI != null && !toDoUI.CanCompleteTask(todoIndex))
        {
            Debug.Log($"Нельзя выполнить пункт {todoIndex}, пока не выполнен предыдущий!");
            return;
        }

        if (taken || isUsed) return;

        StartCoroutine(TakePhotoSequence());
    }

    private IEnumerator TakePhotoSequence()
    {
        taken = true;
        isUsed = true;

        // === Временно скрываем диалог ===
        if (DialogueManager.Instance != null && DialogueManager.Instance.dialoguePanel.activeSelf)
            DialogueManager.Instance.dialoguePanel.SetActive(false);

        // Включаем UI телефона
        if (cameraUIScreen != null)
        {
            cameraUIScreen.SetActive(true);

            // Плавное появление интерфейса
            CanvasGroup uiGroup = cameraUIScreen.GetComponent<CanvasGroup>();
            if (uiGroup == null) uiGroup = cameraUIScreen.AddComponent<CanvasGroup>();
            uiGroup.alpha = 0f;

            float t = 0f;
            while (t < uiFadeInTime)
            {
                t += Time.deltaTime;
                uiGroup.alpha = Mathf.Lerp(0f, 1f, t / uiFadeInTime);
                yield return null;
            }
            uiGroup.alpha = 1f;
        }

        // Небольшая пауза — "фокусировка"
        yield return new WaitForSeconds(0.5f);

        // === ЗВУК ЗАТВОРА СРАЗУ ПРИ НАЖАТИИ E ===
        if (cameraSound != null)
        {
            AudioSource.PlayClipAtPoint(cameraSound, transform.position);
        }

        // ===== Эффект вспышки =====
        GameObject flash = new GameObject("Flash");
        flash.transform.SetParent(cameraUIScreen.transform, false);

        Image img = flash.AddComponent<Image>();
        img.color = Color.white;

        RectTransform rt = flash.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = new Vector2(1920, 1080);

        CanvasGroup cg = flash.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // Плавное появление (вспышка)
        float t1 = 0f;
        while (t1 < flashDuration / 2f)
        {
            t1 += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t1 / (flashDuration / 2f));
            yield return null;
        }

        // Плавное исчезновение
        float t2 = 0f;
        while (t2 < flashDuration / 2f)
        {
            t2 += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t2 / (flashDuration / 2f));
            yield return null;
        }

        Destroy(flash);

        // ===== Отмечаем пункт в ToDo =====
        if (toDoUI != null)
            toDoUI.MarkItemDone(todoIndex);

        // "Фото сохранено"
        Transform savedText = cameraUIScreen.transform.Find("SavedText");
        if (savedText != null)
        {
            savedText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            savedText.gameObject.SetActive(false);
        }

        // Пауза перед закрытием UI
        yield return new WaitForSeconds(photoDuration);

        // Плавное "опускание телефона"
        if (cameraUIScreen != null)
        {
            CanvasGroup uiGroup = cameraUIScreen.GetComponent<CanvasGroup>();
            if (uiGroup != null)
            {
                float t3 = 0f;
                while (t3 < uiFadeInTime)
                {
                    t3 += Time.deltaTime;
                    uiGroup.alpha = Mathf.Lerp(1f, 0f, t3 / uiFadeInTime);
                    yield return null;
                }
            }

            cameraUIScreen.SetActive(false);
        }

        // === Возвращаем диалог ===
        yield return new WaitForSeconds(1.6f);
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.dialoguePanel.SetActive(true);
    }

    public void OnHoverEnter()
    {
        // Если ToDo UI ещё не показан — скрываем курсор
        if (!todoVisible && Cursor.visible)
            Cursor.visible = false;
    }

    public void OnHoverExit()
    {
        // Ничего не делаем — курсор управляется InteractionController
    }
}
