using UnityEngine;
using System.Collections;

public class Tutorial_BossCombat : MonoBehaviour
{
    [SerializeField] int tutorialStep_start = 12;
    [SerializeField] int tutorialStep_combat = 13;
    [SerializeField] int tutorialStep_clear = 14;

    [SerializeField] float targetMissileHPpercent = 0.7f;
    [SerializeField] float targetWinHPpercent = 0.5f;
    [SerializeField] float bossHPpercent = 1f;

    [SerializeField] GameObject missileObj;

    [SerializeField] Direction_TutorialTeller teller;

    [SerializeField] UnitStatus bossStatus;

    [SerializeField] ParticleSystem disappearEft;


    bool isCombat = true;
    bool isMissileStart = false;
    bool isBossClear = false;


    private void OnEnable()
    {
        StartCoroutine(BossStart());
        StartCoroutine(UpdateBossStat());
        StartCoroutine(MissileStart());
        StartCoroutine(BossClear());
    }

    IEnumerator UpdateBossStat()
    {
        yield return new WaitForSeconds(20f);

        while (isCombat)
        {
            bossHPpercent = (float)bossStatus.hpCur / bossStatus.unitParams.u_hp;


            if(bossHPpercent <= targetWinHPpercent)
            {
                isBossClear = true;
                break;
            }
            else if(bossHPpercent <= targetMissileHPpercent && !isMissileStart)
            {
                isMissileStart = true;
                teller.TutorialStepSuccess(tutorialStep_combat);
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator BossStart()
    {
        yield return new WaitForSeconds(5f);

        teller.TutorialStepSuccess(tutorialStep_start);

        yield return new WaitForSeconds(90f);

        if(bossHPpercent < targetMissileHPpercent && !isMissileStart)
        {
            isMissileStart = true;
            teller.TutorialStepSuccess(tutorialStep_combat);
        }
    }

    IEnumerator MissileStart()
    {
        yield return new WaitForSeconds(0.33f);

        Transform _bossTr = bossStatus.gameObject.transform;

        while (isCombat)
        {
            yield return new WaitForSeconds(0.2f);
            while (isMissileStart)
            {
                yield return new WaitForSeconds(2f);
                for (int i = 0; i < 5; i++)
                {
                    if (isCombat)
                    {
                        ShootMissile(_bossTr);
                    }
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }
    }

    IEnumerator BossClear()
    {
        while (!isBossClear)
        {
            yield return new WaitForSeconds(0.5f);
        }

        isCombat = false;
        disappearEft.transform.position = bossStatus.transform.position;
        disappearEft.Play(true);
        bossStatus.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);

        teller.TutorialStepSuccess(tutorialStep_clear);
        teller.isTutorialEnd = true;
    }

    void ShootMissile(Transform target)
    {
        Vector3 _fireTr = target.position + (Vector3.up * 30f);
        _fireTr.z = 0f;

        GameObject bullet = Instantiate(missileObj, _fireTr, Quaternion.identity);

        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.SetBulletStatus(300, 25000f, 30f);
        }

        Transform bulletTr = bullet.transform;
        bulletTr.position = new Vector3(_fireTr.x, _fireTr.y, 0f);

        Vector3 targetPos = target.position;
        targetPos.z = 0f;

        Vector3 direction = (targetPos - bulletTr.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion baseRotation = Quaternion.LookRotation(direction);

            float angleError = Random.Range(-22.5f, 22.5f);
            Quaternion spreadRotation = Quaternion.Euler(new Vector3(0f, angleError, 0f));

            bulletTr.rotation = baseRotation * spreadRotation;
        }
    }
}
