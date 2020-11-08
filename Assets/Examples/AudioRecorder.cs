using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioRecorder : MonoBehaviour
{
    public AudioSource audioSource;
    public int duration = 8;

    void Start()
    {
        audioSource.clip = Microphone.Start(string.Empty, audioSource.loop, duration, AudioSettings.outputSampleRate);
        audioSource.Play();
    }

}
