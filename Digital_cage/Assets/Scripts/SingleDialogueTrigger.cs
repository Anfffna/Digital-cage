using UnityEngine;
using TMPro;
using System.Collections;

public class SingleDialogueTriggerGG : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Dialogue Settings")]
    [TextArea] public string sentence;
    public float delay = 0f;
    public float displayTime = 3f;
    public float typingSpeed = 0.05f;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            StartCoroutine(ShowDialogue());
        }
    }

    IEnumerator ShowDialogue()
    {
        yield return new WaitForSeconds(delay);

        dialoguePanel.SetActive(true);
        dialogueText.alignment = TextAlignmentOptions.Center;
        dialogueText.richText = true;

        DialogueManager.Instance.dialogueActive = true;
        yield return StartCoroutine(TypeSentenceRich(sentence));

        yield return new WaitForSeconds(displayTime);

        dialoguePanel.SetActive(false);
        DialogueManager.Instance.dialogueActive = false;

        GetComponent<Collider>().enabled = false;
    }

    IEnumerator TypeSentenceRich(string sentence)
    {
        dialogueText.text = "";
        int i = 0;
        bool insideTag = false;

        while (i < sentence.Length)
        {
            char c = sentence[i];

            if (c == '<') insideTag = true;
            dialogueText.text += c;
            if (c == '>') insideTag = false;

            i++;
            if (!insideTag)
                yield return new WaitForSeconds(typingSpeed);
        }
    }
}






