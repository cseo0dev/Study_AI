using System.Collections.Generic;
using UnityEngine;

public class PhonemeMappingLipSync : MonoBehaviour
{
    /// <summary>
    /// 직렬화 클래스 선언
    /// 음성 데이터를 분석하여 특정 블렌디셰이프와 연결하는 역할
    /// </summary>
    [System.Serializable]
    public class PhonemeMapping
    {
        public string phoneme; // 음소
        public string blendShapeName; // 블랜더 이름
        public float frequencyThreshold; // 주파수 대역 임계값
        public float maxWeight = 100f; // 블랜드셰이프 가중치
    }

    [Header("Analyze Audio Settings")]
    public float fftResolution = 512; // fft 성능
    public float smoothingSpeed = 3f; // 블렌더셰이프 변환 부드럽게
    public float volumeSensitivity = 50f; // 볼륨 감지 민감도
    public float minVolume = 0.01f; // 최저 볼륨 감지 임계값

    [Header("Animation Settings")]
    public SkinnedMeshRenderer faceMesh; // 표정 애니메이션 타겟
    public PhonemeMapping[] phonemeConfigs; // 표정에 적용할 PhonemeMapping 클래스 배열

    private AudioSource audioSource;
    private Dictionary<string, int> blendShapeIndexMap = new Dictionary<string, int>();
    private float[] currentWeights;
    private float[] spectrumData;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.Play();

        // fftResolution값을 정수로 변환하여 spcrtrumData 배열에 저장
        // spcetrumData 배열을 활용하여 오디오의 특정 주파수 대역을 감지하고 이를 애니메이션과 연결
        spectrumData = new float[(int)fftResolution];

        // 블랜드셰이프 인덱스 매핑 초기화
        for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
        {
            string name = faceMesh.sharedMesh.GetBlendShapeName(i);
            blendShapeIndexMap[name] = i;
            Debug.Log($"BlendShape Index {i}, {name}");
        }

        currentWeights = new float[faceMesh.sharedMesh.blendShapeCount];
    }

    /// <summary>
    /// NPC 애니메이션 실시간 조절
    /// </summary>
    private void LateUpdate()
    {
        // 오디오가 없거나 멈춰 있는 경우
        if (!audioSource.isPlaying || audioSource.clip == null)
        {
            ResetBlendShapes(); // 블렌더셰이프 값 초기화
            return;
        }

        AnalyzeAudio(); // 오디오 데이터 분석
        UpdateBlendShapes(); // 분석 결과를 블렌드셰이프에 적용
    }

    /// <summary>
    /// AudioSource에 등록된 오디오 데이터를 분석하여 NPC의 입 모양을 실시간으로 조절하는 함수
    /// 오디오 주파수 데이터를 FFT로 분석하고 매 프레임마다
    /// 특정 주파수에 반응하는 블렌드셰이프 활성화
    /// </summary>
    private void AnalyzeAudio()
    {
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        // 볼륨 계산 (선형 방식)
        float rms = CalculateRMS(audioSource);
        float linearVolume = Mathf.Clamp(rms * 100f, 0f, 100f);

        if (linearVolume < minVolume)
        {
            ResetBlendShapeWeights();
            return;
        }

        foreach (var config in phonemeConfigs)
        {
            if (!blendShapeIndexMap.TryGetValue(config.blendShapeName, out int targetIndex))
            {
                Debug.LogError($"BlendShape not found : {config.blendShapeName}");
                continue;
            }

            float freqValue = GetFrequencyRangeValue(config.frequencyThreshold);
            float targetWeight = Mathf.Clamp(
                freqValue * linearVolume * volumeSensitivity,
                0,
                config.maxWeight
                );

            currentWeights[targetIndex] = Mathf.Lerp(
                currentWeights[targetIndex],
                targetWeight,
                Time.deltaTime * smoothingSpeed
                );
        }
    }

    /// <summary>
    /// 재생 중인 오디오의 볼륨 측정
    /// RMS 방식을 사용하여 음원의 평균 볼륨을 꼐산하고 이 값으로 NPC의 입모양 크기 조절
    /// </summary>
    private float CalculateRMS(AudioSource source)
    {
        float[] samples = new float[1024];
        source.GetOutputData(samples, 0);
        float sum = 0f;

        foreach (float s in samples) // 제곱합 구하기
            sum += s * s;

        float rms = Mathf.Sqrt(sum / samples.Length); // 제곱근 취하기

        return Mathf.Max(rms, 0.0001f); // RMS 값이 너무 작아 0에 수렴하는 경우 방지
    }

    /// <summary>
    /// NPC 입 모양 애니메이션 조절
    /// </summary>
    private float GetFrequencyRangeValue(float targetFreq)
    {
        int bin = Mathf.FloorToInt(targetFreq * fftResolution / AudioSettings.outputSampleRate);
        bin = Mathf.Clamp(bin, 0, spectrumData.Length - 1);

        return spectrumData[bin] * 1000f;
    }

    private void ResetBlendShapes()
    {
        ResetBlendShapeWeights();
        UpdateBlendShapes();
    }

    private void ResetBlendShapeWeights()
    {
        for (int i = 0; i < currentWeights.Length; i++)
            currentWeights[i] = 0f;
    }

    private void UpdateBlendShapes()
    {
        for (int i = 0; i < currentWeights.Length; i++)
            faceMesh.SetBlendShapeWeight(i, currentWeights[i]);
    }

    /// <summary>
    /// 주파수 스펙트럼 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (spectrumData == null)
            return;

        for (int i = 0; i < spectrumData.Length; i++)
        {
            float height = spectrumData[i] * 1000;
            Gizmos.color = Color.Lerp(Color.blue, Color.red, height / 10f);
            Gizmos.DrawCube(new Vector3(i * 0.1f, height / 2, 0), new Vector3(0.05f, height, 0.05f));
        }
    }
}
