using UnityEngine;
using System.Collections;
using Combat;

public class BossCoreCrystalAttack : MonoBehaviour
{
    UnitStatus crystalStat;

    bool isCrystalAttackReady = false;

    WaitForSeconds crystalStartDelay = new WaitForSeconds(10f);

    WaitForSeconds crystalExplodeDelay = new WaitForSeconds(6f);

    Transform target;

    [SerializeField] GameObject crystalBulletObj;

    [SerializeField] DamagePayload crystalDamagePayload;

    [SerializeField] float crystalDamageMultiplier = 1.5f;

    [SerializeField] float bulletPosXOffset = 2.5f;
    [SerializeField] float bulletPosYOffset = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        crystalStat = GetComponent<UnitStatus>();
        target = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).transform;

        CrystalAttackReady(true);

        crystalDamagePayload.baseDamage = Mathf.RoundToInt(crystalStat.atkCur * crystalDamageMultiplier);
    }



    public void CrystalAttackReady(bool isReady)
    {
        if (isReady)
        {
            StartCoroutine(CrystalExplodeAttack());
        }
        else
        {
            isCrystalAttackReady = isReady;
        }
    }

    IEnumerator CrystalExplodeAttack()
    {
        yield return crystalStartDelay;
        isCrystalAttackReady = true;
        while (isCrystalAttackReady)
        {
            if (target != null)
            {
                float _randX = Random.Range(-bulletPosXOffset, bulletPosXOffset);
                float _randY = Random.Range(-bulletPosYOffset, bulletPosYOffset);

                Vector3 _offset = new Vector3(_randX, _randY, 0f);

                GameObject _bullet = Instantiate(crystalBulletObj, target.position + (Vector3.up * 1.5f) + _offset, Quaternion.identity);
                _bullet.GetComponent<Projectile_ExplodingOrb>().SetBulletStatus(target, crystalDamagePayload);
            }

            yield return crystalExplodeDelay;
        }

    }

}
