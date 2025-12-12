using UnityEngine;
using DamageNumbersPro;
using Combat;

public class UnitStatus : MonoBehaviour
{
    public UnitParamsSO unitDataSource; // ScriptableObject 원본

    public UnitParams unitParams{ get; private set; }
    public UnitParams unitParamsDefault{ get; private set; }

    public event System.Action<int, int> OnHpChanged;

    public bool isUnitHit = false;

    public int hpCur = 0;
    public int hpRegen { get; private set; } = 0;
    public float hpRegenSpeed { get; private set; } = 1f;
    public int atkCur { get; private set; } = 0;
    public float damageCur { get; private set; } = 1f;

    public float criRate { get; private set; } = 0f;
    public float criDamage { get; private set; } = 0f;
    public float moveSpeed { get; private set; } = 5f;
    public float jumpPower { get; private set; } = 10f;

    public float immunePer { get; set; } = 1f;

    [SerializeField] float[] armorRevisionByType = { 1f, 1f, 1f };

    AudioSource soundPlayer;
    [SerializeField] AudioClip[] sounds_GetDamageByType;
    [SerializeField] AudioClip sound_GetCritical;
    [SerializeField] AudioClip sound_DeathSoundByType;

    Data_DamageNumbers data_DNum;
    Transform tr;

    [SerializeField] bool isTurret = false;

    private void Awake()
    {
        unitParams = new UnitParams(unitDataSource);
        unitParamsDefault = new UnitParams(unitParams); // 백업용 복사
        soundPlayer = GetComponent<AudioSource>();
        data_DNum = GameObject.FindGameObjectWithTag("Data").GetComponent<Data_DamageNumbers>();
        tr = transform;

        moveSpeed = unitParams.u_moveSpeed;
        hpCur = unitParams.u_hp;
        hpRegen = unitParams.u_hpRegen;
        hpRegenSpeed = unitParams.u_hpRegenSpeed;

        SetRevision();
        SetCurrentAtk();
        damageCur = 1f;
    }

    private void OnEnable()
    {
        HP_Reset();
    }


    public void UnitGetHeal(int _heal, bool isUIPrint)
    {
        hpCur = Mathf.Clamp(hpCur + _heal, 0, unitParams.u_hp);
        OnHpChanged?.Invoke(hpCur, unitParams.u_hp);

        if(isUIPrint == true)
        {
            DamageNumber number = data_DNum.GetDamageNumberPrefab(Data_DamageNumbers.NumberType.Heal);
            number.Spawn(tr.position + (Vector3.up * 1.75f), _heal);
        }
    }


    public DamageResult TakeDamage(in DamagePayload p)
    {
        // 1) 티어 산정(방어 속성 계수 기반)
        int tier = GetDamageTier(armorRevisionByType[(int)p.atkType]);

        // 2) 최종 피해 계산
        float immune = Mathf.Max(0f, unitParams.u_immunePer * immunePer);
        float armorMul = armorRevisionByType[(int)p.atkType];

        float raw = p.baseDamage;

        if (p.atkType != WeaponParamsSO.AtkTypes.Fixed)
        {
            raw = p.baseDamage - unitParams.u_def;
            if (raw < 0f) raw = 0f;
            raw *= immune * armorMul;
        }

        // 외부 보정
        raw = (raw + p.addFlat) * (p.mul <= 0f ? 1f : p.mul);

        // 반올림/클램프는 마지막에
        int final = Mathf.Clamp(Mathf.RoundToInt(raw), 0, p.baseDamage);


        // 3) 비주얼/사운드
        PrintDamageNumber(final, tier, p.isCritical, p.isWeakPoint, p.hitPoint);
        if (soundPlayer != null)
        {
            if (p.isCritical) soundPlayer.PlayOneShot(sound_GetCritical);
            if ((uint)tier < (uint)sounds_GetDamageByType.Length)
                soundPlayer.PlayOneShot(sounds_GetDamageByType[tier]);
        }

        // 4) 적용 & 사망 체크
        bool died = DamageAndCheckDeath_Internal(final);

        return new DamageResult
        {
            finalDamage = final,
            damageTier = tier,
            isCritical = p.isCritical,
            killed = died
        };
    }

