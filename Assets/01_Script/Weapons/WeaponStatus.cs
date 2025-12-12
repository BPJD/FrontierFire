using UnityEngine;

public class WeaponStatus : MonoBehaviour
{
    public WeaponParamsSO weaponDataSource;// SO 참조

    [SerializeField] WeaponParams w_params;
    private WeaponParams w_paramsDefault;

    private PlayerWeapon weaponSystem;
    private UnitStatus playerStat;
    PlayerShootingStat playerAmmoStat;
    Player_WeaponStatusCur weaponStatusCur;

    public int animationType { get; set; }
    public int bulletID { get; private set; }

    public Sprite weaponIcon;
    [HideInInspector] public int bulletAtk { get; private set; }
    [HideInInspector] public float bulletSpeed { get; private set; }
    [HideInInspector] public float bulletRange { get; private set; }
    [HideInInspector] public float reloadSpeed { get; private set; }

    public int ammoCur { get; set; }
    public int ammoMax { get; set; }
    public int quality { get; set; } = 60;

    public bool _isCamRangeUp { get; private set; }

    bool isSetted = false;

    public float criRate { get; private set; } //PlayerStat에서 그대로 가져오는거임
    public float criDamage { get; private set; } //PlayerStat에서 그대로 가져오는거임
    public float add_ExplodeRadius { get; private set; } = 0f;
    int usingScopeCode = 0;

    void SetComponents()
    {
        if (!isSetted)
        {
            if (GetComponentInParent<PlayerMove>() == null)
            {
                this.enabled = false;
            }

            if (weaponDataSource == null)
            {
                string objName = gameObject.name.Substring(0, 5);

                if (int.TryParse(objName, out int result))
                {
                    weaponDataSource = GameObject.FindGameObjectWithTag("Data").GetComponent<PlayerWeaponData>().GetWeaponStatSO(result);
                }
                else
                {
                    Debug.LogWarning($"앞 5자리를 정수로 변환할 수 없습니다: {objName}");
                }
            }

            w_params = new WeaponParams(WeaponStatRevisionByQuality.GetRevisedParams(weaponDataSource, quality));
            w_paramsDefault = new WeaponParams(w_params);

            animationType = SetWeaponAniType((int)w_params.w_type);
            weaponSystem = GetComponent<PlayerWeapon>();
            playerStat = GetComponentInParent<UnitStatus>();
            playerAmmoStat = GetComponentInParent<PlayerShootingStat>();
            weaponStatusCur = GetComponentInParent<Player_WeaponStatusCur>();

            isSetted = true;

            if (w_params.w_usingAmmo == WeaponParamsSO.Ammos.Default)
            {
                ammoMax = 9999;
                ammoCur = 9999;
                weaponSystem.isDefaultWeapon = true;
            }
        }
    }

    void Awake()
    {
        SetComponents();
        //bulletID = w_params.bulletID;
        _isCamRangeUp = w_params.isCamRangeUp;
    }

    private void OnEnable()
    {
        ApplyStatusInSystem();
    }

    public void SetAmmoCurrent(int ammo, int pickCount)
    {
        SetComponents();

        if (w_params.w_usingAmmo == WeaponParamsSO.Ammos.Default)
        {
            ammoMax = 9999;
            ammoCur = 9999;
            weaponSystem.isDefaultWeapon = true;
        }
        else
        {
            ammoMax = (int)((w_params.w_ammoDefault + playerAmmoStat.playerAmmoDefault) * w_params.w_ammoMulti * playerAmmoStat.playerAmmoRevision);

            if (pickCount == 0)
            {
                ammoCur = ammoMax;
            }
            else
            {
                ammoCur = ammo;
            }
        }

        weaponSystem.pickCount = pickCount + 1;
    }

    public void ApplyStatusInSystem()
    {
        weaponSystem.fireRate = 1 / ((float)w_params.w_rpm / 60);

        if(w_params.w_atkType == WeaponParamsSO.AtkTypes.Fixed)
        {
            bulletAtk = w_params.w_atk;
        }
        else
        {
            bulletAtk = Mathf.CeilToInt((w_params.w_atk + playerStat.atkCur) * playerStat.damageCur);
        }

        bulletID = w_params.bulletID;
        GetComponent<ObjectPool>().RefreshPool(bulletID);

        bulletRange = Mathf.Clamp(w_params.w_range, 5f, 50f);

        criRate = playerStat.criRate;
        criDamage = playerStat.criDamage;

        weaponSystem.magMax = w_params.w_magSize;

        float _accError = Mathf.Lerp(4.75f, 0f, Mathf.Clamp01(w_params.w_accuracy * 0.01f));
        float rangeNormalized = Mathf.Clamp01(Mathf.InverseLerp(5f, 50f, bulletRange));
        float _rangeError = Mathf.Lerp(2.75f, 0f, rangeNormalized);

        weaponSystem.bullet_angleError = Mathf.Clamp(_accError + _rangeError, 0f, 7.5f);

        reloadSpeed = w_params.w_reloadTime;

        if(weaponStatusCur != null)
        {
            weaponStatusCur.SetParamStat(w_params);
        }


    }

