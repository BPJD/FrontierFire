using UnityEngine;

public class UnitStatUpgrade : MonoBehaviour
{
    [SerializeField] private UnitParams upParamsPlus;
    [SerializeField] private UnitParams upParamsMulti;

    UnitStatus playerStat;
    Data_StatUpgrades data; // 필요시 필드 추가

    private void Awake()
    {
        upParamsPlus = new UnitParams();  // 명시적으로 생성
        upParamsMulti = new UnitParams();
    }

    private void Start()
    {
        playerStat = GetComponent<UnitStatus>();
    }

    public void UpgradeStat(int code, float value)
    {
        int statID = code % 100;
        bool isPlus = code < 200;

        UnitParams target = isPlus ? upParamsPlus : upParamsMulti;

        ApplyStatValue(target, statID, value);

        float plusValue = GetStatValue(upParamsPlus, statID);
        float multiValue = GetStatValue(upParamsMulti, statID);

        playerStat.SetStatusByUpgrade(statID, plusValue, multiValue);
    }

    public void UpgradeStatPackageById(int id)
    {
        if (data == null)
        {
            var go = GameObject.FindGameObjectWithTag("Data");
            if (go) data = go.GetComponent<Data_StatUpgrades>();
        }
        var pack = data != null ? data.GetAllStatUps(id) : null;
        if (pack == null || pack.Count == 0) return;

        foreach (var so in pack)
        {
            // 기존 단일 경로 재사용: code 합성 후 호출
            int code = (so.up_type == 0 ? 100 : 200) + so.up_stat;
            UpgradeStat(code, so.up_value); // ← 기존 함수 유지 활용
        }
    }

    private void ApplyStatValue(UnitParams param, int statID, float value)
    { //곱적용으로 올리면, %로 적용되게 만들어져 있음. 예를 들어 체력 200코드 100 증가 해놓으면 체력 2배 되는거
        switch (statID)
        {
            case 0: param.u_hp += (int)value; break;
            case 1: param.u_atk += (int)value; break;
            case 2: param.u_def += (int)value; break;
            case 3: param.u_immunePer += value * 0.01f; break; // float 처리
            case 4: param.u_armorLevel += (int)value; break;
            case 5: param.u_moveSpeed += value; break;
            case 6: param.u_jumpPower += value; break;
            case 7: param.u_multijumpCount += (int)value; break;
            case 8: param.u_shotAccuracy += value * 0.01f; break;
            case 9: param.u_criRate += value * 0.01f; break;
            case 10: param.u_criDamage += value * 0.01f; break;
            case 11: param.u_damage += value * 0.01f; break;
            default:
                Debug.LogWarning($"Unknown statID: {statID}");
                break;
        }
    }

    private float GetStatValue(UnitParams param, int statID)
    {
        return statID switch
        {
            0 => (float)param.u_hp,
            1 => (float)param.u_atk,
            2 => (float)param.u_def,
            3 => param.u_immunePer * 100,
            4 => (float)param.u_armorLevel,
            5 => param.u_moveSpeed,
            6 => param.u_jumpPower,
            7 => param.u_multijumpCount,
            8 => param.u_shotAccuracy * 100,
            9 => param.u_criRate * 100,
            10 => param.u_criDamage * 100,
            11 => param.u_damage * 100,
            _ => 0
        };
    }
}
