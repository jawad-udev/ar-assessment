using UnityEngine;
using System.Collections;
using System.IO;

public class ShareUtil : MonoBehaviour
{
    public static ShareUtil Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShareScreenshot()
    {
        StartCoroutine(TakeScreenshotAndShare());
    }

    private IEnumerator TakeScreenshotAndShare()
    {
        yield return new WaitForEndOfFrame();

        string fileName = "screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        string filePath = Path.Combine(Application.temporaryCachePath, fileName);

        ScreenCapture.CaptureScreenshot(fileName);

        yield return new WaitForSeconds(0.5f);

        if (!File.Exists(filePath))
        {
            string alternativePath = Path.Combine(Application.persistentDataPath, fileName);
            ScreenCapture.CaptureScreenshot(alternativePath);
            filePath = alternativePath;
            yield return new WaitForSeconds(0.5f);
        }

        if (File.Exists(filePath))
        {
            NativeShare(filePath);
        }
        else
        {
            Debug.LogError("Screenshot file not found at: " + filePath);
        }
    }

    private void NativeShare(string filePath)
    {
        new NativeShare()
            .AddFile(filePath)
            .SetSubject("AR Screenshot")
            .SetText("Check out my AR experience!")
            .Share();
    }
}