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
}
