using UnityEngine;

public class BossGiantDripStoneHit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        BossControlSystem bossCheck = other.gameObject.GetComponent<BossControlSystem>();

        if (bossCheck == null)
        {
            bossCheck = other.gameObject.GetComponentInParent<BossControlSystem>();
        }

        if (bossCheck != null)
        {
            bossCheck.gameObject.GetComponent<BossGiantAttackControl>().BossGetStun();
        }

    }
}
