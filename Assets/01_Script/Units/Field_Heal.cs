using UnityEngine;

public class Field_Heal : MonoBehaviour
{
    [SerializeField] float healDelay = 1f;
    float healDelayCur = 0f;
    [SerializeField] int healValue = 10;

    bool isPlayerInZone = false;

    UnitStatus stat;

    // Update is called once per frame
    void Update()
    {
        healDelayCur -= Time.deltaTime;

        if(isPlayerInZone && stat != null)
        {
            Heal();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stat = other.GetComponent<UnitStatus>();
            isPlayerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stat = null;
            isPlayerInZone = false;
        }
    }

    void Heal()
    {
        stat.UnitGetHeal(healValue, true);
        healDelayCur = healDelay;
    }
}
