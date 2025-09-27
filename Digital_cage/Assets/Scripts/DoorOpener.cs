using UnityEngine;
using System.Collections;

public class DoorOpener : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string boolName = "isOpen";

    [Header("UI Settings")]
    [SerializeField] private CanvasGroup pressToE; // CanvasGroup для плавного фейда
    [SerializeField] private float fadeDuration = 0.5f; // скорость появления/исчезновения

    [Header("Close Settings")]
    [SerializeField] private float closeDelay = 2f; // задержка перед закрытием двери

    private bool canInteract = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        if (pressToE != null)
        {
            pressToE.alpha = 0; // изначально невидимо
            pressToE.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (canInteract && Input.GetKeyDown(interactKey))
        {
            animator.SetBool(boolName, true); // открыть дверь
            StartCoroutine(CloseDoorWithDelay()); // запускаем закрытие с задержкой
            FadeOut(); // плавно скрываем надпись
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            FadeIn();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            FadeOut();
        }
    }

    IEnumerator CloseDoorWithDelay()
    {
        yield return new WaitForSeconds(closeDelay); // ждём 2 секунды
        animator.SetBool(boolName, false); // закрываем дверь
    }

    void FadeIn()
    {
        if (pressToE != null)
        {
            pressToE.gameObject.SetActive(true);
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(pressToE, pressToE.alpha, 1, fadeDuration));
        }
    }

    void FadeOut()
    {
        if (pressToE != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(pressToE, pressToE.alpha, 0, fadeDuration, () =>
            {
                pressToE.gameObject.SetActive(false);
            }));
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float end, float duration, System.Action onComplete = null)
    {
        float time = 0f;
        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = end;
        onComplete?.Invoke();
    }
}


