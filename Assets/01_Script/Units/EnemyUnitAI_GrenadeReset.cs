using UnityEngine;

public class EnemyUnitAI_GrenadeReset : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyWeapon_Grenade grenade = animator.GetComponentInChildren<EnemyWeapon_Grenade>();
        if (grenade != null)
        {
            grenade.ResetGrenadeState();
        }
    }
}
