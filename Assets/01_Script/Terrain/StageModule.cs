using System.Collections.Generic;
using UnityEngine;

public class StageModule : MonoBehaviour
{
    DataPortals portalData;
    Transform[] portalPoints;
    int remainEnemies = 0;

    Data_Enemies enemyData;
    //    [SerializeField] int level = 1;

    Stage_EnemySpawnPoint[] spawnPoints;
    GameObject playerUnit;
    [SerializeField] Transform startPoint;
    EnemyAIBroadcastManager broadcastManager;

    [SerializeField] GameObject rewardObj;

    public bool isBossStage = false;
    bool nextIsBoss = false;


    private void Awake()
    {
        portalData = GameObject.FindGameObjectWithTag("Data").GetComponent<DataPortals>();
        enemyData = GameObject.FindGameObjectWithTag("Data").GetComponent<Data_Enemies>();
        playerUnit = GameObject.FindGameObjectWithTag("Player");
        broadcastManager = GetComponent<EnemyAIBroadcastManager>();
    }

    void Start()
    {
        PlayerMoveToStart();



        // 자식 중 Portal 태그를 가진 오브젝트만 필터링
        List<Transform> portalList = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.CompareTag("PortalPoint"))
            {
                portalList.Add(child.transform);
            }
        }
        portalPoints = portalList.ToArray();

        spawnPoints = GetComponentsInChildren<Stage_EnemySpawnPoint>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Vector3 _position = spawnPoints[i].transform.position;
            GameObject prefab = enemyData.GetEnemyPrefab(spawnPoints[i].mobCode);

            Vector3 _rotation = new Vector3(0f, spawnPoints[i].isLookingRight ? 90f : -90f, 0f);

            GameObject unit = Instantiate(prefab, _position, Quaternion.Euler(_rotation), transform);

            var ai = unit.GetComponent<EnemyUnitAI_Controller>();
            if (spawnPoints[i].isPatrol)
            {
                ai.state = EnemyUnitAI_Controller.UnitState.Patrol;
            }
            if (spawnPoints[i].isNotMoving)
            {
                ai.isNotMove = true;
            }


            broadcastManager?.Register(ai); // 연동 추가

            remainEnemies++;
        }

        /**
        for(int i = 0; i < spawnPoints.Length; i++)
        {
            Vector3 _position = spawnPoints[i].transform.position;
            GameObject _unit = enemyData.GetEnemyPrefab(spawnPoints[i].mobCode);

            Vector3 _rotation = new Vector3(0f, -90f, 0f);
            if (spawnPoints[i].isLookingRight)
            {
                _rotation.y = 90f;
            }

            Instantiate(_unit, _position, Quaternion.Euler(_rotation), transform);
            if (spawnPoints[i].isPatrol)
            {
                _unit.GetComponent<EnemyUnitAI_Controller>().state = EnemyUnitAI_Controller.UnitState.Patrol;
            }
            
            remainEnemies++;
        }
        **/

        CheckEnemyRemains();


    }

    public void NextisBoss()
    {
        nextIsBoss = true;
    }

    public void MobSpawned()
    {
        remainEnemies++;
    }

    public void EnemyCountDown()
    {
        remainEnemies--;
        CheckEnemyRemains();
    }

    void CheckEnemyRemains()
    {
        if (!isBossStage)
        {
            if (remainEnemies <= 0)
            {
                if (spawnPoints.Length > 0)
                {
                    Instantiate(rewardObj, playerUnit.transform.position, Quaternion.identity);
                }

                PortalGenerate();
            }
        }
    }

    void PlayerMoveToStart()
    {
        if (playerUnit != null)
        {
            if (startPoint != null)
            {
                playerUnit.transform.position = startPoint.position;
            }
            else
            {
                playerUnit.transform.position = (gameObject.transform.position + Vector3.up);
            }

            playerUnit.GetComponent<PlayerStageOut>().returnPos = playerUnit.transform.position;
        }
    }

    public void BossStageClear()
    {
        PortalGenerate();
        Debug.Log("보스 클리어");
    }

    void PortalGenerate()
    {

        if (nextIsBoss)
        {
            Instantiate(portalData.portalObjs[2], portalPoints[0].position, Quaternion.identity, transform);
        }
        else
        {
            if (isBossStage)
            {

            }
            else
            {
                for (int i = 0; i < portalPoints.Length; i++)
                {
                    int _rand = Random.Range(0, 2);
                    Instantiate(portalData.portalObjs[_rand], portalPoints[i].position, Quaternion.identity, transform);
                }
            }
        }

    }
}
