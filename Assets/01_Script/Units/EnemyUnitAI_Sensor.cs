using UnityEngine;

public class EnemyUnitAI_Sensor : MonoBehaviour
{
    SphereCollider sensor;

    //플레이어 탐지에 대한 스크립트
    EnemyUnitAI_Controller aiCon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        aiCon = GetComponentInParent<EnemyUnitAI_Controller>();
        sensor = GetComponent<SphereCollider>();
        sensor.radius = GetComponentInParent<EnemyAttackSystem>().sightRange;
    }

    private void OnTriggerEnter(Collider other)
    {
        aiCon.PlayerApproach(true);
    }
    private void OnTriggerExit(Collider other)
    {
        aiCon.PlayerApproach(false);
    }
}
