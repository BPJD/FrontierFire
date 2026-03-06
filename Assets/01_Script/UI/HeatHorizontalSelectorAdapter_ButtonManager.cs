using System.Reflection;
using UnityEngine;

public class HeatHorizontalSelectorAdapter_ButtonManager : MonoBehaviour, ILeftRightAdjustable
{
    [Header("Assign Heat UI ButtonManager components")]
    [SerializeField] private MonoBehaviour prevButtonManager;
    [SerializeField] private MonoBehaviour nextButtonManager;

    [Header("Optional")]
    [SerializeField] private bool isInteractable = true;

    public bool IsInteractable => isInteractable && isActiveAndEnabled;

    // 후보 메서드명(Heat UI 내부 구현 차이 대응)
    private static readonly string[] PrevMethodCandidates =
    {
        "Press", "Click", "OnClick", "Interact", "Submit", "Execute", "Trigger"
    };

    private static readonly string[] NextMethodCandidates =
    {
        "Press", "Click", "OnClick", "Interact", "Submit", "Execute", "Trigger"
    };

    public void Prev()
    {
        if (!IsInteractable) return;
        InvokeFirstMatch(prevButtonManager, PrevMethodCandidates);
    }

    public void Next()
    {
        if (!IsInteractable) return;
        InvokeFirstMatch(nextButtonManager, NextMethodCandidates);
    }

    private void InvokeFirstMatch(MonoBehaviour target, string[] candidates)
    {
        if (!target) return;

        var t = target.GetType();
        for (int i = 0; i < candidates.Length; i++)
        {
            string name = candidates[i];
            MethodInfo m = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (m == null) continue;

            // 파라미터 없는 메서드만 호출
            if (m.GetParameters().Length != 0) continue;

            m.Invoke(target, null);
            return;
        }

        // 디버그 도움: 어떤 메서드도 못 찾으면 경고
        Debug.LogWarning($"[HeatHorizontalSelectorAdapter_ButtonManager] No callable method found on {t.Name}. " +
                         $"Tried: {string.Join(", ", candidates)}", target);
    }
}