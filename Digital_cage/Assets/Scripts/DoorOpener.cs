using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] private Animator animator;        // Animator �����
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string triggerName = "Open";

    private bool hasOpened = false;   // ����� ����� �� ����������� �����
    private bool canInteract = false; // ����: ����� ����� � ������

    void Update()
    {
        if (!hasOpened && canInteract && Input.GetKeyDown(interactKey))
        {
            hasOpened = true;
            animator.SetTrigger(triggerName);
        }
    }

    // ���� ��������������
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
