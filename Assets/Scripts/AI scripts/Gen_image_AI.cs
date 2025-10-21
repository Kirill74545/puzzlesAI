using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;
using System.IO;

public class SDClient : MonoBehaviour
{
    [Header("Настройки сервера")]
    public string serverIP = "192.168.206.232";
    public int serverPort = 8000;

    [Header("UI элементы")]
    public TMP_InputField promptInput;
    public Button generateButton;
    public RawImage outputImage;

    void Start()
    {
        if (generateButton != null)
            generateButton.onClick.AddListener(OnGenerateClicked);
    }

    void OnGenerateClicked()
    {
        string prompt = promptInput.text.Trim();
        if (string.IsNullOrEmpty(prompt))
        {
            Debug.LogWarning("Промпт пустой!");
            return;
        }

        string url = $"http://{serverIP}:{serverPort}/generate";
        StartCoroutine(SendPrompt(url, prompt));
    }

    [System.Serializable]
    private class GenerateRequest
    {
        public string prompt;
        public int num_inference_steps = 20;     
        public float guidance_scale = 7.5f;       
    }

    [System.Serializable]
    private class GenerateResponse
    {
        public string image;
    }

    IEnumerator SendPrompt(string url, string prompt)
    {
        var req = new GenerateRequest { prompt = prompt };
        string json = JsonUtility.ToJson(req);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Отправка запроса: " + url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ошибка: " + www.error);
                Debug.LogError("Ответ: " + www.downloadHandler.text);
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                GenerateResponse response = JsonUtility.FromJson<GenerateResponse>(responseJson);

                if (string.IsNullOrEmpty(response?.image))
                {
                    Debug.LogError("Поле 'image' отсутствует в ответе.");
                    yield break;
                }

                string cleanBase64 = response.image.Replace(" ", "").Replace("\n", "").Replace("\r", "");

                try
                {
                    byte[] imageBytes = System.Convert.FromBase64String(cleanBase64);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(imageBytes);

                    if (outputImage != null)
                        outputImage.texture = tex;

                    SaveImage(tex, prompt);

                    Debug.Log("✅ Изображение сохранено!");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Ошибка при сохранении: " + e.Message);
                }
            }
        }
    }

    void SaveImage(Texture2D texture, string prompt)
    {
        byte[] pngData = texture.EncodeToPNG();
        string safePrompt = string.Join("_", prompt.Split(Path.GetInvalidFileNameChars()));
        string fileName = $"sd_{safePrompt}_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";

#if UNITY_EDITOR
        string folderPath = Path.Combine(Application.dataPath, "Resources");
        Directory.CreateDirectory(folderPath);
        string fullPath = Path.Combine(folderPath, fileName);
        File.WriteAllBytes(fullPath, pngData);
        Debug.Log("Сохранено в редакторе: " + fullPath);

       
        UnityEditor.AssetDatabase.Refresh();
#else
        // На мобильном устройстве — в persistentDataPath
        string folderPath = Path.Combine(Application.persistentDataPath, "Resources");
        Directory.CreateDirectory(folderPath);
        string fullPath = Path.Combine(folderPath, fileName);
        File.WriteAllBytes(fullPath, pngData);
        Debug.Log("Сохранено на устройстве: " + fullPath);
#endif
    }
}
