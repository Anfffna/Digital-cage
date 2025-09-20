using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] private Animator animator;        // Animator двери
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string triggerName = "Open";

    private bool hasOpened = false;   // чтобы дверь не открывалась снова
    private bool canInteract = false; // флаг: игрок рядом с дверью

    void Update()
    {
        if (!hasOpened && canInteract && Input.GetKeyDown(interactKey))
        {
            hasOpened = true;
            animator.SetTrigger(triggerName);
        }
    }

    // Зона взаимодействия
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            canInteract = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            canInteract = false;
    }
}