    int GetDamageTier(float revision)
    {
        if (revision >= 0.8f)
        {
            return 0;
        }
        else if (revision >= 0.4f)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    void PrintDamageNumber(int finalDamage, int damageTier, bool isCri, bool isWeakPoint, Vector3 pos)
    {
        DamageNumber _number;
        if (isCri)
        {
            _number = data_DNum.GetDamageNumberPrefab(Data_DamageNumbers.NumberType.Critical);
        }
        else if (isWeakPoint)
        {
            _number = data_DNum.GetDamageNumberPrefab(Data_DamageNumbers.NumberType.WeakPoint);
        }
        else
        {
            _number = data_DNum.GetDamageNumberPrefab((Data_DamageNumbers.NumberType)damageTier + 1);
        }

        _number.Spawn(pos, finalDamage);

    }

    bool DamageAndCheckDeath_Internal(int damage)
    {
        hpCur = Mathf.Clamp(hpCur - damage, 0, unitParams.u_hp);
        OnHpChanged?.Invoke(hpCur, unitParams.u_hp);
        isUnitHit = true;

        if (hpCur > 0) return false;

        // ----- 이하 기존 로직 유지 -----
        gameObject.layer = 10;
        gameObject.tag = "Dead";
        if (soundPlayer != null) soundPlayer.PlayOneShot(sound_DeathSoundByType);

        switch (unitParams.u_type)
        {
            case UnitParamsSO.UnitTypes.Player:
                GetComponent<PlayerDeath>().enabled = true;
                GetComponent<PlayerDeath>()?.DeathAnimationPlay(unitParams.u_hp, damage);
                break;
            case UnitParamsSO.UnitTypes.Enemy:
                if (!isTurret)
                {
                    GetComponent<EnemyUnitDeath>().DeathAnimationPlay(unitParams.u_hp, damage);
                    GetComponent<EnemyAttackSystem>().isDead = true;
                }
                else
                {
                    GetComponent<EnemyTurret>().TurretDown();
                    GetComponent<TurretAttackSystem>().isDead = true;
                }
                break;
            case UnitParamsSO.UnitTypes.Boss:
                GetComponent<BossControlSystem>().BossDead();
                break;
            default:
                gameObject.SendMessage(NeutralUnits.deadMsg, SendMessageOptions.DontRequireReceiver);
                break;
        }
        return true;
    }

    void SetRevision()
    {
        switch (unitParams.u_armorLevel)
        {
            case 0: armorRevisionByType = new float[] { 1f, 0.75f, 0.5f }; break;
            case 1: armorRevisionByType = new float[] { 0.7f, 1f, 1f }; break;
            case 2: armorRevisionByType = new float[] { 0.3f, 0.6f, 0.9f }; break;
        }

        Control_Stage gameControl = GameObject.FindGameObjectWithTag("GameController").GetComponent<Control_Stage>();

        if(unitParams.u_type != UnitParamsSO.UnitTypes.Player)
        {
            StatRevisionByLevel(gameControl.worldCur - 1, gameControl.difficulty);
        }
    }

    void StatRevisionByLevel(int stageLev, int difficulty)
    {
        float hpRev = Mathf.Max(1f, (stageLev * 1.3f) + (difficulty * 1.2f));
        float atkRev = Mathf.Max(1f, (stageLev * 1.15f) + (difficulty * 1.1f));

        hpCur *= (int)hpRev;
        SetCurrentAtk();
        atkCur *= (int)atkRev;
    }

    void SetMoveSpeed()
    {
        if(unitParams.u_type == UnitParamsSO.UnitTypes.Player)
        {
            GetComponent<PlayerMove>().SpeedSet();
        }
    }

    void SetCurrentAtk()
    {
        atkCur = Mathf.Max(0, unitParams.u_atk);
        damageCur = Mathf.Max(0.01f, unitParams.u_damage);
        criRate = Mathf.Max(1f, unitParams.u_criRate);
        criDamage = Mathf.Max(0f, unitParams.u_criDamage);
    }

    public void SetStatusByUpgrade(int _stat, float valuePlus, float valueMulti)
    {
        PlayerShootingStat _playerAmmo = GetComponent<PlayerShootingStat>();

        switch (_stat)
        {
            case 0: // HP
                int _value = Mathf.RoundToInt((unitParamsDefault.u_hp + valuePlus) * (1f + (valueMulti * 0.01f)));
                int _upValue = _value - unitParams.u_hp;
                unitParams.u_hp = _value;
                hpCur += _upValue;
                OnHpChanged?.Invoke(hpCur, unitParams.u_hp);
                break;
            case 1: // 공격력
                unitParams.u_atk = Mathf.RoundToInt((unitParamsDefault.u_atk + valuePlus) * (1f + (valueMulti * 0.01f)));
                SetCurrentAtk();
                break;
            case 2: // 방어력
                unitParams.u_def = Mathf.RoundToInt((unitParamsDefault.u_def + valuePlus) * (1f + (valueMulti * 0.01f)));
                break;
            case 3: // 피해감소율 (float)
                unitParams.u_immunePer = RoundTo2Decimal(unitParamsDefault.u_immunePer + (valueMulti * 0.01f));
                break;
            case 4: // 방어 속성 (곱 연산 없음)
                unitParams.u_armorLevel = Mathf.Clamp(unitParamsDefault.u_armorLevel + (int)valuePlus, 0, 2);
                SetRevision(); // 방어 속성은 리비전 업데이트 필요
                break;
            case 5: // 이동속도
                unitParams.u_moveSpeed = RoundTo2Decimal((unitParamsDefault.u_moveSpeed + valuePlus) * (1f + (valueMulti * 0.01f)));
                moveSpeed = Mathf.Max(1f, unitParams.u_moveSpeed);
                SetMoveSpeed();
                break;
            case 6: // 점프력
                unitParams.u_jumpPower = RoundTo2Decimal((unitParamsDefault.u_jumpPower + valuePlus) * (1f + (valueMulti * 0.01f)));
                jumpPower = unitParams.u_jumpPower;
                SetMoveSpeed();
                break;
            case 7: // 멀티 점프 횟수 (곱 연산 없음)
                unitParams.u_multijumpCount = unitParamsDefault.u_multijumpCount + (int)valuePlus;
                SetMoveSpeed();
                break;
            case 8: // 사격 정확도
                unitParams.u_shotAccuracy = RoundTo2Decimal(unitParamsDefault.u_shotAccuracy + valuePlus);
                break;
            case 9: // 치명타 확률
                unitParams.u_criRate = RoundTo2Decimal(unitParamsDefault.u_criRate + valuePlus);
                break;
            case 10: // 치명타 피해
                unitParams.u_criDamage = RoundTo2Decimal(unitParamsDefault.u_criDamage + valuePlus);
                break;
            case 11: // 피해량
                unitParams.u_damage = RoundTo2Decimal(unitParamsDefault.u_damage + valuePlus);
                SetCurrentAtk();
                break;
            case 12: // 탄약 획득량
                _playerAmmo.playerAmmoGain = RoundTo2Decimal(_playerAmmo.playerAmmoGain + valuePlus);
                break;
            case 13: // 탄약 소지량
                _playerAmmo.playerAmmoRevision = RoundTo2Decimal(_playerAmmo.playerAmmoRevision + valuePlus);
                break;
            case 14: // 아이템 드롭률
                _playerAmmo.playerItemDropRate = RoundTo2Decimal(_playerAmmo.playerItemDropRate + valuePlus);
                break;
            case 15: // HP 회복량
                unitParams.u_hpRegen = Mathf.RoundToInt((unitParamsDefault.u_hpRegen + valuePlus) * (1f + (valueMulti * 0.01f)));
                break;
            case 16: // HP 회복속도
                unitParams.u_hpRegenSpeed = Mathf.RoundToInt((unitParamsDefault.u_hpRegenSpeed + valuePlus) * (1f + (valueMulti * 0.01f)));
                break;
            default:
                //Debug.LogWarning($"Unknown statID: {_stat}");
                break;
        }
    }

    private float RoundTo2Decimal(float value)
    {
        return Mathf.Round(value * 100f) / 100f;
    }

    public void HP_Reset()
    {
        hpCur = unitParams.u_hp;
        SetRevision();
    }

}
