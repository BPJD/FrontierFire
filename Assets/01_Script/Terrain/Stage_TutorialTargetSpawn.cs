using UnityEngine;
using System.Collections.Generic;  // ← 리스트 쓰려면 필요

public class Stage_TutorialTargetSpawn : MonoBehaviour
{
    [SerializeField] Transform[] targetPoints;
    [SerializeField] GameObject[] targetObjs;

    int remainTargets = 0;
    public bool isTargetZero { get; private set; } = false;

    public List<GameObject> spawnedTargets { get; private set; } = new List<GameObject>();

    public void SpawnTargets()
    {
        // 기존 리스트 초기화
        spawnedTargets.Clear();
        remainTargets = 0;
        isTargetZero = false;

        for (int i = 0; i < targetPoints.Length; i++)
        {
            int randObj = Random.Range(0, targetObjs.Length);
            GameObject spawned = Instantiate(targetObjs[randObj], targetPoints[i].position, Quaternion.identity, targetPoints[i]);

            spawnedTargets.Add(spawned);  // 리스트에 추가
            remainTargets++;
        }
    }

    public void TargetHit(GameObject target)
    {
        // 리스트에서 제거 (안전성을 위해 null 체크도 가능)
        if (spawnedTargets.Contains(target))
        {
            spawnedTargets.Remove(target);
        }

        remainTargets--;
        if (remainTargets <= 0)
        {
            remainTargets = 0;
            isTargetZero = true;
        }
    }

    public void RemoveTargets()
    {
        for(int i = 0; i < spawnedTargets.Count; i++)
        {
            GameObject target = spawnedTargets[i];
            Destroy(target);
        }
    }
}
