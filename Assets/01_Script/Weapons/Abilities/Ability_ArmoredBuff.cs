using UnityEngine;

public class Ability_ArmoredBuff : MonoBehaviour, IAbilityUpgradable
{
    PlayerMove playerMove;
    UnitStatus unitStat;

    [SerializeField] float recieveDamageRevision = -0.3f;

    bool isBuffActivated = false;


    void Start()
    {
        GameObject _player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);

        playerMove = _player.GetComponent<PlayerMove>();
        unitStat = _player.GetComponent<UnitStatus>();
    }

    private void FixedUpdate()
    {
        if (playerMove != null)
        {
            if (!playerMove.isJumping)
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
        unitStat.unitParamsAbility.u_immunePer += recieveDamageRevision;
        unitStat.SetCurrentAtk();
    }

    void BuffDeActive()
    {
        if (!isBuffActivated || unitStat == null) return;

        isBuffActivated = false;
        unitStat.unitParamsAbility.u_immunePer -= recieveDamageRevision;
        unitStat.SetCurrentAtk();
    }


    public void UpgradeAbility()
    {
        recieveDamageRevision -= 0.08f;
    }

}
