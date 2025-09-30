using UnityEngine;

public class Tutorial_FourTargets : MonoBehaviour
{
    [SerializeField] GameObject[] targets;

    private void OnTriggerEnter(Collider other)
    {
        for(int i = 0; i < targets.Length; i++)
        {
            targets[i].SetActive(true);
        }
    }
}
