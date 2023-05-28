using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SFXManager : MonoBehaviour {
    public static SFXManager Instance;

    [Tooltip("A list of all the audio sources to play the sounds on.")]
    public List<AudioSource> Sources;
    [HideInInspector]
    public bool Muted;

    [SerializeField]
    private bool dontDestroyOnLoad;

    private int curAS;

	void Awake ()
    {
        if (!Instance)
        {
            Instance = this;
            if (dontDestroyOnLoad)
            {
                transform.parent = null;
                DontDestroyOnLoad(gameObject);
            }
        }
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Play an audio clip.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volume"></param>
    public void PlayClip(AudioClip clip, float volume = 1)
    {
        if (!clip) return;

        Sources[curAS].volume = volume;
        Sources[curAS].clip = clip;
        Sources[curAS].Play();
        curAS = (curAS + 1) % Sources.Count;
    }

    /// <summary>
    /// Mute/unmute sound effects.
    /// </summary>
    /// <param name="muteMe"></param>
    public void Mute(bool muteMe)
    {
        foreach (AudioSource audioSource in Sources)
        {
            audioSource.mute = muteMe;
            Muted = muteMe;
        }
    }
}
