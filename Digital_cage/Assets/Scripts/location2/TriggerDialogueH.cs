using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TriggerDialogueH : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public ManagerDialogue2 dialogueManager;

    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Black Screen Settings")]
    public Image blackScreenImage;
    public float blackScreenDuration = 1f;
    public float fadeOutDuration = 2f;

    private bool triggered = false;
    private bool playerInTrigger = false;
    private bool canStartDialogue = false;

    void Start()
    {
        // Запускаем черный экран + задержку
        StartCoroutine(SceneStartSequence());
    }

    private IEnumerator SceneStartSequence()
    {
        // Шаг 1: Черный экран
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(true);
            blackScreenImage.color = new Color(0, 0, 0, 1);
            Debug.Log("TriggerDialogueH: Черный экран активирован");
        }

        // Шаг 2: Ждем указанное время черного экрана
        yield return new WaitForSeconds(blackScreenDuration);

        // Шаг 3: Плавно убираем черный экран
        if (blackScreenImage != null)
        {
            yield return StartCoroutine(FadeOutBlackScreen());
        }

        // Шаг 4: Разрешаем диалог
        canStartDialogue = true;
        Debug.Log("TriggerDialogueH: Диалог теперь можно начинать");

        // Проверяем, не находится ли игрок уже в триггере
        if (playerInTrigger && !triggered)
        {
            triggered = true;
            dialogueManager.StartDialogue(dialogueLines);
            Debug.Log("TriggerDialogueH: Диалог запущен (игрок уже в триггере)");
        }
    }

    private IEnumerator FadeOutBlackScreen()
    {
        float timer = 0f;
        Color startColor = blackScreenImage.color;
        Color endColor = new Color(0, 0, 0, 0);

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration;
            blackScreenImage.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        blackScreenImage.gameObject.SetActive(false);
        Debug.Log("TriggerDialogueH: Черный экран скрыт");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            Debug.Log("TriggerDialogueH: Игрок вошел в триггер");

            // Если можно начинать диалог и еще не запущен
            if (canStartDialogue && !triggered)
            {
                triggered = true;
                dialogueManager.StartDialogue(dialogueLines);
                Debug.Log("TriggerDialogueH: Диалог запущен после задержки");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            Debug.Log("TriggerDialogueH: Игрок вышел из триггера");
        }
    }
}