# Audio Pitch Estimator for Unity

A simple C# script to estimate the **fundamental frequency** from AudioSource.

This script doesn't require any external dependencies. Just copy [AudioPitchEstimator.cs](./Assets/AudioPitchEstimator.cs) on your Asset directory.

## Demo

Movie: [Pitch estimation with Unity](https://twitter.com/tokaipist_game/status/1327032060318347267) (in Japanese)

This repository has 2 examples:

- `Assets/Examples/Example1.unity`
  - Estimation with **audio file**.
  - Includes sample audio with clear rights (my singing of a public domain song).
- `Assets/Examples/Example2.unity`
  - Estimation with **real-time audio from microphone**.
  - Note: There is a delay of ~1 second because the audio signal is buffered to the AudioSource before estimation.

## Usage

Please attach the `AudioPitchEstimator.cs` to your GameObject.

There are several parameters:

![inspector](./readme/inspector.png)

- **Frequency Min**: The lowest frequency that can estimate.
- **Frequency Max**: The highest frequency that can estimate.
- **Harmonics To Use**: Number of overtones.
- **Smoothing Width**: Frequency bandwidth of spectral smoothing filter.
- **Threshold SRH**: Threshold to determine if . The larger value, the stricter the judge.

### Tuning FAQ

#### Frequency estimation results can easily get jittery / Quickly switches between two frequencies

Lowering **Frequency Max** will improve it by eliminating unwanted high-frequency candidates.

#### Easily misidentified at the tail of the speech

Stricter **Threshold SRH** (Silence Judgment) may improve it.
If this does not help, try tweaking **Harmonics To Use** and **Smoothing Width**.

#### Sometimes the voice is not detected

The silence judgment is too strict. Please lower the value of **Threshold SRH**.
If this does not help, try tweaking **Harmonics To Use** and **Smoothing Width**.

#### Computational load is too high

It is effective to increase the time interval of estimation (ex. lower the `EstimateRate` of `PitchVisualizer.cs` to `8`).

## Example Code

Audio data can be obtained via **AudioSource**.
If you want to use the audio from the microphones, please use [Microphone.Start()](https://docs.unity3d.com/ja/current/ScriptReference/Microphone.Start.html), a Unity built-in function.

`AudioPitchEstimator.Estimate()` takes AudioSource as a parameter and will return an estimate of the fundamental frequency.

```cs
void EstimatePitch()
{
    var estimator = this.GetComponent<AudioPitchEstimator>();
    var audioSource = this.GetComponent<AudioSource>();

    // Estimates fundamental frequency from AudioSource output.
    float frequency = estimator.Estimate(audioSource);

    if (float.IsNaN(frequency))
    {
        // Algorithm didn't detect fundamental frequency (ex. silence).
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

This code implements a modified version of SRH (Summation of Residual Harmonics) [1].

Please see the paper [1] and [Assets/AudioPitchEstimator.cs](./Assets/AudioPitchEstimator.cs) for details.

## License

The Unlicense. For the details, see [LICENSE](./LICENSE).

## References

[1] T. Drugman and A. Alwan: "[Joint Robust Voicing Detection and Pitch Estimation Based on Residual Harmonics](https://arxiv.org/abs/2001.00459)", Interspeech'11, 2011.
