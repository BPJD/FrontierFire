using UnityEngine;

public class Ability_DamageCareBuff : MonoBehaviour, IAbilityUpgradable
{
    UnitStatus playerStat;
    [SerializeField] GameObject healItem;
    Transform tr;

    [SerializeField] int requireCount = 5;
    int hitCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;

        GameObject _player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);

        playerStat = _player.GetComponent<UnitStatus>();
        playerStat.OnDamaged += OnDamaged;
        OnDamaged();
    }


    void OnDamaged()
    {
        hitCount++;
        if (hitCount >= requireCount) 
        {
            Instantiate(healItem, tr.position, Quaternion.identity);
            hitCount = 0;
        }

    }


    public void UpgradeAbility()
    {
        requireCount -= 1;
    }

}
