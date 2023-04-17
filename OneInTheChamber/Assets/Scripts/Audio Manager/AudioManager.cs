using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public List<AudioClip> sfxClips = new List<AudioClip>();
    public List<MusicTrack> musicTracks = new List<MusicTrack>();

    private AudioSource musicSource;

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
        musicSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        
    }

    public void PlaySFX(string name)
    {
        foreach (AudioClip clip in sfxClips)
        {
            if (clip.name == name)
            {
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
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
                musicSource.Play();
            }
        }
    }


}

[Serializable]
public class MusicTrack
{
    public string sceneName;
    public AudioClip audioClip;
}