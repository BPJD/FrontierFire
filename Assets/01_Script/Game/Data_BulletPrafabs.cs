using System.Collections.Generic;
using UnityEngine;

public class Data_BulletPrafabs : MonoBehaviour
{
    [SerializeField]
    private List<BulletPrefabEntry> bulletPrefabEntries = new List<BulletPrefabEntry>();

    private Dictionary<int, GameObject> bulletPrefabDict;

    void Awake()
    {
        bulletPrefabDict = new Dictionary<int, GameObject>();
        foreach (var entry in bulletPrefabEntries)
        {
            if (!bulletPrefabDict.ContainsKey(entry.bulletId))
                bulletPrefabDict.Add(entry.bulletId, entry.bulletPrefab);
            else
                Debug.LogWarning($"중복된 ID 존재: {entry.bulletId}");
        }
    }

    public GameObject GetBulletPrefab(int id)
    {
        if (bulletPrefabDict.TryGetValue(id, out GameObject prefab))
            return prefab;

        Debug.LogWarning($"ID {id} 프리팹 없음");
        return null;
    }
}

[System.Serializable]
public class BulletPrefabEntry
{
    public int bulletId;
    public GameObject bulletPrefab;
}