using UnityEngine;

public class Tutorial_EnemyKillStep : MonoBehaviour
{
    UnitStatus stat;
    [SerializeField] int stepTarget = 8;
    Direction_TutorialTeller tutorial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stat = GetComponent<UnitStatus>();
        tutorial = GetComponentInParent<Direction_TutorialTeller>();
    }

    // Update is called once per frame
    void Update()
    {
        if(stat.hpCur <= 0 && tutorial.tutorialStepTarget == stepTarget)
        {
            tutorial.TutorialStepSuccess(stepTarget);
            this.enabled = false;
        }
    }
}
