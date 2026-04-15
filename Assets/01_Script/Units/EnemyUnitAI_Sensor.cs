using UnityEngine;

public class EnemyUnitAI_Sensor : MonoBehaviour
{
    SphereCollider sensor;

    //플레이어 탐지에 대한 스크립트
    EnemyUnitAI_Controller aiCon;
    TurretAttackSystem turretCon;
    [SerializeField] bool isAttackFirst = true; //적이 플레이어를 먼저 공격할지, 플레이어가 먼저 공격할지

    [SerializeField] bool isRangeCustom = false;

    bool isTurret = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        aiCon = GetComponentInParent<EnemyUnitAI_Controller>();
        isTurret = aiCon == null ? true : false;

        sensor = GetComponent<SphereCollider>();

        if(!isRangeCustom)
        {
            if (!isTurret) //인간형 적 유닛
            {
                sensor.radius = GetComponentInParent<EnemyAttackSystem>().sightRange;
            }
            else //포탑형 적 유닛
            {
                turretCon = GetComponentInParent<TurretAttackSystem>();
                sensor.radius = turretCon.sightRange;
            }
        }


        if (!isAttackFirst)
        {
            sensor.enabled = false;
            sensor.radius = 0f;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject + ", " + other.gameObject);
        SetPlayerApproach(true);
    }
    private void OnTriggerExit(Collider other)
    {
        SetPlayerApproach(false);
    }


    void SetPlayerApproach(bool isApproach)
    {
        switch (isTurret)
        {
            case true:
                if(turretCon == null)
                {
                    turretCon = GetComponentInParent<TurretAttackSystem>();
                }
                turretCon.PlayerApproach(isApproach);
                break;
            case false:
                if (aiCon == null)
                {
                    aiCon = GetComponentInParent<EnemyUnitAI_Controller>();
                }
                aiCon.PlayerApproach(isApproach);
                break;
        }
    }


}
