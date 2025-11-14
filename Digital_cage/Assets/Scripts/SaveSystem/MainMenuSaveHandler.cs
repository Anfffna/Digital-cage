using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuSaveHandler : MonoBehaviour
{
    [Header("Версии List Parent")]
    [SerializeField] private GameObject listParentWithoutContinue;
    [SerializeField] private GameObject listParentWithContinue;

    [Header("Настройки")]
    [SerializeField] private string firstLevelName = "Game";
    [SerializeField] private float menuShowDelay = 0.5f; // Задержка перед показом меню

    void Start()
    {
        if (listParentWithoutContinue == null || listParentWithContinue == null)
        {
            Debug.LogError("List Parents не назначены!");
            return;
        }

        // Сразу скрываем оба меню
        listParentWithoutContinue.SetActive(false);
        listParentWithContinue.SetActive(false);

        // Запускаем отложенный показ меню
        StartCoroutine(ShowMenuWithDelay());
    }

    IEnumerator ShowMenuWithDelay()
    {
        Debug.Log("Ожидаем перед показом меню...");

        // Ждем указанное время
        yield return new WaitForSeconds(menuShowDelay);

        // Ждем SaveManager
        int maxFramesToWait = 10;
        int framesWaited = 0;

        while (SaveManager.Instance == null && framesWaited < maxFramesToWait)
        {
            yield return new WaitForEndOfFrame();
            framesWaited++;
        }

        // Определяем какое меню показывать
        GameObject menuToShow = (SaveManager.Instance != null && SaveManager.HasSave)
            ? listParentWithContinue
            : listParentWithoutContinue;

        // Показываем меню
        menuToShow.SetActive(true);

        // Плавное появление через CanvasGroup
        CanvasGroup canvasGroup = menuToShow.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = menuToShow.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        // Анимация появления
        float fadeTime = 0.5f;
        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeTime);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public void OnContinuePressed()
    {
        if (SaveManager.Instance != null && SaveManager.HasSave)
        {
            Debug.Log("Загружаем сохраненную игру...");
            SaveManager.Instance.LoadGame();
        }
        else
        {
            Debug.LogWarning("Нет сохраненной игры!");
        }
    }

    public void OnNewGamePressed()
    {
        Debug.Log("Начинаем новую игру...");

        if (SaveManager.Instance != null && SaveManager.HasSave)
        {
            SaveManager.Instance.DeleteSave();
        }

        SceneManager.LoadScene(firstLevelName);
    }

    void OnEnable()
    {
        if (listParentWithoutContinue != null && listParentWithContinue != null)
        {
            StartCoroutine(ShowMenuWithDelay());
        }
    }
}