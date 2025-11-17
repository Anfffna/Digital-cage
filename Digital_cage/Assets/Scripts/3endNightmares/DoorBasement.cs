using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DoorBasement : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    public Transform teleportPoint;
    public GameObject player;

    [Header("Dialogue Settings")]
    public ManagerDialogue3 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Dependency Settings")]
    public CarpetMovement carpetMovement;

    [Header("Fade Settings")]
    public Image blackScreen;
    public float fadeDuration = 2.0f;

    [Header("Music Settings")]
    public AudioSource musicController;

    [Header("Audio Settings")]
    public AudioClip doorOpenSound;
    public AudioSource audioSource;

    private bool hasBeenUsed = false;
    private bool dialogueTriggered = false;
    private bool isInteractable = false;
    private CharacterController characterController;
    private Coroutine checkCarpetCoroutine;
    private bool isInBasement = false;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(false);
            blackScreen.color = new Color(0, 0, 0, 0);
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log("DoorBasement: Игрок найден автоматически: " + (player != null));
        }

        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
            Debug.Log("DoorBasement: CharacterController найден: " + (characterController != null));
        }

        if (carpetMovement == null)
        {
            carpetMovement = FindObjectOfType<CarpetMovement>();
            Debug.Log("DoorBasement: CarpetMovement найден автоматически: " + (carpetMovement != null));
        }

        if (musicController == null)
        {
            musicController = FindObjectOfType<AudioSource>();
            Debug.Log("DoorBasement: MusicController найден автоматически: " + (musicController != null));
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("DoorBasement: AudioSource создан автоматически");
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        checkCarpetCoroutine = StartCoroutine(CheckCarpetCompletion());
    }

    private IEnumerator CheckCarpetCompletion()
    {
        Debug.Log("DoorBasement: Ожидание выполнения CarpetMovement...");

        while (carpetMovement == null)
        {
            yield return new WaitForSeconds(0.5f);
            carpetMovement = FindObjectOfType<CarpetMovement>();
        }

        Debug.Log("DoorBasement: CarpetMovement найден, ожидаем завершения...");

        while (!isInteractable && !hasBeenUsed)
        {
            if (carpetMovement != null)
            {
                var carpetType = carpetMovement.GetType();
                var hasBeenActivatedField = carpetType.GetField("hasBeenActivated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

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
        {
            return "";
        }

        return "Нажмите E";
    }

    public void Interact()
    {
        if (!isInteractable || hasBeenUsed || dialogueTriggered)
        {
            return;
        }

        dialogueTriggered = true;

        if (doorOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
            Debug.Log("DoorBasement: Проигрывается звук открывания двери подвала");
        }
        else
        {
            if (doorOpenSound == null)
            {
                Debug.LogWarning("DoorBasement: DoorOpenSound не назначен!");
            }
            if (audioSource == null)
            {
                Debug.LogWarning("DoorBasement: AudioSource не найден!");
            }
        }

        StartCoroutine(FullTeleportSequence());
    }

    private IEnumerator FullTeleportSequence()
    {
        Debug.Log("DoorBasement: Начало последовательности телепортации");

        yield return StartCoroutine(FadeBlackScreen(0.0f, 1.0f, fadeDuration / 2.0f));

        if (musicController != null)
        {
            musicController.Stop();
            Debug.Log("DoorBasement: Музыка выключена");
        }

        if (TeleportPlayer())
        {
            isInBasement = true;

            yield return new WaitForEndOfFrame();

            yield return StartCoroutine(FadeBlackScreen(1.0f, 0.0f, fadeDuration / 2.0f));

            if (dialogueManager != null && dialogueLines != null && dialogueLines.Count > 0)
            {
                dialogueManager.StartDialogue(dialogueLines, OnDialogueEnd);
            }
            else
            {
                OnDialogueEnd();
            }
        }
        else
        {
            yield return StartCoroutine(FadeBlackScreen(1.0f, 0.0f, fadeDuration / 2.0f));
            dialogueTriggered = false;
        }
    }

    private IEnumerator FadeBlackScreen(float fromAlpha, float toAlpha, float duration)
    {
        if (blackScreen == null)
        {
            yield break;
        }

        if (!blackScreen.gameObject.activeInHierarchy)
        {
            blackScreen.gameObject.SetActive(true);
        }

        float timer = 0.0f;
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

        if (toAlpha == 0.0f)
        {
            blackScreen.gameObject.SetActive(false);
        }

        Debug.Log("DoorBasement: Fade завершен " + fromAlpha + " -> " + toAlpha);
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

        Vector3 oldPosition = player.transform.position;

        if (characterController != null)
        {
            characterController.enabled = false;
            player.transform.position = teleportPoint.position;
            player.transform.rotation = teleportPoint.rotation;
            characterController.enabled = true;

            Debug.Log("DoorBasement: Телепортация через отключение CharacterController");
        }
        else
        {
            player.transform.position = teleportPoint.position;
            player.transform.rotation = teleportPoint.rotation;
        }

        Debug.Log("DoorBasement: Игрок телепортирован из " + oldPosition + " в " + player.transform.position);
        Debug.Log("DoorBasement: Расстояние телепортации: " + Vector3.Distance(oldPosition, player.transform.position) + " units");

        return true;
    }

    private void OnDialogueEnd()
    {
        hasBeenUsed = true;
        dialogueTriggered = false;
        isInteractable = false;

        gameObject.layer = LayerMask.NameToLayer("Default");

        Debug.Log("DoorBasement: Взаимодействие с дверью завершено");
    }

    public void EnableMusic()
    {
        if (musicController != null && isInBasement)
        {
            musicController.Play();
            isInBasement = false;
            Debug.Log("DoorBasement: Музыка включена обратно");
        }
    }

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

    void OnDrawGizmosSelected()
    {
        if (teleportPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(teleportPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, teleportPoint.position);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(teleportPoint.position, teleportPoint.forward * 1.0f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(teleportPoint.position + Vector3.up, "Teleport Point");
#endif
        }
    }
}