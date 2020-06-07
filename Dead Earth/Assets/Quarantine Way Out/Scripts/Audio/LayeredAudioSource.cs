using System.Collections.Generic;
using UnityEngine;

public class LayeredAudioSource : ILayeredAudioSource
{
    private AudioSource audioSource = null;
    private List<AudioLayer> audioLayers = new List<AudioLayer>();
    private int activeLayer = -1;

    public AudioSource AudioSource { get { return audioSource; } }

    /// <summary>
    /// Allocates the layer stack.
    /// </summary>
    public LayeredAudioSource(AudioSource source, int layers)
    {
        if (source != null && layers > 0)
        {
            // Assign audio source to this layer stack
            audioSource = source;

            // Create the requested number of layers
            for (int i = 0; i < layers; i++)
            {
                // Create new layer
                AudioLayer newLayer = new AudioLayer();
                newLayer.collection = null;
                newLayer.duration = 0.0f;
                newLayer.time = 0.0f;
                newLayer.looping = false;
                newLayer.bank = 0;
                newLayer.muted = false;
                newLayer.clip = null;

                // Add Layer to stack
                audioLayers.Add(newLayer);
            }
        }
    }

    public bool Play(AudioCollection collection, int bank, int layer, bool looping = true)
    {
        // Layer must be in range
        if (layer >= audioLayers.Count)
            return false;

        // Fetch the layer to configure
        AudioLayer audioLayer = audioLayers[layer];

        // Already doing what is intended
        if (audioLayer.collection == collection && audioLayer.looping == looping && bank == audioLayer.bank)
            return true;

        audioLayer.collection = collection;
        audioLayer.bank = bank;
        audioLayer.looping = looping;
        audioLayer.time = 0.0f;
        audioLayer.duration = 0.0f;
        audioLayer.muted = false;
        audioLayer.clip = null;

        return true;
    }

    public void Stop(int layerIndex)
    {
        if (layerIndex >= audioLayers.Count)
            return;

        AudioLayer layer = audioLayers[layerIndex];

        if (layer != null)
        {
            layer.looping = false;
            layer.time = layer.duration;
        }
    }

    public void Mute(int layerIndex, bool mute)
    {
        if (layerIndex >= audioLayers.Count)
            return;

        AudioLayer layer = audioLayers[layerIndex];

        if (layer != null)
        {
            layer.muted = mute;
        }
    }

    public void Mute(bool mute)
    {
        for (int i = 0; i < audioLayers.Count; i++)
        {
            Mute(i, mute);
        }
    }

    /// <summary>
    /// Updates the time of all layered clips and makes sure that the audio source is playing the clip on the highest layer.
    /// </summary>
    public void Update()
    {
        // Used to record the highest layer with a clip assigned and still playing
        int newActiveLayer = -1;
        bool refreshAudioSource = false;

        // Update the stack each frame by iterating the layers (Working backwards)
        for (int i = audioLayers.Count - 1; i >= 0; i--)
        {
            // Layer being processed
            AudioLayer layer = audioLayers[i];

            // Ignore unassigned layers
            if (layer.collection == null) continue;

            // Update the internal playhead of the layer		
            layer.time += Time.deltaTime;

            // If it has exceeded its duration then we need to take action
            if (layer.time > layer.duration)
            {
                // If its a looping sound OR the first time we have set up this layer
                // we need to assign a new clip from the pool assigned to this layer
                if (layer.looping || layer.clip == null)
                {
                    // Fetch a new clip from the pool
                    AudioClip clip = layer.collection[layer.bank];

                    // Calculate the play position based on the time of the layer and store duration
                    if (clip == layer.clip)
                        layer.time %= layer.clip.length;
                    else
                        layer.time = 0.0f;

                    layer.duration = clip.length;
                    layer.clip = clip;

                    // This is a layer that has focus so we need to chose and play
                    // a new clip from the pool
                    if (newActiveLayer < i)
                    {
                        // This is the active layer index
                        newActiveLayer = i;
                        // We need to issue a play command to the audio source
                        refreshAudioSource = true;
                    }
                }
                else
                {
                    // The clip assigned to this layer has finished playing and is not set to loop
                    // so clear the later and reset its status ready for reuse in the future
                    layer.clip = null;
                    layer.collection = null;
                    layer.duration = 0.0f;
                    layer.bank = 0;
                    layer.looping = false;
                    layer.time = 0.0f;
                }
            }
            // Else this layer is playing
            else
            {
                // If this is the highest layer then record that its the clip currently playing
                if (newActiveLayer < i) newActiveLayer = i;
            }
        }

        // If we found a new active layer (or none)
        if (newActiveLayer != activeLayer || refreshAudioSource)
        {
            // Previous layer expired and no new layer so stop audio source - there are no active layers
            if (newActiveLayer == -1)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
            // We found an active layer but its different than the previous update so its time to switch
            // the audio source to play the clip on the new layer
            else
            {
                // Get the layer
                AudioLayer layer = audioLayers[newActiveLayer];

                audioSource.clip = layer.clip;
                audioSource.volume = layer.muted ? 0.0f : layer.collection.Volume;
                audioSource.spatialBlend = layer.collection.SpatialBlend;
                audioSource.time = layer.time;
                audioSource.loop = false;
                audioSource.outputAudioMixerGroup = AudioManager.Instance.GetAudioGroupFromTrackName(layer.collection.AudioGroup);
                audioSource.Play();
            }
        }

        // Remember the currently active layer for the next update check
        activeLayer = newActiveLayer;

        if (activeLayer != -1 && audioSource)
        {
            AudioLayer audioLayer = audioLayers[activeLayer];

            if (audioLayer.muted)
            {
                audioSource.volume = 0.0f;
            }
            else
            {
                audioSource.volume = audioLayer.collection.Volume;
            }
        }
    }
}
