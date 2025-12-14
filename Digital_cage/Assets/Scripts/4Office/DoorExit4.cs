using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DoorExit4 : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public GameObject doorObject; // сама дверь
    public string targetSceneName = "5Home"; // сцена для перехода

    [Header("Fade Settings")]
    public Image fadeImage; // черный UI Image для затемнения
    public float fadeDuration = 1f; // длительность затемнения

    private bool isInteractable = true; // Сразу интерактивна
    private bool isTransitioning = false;

    void Start()
    {
        if (doorObject == null)
            doorObject = this.gameObject;

        // Настраиваем fade image если он существует
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            // Устанавливаем полностью прозрачным в начале
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("DoorExit4: fadeImage не назначен!");
        }

        // Ставим дверь интерактивной сразу
        doorObject.layer = LayerMask.NameToLayer("Interactable");
        Debug.Log("DoorExit4: Дверь интерактивна с самого начала!");
    }

    public void Interact()
    {
        if (!isInteractable || isTransitioning) return;

        Debug.Log("DoorExit4: Игрок нажал на дверь, переходим в " + targetSceneName);

        // Скрываем курсор
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Начинаем переход
        StartCoroutine(TransitionToScene());
    }

    public string GetInteractionText()
    {
        return isInteractable ? "Нажмите E, чтобы выйти" : "";
    }

    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;

        // Затемняем экран
        if (fadeImage != null)
        {
            yield return StartCoroutine(FadeToBlack());
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            // Если нет fadeImage, просто ждем
            yield return new WaitForSeconds(1f);
        }

        // Загружаем сцену
        SceneManager.LoadScene(targetSceneName);
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    // Для тестирования - принудительный переход
    public void ForceTransition()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToScene());
        }
    }
}