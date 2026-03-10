using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;
    public Animator anim;

    public GameObject balloon; //말풍선 프리팹

    private bool isListening = false;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 객체 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager.Instance is null");
            return;
        }

        if (OpenAITTS.Instance == null)
        {
            Debug.LogError("OpenAITTS.Instance is null");
            return;
        }

        // 입력 발생 시 실행되는 이벤트
        UIManager.Instance.inputField.onValueChanged.AddListener(OnInputFieldChanged);

        // 입력 완료 시 실행되는 이벤트
        UIManager.Instance.onInputFieldSubmit += OnInputFieldSubmit;

        // TTS
        OpenAITTS.Instance.onResponseTTS += StartTalking;

        OpenAITTS.Instance.onStopAudio += StopTalking;

        //WhisperManager.Instance.OnStartRecording += OnInputFieldChanged; // 녹음 시작 시 Listen Anim 호출
        //WhisperManager.Instance.OnStopRecording += OnInputFieldSubmit; // 녹음 중지 시 Think Anim 호출

        //OpenAIManager.Instance.OnReceivedMessage += StartTalk; // Talk Anim 호출
    }

    // InputField에서 입력이 발생했을 때 호출되는 메서드
    private void OnInputFieldChanged(string inputText)
    {
        // 애니메이션 트리거
        if (!isListening)
        {
            // 애니메이터에 "listening" 트리거 설정
            anim.SetTrigger("listen");
            isListening = true; // 플래그 설정
            Debug.Log("Animator Triggered: listen");
        }
    }

    // InputField에서 입력이 완료되었을 때 호출되는 메서드    
    private void OnInputFieldSubmit(string inputText)
    {
        balloon.SetActive(true);
        isListening = false;
    }

    private void StartTalking(string message)
    {
        UIManager.Instance.resultText.gameObject.SetActive(true);
        balloon.SetActive(false);
        anim.SetTrigger("talk");
    }

    private void StopTalking()
    {
        anim.SetTrigger("idle");
        UIManager.Instance.resultText.gameObject.SetActive(false);
    }

    //private void StartTalk()
    //{
    //    StartCoroutine(TalkThenIdle());
    //}

    //public IEnumerator TalkThenIdle()
    //{
    //    balloon.SetActive(false);
    //    anim.SetTrigger("talk");
    //    yield return new WaitForSeconds(5f);
    //    anim.SetTrigger("idle");
    //}

    private void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.onInputFieldSubmit -= OnInputFieldSubmit;
            UIManager.Instance.inputField.onValueChanged.RemoveListener(OnInputFieldChanged);
        }

        if (OpenAITTS.Instance != null)
        {
            OpenAITTS.Instance.onResponseTTS -= StartTalking;
            OpenAITTS.Instance.onStopAudio -= StopTalking;
        }
    }
}
