using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitParams", menuName = "Data/UnitParams")]
public class UnitParamsSO : ScriptableObject
{
    public string u_name;

    public enum UnitTypes
    {
        Default,
        Player,
        Enemy,
        Boss
    }

    public UnitTypes u_type;

    public int u_hp;
    public int u_atk;
    public int u_def;
    public float u_immunePer = 1f;

    public int u_armorLevel;

    public float u_moveSpeed;
    public float u_jumpPower;
    public int u_multijumpCount;

    public float u_shotAccuracy;

    public float u_criRate;
    public float u_criDamage;
    public float u_damage = 1f;

    public int u_hpRegen;
    public float u_hpRegenSpeed = 1f;
}
