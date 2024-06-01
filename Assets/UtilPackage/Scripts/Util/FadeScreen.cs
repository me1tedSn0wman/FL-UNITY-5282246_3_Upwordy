using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScreen : Singleton<FadeScreen>
{
    [SerializeField] protected float fadeDuration = 1.0f;
    [SerializeField] protected CanvasGroup canvasGroup;

    protected float fadeSpeed;
    public bool isFading;

    public event Action FadeOutEnded;

    protected void Awake()
    {
        canvasGroup.alpha = 1f;
        fadeSpeed = 1f / fadeDuration;
    }

    public void FadeOut(Action afterFade) {
        if (!isFading)
            StartCoroutine(Fade(1f, afterFade));
    }

    public void FadeOut()
    {
        if (!isFading)
            StartCoroutine(Fade(1f, null));
    }

    public void FadeIn(Action afterFade) { 
        if(!isFading)
            StartCoroutine(Fade(0f, afterFade));
    }

    public void FadeIn()
    {
        if (!isFading)
            StartCoroutine(Fade(0f, null));
    }

    IEnumerator Fade(float finalAlpha, Action afterFade)
    {
        isFading = true;
        canvasGroup.blocksRaycasts = true;

        while (!Mathf.Approximately(canvasGroup.alpha, finalAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        canvasGroup.alpha = finalAlpha;
        canvasGroup.blocksRaycasts = false;

        afterFade?.Invoke();

        isFading = false;
    }
}
