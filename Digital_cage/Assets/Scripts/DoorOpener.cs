using UnityEngine;
using System.Collections;

public class DoorOpener : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public Animator animator;
    public string boolName = "isOpen";
    public float closeDelay = 2f;

    [Header("Audio Settings")]
    public AudioSource audioSource;          // Аудиоисточник
    public AudioClip doorOpenSound;          // Звук открытия двери
    public AudioClip doorCloseSound;         // Звук закрытия двери
    public float soundDelay = 0.1f;          // Задержка звука после анимации

    [Header("Player Blocker Settings")]
    public Collider playerBlocker;      // Невидимая стена
    public Collider vnuTrigger;         // Внутренний триггер, когда игрок достигнет его, блокер станет настоящей стеной
    private bool playerEntered = false; // Игрок вошёл в область двери
    private bool playerReachedVnu = false; // Игрок дошёл до внутреннего триггера

    private bool isOpen = false;

    void Start()
    {
        // Автоматически находим AudioSource если не назначен
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

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

        // Проигрываем звук в зависимости от действия
        if (isOpen)
        {
            PlayDoorOpenSound();
            StartCoroutine(CloseDoorWithDelay());
        }
        else
        {
            PlayDoorCloseSound();
        }
    }

    public string GetInteractionText()
    {
        return ""; // Возвращаем пустую строку — чтобы система не показывала текст
    }

    IEnumerator CloseDoorWithDelay()
    {
        yield return new WaitForSeconds(closeDelay - soundDelay); // Вычитаем задержку звука

        // Проигрываем звук закрытия перед самой анимацией
        PlayDoorCloseSound();

        yield return new WaitForSeconds(soundDelay);

        isOpen = false;
        if (animator != null)
            animator.SetBool(boolName, isOpen);
    }

    private void PlayDoorOpenSound()
    {
        if (audioSource != null && doorOpenSound != null)
        {
            StartCoroutine(PlaySoundWithDelay(doorOpenSound, soundDelay));
        }
    }

    private void PlayDoorCloseSound()
    {
        if (audioSource != null && doorCloseSound != null)
        {
            StartCoroutine(PlaySoundWithDelay(doorCloseSound, soundDelay));
        }
    }

    IEnumerator PlaySoundWithDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(clip);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!playerEntered && other.CompareTag("Player"))
            playerEntered = true;
    }
}