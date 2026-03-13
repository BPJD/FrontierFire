using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectPool : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] int poolSize = 5; // 초기 풀 사이즈

    // External refs (런타임 재탐색 필요)
    GameObject prefab;                 // 탄환 프리팹 (bulletID 기반)
    GameObject poolParent;
    Transform parentTr;
    WeaponStatus weaponStat;
    Data_BulletPrafabs bulletData;
    UnitStatus unitStat;

    // State
    int bulletIDCur = 0;
    float damageRevisionShotGun = 1f;
    bool isShotGun = false;
    readonly List<GameObject> pool = new List<GameObject>();

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 바뀌면 참조 보정 + 죽은 참조 제거 + 워밍업 보충
        EnsureRefs(force: true);
        PurgeDead();
        WarmupIfNeeded();
    }

    void Awake()
    {
        // 플레이어 자식이 아닐 때 비활성화하고 끝내려는 의도라면 OK
        if (GetComponentInParent<PlayerMove>() == null)
        {
            enabled = false;
            return;
        }
        unitStat = GetComponentInParent<UnitStatus>();
    }

    void Start()
    {
        EnsureRefs(force: true);
        WarmupIfNeeded();
    }

    // ─────────────────────────────────────────────────────────────────────────────

    // 참조 보정: 씬 전환 후나 런타임 중 깨졌을 때 호출
    void EnsureRefs(bool force = false)
    {
        // 이미 유효하면 스킵
        bool missingPrefab = prefab == null;                 // Unity pseudo-null 체크
        bool missingParentTr = parentTr == null || parentTr.Equals(null);
        bool need = force || missingPrefab || missingParentTr || weaponStat == null || bulletData == null;

        if (!need) return;

        // Data / WeaponStatus 재탐색
        if (bulletData == null)
        {
            var dataObj = GameObject.FindGameObjectWithTag("Data");
            if (dataObj) bulletData = dataObj.GetComponent<Data_BulletPrafabs>();
        }

        if (weaponStat == null)
        {
            weaponStat = GetComponent<WeaponStatus>();
        }
         
        if (weaponStat != null)
        {
            bulletIDCur = weaponStat.bulletID;
        }

        // 프리팹 재획득
        if (bulletData != null)
        {
            prefab = bulletData.GetBulletPrefab(bulletIDCur);
        }

        // Pool 루트 재획득(없으면 생성해도 됨)
        if (poolParent == null || poolParent.Equals(null))
        {
            poolParent = GameObject.FindGameObjectWithTag("Pool");
            if (poolParent == null)
            {
                // 태그가 없거나 씬에 없다면 안전하게 하나 만든다 (선호도에 따라 주석 처리 가능)
                poolParent = new GameObject("[Pool]");
                // 필요하면 DontDestroyOnLoad(poolParent);  // 전역 유지 원하면
            }
        }

        parentTr = poolParent ? poolParent.transform : null;

        // 샷건 데미지 계수 재설정
        if (weaponStat != null && weaponStat.GetWeaponType() == WeaponParamsSO.WeaponTypes.SG)
        {
            damageRevisionShotGun = 0.125f;
            isShotGun = true;
        }
        else
        {
            damageRevisionShotGun = 1f;
        }
    }

    // 리스트에서 null(파괴된) 인스턴스 제거
    void PurgeDead()
    {
        pool.RemoveAll(item => item == null || item.Equals(null));
    }

    // 풀 워밍업(부족분 채우기)
    void WarmupIfNeeded()
    {
        if (prefab == null || parentTr == null) return;

        // 이미 있는 수량 기준으로 부족분 생성
        PurgeDead();
        int need = Mathf.Max(0, poolSize - pool.Count);
        for (int i = 0; i < need; i++)
        {
            var obj = Instantiate(prefab, parentTr);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────

    public GameObject GetObject()
    {
        // 사용 직전에 항상 안전망
        EnsureRefs();
        PurgeDead();

        if (prefab == null)
        {
            //Debug.LogError("[ObjectPool] Prefab 이 없습니다. bulletData/weaponStat 세팅 확인.");
            return null;
        }

        // 데미지/사거리 계산은 매번 갱신(무기 스탯 변동 고려)
        float _CriRandValue = Random.Range(0f, 100f);
        bool _isCritical = false;

        int dmg = weaponStat ? (int)(weaponStat.bulletAtk * damageRevisionShotGun) : 1;
        float _absorption = weaponStat ? weaponStat.hpAbsorption : 0f;

        if (weaponStat.criRate >= _CriRandValue)
        {
            dmg = Mathf.FloorToInt(dmg + (dmg * (weaponStat.criDamage * 0.01f)));
            _isCritical = true;
        }

        float _speed = 0f;
        float _range = weaponStat ? weaponStat.bulletRange : 10f;
        if (isShotGun)
        {
            _speed = Random.Range(24f, 36f);
            _range = Random.Range(_range * 0.7f, _range * 1.3f);
        }
        

        // 비활성 인스턴스 재사용
        for (int i = 0; i < pool.Count; i++)
        {
            var obj = pool[i];
            if (obj == null || obj.Equals(null)) continue;

            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                var b = obj.GetComponent<Bullet>();
                if (b) b.SetBulletStatus(dmg, _range, _speed, weaponStat.GetAttackType(), _isCritical, weaponStat.add_ExplodeRadius, _absorption, unitStat);
                return obj;
            }
        }

        // 없으면 새로 생성
        var created = (parentTr != null) ? Instantiate(prefab, parentTr) : Instantiate(prefab);
        created.SetActive(true);
        pool.Add(created);
        var bullet = created.GetComponent<Bullet>();
        if (bullet) bullet.SetBulletStatus(dmg, _range, _speed, weaponStat.GetAttackType(), _isCritical, weaponStat.add_ExplodeRadius, _absorption, unitStat);
        return created;
    }

    public void ReturnObject(GameObject obj)
    {
        if (obj == null || obj.Equals(null)) return;
        obj.SetActive(false);
    }

    // 탄환 ID가 바뀔 때만, 자기 버킷만 안전하게 교체
    public void RefreshPool(int newBulletID)
    {
        if (bulletIDCur == newBulletID) return;

        // 현재 버킷 정리(자기 것만)
        for (int i = pool.Count - 1; i >= 0; i--)
        {
            var obj = pool[i];
            if (obj != null && !obj.Equals(null)) Destroy(obj);
        }
        pool.Clear();

        bulletIDCur = newBulletID;

        // 새 프리팹 할당
        if (bulletData == null)
        {
            var dataObj = GameObject.FindGameObjectWithTag("Data");
            if (dataObj) bulletData = dataObj.GetComponent<Data_BulletPrafabs>();
        }

        prefab = bulletData ? bulletData.GetBulletPrefab(bulletIDCur) : null;

        // 재워밍업
        WarmupIfNeeded();
    }
}
