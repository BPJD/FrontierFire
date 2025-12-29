using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Item_ToolTip))]
public class ItemToolTip_CsvBinder : MonoBehaviour
{
    [Header("CSV Row ID")]
    public int upgradeId = 100;

    [Header("Options")]
    [Tooltip("Description에 Effect까지 합쳐서 보여줄지")]
    public bool mergeDescAndEffect = true;

    [Tooltip("subTitle도 title과 동일하게 맞출지")]
    public bool syncSubTitleToTitle = true;

    Item_ToolTip tooltip;

    void Awake()
    {
        tooltip = GetComponent<Item_ToolTip>();
    }

    IEnumerator Start()
    {
        // DB가 씬에 없으면 자동 생성 (편의)
        if (UpgradeTextDB.I == null)
        {
            var go = new GameObject("[UpgradeTextDB]");
            go.AddComponent<UpgradeTextDB>();
        }

        // 로드 완료 대기
        while (!UpgradeTextDB.I.IsReady)
            yield return null;

        Apply();
    }

    [ContextMenu("Apply Now (Debug)")]
    public void Apply()
    {
        if (UpgradeTextDB.I == null) return;

        if (!UpgradeTextDB.I.TryGet(upgradeId, out var row))
        {
            Debug.LogWarning($"[ItemToolTip_CsvBinder] No row for ID={upgradeId} ({name})");
            return;
        }

        tooltip.title = row.name;
        if (syncSubTitleToTitle) tooltip.subTitle = row.name;

        if (mergeDescAndEffect)
        {
            // 설명 + (한 줄 띄우고) 효과
            if (!string.IsNullOrEmpty(row.desc) && !string.IsNullOrEmpty(row.effect))
                tooltip.description = $"{row.desc}\n{row.effect}";
            else
                tooltip.description = string.IsNullOrEmpty(row.desc) ? row.effect : row.desc;
        }
        else
        {
            tooltip.description = row.desc;
        }

        // 필요하면 titleColor, weaponStat 등도 여기서 채우면 됨
        // tooltip.titleColor = ...;
        // tooltip.weaponStat[0] = ...;

        tooltip.UpdateToolTipUI();
    }
}
