using UnityEngine;
using System.Collections;

public class DoorOpener : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public Animator animator;
    public string boolName = "isOpen";
    public float closeDelay = 2f;

    [Header("Player Blocker Settings")]
    public Collider playerBlocker;      // Невидимая стена
    public Collider vnuTrigger;         // Внутренний триггер, когда игрок достигнет его, блокер станет настоящей стеной
    private bool playerEntered = false; // Игрок вошёл в область двери
    private bool playerReachedVnu = false; // Игрок дошёл до внутреннего триггера

    private bool isOpen = false;

    void Start()
    {
        if (playerBlocker != null)
            playerBlocker.isTrigger = true; // Изначально пропускаем игрока
    }

    void Update()
    {
        if (!playerEntered || playerReachedVnu || vnuTrigger == null || playerBlocker == null) return;

        // Проверяем, достиг ли игрок внутреннего триггера
        if (vnuTrigger.bounds.Contains(GameObject.FindWithTag("Player").transform.position))
        {
            playerReachedVnu = true;
            playerBlocker.isTrigger = false; // Делаем блокер настоящей стеной
        }
    }

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
        return ""; // Возвращаем пустую строку — чтобы система не показывала текст
    }

    IEnumerator CloseDoorWithDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        isOpen = false;
        if (animator != null)
            animator.SetBool(boolName, isOpen);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!playerEntered && other.CompareTag("Player"))
            playerEntered = true;
    }
}









