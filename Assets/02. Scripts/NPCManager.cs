using System.Collections;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;
    public Animator anim;

    public GameObject balloon; //말풍선 프리팹

    void Start()
    {
        // 입력 발생 시 실행되는 이벤트
        UIManager.Instance.inputField.onValueChanged.AddListener(OnInputFieldChanged);

        // 입력 완료 시 실행되는 이벤트
        UIManager.Instance.inputField.onSubmit.AddListener(OnInputFieldSubmit);
    }

    private void OnInputFieldChanged(string inputText)
    {
        // 애니메이션 트리거
        anim.SetTrigger("listen");
        Debug.Log("OnInputFieldEdited");
    }

    private void OnInputFieldSubmit(string inputText)
    {
        balloon.SetActive(true);
    }

    public IEnumerator TalkThenIdle()
    {
        balloon.SetActive(false);
        anim.SetTrigger("talk");
        yield return new WaitForSeconds(5f);
        anim.SetTrigger("idle");
    }
}
