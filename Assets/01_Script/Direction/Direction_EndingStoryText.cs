using Michsky.UI.Heat;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Direction_EndingStoryText : MonoBehaviour
{
    [SerializeField] Direction_Ending directionEnding;

    [SerializeField] private LocalizedObject[] localizedTexts;
    [SerializeField] private float charInterval = 0.04f;
    [SerializeField] private float lineDelay = 0.6f;

    private string[] cachedTexts;
    private bool skip;

    public bool isDirectionEnd { get; private set; } = false;

    private void Awake()
    {
        cachedTexts = new string[localizedTexts.Length];

        for (int i = 0; i < localizedTexts.Length; i++)
        {
            LocalizedObject loc = localizedTexts[i];
            TextMeshProUGUI tmp = loc.GetComponent<TextMeshProUGUI>();

            loc.UpdateItem();
            cachedTexts[i] = loc.GetKeyOutput(loc.localizationKey);

            tmp.text = cachedTexts[i];
            tmp.maxVisibleCharacters = 0;
            tmp.ForceMeshUpdate();

            RectTransform rt = tmp.rectTransform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        Canvas.ForceUpdateCanvases();
    }


    public void EndingPlay()
    {
        if (isDirectionEnd) return;

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < localizedTexts.Length; i++)
        {
            TextMeshProUGUI tmp = localizedTexts[i].GetComponent<TextMeshProUGUI>();
            yield return StartCoroutine(TypeText(tmp));
            skip = false;
            yield return new WaitForSeconds(lineDelay);
        }

        yield return new WaitForSeconds(2f);
        isDirectionEnd = true;
        directionEnding.EndingStoryPrinted();
    }

    private IEnumerator TypeText(TextMeshProUGUI target)
    {
        target.ForceMeshUpdate();
        int total = target.textInfo.characterCount;

        for (int i = 0; i <= total; i++)
        {
            if (skip)
            {
                target.maxVisibleCharacters = total;
                yield break;
            }

            target.maxVisibleCharacters = i;
            yield return new WaitForSeconds(charInterval);
        }
    }

    public void Skip()
    {
        skip = true;
    }

    public IEnumerator FadeOut(
    CanvasGroup group,
    float duration,
    AnimationCurve curve,
    bool ignoreTimeScale = true)
    {
        if (group == null)
            yield break;

        float startAlpha = group.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float delta = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += delta;

            float t = Mathf.Clamp01(elapsed / duration);
            float curved = curve.Evaluate(t);

            group.alpha = Mathf.Lerp(startAlpha, 0f, curved);

            yield return null;
        }

        group.alpha = 0f;
    }
}