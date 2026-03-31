using UnityEngine;
using System.Collections;

public class Direction_EndingCredit : MonoBehaviour
{
    [SerializeField] Direction_Ending ending;
    [SerializeField] private CanvasGroup targetGroup;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine fadeCoroutine;

    public void PlayCredit()
    {
        FadeIn();
        StartCreditScroll();
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
    }




    [Header("Target")]
    [SerializeField] private RectTransform creditRect;

    [Header("Scroll")]
    [SerializeField] private float scrollSpeed = 100f;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Start / End")]
    [SerializeField] private float startY = -800f;
    [SerializeField] private float endY = 1200f;

    private Coroutine scrollCo;

    private void OnEnable()
    {
        if (playOnEnable)
            StartCreditScroll();
    }

    private void OnDisable()
    {
        StopCreditScroll();
    }

    public void StartCreditScroll()
    {
        if (creditRect == null)
            return;

        StopCreditScroll();

        Vector2 pos = creditRect.anchoredPosition;
        pos.y = startY;
        creditRect.anchoredPosition = pos;

        scrollCo = StartCoroutine(CreditScrollRoutine());
    }

    public void StopCreditScroll()
    {
        if (scrollCo == null)
            return;

        StopCoroutine(scrollCo);
        scrollCo = null;
    }

    private IEnumerator CreditScrollRoutine()
    {
        if (startDelay > 0f)
        {
            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(startDelay);
            else
                yield return new WaitForSeconds(startDelay);
        }

        while (creditRect.anchoredPosition.y < endY)
        {
            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            Vector2 pos = creditRect.anchoredPosition;
            pos.y += scrollSpeed * delta;
            creditRect.anchoredPosition = pos;

            yield return null;
        }

        Vector2 finalPos = creditRect.anchoredPosition;
        finalPos.y = endY;
        creditRect.anchoredPosition = finalPos;

        scrollCo = null;
        yield return new WaitForSeconds(2f);
        ending.CreditPrintComplete();
    }
}
