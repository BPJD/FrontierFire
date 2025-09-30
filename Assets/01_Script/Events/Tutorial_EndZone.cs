using UnityEngine;

public class Tutorial_EndZone : MonoBehaviour
{
    [SerializeField] Tutorial_StartZone startZone;

    private void OnTriggerEnter(Collider other)
    {
        startZone.TutorialEnd();
        gameObject.SetActive(false);
    }
}
