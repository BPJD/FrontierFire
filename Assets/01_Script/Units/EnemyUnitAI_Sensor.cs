using UnityEngine;

public class EnemyUnitAI_Sensor : MonoBehaviour
{
    SphereCollider sensor;

    //플레이어 탐지에 대한 스크립트
    EnemyUnitAI_Controller aiCon;
    TurretAttackSystem turretCon;

    bool isTurret = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        aiCon = GetComponentInParent<EnemyUnitAI_Controller>();
        sensor = GetComponent<SphereCollider>();

        if (aiCon != null) //인간형 적 유닛
        {
            sensor.radius = GetComponentInParent<EnemyAttackSystem>().sightRange;
        }
        else //포탑형 적 유닛
        {
            isTurret = true;
            turretCon = GetComponentInParent<TurretAttackSystem>();
            sensor.radius = turretCon.sightRange;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
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
                turretCon.PlayerApproach(isApproach);
                break;
            case false:
                aiCon.PlayerApproach(isApproach);
                break;
        }
    }


}
