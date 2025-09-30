using UnityEngine;
using UnityEngine.UIElements;

public class BossBomberAirborneSpawn : MonoBehaviour
{
    GameObject bossObj;
    BossBomberLookPlayer thisAI;

    StageModule stageCon;
    Transform stageObjTr;
    EnemyAIBroadcastManager aiManager;
    [SerializeField] Transform spawnPoint;

    [SerializeField] GameObject airborneUnit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemySystem"))
        {
            if (bossObj == null)
            {
                bossObj = other.gameObject;
                thisAI = bossObj.GetComponentInParent<BossBomberLookPlayer>();
            }

            if (thisAI != null && thisAI.attackPattern == BossBomberLookPlayer.BossMoveType.Airborne && thisAI.isAttackReady)
            {
                Spawn();
                thisAI.patternAttackCountCur++;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stageCon = GetComponentInParent<StageModule>();
        stageObjTr = stageCon.gameObject.transform;
        aiManager = stageCon.gameObject.GetComponent<EnemyAIBroadcastManager>();
    }

    void Spawn()
    {

        GameObject unit = Instantiate(airborneUnit, spawnPoint.position + (Vector3.down * 1.5f), Quaternion.Euler(0f, 90f, 0f), stageObjTr);

        var ai = unit.GetComponent<EnemyUnitAI_Controller>();


        aiManager?.Register(ai); // 연동 추가

        stageCon.MobSpawned();
    }
}
