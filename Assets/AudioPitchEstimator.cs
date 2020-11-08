using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SRH (Summation of Residual Harmonics) による基本周波数推定
// T. Drugman and A. Alwan: "Joint Robust Voicing Detection and Pitch Estimation Based on Residual Harmonics", Interspeech'11, 2011.

public class AudioPitchEstimator : MonoBehaviour
{
    [Tooltip("最低周波数 [Hz]")]
    [Range(40, 150)]
    public int frequencyMin = 40;

    [Tooltip("最高周波数 [Hz]")]
    [Range(300, 1200)]
    public int frequencyMax = 600;

    [Tooltip("推定に利用する倍音の個数")]
    [Range(1, 8)]
    public int harmonicsToUse = 5;

    [Tooltip("スペクトルの移動平均バンド幅 [Hz]\n幅が大きいほど滑らかになりますが、精度が下がります")]
    public float smoothingWidth = 500;

    [Tooltip("有声音判定のしきい値\n大きな値ほど判定が厳しくなります")]
    public float thresholdSRH = 7;

    const int spectrumSize = 1024;
    const int outputResolution = 200; // SRHの周波数軸の要素数（小さくすると計算負荷が下がる）
    float[] spectrum = new float[spectrumSize];
    float[] specRaw = new float[spectrumSize];
    float[] specCum = new float[spectrumSize];
    float[] specRes = new float[spectrumSize];
    float[] srh = new float[outputResolution];

    public List<float> SRH => new List<float>(srh);

    /// <summary>
    /// 基本周波数を推定します
    /// </summary>
    /// <param name="audioSource">入力音源</param>
    /// <returns>基本周波数[Hz] (存在しないときfloat.NaN)</returns>
    public float Estimate(AudioSource audioSource)
    {
        var nyquistFreq = AudioSettings.outputSampleRate / 2.0f;

        // オーディオスペクトルを取得
        if (!audioSource.isPlaying) return float.NaN;
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Hanning);

        // 振幅スペクトルの対数を計算
        // 以降のスペクトルはすべて対数振幅で扱う（ここは元論文と異なる）
        for (int i = 0; i < spectrumSize; i++)
        {
            // 振幅ゼロのとき-∞になってしまうので小さな値を足しておく
            specRaw[i] = Mathf.Log(spectrum[i] + 1e-9f);
        }

        // スペクトルの累積和（あとで使う）
        specCum[0] = 0;
        for (int i = 1; i < spectrumSize; i++)
        {
            specCum[i] = specCum[i - 1] + specRaw[i];
        }

        // 残差スペクトルを計算
        var halfRange = Mathf.RoundToInt((smoothingWidth / 2) / nyquistFreq * spectrumSize);
        for (int i = 0; i < spectrumSize; i++)
        {
            // スペクトルを滑らかに（累積和を使って移動平均）
            var indexUpper = Mathf.Min(i + halfRange, spectrumSize - 1);
            var indexLower = Mathf.Max(i - halfRange + 1, 0);
            var upper = specCum[indexUpper];
            var lower = specCum[indexLower];
            var smoothed = (upper - lower) / (indexUpper - indexLower);

            // 元のスペクトルから滑らかな成分を除去
            specRes[i] = specRaw[i] - smoothed;
        }

        // SRH (Summation of Residual Harmonics) のスコアを計算
        float bestFreq = 0, bestSRH = 0;
        for (int i = 0; i < outputResolution; i++)
        {
            var currentFreq = (float)i / (outputResolution - 1) * (frequencyMax - frequencyMin) + frequencyMin;

            // 現在の周波数におけるSRHのスコアを計算: 論文の式(1)
            var currentSRH = GetSpectrumAmplitude(specRes, currentFreq, nyquistFreq);
            for (int h = 2; h <= harmonicsToUse; h++)
            {
                // h倍の周波数では、信号が強いほど良い
                currentSRH += GetSpectrumAmplitude(specRes, currentFreq * h, nyquistFreq);

                // h-1倍 と h倍 の中間の周波数では、信号が強いほど悪い
                currentSRH -= GetSpectrumAmplitude(specRes, currentFreq * (h - 0.5f), nyquistFreq);
            }
            srh[i] = currentSRH;

            // スコアが最も大きい周波数を記録
            if (currentSRH > bestSRH)
            {
                bestFreq = currentFreq;
                bestSRH = currentSRH;
            }
        }

        // SRHのスコアが閾値に満たない → 明確な基本周波数が存在しないとみなす
        if (bestSRH < thresholdSRH) return float.NaN;

        return bestFreq;
    }

    // スペクトルデータからfrequency[Hz]における振幅を取得する
    float GetSpectrumAmplitude(float[] spec, float frequency, float nyquistFreq)
    {
        var position = frequency / nyquistFreq * spec.Length;
        var index0 = (int)position;
        var index1 = index0 + 1; // 配列の境界チェックは省略
        var delta = position - index0;
        return (1 - delta) * spec[index0] + delta * spec[index1];
    }

}
