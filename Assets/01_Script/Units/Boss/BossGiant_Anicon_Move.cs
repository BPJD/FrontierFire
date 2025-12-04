using UnityEngine;

public class BossGiant_Anicon_Move : StateMachineBehaviour
{
    public System.Action OnStart; // 스크립트에서 연결해서 쓰고 싶다면

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 외부 스크립트로 콜백 보내기
        if (OnStart != null)
            OnStart.Invoke();
    }
}