using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("No AudioManager found in the scene.");
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    public List<SoundEffect> soundEffects = new List<SoundEffect>();
    public List<MusicTrack> musicTracks = new List<MusicTrack>();

    public AudioSource sfxSource;
    public AudioSource musicSource;
    public AudioSource slideSource;

    private AudioLowPassFilter musicLowPassFilter;
    private float musicVolume;
    private float slideVolume;

    public void Awake()
    {
        if (_instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        musicLowPassFilter = musicSource.gameObject.GetComponent<AudioLowPassFilter>();
        musicVolume = musicSource.volume;
        slideVolume = slideSource.volume;
        slideSource.volume = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void PlaySFX(string name)
    {
        foreach (SoundEffect soundEffect in soundEffects)
        {
            if (soundEffect.audioClip.name.Equals(name))
            {
                sfxSource.PlayOneShot(soundEffect.audioClip, soundEffect.volume);
            }
        }
    }

    public void EnableSlide()
    {
        StopCoroutine("SlideLevel");
        StartCoroutine(SlideLevel(slideSource.volume, slideVolume, 0.1f));
    }

    public void DisableSlide()
    {
        StopCoroutine("SlideLevel");
        StartCoroutine(SlideLevel(slideSource.volume, 0f, 0.1f));
    }

    private IEnumerator SlideLevel(float startVolume, float targetVolume, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            slideSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        slideSource.volume = targetVolume;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (MusicTrack musicTrack in musicTracks)
        {
            if (musicTrack.sceneName.Equals(scene.name))
            {
                if (musicSource != null && musicSource.clip == musicTrack.audioClip)
                {
                    StopCoroutine("SweepLPF");
                    musicLowPassFilter.cutoffFrequency = 22000f;
                    return;
                }
                musicSource.Stop();
                musicSource.clip = musicTrack.audioClip;
                musicSource.volume = musicTrack.volume * musicVolume;
                StartCoroutine(SweepLPF(10f, 22000f, 2f));
                musicSource.Play();
                return;
            }
        }
        Debug.LogWarning("No music track found for scene " + scene.name);
        StopCoroutine("SweepLPF");
        musicLowPassFilter.cutoffFrequency = 22000f;
        return;
    }
    public IEnumerator SweepLPF(float startFrequency, float targetFrequency, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            musicLowPassFilter.cutoffFrequency = Mathf.Lerp(startFrequency, targetFrequency, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        musicLowPassFilter.cutoffFrequency = targetFrequency;
    }
}

[Serializable]
public class SoundEffect
{
    public AudioClip audioClip;
    public float volume = 1f;
}

[Serializable]
public class MusicTrack
{
    public string sceneName;
    public AudioClip audioClip;
    public float volume = 1f;
}