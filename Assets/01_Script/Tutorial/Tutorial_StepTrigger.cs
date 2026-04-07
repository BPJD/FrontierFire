using UnityEngine;

public class Tutorial_StepTrigger : MonoBehaviour
{
    [SerializeField] int stepTarget;
    Direction_TutorialTeller tutorial;

    private void Start()
    {
        tutorial = GetComponentInParent<Direction_TutorialTeller>();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Data_Strings.playerTag))
        {
            tutorial.TutorialStepSuccess(stepTarget);
        }
    }
}
