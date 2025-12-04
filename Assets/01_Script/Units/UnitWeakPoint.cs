using UnityEngine;
using Combat;

public class UnitWeakPoint : MonoBehaviour
{
    UnitStatus unitStat;

    public float addDamage = 1.25f;

    public bool isNormalDamagePoint = false;

    private void Start()
    {
        unitStat = GetComponentInParent<UnitStatus>();

        if (isNormalDamagePoint)
        {
            addDamage = 1f;
        }
    }

    public void WeatPointDamage(in DamagePayload p)
    {

        int _DamageByWeakPoint = Mathf.FloorToInt(p.baseDamage * addDamage);

        var payload = DamagePayload.Create(
            baseDamage: _DamageByWeakPoint,
            ammo: p.ammo,
            atkType: p.atkType,
            isCritical: p.isCritical,
            isWeakPoint: !isNormalDamagePoint,
            hitPoint: p.hitPoint
        );


        if(unitStat.hpCur > 0)
        {
            unitStat.TakeDamage(payload);
        }

    }
}
