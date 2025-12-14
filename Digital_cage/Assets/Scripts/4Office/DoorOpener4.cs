using UnityEngine;
using System.Collections;

public class DoorOpener4 : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public Animator animator;
    public string boolName = "isOpen";
    public float closeDelay = 2f;

    private bool isOpen = false;

    public void Interact()
    {
        isOpen = !isOpen;

        if (animator != null)
            animator.SetBool(boolName, isOpen);

        if (isOpen)
            StartCoroutine(CloseDoorWithDelay());
    }

    public string GetInteractionText()
    {
        return ""; // ѕуста€ строка Ч система не показывает текст
    }

    IEnumerator CloseDoorWithDelay()
    {
        yield return new WaitForSeconds(closeDelay);

        isOpen = false;

        if (animator != null)
            animator.SetBool(boolName, isOpen);
    }
}