using UnityEngine;

public class HeatHorizontalSelectorAdapter : MonoBehaviour, ILeftRightAdjustable
{
    [Header("Drag the Heat UI Horizontal Selector component here")]
    [SerializeField] private MonoBehaviour horizontalSelector; // Horizontal Selector 컴포넌트

    [Header("Optional")]
    [SerializeField] private bool isInteractable = true;

    public bool IsInteractable => isInteractable && horizontalSelector != null && horizontalSelector.isActiveAndEnabled;

    public void Prev()
    {
        if (!IsInteractable) return;
        InvokeIfExists("PreviousItem");
        InvokeIfExists("Previous");
        InvokeIfExists("Prev");
    }

    public void Next()
    {
        if (!IsInteractable) return;
        InvokeIfExists("NextItem");
        InvokeIfExists("Next");
    }

    void InvokeIfExists(string method)
    {
        var t = horizontalSelector.GetType();
        var m = t.GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (m != null && m.GetParameters().Length == 0)
            m.Invoke(horizontalSelector, null);
    }

    void Reset()
    {
        // 같은 오브젝트에 붙어 있으면 자동 할당 시도
        if (!horizontalSelector)
        {
            // 컴포넌트명이 버전마다 다를 수 있어 자동은 최소화
            // 필요하면 인스펙터에서 직접 드래그하는 게 가장 확실함.
        }
    }
}