using System.Collections.Generic;
using UnityEngine;

public class Data_WeaponStatUpgrades : MonoBehaviour
{
    [SerializeField]
    private List<WeaponStatUpgradeEntry> upgradeEntries = new List<WeaponStatUpgradeEntry>();

    private Dictionary<int, WeaponStatUpgradesSO> upgradeDict;

    void Awake()
    {
        BuildDict();
    }

    void OnValidate()
    {
        // 에디터에서 값 변경 시 중복/누락 빠르게 검출
        BuildDict();
    }

    private void BuildDict()
    {
        if (upgradeDict == null) upgradeDict = new Dictionary<int, WeaponStatUpgradesSO>();
        else upgradeDict.Clear();

        foreach (var entry in upgradeEntries)
        {
            if (entry == null) continue;

            if (entry.statUpID <= 0)
            {
                Debug.LogWarning($"[Data_WeaponStatUpgrades] 유효하지 않은 ID 값: {entry.statUpID}");
                continue;
            }

            if (entry.statUp == null)
            {
                Debug.LogWarning($"[Data_WeaponStatUpgrades] ID {entry.statUpID} 에 매핑된 SO가 비어있습니다.");
                continue;
            }

            if (!upgradeDict.ContainsKey(entry.statUpID))
                upgradeDict.Add(entry.statUpID, entry.statUp);
        }
    }

    /// <summary>
    /// ID로 무기 강화 SO를 가져옵니다. (단일)
    /// </summary>
    public WeaponStatUpgradesSO GetUpgrade(int id)
    {
        if (upgradeDict != null && upgradeDict.TryGetValue(id, out var so))
            return so;

        Debug.LogWarning($"[Data_WeaponStatUpgrades] ID {id} 에 해당하는 강화 SO가 없습니다.");
        return null;
    }

    /// <summary>
    /// 동일 ID에 해당하는 모든 강화 SO들을 리스트로 반환합니다. (패키지 효과 지원)
    /// </summary>
    public List<WeaponStatUpgradesSO> GetAllUpgrades(int id)
    {
        List<WeaponStatUpgradesSO> results = new List<WeaponStatUpgradesSO>();

        if (upgradeEntries == null) return results;

        foreach (var entry in upgradeEntries)
        {
            if (entry == null || entry.statUp == null) continue;
            if (entry.statUpID == id)
            {
                results.Add(entry.statUp);
            }
        }

        if (results.Count == 0)
            Debug.LogWarning($"[Data_WeaponStatUpgrades] ID {id} 에 해당하는 강화 SO가 없습니다.");

        return results;
    }

    /// <summary>
    /// 인스펙터에 등록된 강화 항목 수를 반환합니다.
    /// </summary>
    public int GetUpgradeCount()
    {
        return upgradeEntries?.Count ?? 0;
    }

    /// <summary>
    /// 리스트 인덱스로 등록된 ID를 반환합니다. (UI/디버그용)
    /// </summary>
    public int GetUpgradeIDbyList(int index)
    {
        if (upgradeEntries == null || index < 0 || index >= upgradeEntries.Count)
        {
            Debug.LogWarning($"[Data_WeaponStatUpgrades] 잘못된 인덱스: {index}");
            return -1;
        }
        return upgradeEntries[index].statUpID;
    }
}

[System.Serializable]
public class WeaponStatUpgradeEntry
{
    public int statUpID;
    public WeaponStatUpgradesSO statUp;
}
