using UnityEngine;

public class ReloadStateBehaviour : StateMachineBehaviour
{
    public System.Action OnReloadComplete; // 재장전 완료 이벤트

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (OnReloadComplete != null)
        {
            OnReloadComplete.Invoke();
        }
    }
}
