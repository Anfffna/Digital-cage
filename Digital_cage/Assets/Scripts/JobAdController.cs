using UnityEngine;

public class JobAdController : MonoBehaviour
{
    public GameObject jobAdPanel;
    public CursorUI cursorManager;

    void Start()
    {
        jobAdPanel.SetActive(true);

        if (cursorManager != null)
        {
            cursorManager.ShowCursor();
        }

        EntryDialogue entryDialogue = jobAdPanel.GetComponent<EntryDialogue>();
        if (entryDialogue != null)
        {
            entryDialogue.StartDialogue();
        }
        else
        {
            Debug.LogWarning("JobAdController: EntryDialogue не найден!");
        }
    }

    public void CloseAd()
    {
        jobAdPanel.SetActive(false);
        if (cursorManager != null)
        {
            cursorManager.HideCursor();
        }
    }
}