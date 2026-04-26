using UnityEngine;

public class Control_Stage : MonoBehaviour
{
    [SerializeField] private Data_Stages stageData;
    [SerializeField] private int stageCur = 0;
    [SerializeField] private int gameLevel = 1;
    [SerializeField] private int requireForBoss = 8;

    [SerializeField] private int world;

    [SerializeField] private GameObject customBossStage;

    private int playingStageID = -1;

    public int difficulty { get; private set; }
    public int worldCur { get; private set; }

    private readonly Vector3 stagePosition = new Vector3(200f, 0f, 0f);

    private UnitStatus playerStat;

    private void Awake()
    {
        worldCur = world;

        difficulty = ES3.Load<int>(Data_Strings.gameDifficultyKey, 1);
        gameLevel = difficulty;

        GameObject playerObj = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
        if (playerObj != null)
        {
            playerStat = playerObj.GetComponent<UnitStatus>();

            if (playerStat != null)
                playerStat.RefreshStageDifficultyStats(true);
        }
    }

    private void Start()
    {
        if (stageData == null)
        {
            GameObject dataObj = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag);

            if (dataObj != null)
                stageData = dataObj.GetComponent<Data_Stages>();
        }
    }

    /// <summary>
    /// 스테이지를 플레이합니다. typeCode: 0 = 일반, 1 = 정예, 2 = 보스
    /// </summary>
    public void StagePlay(int typeCode, Stage_NextStagePortal.RewardType rewardType)
    {
        if (stageData == null)
            return;

        StageType stageType = GetStageTypeFromInt(typeCode);
        int stageID = GetRandomStageIDAvoidPrevious(worldCur, stageType);

        if (stageID == -1)
            return;

        GameObject stageObj = stageData.GetStagePrefab(stageID);

        if (stageObj == null)
            return;

        stageCur++;

        GameObject stage = Instantiate(
            stageObj,
            stagePosition * stageCur,
            Quaternion.identity
        );

        StageModule module = stage.GetComponent<StageModule>();

        playingStageID = stageID;

        if (module == null)
            return;

        if (stageCur == requireForBoss - 1)
        {
            module.NextisBoss();
        }
        module.RewardObjSet(rewardType, stageType);
    }

    public void BossStagePlay()
    {
        stageCur++;

        if (worldCur != 0)
        {
            Instantiate(
                stageData.bossStages[worldCur - 1],
                stagePosition * stageCur,
                Quaternion.identity
            );
        }
        else
        {
            Instantiate(
                customBossStage,
                stagePosition * stageCur,
                Quaternion.identity
            );
        }
    }

    private int GetRandomStageIDAvoidPrevious(int world, StageType stageType)
    {
        int selectedID = stageData.GetRandomStageIDInWorld(world, stageType);

        if (selectedID == -1)
            return -1;

        if (playingStageID < 0)
            return selectedID;

        const int retryCount = 10;

        for (int i = 0; i < retryCount; i++)
        {
            if (selectedID != playingStageID)
                return selectedID;

            selectedID = stageData.GetRandomStageIDInWorld(world, stageType);

            if (selectedID == -1)
                return -1;
        }

        return selectedID;
    }

    /// <summary>
    /// 외부 int 값을 StageType으로 변환합니다.
    /// </summary>
    private StageType GetStageTypeFromInt(int type)
    {
        return type switch
        {
            1 => StageType.Elite,
            2 => StageType.Boss,
            _ => StageType.Normal,
        };
    }
}