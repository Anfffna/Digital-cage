using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueManager dialogueManager;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Secretary Reference")]
    public SecretaryPath secretaryPath;

    [Header("Camera Glitch")]
    public CameraGlitchEffect cameraGlitch;

    [Header("Blackout Settings")]
    public BlackScreenController blackScreenController;
    public float blackoutDelay = 4f;
    public float blackScreenHold = 2f;

    [Header("Scene Transition")]
    public string nextSceneName = "location2Horror";
    public float transitionDelay = 5f; // Ждем 5 секунд после черного экрана

    private bool started = false;

    public void StartFinalDialogue()
    {
        if (started) return;
        started = true;

        if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
        {
            dialogueManager.StartDialogue(dialogueLines, OnDialogueLineFinished, true, true);
            Debug.Log("FinalDialogue: финальный диалог запущен.");
        }
        else
        {
            Debug.LogWarning("FinalDialogue: отсутствует DialogueManager или нет реплик!");
        }
    }

    private void OnDialogueLineFinished(int lineIndex)
    {
        Debug.Log($"FinalDialogue: завершена строка №{lineIndex}.");

        if (lineIndex == 4)
        {
            if (secretaryPath != null)
            {
                secretaryPath.AllowFinalPath();
                Debug.Log("FinalDialogue: путь секретарши разблокирован после 4-й реплики");

                if (DialogueManager.Instance != null)
                    DialogueManager.Instance.WaitForSecretaryToFinish();

                secretaryPath.OnFinalPathCompleted += OnSecretaryFinishedFinalPath;
            }
        }

        if (lineIndex == 5)
        {
            if (cameraGlitch != null)
            {
                cameraGlitch.StartGlitch();
                Debug.Log("FinalDialogue: запускаем глитч камеры на 5-й реплике!");
            }

            if (blackScreenController != null)
            {
                StartCoroutine(InstantBlackoutAndTransition());
            }
            else
            {
                Debug.LogError("FinalDialogue: BlackScreenController не назначен!");
            }
        }
    }

    private IEnumerator InstantBlackoutAndTransition()
    {
        yield return new WaitForSeconds(blackoutDelay);

        // Скрываем текущий диалог
        if (dialogueManager != null)
        {
            dialogueManager.HideDialogue();
        }

        // Используем BlackScreenController для черного экрана
        if (blackScreenController != null && blackScreenController.blackScreenImage != null)
        {
            // Активируем и делаем полностью черным мгновенно
            blackScreenController.blackScreenImage.gameObject.SetActive(true);
            Color color = blackScreenController.blackScreenImage.color;
            color.a = 1f;
            blackScreenController.blackScreenImage.color = color;

            // Отключаем raycast target чтобы не мешал
            blackScreenController.blackScreenImage.raycastTarget = false;

            Debug.Log("FinalDialogue: экран стал полностью чёрным!");
        }

        yield return new WaitForSeconds(blackScreenHold);

        // Показываем 7-ю строку диалога на чёрном экране
        if (dialogueManager != null && dialogueLines.Count > 6)
        {
            string line6 = dialogueLines[6];
            dialogueManager.ShowSingleLine(line6);
            Debug.Log("FinalDialogue: показываем 7-ю реплику на чёрном экране!");
        }

        // ЖДЕМ 5 СЕКУНД И ПЕРЕХОДИМ
        yield return new WaitForSeconds(transitionDelay);

        // Скрываем диалог если еще виден
        if (dialogueManager != null)
        {
            dialogueManager.HideDialogue();
        }

        // Переходим на новую сцену
        Debug.Log("FinalDialogue: переход на сцену " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnSecretaryFinishedFinalPath()
    {
        Debug.Log("FinalDialogue: секретарша завершила путь — запускаем 5-ю реплику!");

        if (secretaryPath != null)
            secretaryPath.OnFinalPathCompleted -= OnSecretaryFinishedFinalPath;

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ContinueAfterSecretary();
    }
}