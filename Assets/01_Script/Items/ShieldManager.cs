using UnityEngine;
using System.Collections;

public class ShieldManager : MonoBehaviour
{
    [SerializeField] float shieldRespawnTime = 60f;
    Shield shield;
    [SerializeField] GameObject shieldObj;

    int shieldHPperLevel = 300;
    float shieldRevisionPerLevel = 0.2f;
    bool isShieldBroken = false;
    int shieldLevel = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shield = shieldObj.GetComponent<Shield>();
    }

    public void ShieldActivate(bool isOn)
    {
        if(shieldLevel >= 1 && !isShieldBroken)
        {
            shieldObj.SetActive(isOn);
        }
    }

    public void ShieldUpgrade()
    {
        isShieldBroken = false;
        shieldLevel++;
        ShieldReset();

    }

    public IEnumerator ShieldRespawn()
    {
        isShieldBroken = true;
        yield return new WaitForSeconds(shieldRespawnTime);
        ShieldReset();
        isShieldBroken = false;


    }

    void ShieldReset()
    {
        shield.shieldHP = shieldHPperLevel * shieldLevel;
        shield.shieldRevision = Mathf.Max(1f + shieldRevisionPerLevel - (shieldRevisionPerLevel * shieldLevel), 0.5f);
        shield.shieldHPMax = shield.shieldHP;
    }


}
