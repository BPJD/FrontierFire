using UnityEngine;
using System.Collections;

public class Direction_EndingBG : MonoBehaviour
{
    [SerializeField] Direction_Ending ending;
    [SerializeField] private CanvasGroup targetGroup;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine fadeCoroutine;

    public void PlayBG()
    {
        FadeIn();
    }



    void FadeIn()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvasGroupAlpha(targetGroup, 0f, 1f, fadeDuration));
    }

    private IEnumerator FadeCanvasGroupAlpha(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        if (group == null)
            yield break;

        float elapsed = 0f;
        group.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        group.alpha = endAlpha;
        fadeCoroutine = null;
        ending.EndingBGAppear();
    }


}
