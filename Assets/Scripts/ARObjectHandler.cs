using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public class ModelButton
{
    public string modelName; // Display name for the model
    public SpecialButton button;
    public string addressableKey; // The key/address of the model in Addressables
    public TMPro.TextMeshProUGUI buttonLabel; // Optional: Reference to button's text component
}

public class ARObjectHandler : MonoBehaviour
{
    [Header("AR Components")]
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    [Header("Model Buttons")]
    public ModelButton[] modelButtons; // Array of 5 buttons with their model references
    public GameObject buttonPanel; // The UI panel/screen to hide when button is pressed

    [Header("Spawn Settings")]
    public float spawnOffset = 0f; // Height offset from plane
    public bool rotateTowardCamera = true;

    [Header("Radial Menu")]
    public CircularMainMenu radialMenu;

    private string selectedModelKey;
    private GameObject currentModelPrefab;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        // Setup button listeners and labels
        for (int i = 0; i < modelButtons.Length; i++)
        {
            int index = i; // Capture index for closure
            modelButtons[i].button.onButtonClick.AddListener(() => OnModelButtonClicked(index));

            // Update button label if assigned
            if (modelButtons[i].buttonLabel != null && !string.IsNullOrEmpty(modelButtons[i].modelName))
            {
                modelButtons[i].buttonLabel.text = modelButtons[i].modelName;
            }
        }
    }

    void OnModelButtonClicked(int buttonIndex)
    {
        selectedModelKey = modelButtons[buttonIndex].addressableKey;
        Debug.Log($"Selected model: {selectedModelKey}");

        // Hide the button panel
        if (buttonPanel != null)
        {
            buttonPanel.GetComponent<CircularMainMenu>().HideMenu();
        }

        // Optionally load the prefab ahead of time
        LoadModelPrefab(selectedModelKey);
    }

    void Update()
    {
        // Check for touch/mouse input using new Input System
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            HandleScreenInput(touchPosition);
        }
        // Fallback to mouse for editor testing
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            HandleScreenInput(mousePosition);
        }
    }

    void HandleScreenInput(Vector2 screenPosition)
    {
        // Perform raycast to detect AR planes
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            // If a model is selected from menu, spawn it
            if (!string.IsNullOrEmpty(selectedModelKey))
            {
                Pose hitPose = hits[0].pose;
                Vector3 spawnPosition = hitPose.position + Vector3.up * spawnOffset;
                SpawnModel(spawnPosition, hitPose.rotation);
            }
            // If radial menu exists and no model selected, show menu
            else if (radialMenu != null)
            {
                radialMenu.ShowMenu();
            }
        }
    }

    public void OnMenuItemSelected(int index)
    {
        if (index >= 0 && index < modelButtons.Length)
        {
            selectedModelKey = modelButtons[index].addressableKey;
            LoadModelPrefab(selectedModelKey);
        }
    }

    void LoadModelPrefab(string key)
    {
        // Load the prefab from Addressables
        Addressables.LoadAssetAsync<GameObject>(key).Completed += OnModelLoaded;
    }

    void OnModelLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            currentModelPrefab = handle.Result;

            // Try to get Model-Name or Model-Description from the prefab
            Transform modelName = currentModelPrefab.transform.Find("Model-Name");
            Transform modelDesc = currentModelPrefab.transform.Find("Model-Description");

            string displayName = currentModelPrefab.name;
            if (modelName != null)
            {
                TMPro.TextMeshProUGUI nameText = modelName.GetComponent<TMPro.TextMeshProUGUI>();
                if (nameText != null) displayName = nameText.text;
            }
            else if (modelDesc != null)
            {
                TMPro.TextMeshProUGUI descText = modelDesc.GetComponent<TMPro.TextMeshProUGUI>();
                if (descText != null) displayName = descText.text;
            }

            Debug.Log($"Model prefab loaded: {displayName}");
        }
        else
        {
            Debug.LogError($"Failed to load model: {selectedModelKey}");
        }
    }

    void SpawnModel(Vector3 position, Quaternion rotation)
    {
        if (string.IsNullOrEmpty(selectedModelKey)) return;

        // Instantiate using Addressables
        Addressables.InstantiateAsync(selectedModelKey, position, Quaternion.identity).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject spawnedObject = handle.Result;

                // Optionally rotate toward camera
                if (rotateTowardCamera)
                {
                    Vector3 lookDirection = Camera.main.transform.position - position;
                    lookDirection.y = 0; // Keep it horizontal
                    if (lookDirection != Vector3.zero)
                    {
                        spawnedObject.transform.rotation = Quaternion.LookRotation(-lookDirection);
                    }
                }

                Debug.Log($"Spawned {selectedModelKey} at {position}");

                // Clear selection to prevent accidental spawning
                selectedModelKey = null;
            }
            else
            {
                Debug.LogError($"Failed to instantiate model: {selectedModelKey}");
            }
        };
    }

    // Public method to show the button panel (call this from your UI button)
    public void ShowButtonPanel()
    {
        if (buttonPanel != null)
        {
            buttonPanel.GetComponent<CircularMainMenu>().ShowMenu();
        }
    }

    void OnDestroy()
    {
        // Clean up button listeners
        for (int i = 0; i < modelButtons.Length; i++)
        {
            modelButtons[i].button.onButtonClick.RemoveAllListeners();
        }
    }
}