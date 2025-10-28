using UnityEngine;

public class Control_Stage : MonoBehaviour
{
    [SerializeField] private Data_Stages stageData; // Data_Stages 참조
    [SerializeField] private int stageCur = 0;
    [SerializeField] private int gameLevel = 1; // 현재 레벨 (난이도)
    [SerializeField] int requireForBoss = 8;
    

    [SerializeField] private int world;       // 현재 지역


    public int difficulty { get; private set; }
    public int worldCur { get; private set; }

    private Vector3 stagePosition = new Vector3(200f, 0f, 0f);

    private void Awake()
    {
        worldCur = world;
        difficulty = gameLevel;
    }

    void Start()
    {
        if (stageData == null)
        {
            GameObject dataObj = GameObject.FindGameObjectWithTag("Data");
            stageData = dataObj.GetComponent<Data_Stages>();
        }
    }

    /// <summary>
    /// 스테이지를 플레이합니다. (0: 일반, 1: 정예, 2: 보스)
    /// </summary>
    public void StagePlay(int typeCode)
    {
        StageType stageType = GetStageTypeFromInt(typeCode);
        int stageID = stageData.GetRandomStageIDInWorld(worldCur, stageType);

        if (stageID == -1)
        {
            Debug.LogWarning("[Control_Stage] 유효한 스테이지 ID를 찾지 못했습니다.");
            return;
        }

        GameObject stageObj = stageData.GetStagePrefab(stageID);

        if (stageObj == null)
        {
            Debug.LogWarning($"[Control_Stage] 스테이지 프리팹을 찾을 수 없습니다. ID: {stageID}");
            return;
        }

        stageCur++;
        GameObject stage = Instantiate(stageObj, stagePosition * stageCur, Quaternion.identity);

        if(stageCur == requireForBoss - 1)
        {
            stage.GetComponent<StageModule>().NextisBoss();
        }
        Debug.Log($"[Control_Stage] Stage {stageID} 생성 완료");
    }

    public void BossStagePlay()
    {
        stageCur++;
        Instantiate(stageData.bossStages[gameLevel - 1], stagePosition * stageCur, Quaternion.identity);
    }

    /// <summary>
    /// 외부 int → StageType 변환
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
