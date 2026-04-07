using UnityEngine;

public class Tutorial_ChestStep : MonoBehaviour
{
    Direction_TutorialTeller tutorial;
    DefaultUnit_Chest chest;
    [SerializeField] int stepTarget = 4;

    private void Start()
    {
        tutorial = GetComponentInParent<Direction_TutorialTeller>();
        chest = GetComponent<DefaultUnit_Chest>();
    }

    private void Update()
    {
        if(chest.isOpened && tutorial.tutorialStepTarget == stepTarget)
        {
            tutorial.TutorialStepSuccess(stepTarget);
            this.enabled = false;
        }
    }
}
