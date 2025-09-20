using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] private Animator animator;       // Animator двери
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string boolName = "isOpen";

    private bool canInteract = false; // игрок в зоне

    void Update()
    {
        if (canInteract && Input.GetKeyDown(interactKey))
        {
            animator.SetBool(boolName, true); // открыть дверь
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            canInteract = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            animator.SetBool(boolName, false); // закрыть дверь
        }
    }
}
