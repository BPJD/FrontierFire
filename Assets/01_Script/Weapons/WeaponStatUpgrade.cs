using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStatUpgrade : MonoBehaviour
{
    WeaponStatus weaponStat;

    // 누적 버킷 (가산/계수). 키 = statID, 값 = 합계
    private readonly Dictionary<int, float> add = new Dictionary<int, float>();
    private readonly Dictionary<int, float> mult = new Dictionary<int, float>(); // 0.30 = +30%
                                                                                 // 퍼센트(곱연산) 누적용: statID -> 합계(예: -0.10, +0.30 ...)
    readonly Dictionary<int, float> multPercent = new Dictionary<int, float>();


    public List<int> upgradesCur = new List<int>();

    Data_WeaponStatUpgrades upgradeData;
    Data_BulletPrafabs bulletData;
    Data_WeaponUpgradeModels upgradeModelData;

    GameObject upgradeEft;

    private void Awake()
    {
        weaponStat = GetComponent<WeaponStatus>();
    }

    void GetUpgradeData()
    {
        if (upgradeData == null || bulletData == null || upgradeModelData == null)
        {
            var go = GameObject.FindGameObjectWithTag("Data");
            if (go)
            {
                upgradeData = go.GetComponent<Data_WeaponStatUpgrades>();
                bulletData = go.GetComponent<Data_BulletPrafabs>();
                upgradeModelData = go.GetComponent<Data_WeaponUpgradeModels>();
            }
        }
    }


    // 패키지 단위
    public void UpgradeStatPackage(int id)
    {
        GetUpgradeData();
        var pack = upgradeData != null ? upgradeData.GetAllUpgrades(id) : null;
        if (pack == null || pack.Count == 0) return;

        foreach (var so in pack)
        {
            UpgradeStatApply(so.up_type, so.up_stat, so.up_value);
        }
    }

    // type: 0=가산, 1=계수 | statID: CSV up_stat | value: 가산(절대값) or 계수(0.30=+30%)
    void UpgradeStatApply(int type, int statID, float value)
    {
        // 열거형(set성) 스탯은 "마지막 값"만 의미가 있으므로 덮어쓰기
        if (statID == 6 || statID == 8) // usingAmmo, atkType
        {
            add[statID] = value;        // set
            mult.Remove(statID);        // 계수 무시
            PushToStatus(statID);
            return;
        }

        if (type == 0) // Add
        {
            add[statID] = (add.TryGetValue(statID, out var cur) ? cur : 0f) + value;
        }
        else // Multiply
        {
            mult[statID] = (mult.TryGetValue(statID, out var cur) ? cur : 0f) + value; // 0.30 누적
        }

        // 현재 스탯의 누적 합계를 한 번에 반영
        PushToStatus(statID);
    }

    void PushToStatus(int statID)
    {
        if (weaponStat == null)
        {
            weaponStat = GetComponent<WeaponStatus>();
            if (weaponStat == null)
            {
                Debug.LogWarning($"{name}: WeaponStatus가 없어 statID {statID} 적용 불가");
                return;
            }
        }

        float plus = add.TryGetValue(statID, out var p) ? p : 0f;
        float perc = mult.TryGetValue(statID, out var m) ? m : 0f;
        weaponStat.SetStatusByUpgradeF(statID, plus, perc);
    }

    public void ApplyUpgradeByWeaponEquip(List<int> list)
    {
        // 1) Player 태그 강제 체크 제거(또는 transform.root로 완화)
        // if (transform.parent.CompareTag("Player")) ...

        // 2) 널/빈 대응 + 원본 보호
        if (list == null || list.Count == 0)
        {
            upgradesCur = new List<int>();
            return;
        }
        upgradesCur = new List<int>(list); // 복사 저장

        // 3) "한 번씩만" 적용 (중복 의도라면 여기 그대로, 중복 방지 원하면 Distinct 사용)
        for (int i = 0; i < list.Count; i++)
        {
            // upgradesCur.Add(...) 절대 금지 (자기 리스트를 늘리며 순회 X)
            UpgradeStatPackage(list[i]);
        }

        WeaponEffectApply();
    }

    public void WeaponEffectApply()
    {
        GameObject _eft = upgradeModelData.GetWeaponEft(upgradesCur.Count);

        if (_eft != null)
        {
            if (upgradeEft != null)
            {
                Destroy(upgradeEft);
            }

            upgradeEft = Instantiate(_eft, transform);
        }
    }

    // 업그레이드 시스템 스크립트 내부에 추가

    // 1. 수치형 스탯 최종 값
    public float GetFinalStatForUI(int statID, float baseValue)
    {
        float plus = add.TryGetValue(statID, out var p) ? p : 0f;
        float perc = mult.TryGetValue(statID, out var m) ? m : 0f;

        // base * (1 + perc) + plus
        return baseValue * (1f + perc) + plus;
    }

    // 2. set형(열거형) 스탯 최종 값 (발사체, 무기 속성 등)
    public int GetEnumStatForUI(int statID, int baseValue)
    {
        if (add.TryGetValue(statID, out var v))
            return Mathf.RoundToInt(v);

        return baseValue;
    }

    // 3. WeaponParams 기준으로 한 번에 최종값 만들기
    public WeaponParams BuildUpgradedParamsForUI(WeaponParams baseParam)
    {
        // 깊은 복사
        WeaponParams result = new WeaponParams(baseParam);

        // CSV up_stat 매핑
        // 0: 공격력        -> w_atk
        // 1: 발사 속도     -> w_rpm
        // 2: 탄창 크기     -> w_magSize
        // 3: 재장전 시간   -> w_reloadTime
        // 5: 정확도        -> w_accuracy
        // 9: 사거리        -> w_range
        // 13: 사거리        -> w_hpAbsorption

        result.w_atk = (int)GetFinalStatForUI(0, result.w_atk);
        result.w_rpm = (int)GetFinalStatForUI(1, result.w_rpm);
        result.w_magSize = Mathf.RoundToInt(GetFinalStatForUI(2, result.w_magSize));
        result.w_reloadTime = GetFinalStatForUI(3, result.w_reloadTime);
        result.w_accuracy = GetFinalStatForUI(5, result.w_accuracy);
        result.w_range = GetFinalStatForUI(9, result.w_range);
        result.w_hpAbsorption = GetFinalStatForUI(13, result.w_hpAbsorption);

        // 필요하면 나중에 발사체(6), 무기 속성(8), 조준기(12)도 enum으로 적용 가능
        // int projectile = GetEnumStatForUI(6, (int)result.w_projectileType);
        // result.w_projectileType = (ProjectileType)projectile;

        return result;
    }

    // 4. (툴팁에서 접근할 수 있게) 현재 업그레이드 리스트 읽기 전용 프로퍼티
    public IReadOnlyList<int> CurrentUpgrades => upgradesCur;
}
