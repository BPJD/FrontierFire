using Michsky.UI.Heat;
using UnityEngine;

public class BossCoreMobSpawner : MonoBehaviour
{
    [SerializeField] Transform[] mobSpawnPoints;
    [SerializeField] GameObject[] mobSpawnPrefabs;
    [SerializeField] bool[] isSpawned;

    GameObject[] spawnedMobsCur;

    StageModule stageCon;
    Transform stageConTr;
    EnemyAIBroadcastManager aiManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stageCon = GetComponentInParent<StageModule>();
        stageConTr = stageCon.gameObject.transform;
        aiManager = stageCon.gameObject.GetComponent<EnemyAIBroadcastManager>();

        spawnedMobsCur = new GameObject[mobSpawnPoints.Length];

        SpawnMobs();
    }

    public void SpawnMobs()
    {
        for (int i = 0; i < mobSpawnPoints.Length; i++)
        {
            if (spawnedMobsCur[i] != null)
            {
                if(spawnedMobsCur[i].GetComponent<EnemyUnitAI_Controller>().state != EnemyUnitAI_Controller.UnitState.Dead)
                {
                    isSpawned[i] = true;
                }
            }

            if (!isSpawned[i])
            {
                GameObject mob = Instantiate(mobSpawnPrefabs[i], mobSpawnPoints[i].position, Quaternion.Euler(0f, -90f, 0f), stageConTr);

                var ai = mob.GetComponent<EnemyUnitAI_Controller>();
                mob.GetComponent<EnemyUnitAI_Controller>().isNotMove = true;

                spawnedMobsCur[i] = mob;

                aiManager?.Register(ai); // 연동 추가

                stageCon.MobSpawned();
            }
        }
    }
}
