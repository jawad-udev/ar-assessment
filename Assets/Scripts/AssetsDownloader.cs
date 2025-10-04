using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using UnityEngine.Events;
using TMPro;

public class AssetsDownloader : MonoBehaviour
{
    [Header("UI References")]
    public Button downloadButton;
    public TextMeshProUGUI progressText;
    public GameObject downloadMenu;

    [Header("Events")]
    public UnityEvent OnAssetsDownloaded;

    private void Start()
    {
        if (downloadButton != null)
        {
            downloadButton.onClick.AddListener(StartDownload);
        }

        CheckIfAssetsDownloaded();
    }

    private void CheckIfAssetsDownloaded()
    {
        StartCoroutine(CheckDownloadStatusCoroutine());
    }

    private IEnumerator CheckDownloadStatusCoroutine()
    {
        var sizeHandle = Addressables.GetDownloadSizeAsync("Models");
        yield return sizeHandle;

        if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
        {
            long downloadSize = sizeHandle.Result;

            if (downloadSize == 0)
            {
                // Assets already downloaded
                if (downloadMenu != null)
                {
                    downloadMenu.SetActive(false);
                }
            }
            else
            {
                // Assets need to be downloaded
                if (downloadButton != null)
                {
                    downloadButton.gameObject.SetActive(true);
                }
                if (progressText != null)
                {
                    progressText.gameObject.SetActive(false);
                }
            }
        }

        Addressables.Release(sizeHandle);
    }

    public void StartDownload()
    {
        if (downloadButton != null)
        {
            downloadButton.gameObject.SetActive(false);
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
        }

        StartCoroutine(DownloadAddressablesCoroutine());
    }

    private IEnumerator DownloadAddressablesCoroutine()
    {
        if (progressText != null)
        {
            progressText.text = "Initializing download...";
        }

        var downloadHandle = Addressables.DownloadDependenciesAsync("Models");

        while (!downloadHandle.IsDone)
        {
            float progress = downloadHandle.PercentComplete;

            if (progressText != null)
            {
                progressText.text = $"Downloading: {Mathf.RoundToInt(progress * 100)}%";
            }

            yield return null;
        }

        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            if (progressText != null)
            {
                progressText.text = "Download Complete!";
            }

            yield return new WaitForSeconds(1f);

            if (downloadMenu != null)
            {
                downloadMenu.SetActive(false);
            }

            // Models are now available for loading
            OnDownloadComplete();
        }
        else
        {
            if (progressText != null)
            {
                progressText.text = "Download Failed!";
            }

            if (downloadButton != null)
            {
                downloadButton.gameObject.SetActive(true);
            }
        }

        Addressables.Release(downloadHandle);
    }

    private void OnDestroy()
    {
        if (downloadButton != null)
        {
            downloadButton.onClick.RemoveListener(StartDownload);
        }
    }

    private void OnDownloadComplete()
    {
        OnAssetsDownloaded?.Invoke();
    }

    // Example methods to load downloaded models
    public void LoadModel(string addressableKey, System.Action<GameObject> onLoaded = null)
    {
        StartCoroutine(LoadModelCoroutine(addressableKey, onLoaded));
    }

    private IEnumerator LoadModelCoroutine(string addressableKey, System.Action<GameObject> onLoaded)
    {
        var loadHandle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
        yield return loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedModel = loadHandle.Result;
            onLoaded?.Invoke(loadedModel);
        }
        else
        {
            Debug.LogError($"Failed to load model: {addressableKey}");
        }
    }

    public void InstantiateModel(string addressableKey, Transform parent = null, System.Action<GameObject> onInstantiated = null)
    {
        StartCoroutine(InstantiateModelCoroutine(addressableKey, parent, onInstantiated));
    }

    private IEnumerator InstantiateModelCoroutine(string addressableKey, Transform parent, System.Action<GameObject> onInstantiated)
    {
        var instantiateHandle = Addressables.InstantiateAsync(addressableKey, parent);
        yield return instantiateHandle;

        if (instantiateHandle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject instantiatedModel = instantiateHandle.Result;
            onInstantiated?.Invoke(instantiatedModel);
        }
        else
        {
            Debug.LogError($"Failed to instantiate model: {addressableKey}");
        }
    }
}
