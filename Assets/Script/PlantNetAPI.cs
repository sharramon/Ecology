using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using PassthroughCameraSamples.CameraToWorld;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlantNetAPI : MonoBehaviour
{
    [Header("Plant Net API")]
    [SerializeField] private string apiKey = "your_api_key_here";
    [SerializeField] private string project = "all"; // or "weurope", "canada", etc.
    [SerializeField] private int resultNumber = 3;
    [SerializeField] private bool includeRelatedImages = false;

    [Header("Plant info sort")]
    [SerializeField] private PlantNetDataSort dataSort;
    [SerializeField] private CameraToWorldCameraCanvas cameraCanvas;

    private void Start()
    {
        //IdentifyPlant(inputImage);
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        CaptureManager.Instance._onSnapshotTaken += IdentifyPlant;
    }
    public void IdentifyPlant(Texture2D image)
    {
        StartCoroutine(SendPlantImage(image));
    }

    private IEnumerator SendPlantImage(Texture2D rawImage)
    {

        // Copy and convert into readable format
        Texture2D tex2D = rawImage;

        byte[] imageBytes = tex2D.EncodeToPNG();
        if (imageBytes == null || imageBytes.Length == 0)
        {
            Debug.LogError("Failed to encode the copied Texture2D to PNG.");
            yield break;
        }

        // Log image info
        Debug.Log($"Encoded image size: {imageBytes.Length} bytes");
        Debug.Log($"Texture resolution: {tex2D.width}x{tex2D.height}");
        cameraCanvas.UpdateDebugText(($"Encoded image size: {imageBytes.Length} bytes"));
        cameraCanvas.UpdateDebugText(($"Texture resolution: {tex2D.width}x{tex2D.height}"));


        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
        new MultipartFormFileSection("images", imageBytes, "plant.png", "image/png"),
        new MultipartFormDataSection("organs", "auto")
        };

        string url = $"https://my-api.plantnet.org/v2/identify/{project}?include-related-images=false&no-reject=false&nb-results={resultNumber}&lang=en&api-key={apiKey}";

        Debug.Log("Sending request to: " + url);
        cameraCanvas.UpdateDebugText("Sending request to: " + url);

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = www.downloadHandler.text;
            
            var prettyJson = JsonConvert.SerializeObject(
            JsonConvert.DeserializeObject(www.downloadHandler.text),
            Formatting.Indented
    );
            Debug.Log("Success:\n" + prettyJson);
            cameraCanvas.UpdateDebugText("Success");

            // Directly pass JSON response to PlantNetDataSort
            dataSort.SetAndSortJSON(jsonResponse);
        }
        else
        {
            int statusCode = (int)www.responseCode;
            string errorText = www.downloadHandler.text;
            string fallbackMessage = "An unexpected error occurred.";

            switch (statusCode)
            {
                case 400:
                    fallbackMessage = "Bad Request – check image or parameters.";
                    break;
                case 401:
                    fallbackMessage = "Unauthorized – check your API key.";
                    break;
                case 404:
                    fallbackMessage = "Species Not Found – no match.";
                    break;
                case 413:
                    fallbackMessage = "Payload Too Large – try resizing.";
                    break;
                case 414:
                    fallbackMessage = "URI Too Long.";
                    break;
                case 415:
                    fallbackMessage = "Unsupported Media Type – must be PNG or JPG.";
                    break;
                case 429:
                    fallbackMessage = "Too Many Requests – wait a moment.";
                    break;
                case 500:
                    fallbackMessage = "Internal Server Error – PlantNet is down?";
                    break;
            }

            Debug.LogError($"HTTP {statusCode}: {fallbackMessage}\n{errorText}");
            dataSort.ShowError(statusCode.ToString(), fallbackMessage);
        }
    }

    private Texture2D CopyToReadable(Texture texture)
    {
        RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height, 0);
        Graphics.Blit(texture, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readableTex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        readableTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return readableTex;
    }
}