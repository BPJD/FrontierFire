using UnityEngine;
[CreateAssetMenu(fileName = "WeaponStatUpgradesSO", menuName = "Upgrade/WeaponStatUpgradesSO", order = 0)]

public class WeaponStatUpgradesSO : ScriptableObject
{
    [Header("기본 정보")]
    public int id;                // 예: 61000
    public string up_name;       // 예: 체력 강화
    [TextArea]
    public string up_desc;       // 예: 체력을 강화한다.

    [Header("강화 설정")]
    public int up_type;          // 0 = 합연산, 1 = 곱연산
    public int up_stat;          // 0~11: UnitParams의 스탯 ID 기준
    public float up_value;       // 적용 수치 (정수든 소수든 float로 처리)
    public int up_tier;          // 아이템의 등급
    public int up_model;          // 아이템의 모델링

    [Header("UI 설명")]
    public string up_uiDesc;     // 예: 체력 100 증가

    public enum UpgradeType
    {
        Add = 0,
        Multiply = 1
    }

    public enum StatID
    {
        Attack = 0,
        RPM = 1,
        MagSize = 2,
        ReloadTime = 3,
        Quality = 4,
        Accuracy = 5,
        UseAmmo = 6,
        AmmoRevision = 7,
        WeaponType = 8,
        Range = 9,
        Damage = 10,
        ExplodeRadius = 11,
        hpAbsorption = 13
    }

    public enum WeaponItemTier
    {
        D = 0,
        C = 1,
        B = 2,
        A = 3,
        S = 4,
        SS = 5
    }
}
