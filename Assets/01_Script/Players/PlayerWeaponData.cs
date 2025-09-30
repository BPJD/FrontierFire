using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponData : MonoBehaviour
{
    [SerializeField]
    private List<PWeaponPrefabEntry> pWeaponPrefabEntries = new List<PWeaponPrefabEntry>();

    private Dictionary<int, GameObject> pWeaponPrefabDict;
    private Dictionary<int, WeaponParamsSO> pWeaponStatDict;

    void Awake()
    {
        pWeaponPrefabDict = new Dictionary<int, GameObject>();
        pWeaponStatDict = new Dictionary<int, WeaponParamsSO>();

        foreach (var entry in pWeaponPrefabEntries)
        {
            if (!pWeaponPrefabDict.ContainsKey(entry.pWeaponId))
            {
                pWeaponPrefabDict.Add(entry.pWeaponId, entry.pWeaponPrefab);
                pWeaponStatDict.Add(entry.pWeaponId, entry.weaponStatSO);
            }
            else
            {
                Debug.LogWarning($"중복된 ID 존재: {entry.pWeaponId}");
            }
        }
    }

    public GameObject GetpWeaponPrefab(int id)
    {
        if (pWeaponPrefabDict.TryGetValue(id, out GameObject prefab))
            return prefab;

        Debug.LogWarning($"ID {id} 프리팹 없음");
        return null;
    }

    public WeaponParamsSO GetWeaponStatSO(int id)
    {
        if (pWeaponStatDict.TryGetValue(id, out WeaponParamsSO statSO))
            return statSO;

        Debug.LogWarning($"ID {id}에 해당하는 WeaponParamsSO 없음");
        return null;
    }

    public int GetWeaponCount()
    {
        return pWeaponPrefabEntries.Count;
    }

    public int GetWeaponIDbyList(int array)
    {
        return pWeaponPrefabEntries[array].pWeaponId;
    }
}

[System.Serializable]
public class PWeaponPrefabEntry
{
    public int pWeaponId;
    public GameObject pWeaponPrefab;
    public WeaponParamsSO weaponStatSO;

}
