using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class MenuButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public float scaleMultiplier = 1.2f;
    public Color hoverColor = Color.yellow;

    [Header("References")]
    public TextMeshProUGUI text;
    public Image image;

    private Color originalColor;
    private float originalFontSize;
    private float originalScale;
    private Button button;
    private RectTransform rectTransform;

    private bool isAnimating = false;
    private float animationTime = 0f;
    private bool isHovered = false;
    private bool isInitialized = false;

    void Start()
    {
        InitializeButtonAnimator();
    }

    void OnEnable()
    {
        if (isInitialized)
        {
            ResetButtonToOriginalState();
        }
        else
        {
            InitializeButtonAnimator();
        }
    }

    void Update()
    {
        ProcessButtonAnimation();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        HandlePointerEnterEvent();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HandlePointerExitEvent();
    }

    void OnDisable()
    {
        ResetButtonToOriginalState();
    }

    private void InitializeButtonAnimator()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();

        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (text != null)
        {
            originalColor = text.color;
            originalFontSize = text.fontSize;
        }
        else if (image != null)
        {
            originalColor = image.color;
            originalScale = transform.localScale.x;
        }

        if (button != null)
        {
            button.transition = Selectable.Transition.None;
        }

        ResetButtonToOriginalState();
        isInitialized = true;
    }

    private void ProcessButtonAnimation()
    {
        if (isAnimating == false)
        {
            return;
        }

        animationTime += Time.deltaTime;
        float animationProgress = Mathf.Clamp01(animationTime / animationDuration);

        if (isHovered)
        {
            AnimateButtonHoverState(animationProgress);
        }
        else
        {
            AnimateButtonNormalState(animationProgress);
        }

        if (animationProgress >= 1.0f)
        {
            isAnimating = false;
        }
    }

    private void AnimateButtonHoverState(float progress)
    {
        if (text != null)
        {
            text.color = Color.Lerp(originalColor, hoverColor, progress);
            text.fontSize = Mathf.Lerp(originalFontSize, originalFontSize * scaleMultiplier, progress);
        }
        else if (image != null)
        {
            image.color = Color.Lerp(originalColor, hoverColor, progress);
            float currentScale = Mathf.Lerp(originalScale, originalScale * scaleMultiplier, progress);
            transform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }
    }

    private void AnimateButtonNormalState(float progress)
    {
        if (text != null)
        {
            text.color = Color.Lerp(hoverColor, originalColor, progress);
            text.fontSize = Mathf.Lerp(originalFontSize * scaleMultiplier, originalFontSize, progress);
        }
        else if (image != null)
        {
            image.color = Color.Lerp(hoverColor, originalColor, progress);
            float currentScale = Mathf.Lerp(originalScale * scaleMultiplier, originalScale, progress);
            transform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }
    }

    private void HandlePointerEnterEvent()
    {
        if (button != null && button.interactable == false)
        {
            return;
        }

        StartHoverAnimation();
    }

    private void HandlePointerExitEvent()
    {
        if (button != null && button.interactable == false)
        {
            return;
        }

        EndHoverAnimation();
    }

    private void StartHoverAnimation()
    {
        if (isAnimating)
        {
            isAnimating = false;
        }

        isHovered = true;
        isAnimating = true;
        animationTime = 0.0f;
    }

    private void EndHoverAnimation()
    {
        isHovered = false;
        isAnimating = true;
        animationTime = 0.0f;
    }

    private void ResetButtonToOriginalState()
    {
        isAnimating = false;
        isHovered = false;
        animationTime = 0.0f;

        if (button != null)
        {
            button.OnDeselect(null);
        }

        if (text != null)
        {
            text.color = originalColor;
            text.fontSize = originalFontSize;
        }
        else if (image != null)
        {
            image.color = originalColor;
            transform.localScale = new Vector3(originalScale, originalScale, originalScale);
        }
    }
}