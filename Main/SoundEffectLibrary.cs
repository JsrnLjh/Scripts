using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundEffectLibrary : MonoBehaviour
{
    [SerializeField] private SoundEffectGroup[] soundEffectGroups;
    private Dictionary<string, List<AudioClip>> soundDictionary;
    private void Start()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        soundDictionary = new Dictionary<string, List<AudioClip>>();
    
        if (soundEffectGroups == null || soundEffectGroups.Length == 0)
        {
        Debug.LogError("soundEffectGroups is null or empty!");
        return;
        }
    
        foreach (SoundEffectGroup soundEffectGroup in soundEffectGroups)
        {
            if (string.IsNullOrEmpty(soundEffectGroup.name) || soundEffectGroup.audioClips == null)
            {
            Debug.LogError("Found invalid soundEffectGroup!");
            continue;
            }
        soundDictionary[soundEffectGroup.name] = soundEffectGroup.audioClips;
        }
    }

    public AudioClip GetRandomClip(string name)
    {
        if (soundDictionary.ContainsKey(name))
        {
            List<AudioClip> audioClips = soundDictionary[name];
            if (audioClips.Count > 0)
            {
                return audioClips[UnityEngine.Random.Range(0, audioClips.Count)];
            }
        }
        return null;
    }
}

[System.Serializable]
public struct SoundEffectGroup
{
    public string name;
    public List<AudioClip> audioClips;
}
