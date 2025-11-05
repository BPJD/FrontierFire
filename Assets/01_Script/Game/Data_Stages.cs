using System.Collections.Generic;
using UnityEngine;

public enum StageType
{
    Normal,
    Elite,
    Boss
}

[System.Serializable]
public class StagePrefabEntry
{
    public int stageId;
    public GameObject stagePrefab;
}

public class Data_Stages : MonoBehaviour
{
    public GameObject[] bossStages;

    [SerializeField]
    private List<StagePrefabEntry> stagePrefabEntries = new List<StagePrefabEntry>();

    private Dictionary<int, GameObject> stagePrefabDict;

    void Awake()
    {
        stagePrefabDict = new Dictionary<int, GameObject>();

        foreach (var entry in stagePrefabEntries)
        {
            if (!stagePrefabDict.ContainsKey(entry.stageId))
                stagePrefabDict.Add(entry.stageId, entry.stagePrefab);
        }
    }

    /// <summary>
    /// ID로 스테이지 프리팹을 가져옵니다.
    /// </summary>
    public GameObject GetStagePrefab(int id)
    {
        if (stagePrefabDict.TryGetValue(id, out GameObject prefab))
            return prefab;

        return null;
    }

    /// <summary>
    /// 해당 월드의 특정 타입 스테이지 수를 셉니다.
    /// </summary>
    public int CountStagesInWorld(int worldIndex, StageType type)
    {
        int count = 0;

        foreach (var stageId in stagePrefabDict.Keys)
        {
            if (GetStageType(stageId) != type)
                continue;

            int currentWorldIndex = GetWorldIndex(stageId);
            if (currentWorldIndex == worldIndex)
                count++;
        }

        return count;
    }

    /// <summary>
    /// 스테이지 ID를 통해 타입을 판별합니다.
    /// </summary>
    private StageType GetStageType(int stageId)
    {
        if (stageId >= 50000)
            return StageType.Boss;

        int offset = (stageId - 40000) % 1000;
        if (offset < 500)
            return StageType.Normal;
        else
            return StageType.Elite;
    }

    /// <summary>
    /// 스테이지 ID를 통해 월드 인덱스를 계산합니다.
    /// </summary>
    private int GetWorldIndex(int stageId)
    {
        if (stageId >= 50000)
            return stageId - 50000;

        return (stageId - 40000) / 1000;
    }

    public int GetRandomStageIDInWorld(int worldIndex, StageType type)
    {
        List<int> matchingIds = new List<int>();

        foreach (var entry in stagePrefabEntries)
        {
            int id = entry.stageId;

            if (GetStageType(id) != type)
                continue;

            if (GetWorldIndex(id) != worldIndex)
                continue;

            matchingIds.Add(id);
        }

        if (matchingIds.Count == 0)
        {
            //Debug.LogWarning($"[Data_Stages] 월드 {worldIndex}에 {type} 타입의 스테이지가 없습니다.");
            return -1;
        }

        int selectedIndex = Random.Range(0, matchingIds.Count);
        return matchingIds[selectedIndex];
    }
}
