using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Data/WeaponParams")]
public class WeaponParamsSO : ScriptableObject
{
    public string w_name;
    public string w_desc;

    public enum WeaponTypes { Default, AutoRifle, BurstRifle, LightMG, SMG, ChargeRifle, Shotgun, LightSR, HeavyMG, Rocket, HeavySR, SpikeRifle }
    public enum Ammos { Default, Infantry, Armor }
    public enum AtkTypes { Normal, Piercing_Light, Piercing_Heavy, Fixed }

    public WeaponTypes w_type;
    public AtkTypes w_atkType;

    public int w_atk;
    public float w_range;
    public int w_rpm;
    public int w_magSize;
    public float w_accuracy;
    public float w_reloadTime;

    public int e_quality;
    public Ammos w_usingAmmo;
    public float w_ammoMulti;
    public int w_ammoDefault;

    public int bulletID;
    public bool isCamRangeUp = false;

    public float w_hpAbsorption;
}
