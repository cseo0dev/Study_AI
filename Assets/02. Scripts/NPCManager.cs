using System.Collections;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;
    public Animator anim;

    public GameObject balloon; //말풍선 프리팹

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
        //// 입력 발생 시 실행되는 이벤트
        //UIManager.Instance.inputField.onValueChanged.AddListener(OnInputFieldChanged);

        //// 입력 완료 시 실행되는 이벤트
        //UIManager.Instance.inputField.onSubmit.AddListener(OnInputFieldSubmit);

        WhisperManager.Instance.OnStartRecording += OnInputFieldChanged; // 녹음 시작 시 Listen Anim 호출
        WhisperManager.Instance.OnStopRecording += OnInputFieldSubmit; // 녹음 중지 시 Think Anim 호출

        OpenAIManager.Instance.OnReceivedMessage += StartTalk; // Talk Anim 호출
    }

    private void OnInputFieldChanged()
    {
        // 애니메이션 트리거
        anim.SetTrigger("listen");
        Debug.Log("OnInputFieldEdited");
    }

    private void OnInputFieldSubmit()
    {
        balloon.SetActive(true);
        anim.SetTrigger("think");
    }

    private void StartTalk()
    {
        StartCoroutine(TalkThenIdle());
    }

    public IEnumerator TalkThenIdle()
    {
        balloon.SetActive(false);
        anim.SetTrigger("talk");
        yield return new WaitForSeconds(5f);
        anim.SetTrigger("idle");
    }
}
