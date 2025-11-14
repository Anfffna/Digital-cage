using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DoorBasement : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    public Transform teleportPoint; // Точка телепортации
    public GameObject player; // Ссылка на игрока

    [Header("Dialogue Settings")]
    public ManagerDialogue3 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Dependency Settings")]
    public CarpetMovement carpetMovement; // Ссылка на скрипт ковра

    [Header("Fade Settings")]
    public Image blackScreen; // Черный экран
    public float fadeDuration = 2f; // Длительность fade эффекта

    [Header("Music Settings")]
    public AudioSource musicController; // Контроллер музыки

    private bool hasBeenUsed = false;
    private bool dialogueTriggered = false;
    private bool isInteractable = false;
    private CharacterController characterController;
    private Coroutine checkCarpetCoroutine;
    private bool isInBasement = false;

    void Start()
    {
        // Изначально делаем дверь неинтерактивной
        gameObject.layer = LayerMask.NameToLayer("Default");

        // Скрываем черный экран при старте
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(false);
            blackScreen.color = new Color(0, 0, 0, 0);
        }

        // Если игрок не назначен в инспекторе, пытаемся найти автоматически
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log("DoorBasement: Игрок найден автоматически: " + (player != null));
        }

        // Получаем компонент CharacterController
        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
            Debug.Log("DoorBasement: CharacterController найден: " + (characterController != null));
        }

        // Если ковер не назначен в инспекторе, пытаемся найти автоматически
        if (carpetMovement == null)
        {
            carpetMovement = FindObjectOfType<CarpetMovement>();
            Debug.Log("DoorBasement: CarpetMovement найден автоматически: " + (carpetMovement != null));
        }

        // Если музыкальный контроллер не назначен, пытаемся найти автоматически
        if (musicController == null)
        {
            musicController = FindObjectOfType<AudioSource>();
            Debug.Log("DoorBasement: MusicController найден автоматически: " + (musicController != null));
        }

        // Начинаем проверку выполнения ковра
        checkCarpetCoroutine = StartCoroutine(CheckCarpetCompletion());
    }

    private IEnumerator CheckCarpetCompletion()
    {
        Debug.Log("DoorBasement: Ожидание выполнения CarpetMovement...");

        // Ждем пока ковер не будет готов
        while (carpetMovement == null)
        {
            yield return new WaitForSeconds(0.5f);
            carpetMovement = FindObjectOfType<CarpetMovement>();
        }

        Debug.Log("DoorBasement: CarpetMovement найден, ожидаем завершения...");

        // Постоянно проверяем, был ли активирован ковер
        while (!isInteractable && !hasBeenUsed)
        {
            if (carpetMovement != null)
            {
                // Используем рефлексию для проверки приватного поля hasBeenActivated
                var carpetType = carpetMovement.GetType();
                var hasBeenActivatedField = carpetType.GetField("hasBeenActivated",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (hasBeenActivatedField != null)
                {
                    bool carpetActivated = (bool)hasBeenActivatedField.GetValue(carpetMovement);
                    if (carpetActivated)
                    {
                        UnlockDoor();
                        Debug.Log("DoorBasement: Ковер активирован! Дверь разблокирована.");
                        yield break;
                    }
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void UnlockDoor()
    {
        isInteractable = true;
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        Debug.Log("DoorBasement: Дверь подвала теперь интерактивна!");
    }

    public string GetInteractionText()
    {
        if (!isInteractable || hasBeenUsed)
            return "";

        return "Нажмите E";
    }

    public void Interact()
    {
        if (!isInteractable || hasBeenUsed || dialogueTriggered) return;

        dialogueTriggered = true;

        // Запускаем полную последовательность с черным экраном
        StartCoroutine(FullTeleportSequence());
    }

    private IEnumerator FullTeleportSequence()
    {
        Debug.Log("DoorBasement: Начало последовательности телепортации");

        // Шаг 1: Плавное появление черного экрана
        yield return StartCoroutine(FadeBlackScreen(0f, 1f, fadeDuration / 2f));

        // Шаг 2: Выключаем музыку когда экран полностью черный
        if (musicController != null)
        {
            musicController.Stop();
            Debug.Log("DoorBasement: Музыка выключена");
        }

        // Шаг 3: Телепортация игрока (происходит когда экран полностью черный)
        if (TeleportPlayer())
        {
            isInBasement = true;

            // Ждем один кадр чтобы физика обновилась
            yield return new WaitForEndOfFrame();

            // Шаг 4: Плавное исчезновение черного экрана
            yield return StartCoroutine(FadeBlackScreen(1f, 0f, fadeDuration / 2f));

            // Шаг 5: Запускаем диалог после телепортации
            if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
            {
                dialogueManager.StartDialogue(dialogueLines, OnDialogueEnd);
            }
            else
            {
                // Если диалога нет, просто завершаем взаимодействие
                OnDialogueEnd();
            }
        }
        else
        {
            // Если телепортация не удалась, убираем черный экран
            yield return StartCoroutine(FadeBlackScreen(1f, 0f, fadeDuration / 2f));
            dialogueTriggered = false;
        }
    }

    private IEnumerator FadeBlackScreen(float fromAlpha, float toAlpha, float duration)
    {
        if (blackScreen == null) yield break;

        // Активируем черный экран если он выключен
        if (!blackScreen.gameObject.activeInHierarchy)
        {
            blackScreen.gameObject.SetActive(true);
        }

        float timer = 0f;
        Color startColor = new Color(0, 0, 0, fromAlpha);
        Color endColor = new Color(0, 0, 0, toAlpha);

        blackScreen.color = startColor;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            blackScreen.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        blackScreen.color = endColor;

        // Если экран полностью прозрачный - скрываем его
        if (toAlpha == 0f)
        {
            blackScreen.gameObject.SetActive(false);
        }

        Debug.Log($"DoorBasement: Fade завершен {fromAlpha} -> {toAlpha}");
    }

    private bool TeleportPlayer()
    {
        if (player == null)
        {
            Debug.LogError("DoorBasement: Player не найден!");
            return false;
        }

        if (teleportPoint == null)
        {
            Debug.LogError("DoorBasement: TeleportPoint не назначен!");
            return false;
        }

        // Запоминаем старую позицию для отладки
        Vector3 oldPosition = player.transform.position;

        if (characterController != null)
        {
            // Отключаем CharacterController на время телепортации
            characterController.enabled = false;
            player.transform.position = teleportPoint.position;
            player.transform.rotation = teleportPoint.rotation;
            characterController.enabled = true;

            Debug.Log("DoorBasement: Телепортация через отключение CharacterController");
        }
        else
        {
            // Обычная телепортация если нет CharacterController
            player.transform.position = teleportPoint.position;
            player.transform.rotation = teleportPoint.rotation;
        }

        Debug.Log($"DoorBasement: Игрок телепортирован из {oldPosition} в {player.transform.position}");
        Debug.Log($"DoorBasement: Расстояние телепортации: {Vector3.Distance(oldPosition, player.transform.position)} units");

        return true;
    }

    private void OnDialogueEnd()
    {
        // После завершения диалога делаем дверь неинтерактивной
        hasBeenUsed = true;
        dialogueTriggered = false;
        isInteractable = false;

        // Меняем слой чтобы нельзя было взаимодействовать повторно
        gameObject.layer = LayerMask.NameToLayer("Default");

        Debug.Log("DoorBasement: Взаимодействие с дверью завершено");
    }

    // Метод для включения музыки обратно (если понадобится)
    public void EnableMusic()
    {
        if (musicController != null && isInBasement)
        {
            musicController.Play();
            isInBasement = false;
            Debug.Log("DoorBasement: Музыка включена обратно");
        }
    }

    // Метод для принудительного отключения музыки
    public void DisableMusic()
    {
        if (musicController != null)
        {
            musicController.Stop();
            Debug.Log("DoorBasement: Музыка принудительно отключена");
        }
    }

    void OnDestroy()
    {
        if (checkCarpetCoroutine != null)
        {
            StopCoroutine(checkCarpetCoroutine);
        }
    }

    // Визуальная отладка в редакторе
    void OnDrawGizmosSelected()
    {
        if (teleportPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(teleportPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, teleportPoint.position);

            // Рисуем стрелку направления
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(teleportPoint.position, teleportPoint.forward * 1f);

            // Подписываем точку телепорта
#if UNITY_EDITOR
            UnityEditor.Handles.Label(teleportPoint.position + Vector3.up, "Teleport Point");
#endif
        }
    }
}