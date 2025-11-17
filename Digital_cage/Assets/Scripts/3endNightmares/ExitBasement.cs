using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ExitBasement : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    public Transform teleportPoint;
    public GameObject player;

    [Header("Dependency Settings")]
    public GameMachine gameMachine;
    public Note note;
    public ShadowBasement shadowBasement;

    [Header("Audio Settings")]
    public AudioClip teleportAudioClip;
    public AudioClip doorOpenSound;
    public AudioSource audioSource;

    [Header("Dialogue Settings")]
    public ManagerDialogue3 dialogueManager;
    [TextArea(2, 5)]
    public List<string> dialogueLines;

    [Header("Fade Settings")]
    public Image exitBlackScreen;
    public float fadeDuration = 2.0f;

    private bool hasBeenUsed = false;
    private bool dialogueTriggered = false;
    private bool isInteractable = false;
    private CharacterController characterController;
    private Coroutine checkDependenciesCoroutine;
    private bool isExitFadeInProgress = false;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");

        if (exitBlackScreen != null)
        {
            exitBlackScreen.gameObject.SetActive(false);
            exitBlackScreen.color = new Color(0, 0, 0, 0);
        }
        else
        {
            Debug.LogWarning("ExitBasement: ExitBlackScreen не назначен! —оздайте отдельный Image дл€ ExitBasement.");
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log("ExitBasement: »грок найден автоматически: " + (player != null));
        }

        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
            Debug.Log("ExitBasement: CharacterController найден: " + (characterController != null));
        }

        SetupAudioSource();
        FindDependencies();
        checkDependenciesCoroutine = StartCoroutine(CheckDependenciesCompletion());
    }

    private void SetupAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("ExitBasement: AudioSource создан автоматически");
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void FindDependencies()
    {
        if (gameMachine == null)
        {
            gameMachine = FindObjectOfType<GameMachine>();
            Debug.Log("ExitBasement: GameMachine найден автоматически: " + (gameMachine != null));
        }

        if (note == null)
        {
            note = FindObjectOfType<Note>();
            Debug.Log("ExitBasement: Note найден автоматически: " + (note != null));
        }

        if (shadowBasement == null)
        {
            shadowBasement = FindObjectOfType<ShadowBasement>();
            Debug.Log("ExitBasement: ShadowBasement найден автоматически: " + (shadowBasement != null));
        }
    }

    private IEnumerator CheckDependenciesCompletion()
    {
        Debug.Log("ExitBasement: ќжидание выполнени€ всех зависимостей...");

        bool allDependenciesCompleted = false;

        while (!allDependenciesCompleted && !hasBeenUsed)
        {
            bool gameMachineCompleted = CheckObjectCompleted(gameMachine, "GameMachine");
            bool noteCompleted = CheckObjectCompleted(note, "Note");
            bool shadowBasementCompleted = CheckObjectCompleted(shadowBasement, "ShadowBasement");

            allDependenciesCompleted = gameMachineCompleted && noteCompleted && shadowBasementCompleted;

            if (allDependenciesCompleted)
            {
                UnlockExit();
                Debug.Log("ExitBasement: ¬се зависимости выполнены! ¬ыход разблокирован.");
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private bool CheckObjectCompleted(MonoBehaviour obj, string objName)
    {
        if (obj == null)
        {
            Debug.Log("ExitBasement: " + objName + " не найден");
            return false;
        }

        var objType = obj.GetType();
        var hasBeenUsedField = objType.GetField("hasBeenUsed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (hasBeenUsedField != null)
        {
            bool isCompleted = (bool)hasBeenUsedField.GetValue(obj);
            if (isCompleted)
            {
                Debug.Log("ExitBasement: " + objName + " выполнен");
            }
            return isCompleted;
        }

        Debug.Log("ExitBasement: Ќе удалось найти поле hasBeenUsed в " + objName);
        return false;
    }

    private void UnlockExit()
    {
        isInteractable = true;
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        Debug.Log("ExitBasement: ¬ыход из подвала теперь интерактивен!");
    }

    public string GetInteractionText()
    {
        if (!isInteractable || hasBeenUsed)
        {
            return "";
        }

        return "Ќажмите E";
    }

    public void Interact()
    {
        if (!isInteractable || hasBeenUsed || dialogueTriggered || isExitFadeInProgress)
        {
            return;
        }

        dialogueTriggered = true;

        if (doorOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
            Debug.Log("ExitBasement: ѕроигрываетс€ звук открывани€ двери подвала");
        }
        else
        {
            if (doorOpenSound == null)
            {
                Debug.LogWarning("ExitBasement: DoorOpenSound не назначен!");
            }
            if (audioSource == null)
            {
                Debug.LogWarning("ExitBasement: AudioSource не найден!");
            }
        }

        StartCoroutine(FullTeleportSequence());
    }

    private IEnumerator FullTeleportSequence()
    {
        Debug.Log("ExitBasement: Ќачало последовательности телепортации");
        isExitFadeInProgress = true;

        yield return StartCoroutine(FadeExitBlackScreen(0.0f, 1.0f, fadeDuration / 2.0f));

        if (TeleportPlayer())
        {
            yield return new WaitForEndOfFrame();

            yield return StartCoroutine(FadeExitBlackScreen(1.0f, 0.0f, fadeDuration / 2.0f));

            PlayTeleportAudio();

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
            yield return StartCoroutine(FadeExitBlackScreen(1.0f, 0.0f, fadeDuration / 2.0f));
            dialogueTriggered = false;
            isExitFadeInProgress = false;
        }
    }

    private void PlayTeleportAudio()
    {
        if (teleportAudioClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportAudioClip);
            Debug.Log("ExitBasement: јудио клип проигрываетс€ после телепортации");
        }
        else
        {
            if (teleportAudioClip == null)
            {
                Debug.LogWarning("ExitBasement: TeleportAudioClip не назначен!");
            }
            if (audioSource == null)
            {
                Debug.LogWarning("ExitBasement: AudioSource не найден!");
            }
        }
    }

    private IEnumerator FadeExitBlackScreen(float fromAlpha, float toAlpha, float duration)
    {
        if (exitBlackScreen == null)
        {
            Debug.LogError("ExitBasement: ExitBlackScreen не назначен!");
            yield break;
        }

        if (!exitBlackScreen.gameObject.activeInHierarchy)
        {
            exitBlackScreen.gameObject.SetActive(true);
        }

        float timer = 0.0f;
        Color startColor = new Color(0, 0, 0, fromAlpha);
        Color endColor = new Color(0, 0, 0, toAlpha);

        exitBlackScreen.color = startColor;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            exitBlackScreen.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        exitBlackScreen.color = endColor;

        if (toAlpha == 0.0f)
        {
            exitBlackScreen.gameObject.SetActive(false);
            isExitFadeInProgress = false;
        }

        Debug.Log("ExitBasement: Fade ExitBlackScreen завершен " + fromAlpha + " -> " + toAlpha);
    }

    private bool TeleportPlayer()
    {
        if (player == null)
        {
            Debug.LogError("ExitBasement: Player не найден!");
            return false;
        }

        if (teleportPoint == null)
        {
            Debug.LogError("ExitBasement: TeleportPoint не назначен!");
            return false;
        }

        Vector3 oldPosition = player.transform.position;

        if (characterController != null)
        {
            characterController.enabled = false;
            player.transform.position = teleportPoint.position;
            player.transform.rotation = teleportPoint.rotation;
            characterController.enabled = true;

            Debug.Log("ExitBasement: “елепортаци€ через отключение CharacterController");
        }
        else
        {
            player.transform.position = teleportPoint.position;
            player.transform.rotation = teleportPoint.rotation;
        }

        Debug.Log("ExitBasement: »грок телепортирован из " + oldPosition + " в " + player.transform.position);
        Debug.Log("ExitBasement: –ассто€ние телепортации: " + Vector3.Distance(oldPosition, player.transform.position) + " units");

        return true;
    }

    private void OnDialogueEnd()
    {
        hasBeenUsed = true;
        dialogueTriggered = false;
        isInteractable = false;

        gameObject.layer = LayerMask.NameToLayer("Default");

        Debug.Log("ExitBasement: ¬заимодействие с выходом завершено");
    }

    void OnDestroy()
    {
        if (checkDependenciesCoroutine != null)
        {
            StopCoroutine(checkDependenciesCoroutine);
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
            UnityEditor.Handles.Label(teleportPoint.position + Vector3.up, "Exit Teleport Point");
#endif
        }
    }
}