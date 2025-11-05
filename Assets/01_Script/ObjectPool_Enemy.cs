using System.Collections.Generic;
using UnityEngine;

public class ObjectPool_Enemy : MonoBehaviour
{
    GameObject prefab; // 미리 생성할 프리팹
    GameObject poolParent;
    Transform parentTr;
    public int poolSize = 5; // 초기 풀 사이즈
    EnemyAttackSystem weaponStat;
    TurretAttackSystem turretStat;

    int _damage = 5;
    float _range = 10f;
    

    private List<GameObject> pool = new List<GameObject>();

    private void Start()
    {
        SetWeaponStat();

        poolParent = GameObject.FindGameObjectWithTag("Pool");
        parentTr = poolParent.transform;
        if(prefab != null)
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
        weaponStat = GetComponent<EnemyAttackSystem>();
        if (weaponStat != null)
        {
            _damage = weaponStat.w_atk;
            _range = weaponStat.w_range;
            prefab = weaponStat.bulletObj;
        }
        else
        {
            turretStat = GetComponent<TurretAttackSystem>();
            _damage = turretStat.w_atk;
            _range = turretStat.w_range;
            prefab = turretStat.bulletObj;
        }



    }

    public GameObject GetObject()
    {

        if(poolParent != null)
        {
            // 비활성화된 오브젝트 찾기
            foreach (GameObject obj in pool)
            {
                if (!obj.activeInHierarchy)
                {
                    obj.GetComponent<Bullet>().SetBulletStatus(_damage, _range, 0f, WeaponParamsSO.AtkTypes.Normal, false, 0f);
                    obj.SetActive(true);
                    return obj;
                }
            }

            // 남는 오브젝트가 없으면 새로 생성
            GameObject newObj = Instantiate(prefab, parentTr);
            newObj.SetActive(false);
            pool.Add(newObj);
            newObj.SetActive(true);
            newObj.GetComponent<Bullet>().SetBulletStatus(_damage, _range, 0f, WeaponParamsSO.AtkTypes.Normal, false, 0f);
            return newObj;
        }


        else
        {
            GameObject newObj = Instantiate(prefab);
            newObj.SetActive(false);
            pool.Add(newObj);
            newObj.SetActive(true);
            newObj.GetComponent<Bullet>().SetBulletStatus(_damage, _range, 0f, WeaponParamsSO.AtkTypes.Normal, false, 0f);
            return newObj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false); // 오브젝트 비활성화
    }
}
