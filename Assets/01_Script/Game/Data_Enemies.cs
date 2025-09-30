using System.Collections.Generic;
using UnityEngine;

public class Data_Enemies : MonoBehaviour
{
    [SerializeField]
    private List<EnemyPrefabEntry> enemyPrefabEntries = new List<EnemyPrefabEntry>();

    private Dictionary<int, GameObject> enemyPrefabDict;

    void Awake()
    {
        enemyPrefabDict = new Dictionary<int, GameObject>();
        foreach (var entry in enemyPrefabEntries)
        {
            if (!enemyPrefabDict.ContainsKey(entry.enemyId))
                enemyPrefabDict.Add(entry.enemyId, entry.enemyPrefab);
            else
                Debug.LogWarning($"중복된 ID 존재: {entry.enemyId}");
        }
    }

    public GameObject GetEnemyPrefab(int id)
    {
        if (enemyPrefabDict.TryGetValue(id, out GameObject prefab))
            return prefab;

        Debug.LogWarning($"ID {id} 프리팹 없음");
        if (enemyPrefabDict.TryGetValue(20000, out GameObject errorPrefab))
            return errorPrefab;

        return null;
    }
}

[System.Serializable]
public class EnemyPrefabEntry
{
    public int enemyId;
    public GameObject enemyPrefab;
}
