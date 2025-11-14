using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EntryScene3 : MonoBehaviour
{
    [Header("Cursor Settings")]
    public CursorUI cursorManager;

    [Header("Black Screen Settings")]
    public Image blackScreenImage;
    public float blackScreenDuration = 1f;
    public float fadeOutDuration = 2f;

    private Coroutine sceneSequenceCoroutine;
    private Coroutine fadeCoroutine;

    void Start()
    {
        sceneSequenceCoroutine = StartCoroutine(SceneStartSequence());
    }

    private IEnumerator SceneStartSequence()
    {
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(true);
            blackScreenImage.color = new Color(0, 0, 0, 1);
        }

        if (cursorManager == null)
        {
            cursorManager = FindObjectOfType<CursorUI>();
        }

        if (cursorManager != null)
        {
            cursorManager.HideCursor();
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        yield return StartCoroutine(ForceHideCursor());

        yield return new WaitForSeconds(blackScreenDuration);

        if (blackScreenImage != null)
        {
            yield return StartCoroutine(FadeOutBlackScreen());
        }

        CheckCursorFinalState();
    }

    private IEnumerator ForceHideCursor()
    {
        yield return new WaitForEndOfFrame();

        if (cursorManager != null)
        {
            cursorManager.HideCursor();
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        yield return new WaitForSeconds(0.1f);

        if (cursorManager != null)
        {
            cursorManager.HideCursor();
        }
    }

    private IEnumerator FadeOutBlackScreen()
    {
        if (blackScreenImage == null) yield break;

        float timer = 0f;
        Color startColor = blackScreenImage.color;
        Color endColor = new Color(0, 0, 0, 0);

        while (timer < fadeOutDuration)
        {
            if (blackScreenImage == null) yield break;

            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration;
            blackScreenImage.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(false);
        }
    }

    private void CheckCursorFinalState()
    {
        CursorUI[] finalCursors = FindObjectsOfType<CursorUI>();
        foreach (CursorUI cursor in finalCursors)
        {
            // Final check logic if needed
        }
    }

    void Update()
    {
        // Debug check removed
    }

    void OnDestroy()
    {
        if (sceneSequenceCoroutine != null)
        {
            StopCoroutine(sceneSequenceCoroutine);
        }
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}