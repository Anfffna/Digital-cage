using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [Header("Loading Screen Settings")]
    public GameObject loadingScreenPanel;
    public float loadingDuration = 5f;
    public float segmentUpdateInterval = 0.5f;
    public float fadeDuration = 2f;

    [Header("Loading Segments")]
    public Image[] loadingSegments;

    [Header("Auto Start (For Testing)")]
    public bool autoStartOnAwake = true;

    [Header("Scene Settings")]
    public string nextSceneName = "3endNightmares";

    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (loadingScreenPanel != null)
        {
            canvasGroup = loadingScreenPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = loadingScreenPanel.AddComponent<CanvasGroup>();
            }
        }

        if (autoStartOnAwake)
        {
            StartLoadingScreen();
        }
    }

    private void Start()
    {
        if (!autoStartOnAwake && loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        HideAllSegments();
    }

    public void StartLoadingScreen()
    {
        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(true);
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// Плавное появление
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        float timer = 0f;
        canvasGroup.alpha = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        // После появления запускаем загрузку
        StartCoroutine(LoadingRoutine());
    }

    /// <summary>
    /// Корутина загрузки с переходом на сцену
    /// </summary>
    private IEnumerator LoadingRoutine()
    {
        HideAllSegments();

        float timer = 0f;
        int currentSegment = 0;
        int totalSegments = loadingSegments.Length;

        while (timer < loadingDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / loadingDuration;

            int targetSegment = Mathf.FloorToInt(progress * totalSegments);

            while (currentSegment < targetSegment && currentSegment < totalSegments)
            {
                ShowSegment(currentSegment);
                currentSegment++;
                yield return new WaitForSeconds(segmentUpdateInterval);
            }

            yield return null;
        }

        // Убеждаемся что все сегменты заполнены
        for (int i = currentSegment; i < totalSegments; i++)
        {
            ShowSegment(i);
            yield return new WaitForSeconds(segmentUpdateInterval);
        }

        // Загрузка завершена - мгновенный переход на сцену
        SceneManager.LoadScene(nextSceneName);
    }

    private void ShowSegment(int index)
    {
        if (index >= 0 && index < loadingSegments.Length && loadingSegments[index] != null)
        {
            loadingSegments[index].gameObject.SetActive(true);
        }
    }

    private void HideAllSegments()
    {
        foreach (Image segment in loadingSegments)
        {
            if (segment != null)
            {
                segment.gameObject.SetActive(false);
            }
        }
    }

    [ContextMenu("Test Loading Screen")]
    public void TestLoadingScreen()
    {
        StartLoadingScreen();
    }
}