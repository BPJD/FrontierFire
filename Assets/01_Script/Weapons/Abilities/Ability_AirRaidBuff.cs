using UnityEngine;

public class Ability_AirRaidBuff : MonoBehaviour
{
    PlayerMove playerMove;
    UnitStatus unitStat;
    PlayerWeaponController weaponController;

    [SerializeField] float damageRevision = 1f;
    [SerializeField] float recieveDamageRevision = 0.5f;

    bool isBuffActivated = false;


    void Start()
    {
        GameObject _player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
        playerMove =_player.GetComponent<PlayerMove>();
        unitStat = _player.GetComponentInParent<UnitStatus>();
        weaponController = _player.GetComponentInParent<PlayerWeaponController>();
    }

    private void FixedUpdate()
    {
        if (playerMove != null)
        {
            if (playerMove.isJumping)
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
        if (isBuffActivated || unitStat == null) return;

        isBuffActivated = true;
        unitStat.unitParamsAbility.u_damage += damageRevision;
        unitStat.unitParamsAbility.u_immunePer += recieveDamageRevision;
        unitStat.SetCurrentAtk();
        weaponController.GetWeaponStatCur().ApplyStatusInSystem();
    }

    void BuffDeActive()
    {
        if (!isBuffActivated || unitStat == null) return;

        isBuffActivated = false;
        unitStat.unitParamsAbility.u_damage -= damageRevision;
        unitStat.unitParamsAbility.u_immunePer -= recieveDamageRevision;
        unitStat.SetCurrentAtk();
        weaponController.GetWeaponStatCur().ApplyStatusInSystem();
    }

}
