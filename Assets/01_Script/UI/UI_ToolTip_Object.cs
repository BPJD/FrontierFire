using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Michsky.UI.Heat;

public class UI_ToolTip_Object : MonoBehaviour
{
    public enum ObjectType { Normal, Weapon, Stat };

    public ObjectType type = ObjectType.Normal;

    [SerializeField] Image img_icon;
    [SerializeField] TextMeshProUGUI text_subName;
    [SerializeField] TextMeshProUGUI text_Desc;
    [SerializeField] TextMeshProUGUI text_Name;

    [SerializeField] TextMeshProUGUI[] text_Stats = new TextMeshProUGUI[9];

    [SerializeField] Image img_Frame;
    RectTransform tr;

    // ▼ 기존 값
    Vector3 selectedScale = Vector3.one;
    Vector3 defaultScale = Vector3.one * 0.6f;

    [SerializeField] Color defaultColor;
    [SerializeField] Color deselectedColor;
    [SerializeField] Color selectedColor;
    [SerializeField] Color selectedColor_single;

    ItemSelector itemGroup;

    // ▼ 추가: 트윈 설정
    [Header("Tween")]
    [SerializeField, Min(0f)] float tweenDuration = 0.18f;
    [SerializeField] bool useUnscaledTime = true;
    [SerializeField] AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ▼ 선택: 알파 페이드 (원하면 체크)
    [SerializeField] bool fadeAlphaWithScale = false;
    [SerializeField, Range(0f, 1f)] float deselectedAlpha = 0.75f;
    CanvasGroup cg;

    Coroutine tweenCo;

    public void SetText(Item_ToolTip toolTip)
    {
        itemGroup = toolTip.gameObject.GetComponentInParent<ItemSelector>();

        switch (type)
        {
            case ObjectType.Normal:
                text_Name.text = toolTip.title;
                text_Desc.text = toolTip.description;
                break;

            case ObjectType.Weapon:
                text_Name.text = toolTip.title;
                text_subName.text = toolTip.subTitle;
                text_Desc.text = toolTip.description;
                for (int i = 0; i < text_Stats.Length && i < toolTip.weaponStat.Length; i++)
                {
                    text_Stats[i].text = toolTip.weaponStat[i];
                }
                break;

            case ObjectType.Stat:
                text_Name.text = toolTip.title;
                text_subName.text = toolTip.subTitle;
                text_Desc.text = toolTip.description;
                break;
        }
    }

    public void ThisItemSelected(bool isSelected)
    {
        if (!isActiveAndEnabled)
            return;

        if (tr == null) tr = GetComponent<RectTransform>();
        if (fadeAlphaWithScale)
        {
            if (cg == null) cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        }

        bool _isSingleItem = (itemGroup != null && itemGroup.GetItemCount() == 1);

        // 목표 스케일/컬러/알파 계산
        Vector3 targetScale = isSelected ? selectedScale : defaultScale;

        Color targetColor;
        if (isSelected)
        {
            targetColor = _isSingleItem ? selectedColor_single : selectedColor;
        }
        else
        {
            targetColor = _isSingleItem ? defaultColor : deselectedColor;
        }

        float targetAlpha = 1f;
        if (fadeAlphaWithScale)
            targetAlpha = isSelected ? 1f : deselectedAlpha;

        // 이미 진행 중이던 트윈 정리 후 새로 시작
        if (tweenCo != null) StopCoroutine(tweenCo);
        tweenCo = StartCoroutine(CoTween(tr.localScale, targetScale,
                                         img_Frame != null ? img_Frame.color : Color.white, targetColor,
                                         fadeAlphaWithScale && cg != null ? cg.alpha : 1f, targetAlpha));
    }

    System.Collections.IEnumerator CoTween(Vector3 fromScale, Vector3 toScale,
                                           Color fromColor, Color toColor,
                                           float fromAlpha, float toAlpha)
    {
        float t = 0f;
        float dur = Mathf.Max(0.0001f, tweenDuration);

        // 0 duration 방어: 즉시 적용
        if (dur <= Mathf.Epsilon)
        {
            ApplyFrame(toScale, toColor, toAlpha);
            yield break;
        }

        while (t < 1f)
        {
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) / dur;
            float k = ease != null ? ease.Evaluate(Mathf.Clamp01(t)) : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

            // 스케일
            if (tr != null)
                tr.localScale = Vector3.LerpUnclamped(fromScale, toScale, k);

            // 프레임 컬러
            if (img_Frame != null)
                img_Frame.color = Color.LerpUnclamped(fromColor, toColor, k);

            // 알파
            if (fadeAlphaWithScale && cg != null)
                cg.alpha = Mathf.LerpUnclamped(fromAlpha, toAlpha, k);

            yield return null;
        }

        ApplyFrame(toScale, toColor, toAlpha);
        tweenCo = null;
    }

    void ApplyFrame(Vector3 s, Color c, float a)
    {
        if (tr != null) tr.localScale = s;
        if (img_Frame != null) img_Frame.color = c;
        if (fadeAlphaWithScale && cg != null) cg.alpha = a;
    }
}