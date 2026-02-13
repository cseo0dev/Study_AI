using System;
using System.IO;
using UnityEngine;

public class SaveWav : MonoBehaviour
{
    const int HEADER_SIZE = 44;

    public static bool Save(string path, AudioClip clip, float minThreshold = 0.01f)
    {
        if (!path.EndsWith(".wav", System.StringComparison.OrdinalIgnoreCase))
            path += ".wav";

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        AudioClip trimmed = TrimSilence(clip, minThreshold);

        if (trimmed == null)
        {
            Debug.Log("저장할 오디오가 모두 무음입니다.");
            return false;
        }

        using (FileStream fs = CreateEmpty(path))
        {
            ConvertAndWrite(fs, trimmed);
            WriteHeader(fs, trimmed);
        }

        return true;
    }

    /// <summary>
    /// [최적화 작업]
    /// 오디오클립에서 지정된 임계값 이하의 음량(무음) 부분을 제거하는 작업
    /// </summary>
    static AudioClip TrimSilence(AudioClip clip, float threshold)
    {
        // 오디오 데이터 가져오기
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // 오디오의 시작점과 종료점 찾기
        int start = 0, end = samples.Length - 1;

        for (int i = 0; i < samples.Length; i++)
        {
            if (Mathf.Abs(samples[i]) > threshold)
            {
                start = i;
                break;
            }
        }

        for (int i = samples.Length - 1; i >= 0; i--)
        {
            if (Mathf.Abs(samples[i]) > threshold)
            {
                end = i;
                break;
            }
        }

        // 유효한 길이의 데이터가 존재하지않으면 무음으로 처리
        int len = end - start + 1;
        if (len <= 0)
            return null;

        // 무음이 제거된 버전으로 데이터 저장
        float[] trimmedSamples = new float[len];
        Array.Copy(samples, start, trimmedSamples, 0, len);
        AudioClip tClip = AudioClip.Create(clip.name + "_trimmed", len / clip.channels, clip.channels, clip.frequency, false);
        tClip.SetData(trimmedSamples, 0);
        return tClip;
    }

    /// <summary>
    /// 빈 파일 생성
    /// WAV 파일 헤더 영역에 임시로 0을 채워 넣어 헤더 자리 공간 확보
    /// </summary>
    static FileStream CreateEmpty(string path)
    {
        FileStream fs = new FileStream(path, FileMode.Create);
        for (int i = 0; i < HEADER_SIZE; i++)
            fs.WriteByte(0);
        return fs;
    }

    /// <summary>
    /// 음성 데이터를 WAV 형식에 맞는 바이트 배열로 변환
    /// </summary>
    static void ConvertAndWrite(FileStream fs, AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] bytes = new byte[samples.Length * 2];
        const int rescale = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            short s = (short)(samples[i] * rescale);
            Array.Copy(BitConverter.GetBytes(s), 0, bytes, i * 2, 2);
        }
        fs.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// WAV 파일의 헤더를 작성하여 오디오 데이터의 메타 데이터를 저장
    /// </summary>
    static void WriteHeader(FileStream fs, AudioClip clip)
    {
        int hz = clip.frequency, channels = clip.channels, samples = clip.samples;
        fs.Seek(0, SeekOrigin.Begin);

        void WriteStr(string s)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
            fs.Write(bytes, 0, bytes.Length);
        }

        WriteStr("RIFF");
        fs.Write(BitConverter.GetBytes((int)fs.Length - 8), 0, 4);
        WriteStr("WAVEfmt ");
        fs.Write(BitConverter.GetBytes(16), 0, 4);
        fs.Write(BitConverter.GetBytes((ushort)1), 0, 2);
        fs.Write(BitConverter.GetBytes((ushort)channels), 0, 2);
        fs.Write(BitConverter.GetBytes(hz), 0, 4);
        fs.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4);
        fs.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2);
        fs.Write(BitConverter.GetBytes((ushort)16), 0, 2);
        WriteStr("data");
        fs.Write(BitConverter.GetBytes(samples * channels * 2), 0, 4);
    }
}
