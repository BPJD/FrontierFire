using UnityEngine;
using System.Collections;

public class Ability_CounterBuff : MonoBehaviour
{
    UnitStatus playerStat;
    PlayerWeaponController weaponController;
    [SerializeField] float atkRevision = 1f;
    [SerializeField] float buffDuration = 3f;
    float buffDurationCur = 3f;
    public int atkBuffValue = 0;

    bool isBuffAction = false;
    static float buffTimeTick = 0.1f;
    WaitForSeconds buffTime = new WaitForSeconds(buffTimeTick);

    [SerializeField] float buffGetRate = 0.2f; //¹öÇÁ ¹ßµ¿ È®·ü

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        GameObject _player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);

        weaponController = _player.GetComponent<PlayerWeaponController>();
        playerStat = _player.GetComponent<UnitStatus>();
        playerStat.OnDamaged += OnDamaged;
        OnDamaged();
    }


    void OnDamaged()
    {
        float _value = Random.Range(0f, 1f);
        if (_value < buffGetRate)
        {
            if (!isBuffAction)
            {
                StartCoroutine(BuffAction());
            }
            else
            {
                buffDurationCur = buffDuration;
                Debug.Log("¹öÇÁ °»½Å");
            }
        }
    }

    IEnumerator BuffAction()
    {
        if (!isBuffAction)
        {
            Debug.Log("¹öÇÁ ÄÑÁü");
            isBuffAction = true;

            atkBuffValue = Mathf.RoundToInt(playerStat.unitParams.u_atk * atkRevision);

            playerStat.unitParamsAbility.u_atk += atkBuffValue;
            playerStat.SetCurrentAtk();
            weaponController.GetWeaponStatCur().ApplyStatusInSystem();

            buffDurationCur = buffDuration;


            while (buffDurationCur >= 0f)
            {
                buffDurationCur -= buffTimeTick;
                yield return buffTime;
            }

            playerStat.unitParamsAbility.u_atk -= atkBuffValue;
            playerStat.SetCurrentAtk();
            weaponController.GetWeaponStatCur().ApplyStatusInSystem();
            Debug.Log("¹öÇÁ ²¨Áü");

            isBuffAction = false;
            atkBuffValue = 0;

        }

    }
}
