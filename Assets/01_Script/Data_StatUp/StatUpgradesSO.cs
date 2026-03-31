using UnityEngine;
[CreateAssetMenu(fileName = "StatUpgradesSO", menuName = "Upgrade/StatUpgradesSO", order = 0)]

public class StatUpgradesSO : ScriptableObject
{
    [Header("기본 정보")]
    public int id;                // 예: 61000
    public string up_name;       // 예: 체력 강화
    [TextArea]
    public string up_desc;       // 예: 체력을 강화한다.

    [Header("강화 설정")]
    public int up_type;          // 0 = 합연산, 1 = 곱연산, 2 = 절대값 설정
    public int up_stat;          // 0~11: UnitParams의 스탯 ID 기준
    public float up_value;       // 적용 수치 (정수든 소수든 float로 처리)


    [Header("모델링")]
    public int up_tier;          // 아이템 등급, D = 0 ~ SS = 5
    public int up_category;          // 스탯 유형, 0 없음, 1 방어, 2 공격, 3 조작, 4 자원, 5 기동

    public enum UpgradeType
    {
        Add = 0,
        Multiply = 1
    }

    public enum StatID
    {
        HP = 0,
        Attack = 1,
        Defense = 2,
        DamageReduction = 3,
        ArmorLevel = 4,
        MoveSpeed = 5,
        JumpPower = 6,
        MultiJump = 7,
        ShotAccuracy = 8,
        CriticalRate = 9,
        CriticalDamage = 10,
        FinalDamage = 11
    }

    public enum StatItemTier
    {
        D = 0,
        C = 1,
        B = 2,
        A = 3,
        S = 4,
        SS = 5
    }
}
