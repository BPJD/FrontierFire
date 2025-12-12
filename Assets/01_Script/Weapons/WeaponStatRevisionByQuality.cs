using UnityEngine;

public class WeaponStatRevisionByQuality
{
    /// <summary>
    /// WeaponParamsSO + quality를 받아서,
    /// 품질에 따른 배수를 적용한 WeaponParams 런타임 인스턴스를 반환.
    /// SO(weaponSO)는 건드리지 않는다.
    /// </summary>
    public static WeaponParams GetRevisedParams(WeaponParamsSO defaultParams, int quality)
    {
        // SO → WeaponParams 기본 복사
        WeaponParams wp = new WeaponParams(defaultParams);

        // 품질 클램프
        quality = Mathf.Clamp(quality, 0, 100);

        // 품질 → revision 배수 계산
        float rev = CalcRevisionByQuality(quality);

        // 정수 스탯
        wp.w_atk = Mathf.Max(1, Mathf.RoundToInt(defaultParams.w_atk * rev));
        wp.w_rpm = Mathf.Max(1, Mathf.RoundToInt(defaultParams.w_rpm * rev));

        // float 스탯
        wp.w_range = defaultParams.w_range * (1f + (rev - 1f) * 0.5f); // 사거리는 배수의 절반만큼 증감

        float A = 1.5f; // 영향도 1.5배
        float revAcc = 1f + (rev - 1f) * A;
        wp.w_accuracy = Mathf.Clamp(defaultParams.w_accuracy * revAcc, 0f, 100f);

        // 품질값도 저장
        wp.e_quality = quality;

        return wp;
    }

    /// <summary>
    /// quality(0~100) → revision(0.4~1.25) 매핑
    /// 0   → 0.4
    /// 60  → 1.0
    /// 100 → 1.25
    /// </summary>
    static float CalcRevisionByQuality(int quality)
    {
        quality = Mathf.Clamp(quality, 0, 100);

        if (quality <= 60)
        {
            float t = quality / 60f; // 0~1
            return Mathf.Lerp(0.4f, 1.0f, t);
        }
        else
        {
            float t = (quality - 60f) / 40f; // 0~1
            return Mathf.Lerp(1.0f, 1.25f, t);
        }
    }
}
