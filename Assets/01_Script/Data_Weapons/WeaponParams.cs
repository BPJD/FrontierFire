using UnityEngine;

[System.Serializable]
public class WeaponParams
{
    public string w_name;
    public string w_desc;

    public WeaponParamsSO.WeaponTypes w_type;
    public WeaponParamsSO.Ammos w_usingAmmo;
    public WeaponParamsSO.AtkTypes w_atkType;

    public int w_atk;
    public float w_range;
    public int w_rpm;
    public int w_magSize;
    public float w_accuracy;
    public float w_reloadTime;

    public int e_quality;
    public float w_ammoMulti;
    public int w_ammoDefault;

    public int bulletID;
    public bool isCamRangeUp;

    public WeaponParams() { } // CS1729 방지용

    public WeaponParams(WeaponParamsSO so)
    {
        w_name = so.w_name;
        w_desc = so.w_desc;
        w_type = so.w_type;
        w_usingAmmo = so.w_usingAmmo;
        w_atkType = so.w_atkType;

        w_atk = so.w_atk;
        w_range = so.w_range;
        w_rpm = so.w_rpm;
        w_magSize = so.w_magSize;
        w_accuracy = so.w_accuracy;
        w_reloadTime = so.w_reloadTime;

        e_quality = so.e_quality;
        w_ammoMulti = so.w_ammoMulti;
        w_ammoDefault = so.w_ammoDefault;

        bulletID = so.bulletID;
        isCamRangeUp = so.isCamRangeUp;
    }

    public WeaponParams(WeaponParams other) // 복사 생성자 (강화 이전 값 보관용)
    {
        w_name = other.w_name;
        w_desc = other.w_desc;
        w_type = other.w_type;
        w_usingAmmo = other.w_usingAmmo;
        w_atkType = other.w_atkType;

        w_atk = other.w_atk;
        w_range = other.w_range;
        w_rpm = other.w_rpm;
        w_magSize = other.w_magSize;
        w_accuracy = other.w_accuracy;
        w_reloadTime = other.w_reloadTime;

        e_quality = other.e_quality;
        w_ammoMulti = other.w_ammoMulti;
        w_ammoDefault = other.w_ammoDefault;

        bulletID = other.bulletID;
        isCamRangeUp = other.isCamRangeUp;
    }
}
