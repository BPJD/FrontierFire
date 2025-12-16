using Combat;
using UnityEngine;

public class Turning_Ball_Collide : MonoBehaviour
{
    UnitStatus playerStat;
    [SerializeField] float atkRevision = 1f;

    Transform tr;
    AudioSource soundPlayer;
    [SerializeField] AudioClip hitSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStat = GetComponentInParent<UnitStatus>();
        tr = transform;
        soundPlayer = GetComponentInParent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Data_Strings.UnitTag))
        {
            if (playerStat != null && playerStat.hpCur != 0)
            {
                UnitStatus _unitStatus = other.gameObject.GetComponent<UnitStatus>();
                if (_unitStatus != null)
                {
                    _unitStatus.TakeDamage(GetPayload());
                    soundPlayer.PlayOneShot(hitSound);
                }
            }
        }

    }

    DamagePayload GetPayload()
    {
        UnitParams _params = playerStat.unitParams;

        DamagePayload _payload = DamagePayload.Create(
            baseDamage: Mathf.RoundToInt(_params.u_atk * atkRevision),
            ammo: 0,
            atkType: WeaponParamsSO.AtkTypes.Fixed,
            isCritical: false,
            isWeakPoint: false,
            hitPoint: tr.position,
            absorption: 0f,
            attackerStat: playerStat
            );

        return _payload;
    }


}
