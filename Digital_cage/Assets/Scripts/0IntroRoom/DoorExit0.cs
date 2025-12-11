using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorExit0 : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public GameObject doorObject; // сама дверь (можно использовать Collider)
    public string targetSceneName = "1locationOffice"; // сцена для перехода

    [Header("Entry Dialogue")]
    public EntryDialogue entryDialogue; // ссылка на EntryDialogue оферты

    private bool isInteractable = false;

    void Start()
    {
        if (doorObject == null)
            doorObject = this.gameObject;

        // Ставим дверь неинтерактивной
        doorObject.layer = LayerMask.NameToLayer("Default");

        if (entryDialogue != null)
        {
            StartCoroutine(WaitForSignatureCompletion());
        }
        else
        {
            Debug.LogWarning("DoorExit0: EntryDialogue не назначен!");
        }
    }

    private System.Collections.IEnumerator WaitForSignatureCompletion()
    {
        // Ждём, пока подпись в EntryDialogue не будет завершена
        while (!entryDialogue.SignatureCompleted)
            yield return null;


        // Делаем дверь интерактивной
        isInteractable = true;
        doorObject.layer = LayerMask.NameToLayer("Interactable");
        Debug.Log("DoorExit0: подпись завершена, дверь теперь интерактивна!");
    }

    public void Interact()
    {
        if (!isInteractable) return;

        Debug.Log("DoorExit0: игрок нажал на дверь, загружаем сцену " + targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }

    public string GetInteractionText()
    {
        return isInteractable ? "Нажмите E, чтобы выйти" : "";
    }
}
