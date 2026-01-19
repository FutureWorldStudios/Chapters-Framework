using UnityEngine;
using System.Threading.Tasks;
using System;

public class FWS_OVRScreenFade : OVRScreenFade
{
    public static FWS_OVRScreenFade Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void FadeOut(float duration)
    {
        fadeTime = duration;
        FadeOut();
    }

    public void FadeIn(float duration)
    {
        fadeTime = duration;
        FadeIn();
    }

    public async Task Blink(int blinkDuration = 1)
    {
        float defaultFadeTime = fadeTime;
        fadeTime = blinkDuration;

        FadeOut();
        await Task.Delay(blinkDuration * 1000);

        FadeIn();

        fadeTime = defaultFadeTime;
    }

    public void BlackOut()
    {
        float defaultFadeTime = fadeTime;
        fadeTime = 0;
        FadeOut();
        fadeTime = defaultFadeTime; 
    }
}
