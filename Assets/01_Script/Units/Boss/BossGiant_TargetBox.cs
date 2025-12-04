using UnityEngine;

public class BossGiant_TargetBox : MonoBehaviour
{

    public bool isPlayerInBox { get; private set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Data_Strings.playerTag))
        {
            isPlayerInBox = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Data_Strings.playerTag))
        {
            isPlayerInBox = false;
        }
    }
}
