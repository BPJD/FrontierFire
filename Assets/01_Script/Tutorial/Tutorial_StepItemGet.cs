using UnityEngine;

public class Tutorial_StepItemGet : MonoBehaviour
{
    Direction_TutorialTeller tutorial;
    [SerializeField] int stepTarget = 5;

    private void Start()
    {
        GameObject tutorialObj = GameObject.Find("Stage_Tutorial");
        tutorial = tutorialObj.GetComponent<Direction_TutorialTeller>();
    }

    private void OnDestroy()
    {
        tutorial.TutorialStepSuccess(stepTarget);
    }
}
