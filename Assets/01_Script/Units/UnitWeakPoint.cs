using UnityEngine;
using Combat;

public class UnitWeakPoint : MonoBehaviour
{
    UnitStatus unitStat;

    [SerializeField] float addDamage = 1.25f;

    private void Start()
    {
        unitStat = GetComponentInParent<UnitStatus>();
    }

    public void WeatPointDamage(in DamagePayload p)
    {
        int _DamageByWeakPoint = Mathf.FloorToInt(p.baseDamage * addDamage);

        var payload = DamagePayload.Create(
            baseDamage: _DamageByWeakPoint,
            ammo: p.ammo,
            atkType: p.atkType,
            isCritical: p.isCritical,
            isWeakPoint: true,
            hitPoint: p.hitPoint
        );

        unitStat.TakeDamage(payload);



    }
}
