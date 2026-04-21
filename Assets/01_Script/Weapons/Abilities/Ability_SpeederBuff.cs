using UnityEngine;

public class Ability_SpeederBuff : MonoBehaviour, IAbilityUpgradable
{
    PlayerMove playerMove;
    UnitStatus playerStat;
    PlayerWeaponController weaponController;

    [SerializeField] float damageRevision = 0.5f;
    bool isBuffActive = false;

    float revisionCur = 0f;

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
            if (playerMove.moveDir != 0f)
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

        float t = Mathf.InverseLerp(2.5f, 8f, playerMove.moveSpeed_anim);
        revisionCur = Mathf.Lerp(0.1f, 0.75f, t);

        playerStat.unitParamsAbility.u_damage += revisionCur;
        playerStat.SetCurrentAtk();
        weaponController.GetWeaponStatCur().ApplyStatusInSystem();
    }

    void BuffDeActive()
    {
        if (!isBuffActive) return;

        isBuffActive = false;
        playerStat.unitParamsAbility.u_damage -= revisionCur;
        playerStat.SetCurrentAtk();
        weaponController.GetWeaponStatCur().ApplyStatusInSystem();
    }

    public void UpgradeAbility()
    {
        damageRevision += 0.15f;
    }
}
