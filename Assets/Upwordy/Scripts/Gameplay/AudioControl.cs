using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioType { 
    Sound
}

[RequireComponent(typeof(AudioSource))]
public class AudioControl : MonoBehaviour
{
    [SerializeField] private AudioType audioType;
    [SerializeField] private AudioSource audioSource;

    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        switch (audioType) {
            case AudioType.Sound:
                GameManager.OnSoundVolumeChanged += SetVolume;
                SetVolume(GameManager.soundVolume);
                break;
        }
    }

    private void SetVolume(float volume) {
        audioSource.volume = volume;
    }

    private void OnDestroy()
    {
        switch (audioType)
        {
            case AudioType.Sound:
                GameManager.OnSoundVolumeChanged -= SetVolume;
                break;
        }
    }
}