using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public List<SoundEffect> soundEffects = new List<SoundEffect>();
    public List<MusicTrack> musicTracks = new List<MusicTrack>();

    public AudioSource sfxSource;
    public AudioSource musicSource;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (MusicTrack musicTrack in musicTracks)
        {
            if (musicTrack.sceneName.Equals(scene.name))
            {
                if (musicSource.clip == musicTrack.audioClip)
                {
                    return;
                }
                musicSource.Stop();
                musicSource.clip = musicTrack.audioClip;
                musicSource.volume = musicTrack.volume;
                musicSource.Play();
            }
        }
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