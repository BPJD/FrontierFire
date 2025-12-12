using System.Collections.Generic;
using UnityEngine;

public static class WeaponUpgradeUtil
{
    // add/mult 딕셔너리에만 누적 (실제 WeaponStatus에는 안 건드림)
    public static void ApplyUpgradeToDict(
        int type, int statID, float value,
        Dictionary<int, float> add,
        Dictionary<int, float> mult)
    {
        // set형 (발사체, 무기 속성 등)
        if (statID == 6 || statID == 8)
        {
            add[statID] = value;
            mult.Remove(statID);
            return;
        }

        if (type == 0) // Add
        {
            add[statID] = (add.TryGetValue(statID, out var cur) ? cur : 0f) + value;
        }
        else          // Multiply (0.30 = +30%)
        {
            mult[statID] = (mult.TryGetValue(statID, out var cur) ? cur : 0f) + value;
        }
    }

    public static float GetFinalStat(
        int statID, float baseValue,
        Dictionary<int, float> add,
        Dictionary<int, float> mult)
    {
        float plus = add.TryGetValue(statID, out var p) ? p : 0f;
        float perc = mult.TryGetValue(statID, out var m) ? m : 0f;

        return baseValue * (1f + perc) + plus;
    }

    public static WeaponParams BuildParamsWithUpgrades(
        WeaponParams baseParam,
        Dictionary<int, float> add,
        Dictionary<int, float> mult)
    {
        WeaponParams result = new WeaponParams(baseParam);

        // CSV up_stat 매핑 사용
        result.w_atk = (int)GetFinalStat(0, result.w_atk, add, mult);             // 공격력
        result.w_rpm = (int)GetFinalStat(1, result.w_rpm, add, mult);             // 발사 속도
        result.w_magSize = Mathf.RoundToInt(
                              GetFinalStat(2, result.w_magSize, add, mult));           // 탄창 크기
        result.w_reloadTime = GetFinalStat(3, result.w_reloadTime, add, mult);             // 재장전
        result.w_accuracy = GetFinalStat(5, result.w_accuracy, add, mult);             // 정확도
        result.w_range = GetFinalStat(9, result.w_range, add, mult);             // 사거리

        // 필요하면 나중에 7,10,11,12도 추가

        return result;
    }
}