    public int SetWeaponAniType(int weaponType)
    {
        switch (weaponType)
        { // 0 : 권총, 1 : AR, 2 : SR, 3 : 로켓
            case 0: return 0;
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6: return 1;
            case 7: return 2;
            case 8: return 1;
            case 9: return 3;
            case 10: return 2;
            case 11: return 1;
            default: return 0;
        }
    }

    // 새 float 버전 (가산: float, 계수: 0~1)
    public void SetStatusByUpgradeF(int _stat, float plus, float percent01)
    {
        float k = 1f + percent01;

        switch (_stat)
        {
            // 정수 스탯
            case 0: // atk
                w_params.w_atk = Mathf.Max(1, Mathf.RoundToInt((w_paramsDefault.w_atk + Mathf.RoundToInt(plus)) * k));
                break;
            case 1: // rpm
                w_params.w_rpm = Mathf.RoundToInt((w_paramsDefault.w_rpm + Mathf.RoundToInt(plus)) * k);
                break;
            case 2: // mag
                w_params.w_magSize = Mathf.Max(1, Mathf.RoundToInt((w_paramsDefault.w_magSize + Mathf.RoundToInt(plus)) * k));
                break;
            case 4: // quality
                w_params.e_quality = Mathf.Clamp(Mathf.RoundToInt((w_paramsDefault.e_quality + Mathf.RoundToInt(plus)) * k), 0, 100);
                break;
            case 6: // bulletID (set)
                w_params.bulletID = Mathf.RoundToInt(plus);
                break;
            case 8: // atkType (set)
                w_params.w_atkType = (WeaponParamsSO.AtkTypes)Mathf.RoundToInt(plus);
                break;
            case 12: // 조준경 (set)
                usingScopeCode = Mathf.RoundToInt(plus);
                weaponSystem.SetLaserScope(usingScopeCode);
                break;

            // 실수 스탯
            case 3: // reload time (sec)
                w_params.w_reloadTime = RoundTo2Decimal((w_paramsDefault.w_reloadTime + plus) * k);
                break;
            case 5: // accuracy (0~100)
                w_params.w_accuracy = Mathf.Clamp(RoundTo2Decimal((w_paramsDefault.w_accuracy + plus) * k), 0, 100);
                break;
            case 7: // ammo multiplier
                w_params.w_ammoMulti = RoundTo2Decimal((w_paramsDefault.w_ammoMulti + plus) * k);
                RecalcAmmoMax(); // 선택
                break;
            case 9: // range
                w_params.w_range = Mathf.Clamp(RoundTo2Decimal((w_paramsDefault.w_range + plus) * k), 5f, 50f);
                break;
            case 11:
                add_ExplodeRadius = RoundTo2Decimal((add_ExplodeRadius + plus) * k);
                break;

            default:
                //Debug.LogWarning($"Unknown statID: {_stat}");
                break;
        }

        ApplyStatusInSystem();
    }

    void RecalcAmmoMax()
    {
        if (w_params.w_usingAmmo == WeaponParamsSO.Ammos.Default)
        {
            ammoMax = 9999;
            ammoCur = 9999;
        }
        else
        {
            ammoMax = (int)((w_params.w_ammoDefault + playerAmmoStat.playerAmmoDefault) * w_params.w_ammoMulti * playerAmmoStat.playerAmmoRevision);
            // ammoCur는 게임 디자인에 맞게 유지/보정 선택
        }
    }

    private float RoundTo2Decimal(float value)
    {
        return Mathf.Round(value * 100f) / 100f;
    }

    public WeaponParamsSO.Ammos GetUsingAmmo()
    {
        return w_params.w_usingAmmo;
    }

    public WeaponParamsSO.WeaponTypes GetWeaponType()
    {
        return w_params.w_type;
    }

    public WeaponParamsSO.AtkTypes GetAttackType()
    {
        return w_params.w_atkType;
    }
}
