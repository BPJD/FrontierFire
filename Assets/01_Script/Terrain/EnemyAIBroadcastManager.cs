using System.Collections.Generic;
using UnityEngine;

public class EnemyAIBroadcastManager : MonoBehaviour
{
    private List<EnemyUnitAI_Controller> registeredUnits = new List<EnemyUnitAI_Controller>();
    private float broadcastRadius = 5f;

    public void Register(EnemyUnitAI_Controller unit)
    {
        if (!registeredUnits.Contains(unit))
            registeredUnits.Add(unit);
    }

    public void Unregister(EnemyUnitAI_Controller unit)
    {
        registeredUnits.Remove(unit);
    }

    public void BroadcastEngage(Vector3 fromPosition)
    {
        foreach (var unit in registeredUnits)
        {
            if (unit == null || unit.IsDead()) continue;

            float sqrDistance = (unit.transform.position - fromPosition).sqrMagnitude;
            if (sqrDistance <= broadcastRadius * broadcastRadius)
            {
                // 이미 Chase/Attack 중이면 다시 호출 X
                if (unit.IsEngagingOrChasing()) continue;

                unit.ForceChase(); // 감지 상태로 강제 진입
            }
        }
    }
}
