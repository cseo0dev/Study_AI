using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OpenAIManager : MonoBehaviour
{
    private const string apiUrl = "https://api.openai.com/v1/chat/completions";
    public string apiKey = "";
    public string prompt = "You are a helpful assistant. Answer questions concisely using only standard alphanumeric characters and basic punctuation (e.g., periods, commas). Avoid symbols, emojis, or markdown formatting to ensure compatibility with text-to-speech APIs.";

    public static OpenAIManager Instance;

    // Whisper API
    // NPC 설정 추가
    //public event Action OnReceivedMessage;
    //public string currentPrompt = "당신은 NPC 캐릭터 RobotKyle입니다. 질문에 답해주세요." + "특히 철자 오류를 교정하세요" + "Chat gpt";
    //public Text uiText;

    // TTS API
    public event Action<string> onResponseOpenAI;  // InputField 텍스트 완료 이벤트

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // WhisperManager.Instance.OnReceivedWhisper += RecievedWhisper;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.onInputFieldSubmit += OnInputFieldCompleted;
        }
        else
        {
            Debug.LogError("UIManager 인스턴스가 없습니다.");
        }
    }

    //private void RecievedWhisper(string transcribedText)
    //{
    //    StartCoroutine(SendOpenAIRequest(currentPrompt, transcribedText, uiText));
    //}

    private void OnInputFieldCompleted(string message)
    {
        StartCoroutine(SendOpenAIRequest(prompt, message));
    }

    public IEnumerator SendOpenAIRequest(string prompt, string message)
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
                    ""content"": """ + message + @"""
                }
            ],
            ""store"": false
        }";

        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result ==
                UnityWebRequest.Result.ProtocolError)
                Debug.Log("Error : " + request.error);
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Response : " + responseText);

                var responseData = JsonUtility.FromJson<OpenAIResponse>(responseText);
                if (responseData.choices != null && responseData.choices.Length > 0)
                {
                    string assistantMessage = responseData.choices[0].message.content;
                    // resultText.text = assistantMessage;
                    // OnReceivedMessage?.Invoke();
                    onResponseOpenAI?.Invoke(assistantMessage);
                }
                else
                    Debug.LogWarning("No valid response from the assistant");
            }
        }
    }

    private void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.onInputFieldSubmit -= OnInputFieldCompleted;
        }
    }
}

[System.Serializable]
public class OpenAIResponse
{
    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public Message message;
}

[System.Serializable]
public class Message
{
    public string content;
}
