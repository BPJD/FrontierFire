using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    GameObject prefab; // 미리 생성할 프리팹
    GameObject poolParent;
    Transform parentTr;
    public int poolSize = 5; // 초기 풀 사이즈
    WeaponStatus weaponStat;
    Data_BulletPrafabs bulletData;

    int bulletIDCur = 0;

    float damageRevisionShotGun = 1f;
    

    private List<GameObject> pool = new List<GameObject>();

    private void Awake()
    {
        if (GetComponentInParent<PlayerMove>() == null)
        {
            this.enabled = false;
        }
    }

    private void Start()
    {
        bulletData = GameObject.FindGameObjectWithTag("Data").GetComponent<Data_BulletPrafabs>();
        weaponStat = GetComponent<WeaponStatus>();
        bulletIDCur = weaponStat.bulletID;
        prefab = bulletData.GetBulletPrefab(bulletIDCur);
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

        if (weaponStat.GetWeaponType() == WeaponParamsSO.WeaponTypes.Shotgun)
        {
            damageRevisionShotGun = 0.125f;
        }
    }

    public GameObject GetObject()
    {

        Debug.Log(damageRevisionShotGun);
        Debug.Log(weaponStat.bulletAtk);
        int _damage = (int)(weaponStat.bulletAtk * damageRevisionShotGun);
        float _range = weaponStat.bulletRange;


        if(poolParent != null)
        {
            // 비활성화된 오브젝트 찾기
            foreach (GameObject obj in pool)
            {
                if (!obj.activeInHierarchy)
                {
                    obj.SetActive(true);
                    obj.GetComponent<Bullet>().SetBulletStatus(_damage, _range);
                    return obj;
                }
            }

            // 남는 오브젝트가 없으면 새로 생성
            GameObject newObj = Instantiate(prefab, parentTr);
            newObj.SetActive(false);
            pool.Add(newObj);
            newObj.SetActive(true);
            newObj.GetComponent<Bullet>().SetBulletStatus(_damage, _range);
            return newObj;
        }


        else
        {
            Debug.Log("오브젝트 풀이 안 보이는데요? 님 죠짐 ㅅㄱ");


            GameObject newObj = Instantiate(prefab);
            newObj.SetActive(false);
            pool.Add(newObj);
            newObj.SetActive(true);
            newObj.GetComponent<Bullet>().SetBulletStatus(_damage, _range);
            return newObj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false); // 오브젝트 비활성화
    }


    public void RefreshPool(int newBulletID)
    {
        if (bulletIDCur == newBulletID) return;

        // 기존 오브젝트 제거
        foreach (GameObject obj in pool)
        {
            Destroy(obj);
        }
        pool.Clear();

        prefab = bulletData?.GetBulletPrefab(newBulletID);
        if (prefab == null)
        {
            Debug.LogError($"bulletID {newBulletID}에 해당하는 프리팹을 찾을 수 없습니다.");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, parentTr);
            obj.SetActive(false);
            pool.Add(obj);
        }

        bulletIDCur = newBulletID;
    }
}
