using System;
using System.Collections;
using System.Text;
using Unity.Multiplayer.Center.Common;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class OpenAIManager : MonoBehaviour
{
    private const string apiUrl = "https://api.openai.com/v1/chat/completions";
    private const string apiKey = "";

    public static OpenAIManager Instance;

    // 액션 추가
    public event Action OnReceivedMessage;

    // NPC 설정 추가
    public string currentPrompt = "당신은 NPC 캐릭터 RobotKyle입니다. 질문에 답해주세요." + "특히 철자 오류를 교정하세요" + "Chat gpt";
    public Text uiText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        WhisperManager.Instance.OnReceivedWhisper += RecievedWhisper;
    }

    private void RecievedWhisper(string transcribedText)
    {
        StartCoroutine(SendOpenAIRequest(currentPrompt, transcribedText, uiText));
    }

    public IEnumerator SendOpenAIRequest(string prompt, string message, Text resultText)
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

            // ���� �ڵ鸵
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result ==
                UnityWebRequest.Result.ProtocolError)
                Debug.Log("Error : " + request.error);
            else
            {
                // ���� ó��
                string responseText = request.downloadHandler.text;
                Debug.Log("Response : " + responseText);

                // ���� �����Ϳ��� assistant �޼��� ����
                var responseData = JsonUtility.FromJson<OpenAIResponse>(responseText);
                if (responseData.choices != null && responseData.choices.Length > 0)
                {
                    string assistantMessage = responseData.choices[0].message.content;
                    resultText.text = assistantMessage;
                    OnReceivedMessage?.Invoke();
                }
                else
                    Debug.LogWarning("No valid response from the assistant");
            }
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
