using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SetMicrophone : MonoBehaviour
{
    public Dropdown dropdown; // 마이크 목록을 표시할 드롭다운
    public string currentMicrophone; // 현재 선택된 마이크

    void Start()
    {
        SetDeviceMicrophone();
    }

    void SetDeviceMicrophone()
    {
        // 디바이스에 연결된 마이크 목록 가져오기
        string[] microphones = Microphone.devices;

        // 기존 드롭다운 옵션 초기화
        dropdown.ClearOptions();

        // 마이크 목록을 드롭다운 형식으로 변환
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        foreach (string device in microphones)
        {
            options.Add(new Dropdown.OptionData(device));
        }

        // 변환된 옵션을 드롭다운에 추가
        dropdown.AddOptions(options);

        // currentMicrophone에 첫번째 마이크 지정
        if (microphones.Length > 0)
        {
            currentMicrophone = microphones[0];
            dropdown.value = 0;
        }
        else
            currentMicrophone = null;

        // 드롭다운의 값이 변경될 때마다 호출되는 이벤트
        dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(dropdown); });
    }

    void DropdownValueChanged (Dropdown change)
    {
        // 현재 선텍된 옵션의 텍스트를 currentMicrophone에 저장
        currentMicrophone = change.options[change.value].text;
        Debug.Log(currentMicrophone);
    }
}
