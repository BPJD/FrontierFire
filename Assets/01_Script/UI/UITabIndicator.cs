using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITabIndicator : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform buttonsRoot;   // HorizontalLayoutGroup 붙은 부모
    [SerializeField] private RectTransform indicator;     // 노란 바 RectTransform

    [Header("Anim")]
    [SerializeField] private float moveDuration = 0.18f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useUnscaledTime = true;

    Coroutine _co;

    /// <summary>
    /// 탭 버튼을 눌렀을 때 호출: indicator가 해당 버튼 아래로 이동 + 폭 맞춤
    /// </summary>
    public void SnapTo(RectTransform targetButton)  // 즉시 반영
    {
        if (targetButton == null) return;

        // Layout 결과가 최신이도록 강제 갱신(중요)
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsRoot);

        var (centerX, width) = GetTargetCenterXAndWidth(targetButton, indicator.parent as RectTransform);

        SetIndicator(centerX, width);
    }

    public void AnimateTo(RectTransform targetButton) // 부드럽게 이동
    {
        if (targetButton == null) return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsRoot);

        var parent = indicator.parent as RectTransform;
        var (toX, toW) = GetTargetCenterXAndWidth(targetButton, parent);

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(CoAnimate(toX, toW));
    }

    IEnumerator CoAnimate(float toX, float toW)
    {
        float fromX = indicator.anchoredPosition.x;
        float fromW = indicator.rect.width; // 현재 실제 폭

        float t = 0f;
        while (t < 1f)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt / Mathf.Max(0.0001f, moveDuration);

            float k = ease.Evaluate(Mathf.Clamp01(t));

            float x = Mathf.LerpUnclamped(fromX, toX, k);
            float w = Mathf.LerpUnclamped(fromW, toW, k);

            SetIndicator(x, w);
            yield return null;
        }

        SetIndicator(toX, toW);
        _co = null;
    }

    void SetIndicator(float centerX, float width)
    {
        // X 위치만 이동(센터 기준)
        var p = indicator.anchoredPosition;
        p.x = centerX;
        indicator.anchoredPosition = p;

        // 폭 조절: Layout에 영향 안 받게 indicator는 LayoutElement 없이 두는걸 권장
        var s = indicator.sizeDelta;
        s.x = width;
        indicator.sizeDelta = s;
    }

    /// <summary>
    /// targetButton의 좌/우 코너를 indicatorParent 로컬로 변환해
    /// centerX(anchoredPosition.x로 넣을 값)와 width를 얻는다.
    /// </summary>
    (float centerX, float width) GetTargetCenterXAndWidth(RectTransform targetButton, RectTransform indicatorParent)
    {
        // 버튼의 월드 코너 4개
        Vector3[] corners = new Vector3[4];
        targetButton.GetWorldCorners(corners);

        // 좌(0), 우(2) 코너를 indicatorParent 로컬로 변환
        Vector3 leftLocal = indicatorParent.InverseTransformPoint(corners[0]);
        Vector3 rightLocal = indicatorParent.InverseTransformPoint(corners[2]);

        float width = rightLocal.x - leftLocal.x;
        float centerX = (leftLocal.x + rightLocal.x) * 0.5f;

        return (centerX, width);
    }
}
