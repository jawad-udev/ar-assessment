using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CircularScrollView : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Transform circleContainer;   // Parent of all items
    public float rotateDuration = 0.5f; // Smooth rotation
    private int currentIndex = 0;
    private float dragThreshold = 50f;
    private Vector2 dragStart;

    private int totalItems;
    private float rotationStep;

    void Start()
    {
        totalItems = circleContainer.childCount;
        rotationStep = 360f / totalItems;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragStart == Vector2.zero)
            dragStart = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float dragDist = eventData.position.x - dragStart.x;
        dragStart = Vector2.zero;

        if (Mathf.Abs(dragDist) > dragThreshold)
        {
            if (dragDist > 0) currentIndex--; // Swipe right
            else currentIndex++;              // Swipe left

            // Keep index in range
            if (currentIndex < 0) currentIndex = totalItems - 1;
            if (currentIndex >= totalItems) currentIndex = 0;

            // Animate rotation
            circleContainer.DOLocalRotate(
                new Vector3(0, currentIndex * rotationStep, 0), 
                rotateDuration
            ).SetEase(Ease.OutCubic);
        }
    }
}
