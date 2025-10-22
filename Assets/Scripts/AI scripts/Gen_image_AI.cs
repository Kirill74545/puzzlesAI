// Gen_image_AI.cs
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using System.IO;

public class Gen_image_AI : MonoBehaviour
{
    [Serializable]
    private class GenerateRequest
    {
        public string prompt;
        public int num_inference_steps = 20;
        public float guidance_scale = 7.5f;
    }

    [Serializable]
    private class GenerateResponse
    {
        public string image; // base64 строка
    }

    public string serverIP = "192.168.206.232";
    public int serverPort = 8000;

    // Основной метод: генерация изображения по промпту
    public IEnumerator GenerateImage(string prompt, Action<Texture2D> onCompleted)
    {
        string url = $"http://{serverIP}:{serverPort}/generate";
        var req = new GenerateRequest { prompt = prompt };
        string json = JsonUtility.ToJson(req);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ошибка генерации: " + www.error);
                Debug.LogError("Ответ сервера: " + www.downloadHandler.text);
                onCompleted?.Invoke(null);
                yield break;
            }

            string responseJson = www.downloadHandler.text;
            GenerateResponse response = JsonUtility.FromJson<GenerateResponse>(responseJson);

            if (string.IsNullOrEmpty(response?.image))
            {
                Debug.LogError("Сервер не вернул изображение.");
                onCompleted?.Invoke(null);
                yield break;
            }

            try
            {
                string cleanBase64 = response.image
                    .Replace(" ", "")
                    .Replace("\n", "")
                    .Replace("\r", "");

                byte[] imageBytes = Convert.FromBase64String(cleanBase64);
                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(imageBytes))
                {
                    // Сохраняем на устройстве (опционально)
                    SaveImage(tex, prompt);
                    onCompleted?.Invoke(tex);
                }
                else
                {
                    Debug.LogError("Не удалось загрузить изображение из байтов.");
                    onCompleted?.Invoke(null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Ошибка обработки изображения: " + e.Message);
                onCompleted?.Invoke(null);
            }
        }
    }

    void SaveImage(Texture2D texture, string prompt)
    {
        byte[] pngData = texture.EncodeToPNG();
        string safePrompt = string.Join("_", prompt.Split(Path.GetInvalidFileNameChars()));
        string fileName = $"AI_{safePrompt}.png";

#if UNITY_EDITOR
        string path = Path.Combine(Application.dataPath, "GeneratedImages", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, pngData);
        Debug.Log("Сохранено в редакторе: " + path);
        UnityEditor.AssetDatabase.Refresh();
#else
        string path = Path.Combine(Application.persistentDataPath, "GeneratedImages", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, pngData);
        Debug.Log("Сохранено на устройстве: " + path);
#endif
    }
}