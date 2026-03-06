using UnityEngine;

public class Tutorial_StartZone : MonoBehaviour
{
    [SerializeField] GameObject endZone;
    [SerializeField] Stage_TutorialTargetSpawn targetSystem;
    [SerializeField] Tutorial_TimerController timerController;


    private void OnTriggerEnter(Collider other)
    {
        endZone.SetActive(true);
        if (!timerController.isTimerStart)
        {
            targetSystem.SpawnTargets();
        }
        timerController.TutorialStart();
    }


    public void TutorialEnd()
    {
        timerController.TutorialEnd(targetSystem.isTargetZero);

        if (!targetSystem.isTargetZero)
        {
            targetSystem.RemoveTargets();
        }

    }

}
