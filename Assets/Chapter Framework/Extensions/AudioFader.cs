using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System;

namespace VRG.ChapterFramework
{
    public static class AudioFader
    {
        public static async Task FadeOutAsync(float duration, AudioSource audioSource)
        {
            if (audioSource == null || !audioSource.isPlaying || duration <= 0f)
                return;
            float startVolume = audioSource.volume;
            float time = 0f;
            // Loop until fully faded
            while (time < duration && audioSource != null)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                // Lerp volume down
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                // Wait until next frame
                await Task.Yield();
            }
            if (audioSource != null)
            {
                audioSource.volume = 0f;
                audioSource.Stop();
            }
        }

        public static async Task FadeInAsync(AudioSource source, float duration, float targetVolume = 1f)
        {
            if (source == null || duration <= 0f)
                return;
            source.volume = 0f;
            source.Play();
            float time = 0f;
            while (time < duration && source != null)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                source.volume = Mathf.Lerp(0f, targetVolume, t);
                await Task.Yield();
            }
            if (source != null)
                source.volume = targetVolume;
        }

        public static async Task PlayAsync(this AudioSource source, AudioClip clip, Action onCompleted = null, CancellationToken token = default)
        {
            if (source == null || clip == null)
                return;

            source.clip = clip;
            source.Play();

            float duration = clip.length;
            float time = 0f;

            while (time < duration && source != null && source.isPlaying)
            {
                if (token.IsCancellationRequested)
                    return; // stop immediately, no callback

                time += Time.deltaTime;
                await Task.Yield();
            }

            onCompleted?.Invoke();
        }

    }
}