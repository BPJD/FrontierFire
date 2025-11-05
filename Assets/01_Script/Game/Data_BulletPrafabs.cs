using System.Collections.Generic;
using UnityEngine;

public class Data_BulletPrafabs : MonoBehaviour
{
    [SerializeField]
    private List<BulletPrefabEntry> bulletPrefabEntries = new List<BulletPrefabEntry>();

    private Dictionary<int, GameObject> bulletPrefabDict;


    [SerializeField] GameObject[] playerLaserScopes;

    void Awake()
    {
        bulletPrefabDict = new Dictionary<int, GameObject>();
        foreach (var entry in bulletPrefabEntries)
        {
            if (!bulletPrefabDict.ContainsKey(entry.bulletId))
                bulletPrefabDict.Add(entry.bulletId, entry.bulletPrefab);
        }
    }

    public GameObject GetBulletPrefab(int id)
    {
        if (bulletPrefabDict.TryGetValue(id, out GameObject prefab))
            return prefab;

        return null;
    }

    public GameObject GetLaserScopePrefab(int code)
    {
        return playerLaserScopes[code];
    }
}

[System.Serializable]
public class BulletPrefabEntry
{
    public int bulletId;
    public GameObject bulletPrefab;
}