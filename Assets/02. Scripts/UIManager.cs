using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public InputField inputField;

    public Text resultText;

    public event Action<string> onInputFieldSubmit;  // InputField 텍스트 완료 이벤트

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
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter))
            return;

        if (!string.IsNullOrWhiteSpace(inputText))
        {
            onInputFieldSubmit?.Invoke(inputText);
            inputField.text = "";
            inputField.ActivateInputField();
        }
        else
        {
            Debug.LogWarning("Input field is empty or null");
        }
    }

    private void OnResponseOpenAI(string message)
    {
        resultText.text = message;
    }
}
