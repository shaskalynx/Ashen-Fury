using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class UIFade : MonoBehaviour
{
    public float fadeTime = 0.5f;
    public void FadeIn(TMP_Text textObject)
    {
        CanvasGroup canvasGroup = textObject.GetComponentInParent<CanvasGroup>();
        if(canvasGroup != null){
            StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f));
        }
    }

    public void FadeOut(TMP_Text textObject)
    {
        CanvasGroup canvasGroup = textObject.GetComponentInParent<CanvasGroup>();
        if(canvasGroup != null){
            StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f));
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float targetAlpha)
    {
        float time = 0f;
        
        while(time < fadeTime){
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time/fadeTime);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }
}