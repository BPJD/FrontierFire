using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_AutoScrollToSelection : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField] private float paddingTop = 20f;
    [SerializeField] private float paddingBottom = 20f;
    [SerializeField] private float smoothTime = 0.10f; // 0이면 즉시

    private GameObject _lastSelected;
    private Coroutine _co;

    void Update()
    {
        if (!EventSystem.current) return;

        var cur = EventSystem.current.currentSelectedGameObject;
        if (!cur || cur == _lastSelected) return;

        _lastSelected = cur;
        TryScrollToMakeVisible(cur);
    }

    void TryScrollToMakeVisible(GameObject selected)
    {
        var sr = selected.GetComponentInParent<ScrollRect>();
        if (!sr) return;

        var content = sr.content;
        var viewport = sr.viewport ? sr.viewport : sr.GetComponent<RectTransform>();
        if (!content || !viewport) return;

        var itemRT = selected.GetComponent<RectTransform>();
        if (!itemRT) return;
        if (!itemRT.IsChildOf(content)) return;

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(CoEnsureVisible(sr, viewport, content, itemRT));
    }

    IEnumerator CoEnsureVisible(ScrollRect sr, RectTransform viewport, RectTransform content, RectTransform itemRT)
    {
        // 레이아웃 갱신 대기(VerticalLayoutGroup/ContentSizeFitter 등)
        yield return null;

        if (!sr || !viewport || !content || !itemRT) yield break;

        float contentH = content.rect.height;
        float viewH = viewport.rect.height;

        float maxOffset = Mathf.Max(0f, contentH - viewH);
        if (maxOffset <= 0f) yield break; // 스크롤 필요 없음

        // item bounds를 content 로컬 좌표로 계산
        Bounds itemBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, itemRT);

        // content의 "Top edge Y" (content 로컬에서)
        float topEdgeY = (1f - content.pivot.y) * contentH;

        // item의 Top/Bottom을 "Top에서 얼마나 떨어졌는지" 거리로 변환(아래로 갈수록 커짐)
        float itemTopDist = topEdgeY - itemBounds.max.y;
        float itemBottomDist = topEdgeY - itemBounds.min.y;

        // 현재 verticalNormalizedPosition을 "Top에서의 오프셋"으로 변환
        float curOffset = (1f - sr.verticalNormalizedPosition) * maxOffset;

        // 현재 보이는 구간(Top 기준 거리)
        float visibleTop = curOffset + paddingTop;
        float visibleBottom = curOffset + viewH - paddingBottom;

        float targetOffset = curOffset;

        // item이 위로 튀어나왔으면 위쪽으로 올리기(오프셋 감소)
        if (itemTopDist < visibleTop)
        {
            targetOffset = itemTopDist - paddingTop;
        }
        // item이 아래로 튀어나왔으면 아래쪽으로 내리기(오프셋 증가)
        else if (itemBottomDist > visibleBottom)
        {
            targetOffset = itemBottomDist - (viewH - paddingBottom);
        }
        else
        {
            yield break; // 이미 충분히 보임
        }

        targetOffset = Mathf.Clamp(targetOffset, 0f, maxOffset);

        float targetNorm = 1f - (targetOffset / maxOffset);

        if (smoothTime <= 0f)
        {
            sr.verticalNormalizedPosition = targetNorm;
            yield break;
        }

        float start = sr.verticalNormalizedPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / smoothTime;
            sr.verticalNormalizedPosition = Mathf.Lerp(start, targetNorm, Mathf.Clamp01(t));
            yield return null;
        }
    }
}