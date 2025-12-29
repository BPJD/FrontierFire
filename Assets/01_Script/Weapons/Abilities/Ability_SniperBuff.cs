using UnityEngine;

public class Ability_SniperBuff : MonoBehaviour
{
    PlayerMove playerMove;
    UnitStatus playerStat;
    PlayerWeaponController weaponController;

    [SerializeField] float criRateRevision = 50f;
    bool isBuffActive = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMove = GetComponentInParent<PlayerMove>();
        playerStat = GetComponentInParent<UnitStatus>();
        weaponController = GetComponentInParent<PlayerWeaponController>();
    }


    private void FixedUpdate()
    {
        if (playerMove != null)
        {
            if (Mathf.Abs(playerMove.moveDir) <= 0.1f)
            {
                BuffActive();
            }
            else
            {
                BuffDeActive();
            }
        }
    }

    void BuffActive()
    {
        if (isBuffActive) return;

        isBuffActive = true;
        playerStat.unitParamsAbility.u_criRate += criRateRevision;
        playerStat.SetCurrentAtk();
        weaponController.GetWeaponStatCur().ApplyStatusInSystem();
    }

    void BuffDeActive()
    {
        if (!isBuffActive) return;

        isBuffActive = false;
        playerStat.unitParamsAbility.u_criRate -= criRateRevision;
        playerStat.SetCurrentAtk();
        weaponController.GetWeaponStatCur().ApplyStatusInSystem();
    }
}
