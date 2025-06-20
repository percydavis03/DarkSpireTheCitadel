using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

[System.Serializable]
public class AudioChannelSlider
{
    [Header("Audio Channel Settings")]
    public string channelName;
    [Tooltip("FMOD bus path (e.g., 'bus:/', 'bus:/Music', 'bus:/SFX')")]
    public string busPath;
    public Slider slider;
    [Range(0f, 1f)]
    public float defaultVolume = 1f;

    [Header("Optional UI Elements")]
    public Text volumeLabel;
    public Text percentageText;

    [HideInInspector]
    public Bus bus;
}

public class AudioSliders : MonoBehaviour
{
    [Header("Audio Channel Configurations")]
    public AudioChannelSlider[] audioChannels = new AudioChannelSlider[]
    {
        new AudioChannelSlider { channelName = "Master", busPath = "bus:/", defaultVolume = 1f },
        new AudioChannelSlider { channelName = "Music", busPath = "bus:/Music", defaultVolume = 0.8f },
        new AudioChannelSlider { channelName = "SFX", busPath = "bus:/SFX", defaultVolume = 1f },
        new AudioChannelSlider { channelName = "Voice", busPath = "bus:/VA", defaultVolume = 1f },
        new AudioChannelSlider { channelName = "Ambience", busPath = "bus:/Ambience", defaultVolume = 0.6f },
        new AudioChannelSlider { channelName = "Environment", busPath = "bus:/Ambience/Environment", defaultVolume = 0.7f },
        new AudioChannelSlider { channelName = "Nyx", busPath = "bus:/SFX/Nyx", defaultVolume = 1f }
    };

    [Header("Settings")]
    public bool loadVolumeOnStart = true;
    public bool saveVolumeOnChange = true;
    public bool showPercentage = true;

    private void Start()
    {
        InitializeAudioChannels();
        
        if (loadVolumeOnStart)
        {
            LoadVolumeSettings();
        }
        else
        {
            SetDefaultVolumes();
        }
    }

    private void InitializeAudioChannels()
    {
        foreach (var channel in audioChannels)
        {
            if (!string.IsNullOrEmpty(channel.busPath))
            {
                // Get the FMOD bus
                channel.bus = RuntimeManager.GetBus(channel.busPath);
                
                // Setup slider if assigned
                if (channel.slider != null)
                {
                    // Set slider range
                    channel.slider.minValue = 0f;
                    channel.slider.maxValue = 1f;
                    
                    // Add listener for value changes
                    channel.slider.onValueChanged.AddListener((value) => OnVolumeChanged(channel, value));
                    
                    // Update label if assigned
                    if (channel.volumeLabel != null)
                    {
                        channel.volumeLabel.text = channel.channelName;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"AudioSliders: Bus path not set for channel '{channel.channelName}'");
            }
        }
    }

    private void OnVolumeChanged(AudioChannelSlider channel, float value)
    {
        // Set FMOD bus volume
        if (channel.bus.isValid())
        {
            channel.bus.setVolume(value);
        }
        
        // Update percentage text if assigned
        if (channel.percentageText != null && showPercentage)
        {
            channel.percentageText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
        
        // Save volume setting if enabled
        if (saveVolumeOnChange)
        {
            SaveVolumeSettings();
        }
    }

    public void SetChannelVolume(string channelName, float volume)
    {
        var channel = System.Array.Find(audioChannels, c => c.channelName.Equals(channelName, System.StringComparison.OrdinalIgnoreCase));
        if (channel != null)
        {
            // Clamp volume between 0 and 1
            volume = Mathf.Clamp01(volume);
            
            // Update slider if assigned
            if (channel.slider != null)
            {
                channel.slider.value = volume;
            }
            else
            {
                // If no slider, set volume directly
                if (channel.bus.isValid())
                {
                    channel.bus.setVolume(volume);
                }
            }
        }
        else
        {
            Debug.LogWarning($"AudioSliders: Channel '{channelName}' not found");
        }
    }

    public float GetChannelVolume(string channelName)
    {
        var channel = System.Array.Find(audioChannels, c => c.channelName.Equals(channelName, System.StringComparison.OrdinalIgnoreCase));
        if (channel != null && channel.bus.isValid())
        {
            channel.bus.getVolume(out float volume);
            return volume;
        }
        return 0f;
    }

    public void SetDefaultVolumes()
    {
        foreach (var channel in audioChannels)
        {
            SetChannelVolume(channel.channelName, channel.defaultVolume);
        }
    }

    public void SaveVolumeSettings()
    {
        foreach (var channel in audioChannels)
        {
            if (channel.slider != null)
            {
                PlayerPrefs.SetFloat($"AudioVolume_{channel.channelName}", channel.slider.value);
            }
        }
        PlayerPrefs.Save();
    }

    public void LoadVolumeSettings()
    {
        foreach (var channel in audioChannels)
        {
            if (channel.slider != null)
            {
                float savedVolume = PlayerPrefs.GetFloat($"AudioVolume_{channel.channelName}", channel.defaultVolume);
                channel.slider.value = savedVolume;
            }
        }
    }

    // Method to mute/unmute a specific channel
    public void SetChannelMute(string channelName, bool muted)
    {
        var channel = System.Array.Find(audioChannels, c => c.channelName.Equals(channelName, System.StringComparison.OrdinalIgnoreCase));
        if (channel != null && channel.bus.isValid())
        {
            channel.bus.setMute(muted);
        }
    }

    // Method to check if a channel is muted
    public bool IsChannelMuted(string channelName)
    {
        var channel = System.Array.Find(audioChannels, c => c.channelName.Equals(channelName, System.StringComparison.OrdinalIgnoreCase));
        if (channel != null && channel.bus.isValid())
        {
            channel.bus.getMute(out bool muted);
            return muted;
        }
        return false;
    }

    // Utility method to fade volume over time
    public void FadeChannelVolume(string channelName, float targetVolume, float fadeTime)
    {
        var channel = System.Array.Find(audioChannels, c => c.channelName.Equals(channelName, System.StringComparison.OrdinalIgnoreCase));
        if (channel != null)
        {
            StartCoroutine(FadeVolumeCoroutine(channel, targetVolume, fadeTime));
        }
    }

    private IEnumerator FadeVolumeCoroutine(AudioChannelSlider channel, float targetVolume, float fadeTime)
    {
        if (channel.slider == null) yield break;

        float startVolume = channel.slider.value;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float currentVolume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeTime);
            channel.slider.value = currentVolume;
            yield return null;
        }

        channel.slider.value = targetVolume;
    }

    private void OnDestroy()
    {
        // Clean up slider listeners
        foreach (var channel in audioChannels)
        {
            if (channel.slider != null)
            {
                channel.slider.onValueChanged.RemoveAllListeners();
            }
        }
    }

    // Inspector helper methods
    #if UNITY_EDITOR
    [ContextMenu("Auto-Assign Missing Components")]
    public void AutoAssignComponents()
    {
        // This method can be used in the inspector to help auto-assign sliders
        Slider[] sliders = GetComponentsInChildren<Slider>();
        Text[] texts = GetComponentsInChildren<Text>();

        for (int i = 0; i < audioChannels.Length && i < sliders.Length; i++)
        {
            if (audioChannels[i].slider == null)
            {
                audioChannels[i].slider = sliders[i];
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }
    #endif
}
