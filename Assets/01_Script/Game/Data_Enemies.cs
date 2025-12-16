using System.Collections.Generic;
using UnityEngine;

public class Data_Enemies : MonoBehaviour
{
    [SerializeField]
    private List<EnemyPrefabEntry> enemyPrefabEntries = new List<EnemyPrefabEntry>();

    private Dictionary<int, GameObject> enemyPrefabDict;

    [SerializeField] GameObject eliteEffect;

    void Awake()
    {
        enemyPrefabDict = new Dictionary<int, GameObject>();
        foreach (var entry in enemyPrefabEntries)
        {
            if (!enemyPrefabDict.ContainsKey(entry.enemyId))
                enemyPrefabDict.Add(entry.enemyId, entry.enemyPrefab);
        }
    }

    public GameObject GetEnemyPrefab(int id)
    {
        if (enemyPrefabDict.TryGetValue(id, out GameObject prefab))
            return prefab;

        if (enemyPrefabDict.TryGetValue(20000, out GameObject errorPrefab))
        {
            Debug.Log("없는 유닛코드요");
            return errorPrefab;
        }
            

        return null;
    }

    public GameObject GetEliteEffect(Transform unitTr)
    {
        return Instantiate(eliteEffect, Vector3.up, Quaternion.identity, unitTr);
    }
}

[System.Serializable]
public class EnemyPrefabEntry
{
    public int enemyId;
    public GameObject enemyPrefab;
}
