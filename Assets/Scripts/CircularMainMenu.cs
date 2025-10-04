using UnityEngine;
using DG.Tweening;

public class CircularMainMenu : MonoBehaviour
{
    [Header("Radial Menu Settings")]
    public CanvasGroup menuCanvasGroup;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public float distanceFromCamera = 2f;

    private bool isMenuVisible = false;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (isMenuVisible)
        {
            PositionMenuInFrontOfCamera();
        }
    }

    private void PositionMenuInFrontOfCamera()
    {
        if (mainCamera == null) return;

        Vector3 menuPosition = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera;
        transform.position = menuPosition;
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }

    public void ShowMenu()
    {
        if (isMenuVisible) return;

        isMenuVisible = true;
        PositionMenuInFrontOfCamera();

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.DOKill();
            menuCanvasGroup.DOFade(1f, fadeDuration).OnStart(() =>
            {
                menuCanvasGroup.interactable = true;
                menuCanvasGroup.blocksRaycasts = true;
            });
        }
    }

    public void HideMenu()
    {
        if (!isMenuVisible) return;

        isMenuVisible = false;

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.DOKill();
            menuCanvasGroup.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                menuCanvasGroup.interactable = false;
                menuCanvasGroup.blocksRaycasts = false;
            });
        }
    }
}
