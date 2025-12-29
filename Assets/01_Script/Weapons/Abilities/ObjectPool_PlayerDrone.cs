using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ObjectPool_PlayerDrone : MonoBehaviour
{
    [SerializeField] GameObject prefab; // 미리 생성할 프리팹
    [SerializeField] Transform fireTr;
    [SerializeField] Transform goToTr;
    GameObject poolParent;
    Transform parentTr;
    public int poolSize = 5; // 초기 풀 사이즈

    UnitStatus unitStat;
    PlayerWeaponController playerWeaponController;

    WeaponStatus weaponStatCur;

    int _damage = 5;
    float _range = 10f;

    [SerializeField] float atkRevision = 1f;

    float bulletAngleError = 30f;


    private List<GameObject> pool = new List<GameObject>();

    private void Start()
    {
        unitStat = GetComponentInParent<UnitStatus>();
        playerWeaponController = GetComponentInParent<PlayerWeaponController>();
        SetWeaponStat();

        poolParent = GameObject.FindGameObjectWithTag("Pool");
        parentTr = poolParent.transform;
        if (prefab != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab, parentTr);
                obj.SetActive(false); // 비활성화
                pool.Add(obj);
            }
        }
    }

    public void SetWeaponStat()
    {

        weaponStatCur = playerWeaponController.GetWeaponStatCur();
        if (weaponStatCur != null)
        {
            _range = Mathf.Max(4f, weaponStatCur.bulletRange * 0.7f);
            bulletAngleError = Mathf.Lerp(30f, 0f, Mathf.Clamp01(weaponStatCur.weaponAccuracy * 0.01f));
        }
    }

    public void DroneWeaponShoot()
    {
        StartCoroutine(Shoot());
    }


    IEnumerator Shoot()
    {
        float _randDelay = Random.Range(0.03f, 0.12f);
        yield return new WaitForSeconds(_randDelay);

        _damage = Mathf.RoundToInt(unitStat.atkCur * atkRevision);

        GameObject _bullet = GetObject();

        //bullet.GetComponent<Bullet>().SetBulletStatus(weaponStat.bulletAtk, weaponStat.bulletRange, 0f);
        Transform bulletTr = _bullet.transform;
        bulletTr.position = new Vector3(fireTr.position.x, fireTr.position.y, 0f);

        // 방향 설정
        //Vector3 direction = (playerPointer.targetPos - bulletTr.position).normalized;
        Vector3 direction = (goToTr.position - fireTr.position).normalized;
        Quaternion baseRotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, 0f));

        // X축 회전 수정
        float _angleError = Random.Range(-bulletAngleError, bulletAngleError);
        Vector3 eulerAngles = baseRotation.eulerAngles;
        eulerAngles.x += Random.Range(-_angleError, _angleError); // X축에 _accuracy 값 추가
        bulletTr.rotation = Quaternion.Euler(eulerAngles);
    }

    public GameObject GetObject()
    {

        if (poolParent != null)
        {
            // 비활성화된 오브젝트 찾기
            foreach (GameObject obj in pool)
            {
                if (!obj.activeInHierarchy)
                {
                    obj.GetComponent<Bullet>().SetBulletStatus(_damage, _range, 0f, WeaponParamsSO.AtkTypes.Normal, false, 0f, 0f, unitStat);
                    obj.SetActive(true);
                    return obj;
                }
            }

            // 남는 오브젝트가 없으면 새로 생성
            GameObject newObj = Instantiate(prefab, parentTr);
            newObj.SetActive(false);
            pool.Add(newObj);
            newObj.SetActive(true);
            newObj.GetComponent<Bullet>().SetBulletStatus(_damage, _range, 0f, WeaponParamsSO.AtkTypes.Normal, false, 0f, 0f, unitStat);
            return newObj;
        }


        else
        {
            GameObject newObj = Instantiate(prefab);
            newObj.SetActive(false);
            pool.Add(newObj);
            newObj.SetActive(true);
            newObj.GetComponent<Bullet>().SetBulletStatus(_damage, _range, 0f, WeaponParamsSO.AtkTypes.Normal, false, 0f, 0f, unitStat);
            return newObj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false); // 오브젝트 비활성화
    }
}
