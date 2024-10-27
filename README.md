# Audio Pitch Estimator for Unity

[日本語](README.ja.md)

A simple C# script to estimate **fundamental frequency** from Unity's AudioSource component.

Just copy [AudioPitchEstimator.cs](./Assets/AudioPitchEstimator.cs) to your Asset directory. This script doesn't require any external dependencies except for built-in Unity components.

Originally I made it for speech and singing, but it may be applied to some single-note instrument sounds (e.g. guitar tuner).

## Demo

Movie: [Pitch estimation with Unity](https://twitter.com/tokaipist_game/status/1327032060318347267) (in Japanese)

This repository has 2 examples:

- `Assets/Examples/Example1.unity`
  - Estimation with **audio file**.
  - Includes sample audio (my singing of a public domain song).
- `Assets/Examples/Example2.unity`
  - Estimation with **real-time audio from microphone**.
  - Note: There is a delay of ~1 second because the audio signal is buffered to the AudioSource before estimation.

## Usage

Please attach the `AudioPitchEstimator.cs` to your GameObject.

There are several parameters. You should set these parameters according to the typical characteristics of your input audio signals (e.g. the range of the voice).

![inspector](./readme/inspector.png)

- `Frequency Min`: The lowest frequency that can estimate.
- `Frequency Max`: The highest frequency that can estimate.
- `Harmonics To Use`: Number of overtones to use for estimation.
- `Smoothing Width`: Frequency bandwidth of spectral smoothing filter.
- `Threshold SRH`: Threshold to judge silence or not.

The boundary frequencies `Frequency Min` and `Frequency Max` should be set as nessesary and sufficient range. For example, if the fundamental frequency never exceeds 600 Hz (in your application), set `Frequency Max` to 600 Hz or above.

### Tuning Tips

#### Quickly switches between two frequencies

Decreasing `Frequency Max` will fix the problem by eliminating unwanted high-frequency candidates.

#### Misidentifies silence at the tail of your speech

Increasing `Threshold SRH` may help.
If it does not change, try tweaking `Harmonics To Use` and `Smoothing Width`.

#### Sometimes doesn't detect your speech

Decreasing `Threshold SRH` may help.
If it does not change, try tweaking `Harmonics To Use` and `Smoothing Width`.

#### CPU load is too high

It is effective to decrease your estimation update rate (e.g. lower the `PitchVisualizer.estimateRate` to `8`).

## Example Code

Audio data can be obtained via **AudioSource**.
If you want to use the audio from the microphones, please use [Microphone.Start()](https://docs.unity3d.com/ja/current/ScriptReference/Microphone.Start.html), a built-in method in Unity.

`AudioPitchEstimator.Estimate()` takes an argument for target AudioSource and will return the estimate of fundamental frequency in the input audio signal.

```cs
void EstimatePitch()
{
    var estimator = this.GetComponent<AudioPitchEstimator>();
    var audioSource = this.GetComponent<AudioSource>();

    // Estimates fundamental frequency from AudioSource output.
    float frequency = estimator.Estimate(audioSource);

    if (float.IsNaN(frequency))
    {
        // Algorithm didn't detect fundamental frequency (e.g. silence).
    }
    else
    {
        // Algorithm detected fundamental frequency.
        // The frequency is stored in the variable `frequency` (in Hz).
    }
}

void Update()
{
    // It is NOT recommended to run the estimation every frame.
    // This will take a high computational load.
    // EstimatePitch();
}

void Start()
{
    // It is recommended to estimate at appropriate time intervals.
    // This example runs every 0.1 seconds.
    InvokeRepeating("EstimatePitch", 0, 0.1f);
}
```

## Algorithm

This package implements a modified version of SRH (Summation of Residual Harmonics) [1], a spectrum-based pitch estimation algorithm.

Please see the original paper [1] and [AudioPitchEstimator.cs](./Assets/AudioPitchEstimator.cs) for details.

## License

The Unlicense. For the details, see [LICENSE](./LICENSE).

## References

[1] T. Drugman and A. Alwan: "[Joint Robust Voicing Detection and Pitch Estimation Based on Residual Harmonics](https://arxiv.org/abs/2001.00459)", Interspeech'11, 2011.
