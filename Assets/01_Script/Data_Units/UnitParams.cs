[System.Serializable]
public class UnitParams
{
    public string u_name;
    public UnitParamsSO.UnitTypes u_type;

    public int u_hp;
    public int u_atk;
    public int u_def;
    public float u_immunePer = 1f;
    public int u_hpRegen;
    public float u_hpRegenSpeed = 1f;

    public int u_armorLevel;

    public float u_moveSpeed;
    public float u_jumpPower;
    public int u_multijumpCount;

    public float u_shotAccuracy;

    public float u_criRate;
    public float u_criDamage;
    public float u_damage = 1f;

    public UnitParams() { }

    // 복사 생성자
    public UnitParams(UnitParamsSO src)
    {
        u_name = src.u_name;
        u_type = src.u_type;
        u_hp = src.u_hp;
        u_atk = src.u_atk;
        u_def = src.u_def;
        u_immunePer = src.u_immunePer;
        u_armorLevel = src.u_armorLevel;
        u_moveSpeed = src.u_moveSpeed;
        u_jumpPower = src.u_jumpPower;
        u_multijumpCount = src.u_multijumpCount;
        u_shotAccuracy = src.u_shotAccuracy;
        u_criRate = src.u_criRate;
        u_criDamage = src.u_criDamage;
        u_damage = src.u_damage;
        u_hpRegen = src.u_hpRegen;
        u_hpRegenSpeed = src.u_hpRegenSpeed;
    }

    // 깊은 복사 생성자 (기본값 백업용)
    public UnitParams(UnitParams other)
    {
        u_name = other.u_name;
        u_type = other.u_type;
        u_hp = other.u_hp;
        u_atk = other.u_atk;
        u_def = other.u_def;
        u_immunePer = other.u_immunePer;
        u_armorLevel = other.u_armorLevel;
        u_moveSpeed = other.u_moveSpeed;
        u_jumpPower = other.u_jumpPower;
        u_multijumpCount = other.u_multijumpCount;
        u_shotAccuracy = other.u_shotAccuracy;
        u_criRate = other.u_criRate;
        u_criDamage = other.u_criDamage;
        u_damage = other.u_damage;
        u_hpRegen = other.u_hpRegen;
        u_hpRegenSpeed = other.u_hpRegenSpeed;
    }
}
