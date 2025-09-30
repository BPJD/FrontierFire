using UnityEngine;

public class UnitStatus : MonoBehaviour
{
    public UnitParamsSO unitDataSource; // ScriptableObject 원본

    public UnitParams unitParams{ get; private set; }
    public UnitParams unitParamsDefault{ get; private set; }

    public event System.Action<int, int> OnHpChanged;

    public bool isUnitHit = false;

    public int hpCur = 0;
    public int atkCur { get; private set; } = 0;
    public float moveSpeed { get; private set; } = 5f;
    public float jumpPower { get; private set; } = 10f;

    public float immunePer { get; set; } = 1f;

    [SerializeField] float[] armorRevisionByType = { 1f, 1f, 1f };

    private void Awake()
    {
        unitParams = new UnitParams(unitDataSource);
        unitParamsDefault = new UnitParams(unitParams); // 백업용 복사
        

        moveSpeed = unitParams.u_moveSpeed;
        hpCur = unitParams.u_hp;
        SetRevision();
        SetCurrentAtk();
    }

    private void OnEnable()
    {
        HP_Reset();
    }


    public void UnitGetHeal(int _heal)
    {
        hpCur = Mathf.Clamp(hpCur + _heal, 0, unitParams.u_hp);
        OnHpChanged?.Invoke(hpCur, unitParams.u_hp);
    }

    public void UnitGetDamage(int _damage, int _weaponType, int _attackType)
    {
        int finalDamage = Mathf.Clamp(Mathf.RoundToInt((_damage - unitParams.u_def) * unitParams.u_immunePer * immunePer * armorRevisionByType[_attackType]), 0, _damage);
        DamageAndCheckDeath(finalDamage);
        Debug.Log(finalDamage);
    }

    void DamageAndCheckDeath(int damage)
    {
        hpCur = Mathf.Clamp(hpCur - damage, 0, unitParams.u_hp);
        OnHpChanged?.Invoke(hpCur, unitParams.u_hp);
        isUnitHit = true;

        if (hpCur <= 0)
        {
            Debug.Log("UnitDead");
            gameObject.layer = 10;
            gameObject.tag = "Dead";

            switch (unitParams.u_type)
            {
                case UnitParamsSO.UnitTypes.Player:
                    GetComponent<PlayerDeath>().enabled = true;
                    GetComponent<PlayerDeath>()?.DeathAnimationPlay(unitParams.u_hp, damage);
                    break;
                case UnitParamsSO.UnitTypes.Enemy:
                    GetComponent<EnemyUnitDeath>()?.DeathAnimationPlay(unitParams.u_hp, damage);
                    GetComponent<EnemyAttackSystem>().isDead = true;
                    break;
                case UnitParamsSO.UnitTypes.Boss:
                    GetComponent<BossControlSystem>().BossDead();
                    break;
                default:
                    gameObject.SendMessage(NeutralUnits.deadMsg, SendMessageOptions.DontRequireReceiver);
                    break;
            }
        }
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
        atkCur = Mathf.CeilToInt(unitParams.u_atk * unitParams.u_damage);
    }

    public void SetStatusByUpgrade(int _stat, float valuePlus, float valueMulti)
    {
        switch (_stat)
        {
            case 0: // HP
                int _value = Mathf.RoundToInt((unitParamsDefault.u_hp + valuePlus) * (1f + valueMulti * 0.01f));
                int _upValue = _value - unitParams.u_hp;
                unitParams.u_hp = _value;
                hpCur += _upValue;
                OnHpChanged?.Invoke(hpCur, unitParams.u_hp);
                break;
            case 1: // 공격력
                unitParams.u_atk = Mathf.RoundToInt((unitParamsDefault.u_atk + valuePlus) * (1f + valueMulti * 0.01f));
                SetCurrentAtk();
                break;
            case 2: // 방어력
                unitParams.u_def = Mathf.RoundToInt((unitParamsDefault.u_def + valuePlus) * (1f + valueMulti * 0.01f));
                break;
            case 3: // 피해감소율 (float)
                unitParams.u_immunePer = RoundTo2Decimal(unitParamsDefault.u_immunePer + valuePlus * 0.01f);
                break;
            case 4: // 방어 속성 (곱 연산 없음)
                unitParams.u_armorLevel = Mathf.Clamp(unitParamsDefault.u_armorLevel + (int)valuePlus, 0, 2);
                SetRevision(); // 방어 속성은 리비전 업데이트 필요
                break;
            case 5: // 이동속도
                unitParams.u_moveSpeed = RoundTo2Decimal((unitParamsDefault.u_moveSpeed + valuePlus) * (1f + valueMulti * 0.01f));
                moveSpeed = unitParams.u_moveSpeed;
                SetMoveSpeed();
                break;
            case 6: // 점프력
                unitParams.u_jumpPower = RoundTo2Decimal((unitParamsDefault.u_jumpPower + valuePlus) * (1f + valueMulti * 0.01f));
                jumpPower = unitParams.u_jumpPower;
                SetMoveSpeed();
                break;
            case 7: // 멀티 점프 횟수 (곱 연산 없음)
                unitParams.u_multijumpCount = unitParamsDefault.u_multijumpCount + (int)valuePlus;
                SetMoveSpeed();
                break;
            case 8: // 사격 정확도
                unitParams.u_shotAccuracy = RoundTo2Decimal((unitParamsDefault.u_shotAccuracy + valuePlus) * (1f + valueMulti * 0.01f));
                break;
            case 9: // 치명타 확률
                unitParams.u_criRate = RoundTo2Decimal((unitParamsDefault.u_criRate + valuePlus) * (1f + valueMulti * 0.01f));
                break;
            case 10: // 치명타 피해
                unitParams.u_criDamage = RoundTo2Decimal((unitParamsDefault.u_criDamage + valuePlus) * (1f + valueMulti * 0.01f));
                break;
            case 11: // 피해량
                unitParams.u_damage = RoundTo2Decimal((unitParamsDefault.u_damage + valuePlus) * (1f + valueMulti * 0.01f));
                SetCurrentAtk();
                break;
            default:
                Debug.LogWarning($"Unknown statID: {_stat}");
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
