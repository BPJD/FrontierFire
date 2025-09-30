using System.Collections.Generic;
using UnityEngine;

public class Data_StatUpgrades : MonoBehaviour
{
    [SerializeField]
    private List<StatUpSOEntry> statUpEntries = new List<StatUpSOEntry>();

    private Dictionary<int, StatUpgradesSO> statUpDict;

    void Awake()
    {
        statUpDict = new Dictionary<int, StatUpgradesSO>();
        foreach (var entry in statUpEntries)
        {
            if (!statUpDict.ContainsKey(entry.statUpID))
                statUpDict.Add(entry.statUpID, entry.statUp);
            else
                Debug.LogWarning($"중복된 ID 존재: {entry.statUpID}");
        }
    }

    public StatUpgradesSO GetStatUp(int id)
    {
        if (statUpDict.TryGetValue(id, out StatUpgradesSO prefab))
            return prefab;

        Debug.LogWarning($"ID {id} 프리팹 없음");
        return null;
    }

    public int GetStatUpCount()
    {
        return statUpEntries.Count;
    }

    public int GetWeaponIDbyList(int array)
    {
        return statUpEntries[array].statUpID;
    }

    public List<StatUpgradesSO> GetAllStatUps(int id)
    {
        var results = new List<StatUpgradesSO>();
        if (statUpEntries == null) return results;

        foreach (var e in statUpEntries)
        {
            if (e == null || e.statUp == null) continue;
            if (e.statUpID == id) results.Add(e.statUp);
        }

        if (results.Count == 0)
            Debug.LogWarning($"[Data_StatUpgrades] ID {id} 패키지 SO 없음");
        return results;
    }
}

[System.Serializable]
public class StatUpSOEntry
{
    public int statUpID;
    public StatUpgradesSO statUp;
}
