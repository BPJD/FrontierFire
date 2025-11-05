using UnityEngine;

public class Tutorial_StartZone : MonoBehaviour
{
    [SerializeField] GameObject endZone;
    [SerializeField] Stage_TutorialTargetSpawn targetSystem;

    bool isTutorialStarted = false;
    float time = 0f;

    private void OnTriggerEnter(Collider other)
    {
        endZone.SetActive(true);
        targetSystem.SpawnTargets();
        isTutorialStarted = true;
    }

    private void Update()
    {
        if (isTutorialStarted)
        {
            time += Time.deltaTime;
        }
    }

    public void TutorialEnd()
    {
        if (targetSystem.isTargetZero)
        {
            Debug.Log("당신의 훈련장 기록 : " + time + " 초");
            isTutorialStarted = false;
            time = 0f;
        }
        else
        {
            Debug.Log("모든 타겟을 맞추지 못했습니다.  걸린 시간 : " + time + " 초");
            isTutorialStarted = false;
            time = 0f;
            targetSystem.RemoveTargets();
        }
        
    }

}
