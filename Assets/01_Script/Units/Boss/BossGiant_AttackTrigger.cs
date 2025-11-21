using Combat;
using UnityEngine;

public class BossGiant_AttackTrigger : MonoBehaviour
{
    
    [SerializeField] BossGiantAttackControl.GiantPattern pattern;
    BossGiantAttackControl giantAI;
    UnitStatus unitStat;
    Transform target;

    [SerializeField] float damageRevision = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        giantAI = GetComponentInParent<BossGiantAttackControl>();
        unitStat = GetComponentInParent<UnitStatus>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (giantAI.isPatternUsing)
        {
            switch (pattern)
            {
                case BossGiantAttackControl.GiantPattern.Swing:
                    if (other.CompareTag(Data_Strings.playerTag))
                    {
                        target = other.transform;
                        other.gameObject.GetComponent<UnitStatus>().TakeDamage(DamagePayLoad());
                    }
                    break;
                case BossGiantAttackControl.GiantPattern.Smash:

                    break;
                case BossGiantAttackControl.GiantPattern.RStomp:

                    break;
                case BossGiantAttackControl.GiantPattern.StoneStomp:

                    break;
                default:
                    break;
            }
        }
    }


    DamagePayload DamagePayLoad()
    {
        var payload = DamagePayload.Create(
        baseDamage: (int)(unitStat.atkCur * damageRevision),
        ammo: 0,
        atkType: WeaponParamsSO.AtkTypes.Normal,
        isCritical: false,
        isWeakPoint: false,
        hitPoint: target.position
        );

        return payload;
    }




}
