using UnityEngine;

public class BossCorePhaseEndProj : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {

        BossCorePhaseEndRange pattern = GameObject.FindGameObjectWithTag("BossGimmick").GetComponent<BossCorePhaseEndRange>();
        pattern.PlayerThrow();

        Debug.Log(pattern.gameObject.name);

        Destroy(gameObject);
    }
}
