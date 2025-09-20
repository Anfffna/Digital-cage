using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] private Animator animator;       // Animator �����
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string boolName = "isOpen";

    private bool canInteract = false; // ����� � ����

    void Update()
    {
        if (canInteract && Input.GetKeyDown(interactKey))
        {
            animator.SetBool(boolName, true); // ������� �����
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
            animator.SetBool(boolName, false); // ������� �����
        }
    }
}
