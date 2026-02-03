using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

public class TextGeneration : MonoBehaviour
{
    private const string apiUrl = "https://api.openai.com/v1/chat/completions";
    private const string apiKey = "";

    public string prompt = "You are a helpful assistant.";
    public string content = "Write a haiku about recursion in programming.";

    void Start()
    {
        StartCoroutine(SendOpenAIRequest());
    }

    public IEnumerator SendOpenAIRequest()
    {
        string jsonData = @"{
            ""model"": ""gpt-4o"",
            ""messages"": [
                {
                    ""role"": ""system"",
                    ""content"": """ + prompt + @"""
                },
                {
                    ""role"": ""user"",
                    ""content"": """ + content + @"""
                }
            ]
        }";

        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error : " + request.error);
            }
            else
            {
                string responsText = request.downloadHandler.text;
                Debug.Log("Response : " + responsText);

                string pattern = @"""content"":\s*""(.*?)""";
                Match match = Regex.Match(responsText, pattern);

                if (match.Success)
                {
                    string assistantMessage = match.Groups[1].Value;
                    Debug.Log("Assistant Message : " + assistantMessage);
                }
                else
                {
                    Debug.LogWarning("No valid response found.");
                }
            }
        }
    }
}
