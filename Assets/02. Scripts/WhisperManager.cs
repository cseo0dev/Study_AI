using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System;
using Unity.VisualScripting;

public class WhisperManager : MonoBehaviour
{
    public Toggle recordToggle;
    private AudioClip clip;
    private SetMicrophone setMicrophoneScript;

    private bool isRecording = false;
    private int duration = 300; // 최대 녹음 시간
    private string fileName = "recordedAudio.wav";

    private string url = "https://api.openai.com/v1/audio/transcriptions";
    public string apiKey;

    public event Action<string> OnReceivedWhisper;
    public event Action OnStartRecording;
    public event Action OnStopRecording;

    /// <summary>
    /// JSON 파싱을 위해 text를 저장할 수 있는 구조 생성
    /// </summary>
    [Serializable]
    public class WhisperResponse
    {
        public string text;
    }

    public static WhisperManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    void Start()
    {
        // 씬에서 SetMicrophone 스크립트 찾기
        setMicrophoneScript = FindAnyObjectByType<SetMicrophone>();
        if (setMicrophoneScript == null)
            Debug.LogError("SetMicrophone 스크립트를 찾을 수 없습니다.");

        recordToggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    void StartRecording()
    {
        if (setMicrophoneScript == null || string.IsNullOrEmpty(setMicrophoneScript.currentMicrophone))
        {
            Debug.LogError("선택된 마이크가 없습니다.");
            return;
        }

        if (!isRecording)
        {
            // 선택된 마이크를 통해서 녹음 ㅅ ㅣ작
            clip = Microphone.Start(setMicrophoneScript.currentMicrophone, false, duration, 44100);
            isRecording = true;
            Debug.Log($"녹음을 시작합니다: {setMicrophoneScript.currentMicrophone}");
            OnStartRecording?.Invoke();
        }
    }

    void StopRecording()
    {
        if (isRecording)
        {
            // 녹음 중지
            Microphone.End(setMicrophoneScript.currentMicrophone);
            isRecording= false;
            Debug.Log("녹음을 중지하였습니다.");

            // 오디오 클립 저장
            SaveClip();
            OnStopRecording?.Invoke();
        }
    }

    void SaveClip()
    {
        if (clip != null)
        {
            var filePath = Path.Combine(Application.persistentDataPath, fileName);
            SaveWav.Save(filePath, clip);

            Debug.Log($"녹음이 저장되었습니다: {filePath}");

            // Whisper API에 오디오 파일을 전송하여 텍스트 받기
            StartCoroutine(SendWhisperRequest(filePath));
        }
        else
            Debug.LogError("저장할 오디오 클립이 없습니다.");
    }

    IEnumerator SendWhisperRequest(string filepath)
    {
        // 오디오 파일을 바이트 배열로 읽어오기
        byte[] audioData = File.ReadAllBytes(filepath);

        // Multipart form 데이터 생성
        WWWForm form = new WWWForm();
        form.AddField("model", "whisper-1");
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");

        // 요청 생성
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // 응답 대기
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // 응답 처리
            string responseText = request.downloadHandler.text;
            Debug.Log("Whisper API 응답 : " + responseText);

            // JSON 파싱을 통해 텍스트 추출
            try
            {
                var jsonResponse = JsonUtility.FromJson<WhisperResponse>(responseText);
                string transcribedText = jsonResponse.text;
                Debug.Log("인식된 텍스트 : " + transcribedText);
                OnReceivedWhisper?.Invoke(transcribedText);
            }
            catch (Exception e)
            {
                Debug.LogError("JSON 파싱 오류 : " + e.Message);
            }
        }
        else 
            Debug.LogError("Whisper API 요청 실패 : " + request.error);
    }
}
