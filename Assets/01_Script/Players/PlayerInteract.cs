using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    readonly List<GameObject> currentCandidates = new List<GameObject>();

    public GameObject SelectedObj; // { get; private set; }
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
            bool? _isSuccess = SelectedObj.GetComponentInParent<IInteractable>()?.TryInteract();

            if (_isSuccess != null)
            {
                CheckInteractSuccess((bool)_isSuccess);
            }

        }
    }

    void CheckInteractSuccess(bool isSuccess)
    {
        if (isSuccess)
        {
            Item_ToolTip toolTip = SelectedObj.GetComponent<Item_ToolTip>();
            if (toolTip != null)
                toolTip.ObjInteracted();

            // 선택 해제 시 외곽선 안전 OFF
            LayerOutlineSet(SelectedObj, false);

            currentCandidates.Remove(SelectedObj);
            SelectedObj = null;
            OnSelectedChanged?.Invoke(null);
        }
    }

    public bool CheckInteractable()
    {
        return isInteractable;
    }

    // ★ 추가: 항상 IInteractable 루트를 반환
    GameObject GetInteractableRoot(GameObject go)
    {
        var it = go ? go.GetComponentInParent<IInteractable>() : null;
        return it != null ? ((MonoBehaviour)it).gameObject : null;
    }

    private void UpdateSelection()
    {
        float closestSqr = float.MaxValue;
        GameObject closest = null;

        // null 제거
        for (int i = currentCandidates.Count - 1; i >= 0; i--)
        {
            if (currentCandidates[i] == null)
                currentCandidates.RemoveAt(i);
        }

        foreach (var obj in currentCandidates)
        {
            if (obj == null) continue;

            // 부모에 IInteractable이 달려있는 구조도 허용
            var root = GetInteractableRoot(obj);
            if (root == null) continue;

            // ★ 루트 기준 거리
            float sqr = (transform.position - root.transform.position).sqrMagnitude;
            if (sqr < closestSqr)
            {
                closestSqr = sqr;
                closest = root;
            }
        }

        if (SelectedObj != closest)
        {
            // ★ 이전 선택 OFF → 신규 ON (순서 보장)
            if (SelectedObj != null)
                LayerOutlineSet(SelectedObj, false);

            SelectedObj = closest;

            if (SelectedObj != null)
                LayerOutlineSet(SelectedObj, true);

            OnSelectedChanged?.Invoke(SelectedObj);
        }

        isInteractable = closest != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ★ 항상 루트로 정규화해서 관리
        var root = GetInteractableRoot(other.gameObject);
        if (root == null) return;

        if (!currentCandidates.Contains(root))
            currentCandidates.Add(root);
    }

    private void OnTriggerExit(Collider other)
    {
        // ★ 루트 기준 제거
        var root = GetInteractableRoot(other.gameObject);
        if (root == null) return;

        currentCandidates.Remove(root);

        // 선택 대상이 나갈 때 안전하게 OFF
        if (SelectedObj == root)
        {
            LayerOutlineSet(SelectedObj, false);
            SelectedObj = null;
            OnSelectedChanged?.Invoke(null);
            isInteractable = false;
        }
    }

    // ---------- Linework(URP RenderingLayerMask) 토글 부분: 기존 유지 ----------
    [SerializeField] int outlineLayerIndex = 1; // Project Settings에서 만든 Rendering Layer의 인덱스
    private readonly Dictionary<Renderer, uint> _originalMask = new();
    uint OutlineBit => 1u << outlineLayerIndex;

    /// <summary>
    /// 선택된 오브젝트의 모든 Renderer에 대해 Linework용 RenderingLayer 비트를 켜거나 끈다.
    /// isSet=true → 켬, false → 끔(원복)
    /// </summary>
    public void LayerOutlineSet(GameObject obj, bool isSet)
    {
        if (!obj) return;

        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            // 최초 한 번만 원래 값을 저장
            if (!_originalMask.ContainsKey(r))
                _originalMask[r] = r.renderingLayerMask;

            uint mask = r.renderingLayerMask;
            mask = isSet ? (mask | OutlineBit) : (mask & ~OutlineBit);
            r.renderingLayerMask = mask;

            // 끔(원복) 상태라면 캐시도 정리
            if (!isSet) _originalMask.Remove(r);
        }
    }

    /// <summary>
    /// 강제 완전 원복(혹시 모를 드리프트 방지)
    /// </summary>
    public void RestoreOriginal(GameObject obj)
    {
        if (!obj) return;
        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (_originalMask.TryGetValue(r, out var original))
            {
                r.renderingLayerMask = original;
                _originalMask.Remove(r);
            }
        }
    }
}
