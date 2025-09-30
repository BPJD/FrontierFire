// 변경 포인트 요약:
// 1) 후보 필터링과 선택에서 GetComponentInParent<IInteractable>() 사용
// 2) 나머지는 동일 (OnSelectedChanged 이벤트 그대로)
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    readonly List<GameObject> currentCandidates = new List<GameObject>();

    public GameObject SelectedObj;// { get; private set; }
    public event Action<GameObject> OnSelectedChanged;

    bool isInteractable = false;

    void Update()
    {
        UpdateSelection();


    }

    public void Interacted()
    {
        if (SelectedObj != null)
        {
            SelectedObj.GetComponentInParent<IInteractable>()?.Interact();

            Item_ToolTip toolTip = SelectedObj.GetComponent<Item_ToolTip>();

            if (toolTip != null)
            {
                toolTip.ObjInteracted();
            }
            currentCandidates.Remove(SelectedObj);
            SelectedObj = null;
        }
    }

    public bool CheckInteractable()
    {
        return isInteractable;
    }

    private void UpdateSelection()
    {
        float closestSqr = float.MaxValue;
        GameObject closest = null;

        for (int i = currentCandidates.Count - 1; i >= 0; i--)
        {
            if (currentCandidates[i] == null)
                currentCandidates.RemoveAt(i);
        }

        foreach (var obj in currentCandidates)
        {
            if (obj == null) continue;

            // 부모에 IInteractable이 달려있는 구조도 허용
            if (obj.GetComponentInParent<IInteractable>() == null) continue;

            float sqr = (transform.position - obj.transform.position).sqrMagnitude;
            if (sqr < closestSqr)
            {
                closestSqr = sqr;
                closest = obj;
            }
        }

        if (SelectedObj != closest)
        {
            SelectedObj = closest;
            OnSelectedChanged?.Invoke(SelectedObj);
        }

        isInteractable = closest != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        var go = other.gameObject;

        // 부모까지 찾아서 인터랙션 가능 여부 판단
        if (go.GetComponentInParent<IInteractable>() == null) return;

        if (!currentCandidates.Contains(go))
            currentCandidates.Add(go);
    }

    private void OnTriggerExit(Collider other)
    {
        var go = other.gameObject;
        currentCandidates.Remove(go);

        if (SelectedObj == go)
        {
            SelectedObj = null;
            OnSelectedChanged?.Invoke(null);
        }
    }
}
