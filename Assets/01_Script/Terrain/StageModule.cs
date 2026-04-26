using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageModule : MonoBehaviour
{
    DataPortals portalData;
    Transform[] portalPoints;
    [SerializeField] int remainEnemies = 0;
    Data_RewardObjs rewardData;

    Data_Enemies enemyData;
    //    [SerializeField] int level = 1;

    Stage_EnemySpawnPoint[] spawnPoints;
    GameObject playerUnit;
    [SerializeField] Transform startPoint;
    EnemyAIBroadcastManager broadcastManager;

    [SerializeField] List<GameObject> rewardObjs = new List<GameObject>();

    public bool isBossStage = false;
    bool nextIsBoss = false;

    bool isAllGenerated = false;

    GameSoundPlayer soundPlayer;

    float generateDelay = 0.5f;
    WaitForSeconds delay;
    int generateCount = 2;

    Data_AudioClips clipData;
    [SerializeField] GameSoundPlayer.SoundType portalSoundType = GameSoundPlayer.SoundType.SFX;

    [SerializeField] GameObject spawnPointPlayEft;
    [SerializeField] GameObject spawnPointIdleEft;

    [SerializeField] bool isMainStage = false;

    [SerializeField] AudioClip stageBgm;
    Direction_BGMPlay bgmPlayer;

    GameObject data;

    void Start()
    {
        data = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag);
        portalData = data.GetComponent<DataPortals>();
        enemyData = data.GetComponent<Data_Enemies>();
        rewardData = data.GetComponent<Data_RewardObjs>();
        playerUnit = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
        soundPlayer = GameObject.FindGameObjectWithTag("Sound").GetComponent<GameSoundPlayer>();
        bgmPlayer = soundPlayer.gameObject.GetComponent<Direction_BGMPlay>();

        if (stageBgm != null)
        {
            bgmPlayer.PlayBGM(stageBgm, 3f);
        }

        clipData = soundPlayer.gameObject.GetComponent<Data_AudioClips>();
        broadcastManager = GetComponent<EnemyAIBroadcastManager>();

        PlayerMoveToStart();

        delay = new WaitForSeconds(generateDelay);


        // 자식 중 Portal 태그를 가진 오브젝트만 필터링
        List<Transform> portalList = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.CompareTag("PortalPoint"))
            {
                portalList.Add(child.transform);
            }
        }

        if (!isMainStage)
        {
            portalPoints = portalList.ToArray();

            spawnPoints = GetComponentsInChildren<Stage_EnemySpawnPoint>();

            StartCoroutine(GenerateEnemies());
        }
        
    }


    IEnumerator GenerateEnemies()
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < spawnPoints.Length; i += generateCount)
        {
            for (int j = 0; j < generateCount; j++)
            {
                int spawnIndex = i + j;

                if (spawnIndex < spawnPoints.Length)
                    SpawnEnemy(spawnIndex);
            }

            yield return delay;
        }
        isAllGenerated = true;
        CheckEnemyRemains();
    }


    void SpawnEnemy(int spawnCount)
    {
        Vector3 _position = spawnPoints[spawnCount].transform.position;
        GameObject prefab = enemyData.GetEnemyPrefab(spawnPoints[spawnCount].mobCode);

        Vector3 _rotation = new Vector3(0f, spawnPoints[spawnCount].isLookingRight ? 90f : -90f, 0f);

        spawnPoints[spawnCount].SpawnEftPlay(spawnPointPlayEft);
        GameObject unit = Instantiate(prefab, _position, Quaternion.Euler(_rotation), transform);

        var ai = unit.GetComponent<EnemyUnitAI_Controller>();

        if(ai != null)
        {
            if (spawnPoints[spawnCount].isPatrol)
            {
                ai.state = EnemyUnitAI_Controller.UnitState.Patrol;
            }
            if (spawnPoints[spawnCount].isNotMoving)
            {
                ai.isNotMove = true;
            }
        }

        broadcastManager?.Register(ai); // 연동 추가

        remainEnemies++;
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
        PlayerUIRangeSet(false);

        if (!isBossStage)
        {
            if (remainEnemies <= 0)
            {
                if (spawnPoints.Length > 0)
                {

                    DropRewards();
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

        bgmPlayer.FadeCurrentVolume(1f, 1f);
    }

    public void BossStageClear()
    {
        StartCoroutine(BossClear());
    }

    IEnumerator BossClear()
    {
        yield return new WaitForSeconds(1.5f);
        Control_Stage _stageCon = GameObject.FindGameObjectWithTag("GameController").GetComponent<Control_Stage>();
        Data_Scenes _sceneData = data.GetComponent<Data_Scenes>();
        if (_stageCon.worldCur == _sceneData.stageScenes.Length - 1)
        {
            GameObject _module = GameObject.FindGameObjectWithTag("Module");
            _module.GetComponent<Direction_GameOver>().GameWin();
        }
        else
        {
            PortalGenerate();
            DropRewards();
        }
    }

    void PortalGenerate()
    {
        AudioClip _clip = clipData.GetPortalSoundClipByPortalType(0);

        if (nextIsBoss)
        {
            _clip = clipData.GetPortalSoundClipByPortalType(2);
            Instantiate(portalData.portalObjs[2], portalPoints[0].position, Quaternion.identity, transform);
            soundPlayer.GameSoundPlayByType(_clip, portalSoundType);
            bgmPlayer.StopBGM(2f);
        }
        else
        {
            soundPlayer.GameSoundPlayByType(_clip, portalSoundType);
            if (isBossStage)
            {

                Instantiate(portalData.stageClearPortalObj, portalPoints[0].position, Quaternion.identity, transform);
            }
            else
            {
                bgmPlayer.FadeCurrentVolume(0.5f, 2f);
                for (int i = 0; i < portalPoints.Length; i++)
                {
                    int _rand = Random.Range(0, 2);
                    Instantiate(portalData.portalObjs[_rand], portalPoints[i].position, Quaternion.identity, transform);
                }
            }
        }


        PlayerUIRangeSet(true);
    }


    void DropRewards()
    {
        if (rewardObjs == null || rewardObjs.Count <= 0)
        {
            Debug.LogWarning("[StageModule] DropRewards failed. rewardObjs is empty.");
            playerUnit.GetComponent<PlayerHeal>().HealFlag();
            return;
        }

        for (int i = 0; i < rewardObjs.Count; i++)
        {
            if (rewardObjs[i] == null)
            {
                Debug.LogWarning($"[StageModule] rewardObjs[{i}] is null.");
                continue;
            }

            Instantiate(rewardObjs[i], playerUnit.transform.position, Quaternion.identity);
        }

        rewardObjs.Clear();

        playerUnit.GetComponent<PlayerHeal>().HealFlag();
    }


    void PlayerUIRangeSet(bool isStageClear)
    {
        float _uiShowDistance = isStageClear ? 20f : 5f;

        playerUnit.GetComponentInChildren<PlayerInteract>().uiShowDistance = _uiShowDistance;
    }

    public GameObject GetIdleParticleObj()
    {
        return spawnPointIdleEft;
    }

    public void RewardObjSet(Stage_NextStagePortal.RewardType rewardType, StageType stageType)
    {
        if(rewardData == null)
        {
            data = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag);
            rewardData = data.GetComponent<Data_RewardObjs>();
        }

        rewardObjs.Add(rewardData.GetRewardObj(rewardType, stageType));
    }
}
