using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using System.IO;

[System.Serializable]
public class GenerateRequest
{
    public string prompt;
    public int num_inference_steps = 20;
    public float guidance_scale = 7.5f;
}

[System.Serializable]
public class GenerateResponse
{
    public string image; 
}

// --- Основной класс ---
public class Gen_image_AI : MonoBehaviour
{
    public string serverIP = "80.64.24.133";
    public int serverPort = 8000;


    public IEnumerator GenerateImage(string prompt, Action<Texture2D> onCompleted)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            Debug.LogError("Промпт пустой.");
            onCompleted?.Invoke(null);
            yield break;
        }

        string url = $"http://{serverIP}:{serverPort}/generate";
        Debug.Log(">>> Отправка запроса на: " + url);

        var req = new GenerateRequest { prompt = prompt };
        string json = JsonUtility.ToJson(req);

        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("User-Agent", "UnityClient/1.0");
            www.timeout = 240; 

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Ошибка генерации: " + www.error);
                if (!string.IsNullOrEmpty(www.downloadHandler.text))
                    Debug.LogError("📄 Ответ сервера: " + www.downloadHandler.text);
                onCompleted?.Invoke(null);
                yield break;
            }

            string responseJson = www.downloadHandler.text;
            Debug.Log("✅ Ответ от сервера: " + responseJson);

            GenerateResponse response = JsonUtility.FromJson<GenerateResponse>(responseJson);

            if (string.IsNullOrEmpty(response?.image))
            {
                Debug.LogError("Сервер не вернул изображение (поле 'image' пустое).");
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
                    Debug.Log("✅ Изображение успешно загружено. Размер: " + tex.width + "x" + tex.height);
                    SaveImage(tex, prompt);
                    onCompleted?.Invoke(tex);
                }
                else
                {
                    Debug.LogError("❌ Не удалось загрузить изображение из байтов.");
                    onCompleted?.Invoke(null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("💥 Ошибка обработки изображения: " + e.Message);
                onCompleted?.Invoke(null);
            }
        }
    }

    /// <summary>
    /// Сохраняет изображение в надёжное место (работает в Editor и на всех платформах).
    /// </summary>
    void SaveImage(Texture2D texture, string prompt)
    {
        try
        {
            byte[] pngData = texture.EncodeToPNG();
            string safePrompt = string.Join("_", prompt.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"AI_{safePrompt}.png";

            string folderPath = Path.Combine(Application.persistentDataPath, "GeneratedImages");
            string filePath = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath); // Создаёт всю цепочку папок

            File.WriteAllBytes(filePath, pngData);
            Debug.Log("💾 Изображение сохранено: " + filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("📁 Ошибка сохранения файла: " + e.Message);
        }
    }
}