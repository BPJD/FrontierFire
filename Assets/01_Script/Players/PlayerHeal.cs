using UnityEngine;

public class PlayerHeal : MonoBehaviour
{
    UnitStatus unitStat;

    float healCooldownCur = 0;

    [SerializeField] GameObject healFlag;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unitStat = GetComponent<UnitStatus>();
        healCooldownCur = unitStat.hpRegenSpeed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        healCooldownCur -= Time.fixedDeltaTime;

        if(healCooldownCur <= 0)
        {
            unitStat.UnitGetHeal(unitStat.hpRegen, false);
            healCooldownCur = unitStat.hpRegenSpeed;
        }
    }

    public void HealFlag()
    {
        Instantiate(healFlag, transform.position, Quaternion.identity);
    }


}
