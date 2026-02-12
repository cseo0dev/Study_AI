using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public InputField inputField;

    public Text resultText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
    }

    private void OnInputFieldEndEdit(string inputText)
    {
        if (!string.IsNullOrEmpty(inputText))
        {
            StartCoroutine(OpenAIManager.Instance.SendOpenAIRequest("Answer any question.", inputText, resultText));
            inputField.text = "";
        }
        else
        {
            Debug.LogWarning("Input field is empty or null");
        }
    }
}
