using UnityEngine;

public class Ability_SpeederBuff : MonoBehaviour
{
    PlayerMove playerMove;
    UnitStatus playerStat;
    PlayerWeaponController weaponController;

    [SerializeField] float damageRevision = 0.5f;
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
            if (playerMove.isSprinting && playerMove.moveDir != 0f)
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
        playerStat.unitParamsAbility.u_damage += damageRevision;
        playerStat.SetCurrentAtk();
        weaponController.GetWeaponStatCur().ApplyStatusInSystem();
    }

    void BuffDeActive()
    {
        if (!isBuffActive) return;

        isBuffActive = false;
        playerStat.unitParamsAbility.u_damage -= damageRevision;
        playerStat.SetCurrentAtk();
        weaponController.GetWeaponStatCur().ApplyStatusInSystem();
    }
}
