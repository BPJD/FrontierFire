using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] GameObject doorObj;

    public void NeutralUnitDead()
    {
        doorObj.SetActive(false);
    }
}
