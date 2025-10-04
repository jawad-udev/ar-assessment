using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SpecialButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hover Image")]
    public Image hoverImage;

    [Header("Events")]
    public UnityEvent onButtonClick;

    void Start()
    {
        if (hoverImage != null)
            hoverImage.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverImage != null)
            hoverImage.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverImage != null)
            hoverImage.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onButtonClick?.Invoke();
    }
}
