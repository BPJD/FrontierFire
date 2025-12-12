using System.Collections.Generic;
using UnityEngine;

public class UnitStatUpgrade : MonoBehaviour
{
    [Header("´©Àû ¹öÅ¶ (°¡»ê/°è¼ö)")]
    [SerializeField] private UnitParams upParamsPlus;   // +°¡»ê ´©Àû
    [SerializeField] private UnitParams upParamsMulti;  // ¡¿°è¼ö ´©Àû(ÆÛ¼¾Æ® Ç×¸ñÀº 0~1·Î ´©Àû)

    [Header("´©Àû ÀÚ¿ø ¹öÅ¶(°¡»ê¸¸)")]
    [SerializeField] private float res_AmmoGain;
    [SerializeField] private float res_AmmoMax;
    [SerializeField] private float res_DropRate;

    [Header("ÂüÁ¶")]
    [SerializeField] private UnitStatus playerStat;               // ½ÇÁ¦ ½ºÅÈ º¸À¯
    [SerializeField] private PlayerShootingStat playerAmmoStat;   // ÇÃ·¹ÀÌ¾î Åº¾à ½ºÅÈ
    [SerializeField] private PlayerWeaponController weaponCon;    // ¹«±â/ÆÄ»ý °»½Å¿ë
    [SerializeField] private Data_StatUpgrades data;              // ¾÷±×·¹ÀÌµå SO Ç®



    // (¼±ÅÃ) Àû¿ë ÀÌ·Â: ¾ÆÀÌµð(=¾÷±×·¹ÀÌµå ID) ´ÜÀ§·Î ±â·Ï
    public List<int> upgradesCur { get; private set; } = new List<int>();

    void Awake()
    {
        // ¸í½ÃÀûÀ¸·Î »ý¼º (ÂüÁ¶ÇüÀÌ¹Ç·Î null ¹æÁö)
        upParamsPlus = new UnitParams();
        upParamsMulti = new UnitParams();
    }

    void Start()
    {
        if (!playerStat) playerStat = GetComponent<UnitStatus>();
        if (!playerAmmoStat) playerAmmoStat = GetComponent<PlayerShootingStat>();
        if (!weaponCon) weaponCon = GetComponent<PlayerWeaponController>();
        if (!data)
        {
            var go = GameObject.FindGameObjectWithTag("Data");
            if (go) data = go.GetComponent<Data_StatUpgrades>();
        }


    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // °ø°³ API: ¾÷±×·¹ÀÌµå "ÄÚµå" ´ÜÀ§ Àû¿ë (±âÁ¸ ·ÎÁ÷ À¯Áö)
    //  - code < 200 ¡æ °¡»ê, code >= 200 ¡æ °è¼ö
    //  - statID = code % 100
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void UpgradeStat(int code, float value)
    {
        int statID = code % 100;
        bool isPlus = code < 200;

        UnitParams target = isPlus ? upParamsPlus : upParamsMulti;

        ApplyStatValue(target, statID, value);

        // ÇöÀç±îÁö ´©ÀûµÈ plus/multi °ªÀ» ÀÐ¾î ÃÖÁ¾ ¹Ý¿µ
        float plusValue = GetStatValue(upParamsPlus, statID);
        float multiValue = GetStatValue(upParamsMulti, statID);

        // ½ÇÁ¦ ½ºÅÈ¿¡ ´øÁö±â (ÇÁ·ÎÁ§Æ® Á¤ÀÇ)
        if (playerStat)
            playerStat.SetStatusByUpgrade(statID, plusValue, multiValue);

        // ¹«±â/ÆÄ»ýÄ¡ ¹Ý¿µ (ÀÖÀ¸¸é)
        weaponCon?.ApplyUnitUpgrade();
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // °ø°³ API: "¾÷±×·¹ÀÌµå ID" ÆÐÅ°Áö(´ÙÇà) ÀüÃ¼ Àû¿ë
    //  - µ¿ÀÏ IDÀÇ ¸ðµç SO¸¦ ¼ø¼­´ë·Î Àû¿ëÇÑ´Ù.
    //  - ³»ºÎÀûÀ¸·Î ±âÁ¸ UpgradeStat(code, value) °æ·Î¸¦ »ç¿ë
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void UpgradeStatPackageById(int id)
    {
        // Áö¿¬ Ä³½Ã
        if (!data)
        {
            var go = GameObject.FindGameObjectWithTag("Data");
            if (go) data = go.GetComponent<Data_StatUpgrades>();
        }

        var pack = data != null ? data.GetAllStatUps(id) : null;
        if (pack == null || pack.Count == 0)
        {
            Debug.LogWarning($"[UnitStatUpgrade] ¾÷±×·¹ÀÌµå ID {id} ¸¦ Ã£Áö ¸øÇÔ");
            return;
        }

        // µ¿ÀÏ ID ¹­À½ ÀüÃ¼ Àû¿ë
        foreach (var so in pack)
        {
            if (so == null) continue;
            int code = (so.up_type == 0 ? 100 : 200) + so.up_stat;
            UpgradeStat(code, so.up_value);
        }

        // ÀÌ·Â ÀúÀå(Áßº¹ Çã¿ë: ¹«±â ÂÊ°ú µ¿ÀÏ Èå¸§)
        upgradesCur.Add(id);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ³»ºÎ: ´©Àû ¹öÅ¶¿¡ ½ÇÁ¦ °ª ´õÇÏ±â
    //  - °¡»ê/°è¼ö´Â È£ÃâºÎ¿¡¼­ UnitParams ¼±ÅÃÀ¸·Î ºÐ¸®
    //  - ÀÏºÎ ½ºÅÈÀº ÆÛ¼¾Æ® ¡æ 0~1·Î È¯»ê ÀúÀå(¿©±â¼­ *0.01f)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void ApplyStatValue(UnitParams param, int statID, float value)
    {
        switch (statID)
        {
            case 0: param.u_hp += (int)value; break; // Á¤¼ö °¡»ê/°è¼ö ´©Àû
            case 1: param.u_atk += (int)value; break;
            case 2: param.u_def += (int)value; break;
            case 3: param.u_immunePer += value * 0.01f; break; // % ¡æ 0~1
            case 4: param.u_armorLevel += (int)value; break;
            case 5: param.u_moveSpeed += value; break; // ½Ç¼ö °¡»ê/°è¼ö
            case 6: param.u_jumpPower += value; break;
            case 7: param.u_multijumpCount += (int)value; break;
            case 8: param.u_shotAccuracy += value * 0.01f; break; // % ¡æ 0~1
            case 9: param.u_criRate += value * 0.01f; break;
            case 10: param.u_criDamage += value * 0.01f; break;
            case 11: param.u_damage += value * 0.01f; break;
            case 12: res_AmmoGain += value * 0.01f; break;
            case 13: res_AmmoMax += value * 0.01f; break;
            case 14: res_DropRate += value * 0.01f; break;
            case 15: param.u_hpRegen += (int)value; break;
            case 16: param.u_hpRegenSpeed += value; break;
            default:
                Debug.LogWarning($"[UnitStatUpgrade] Unknown statID: {statID}");
                break;
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ³»ºÎ: ´©Àû ¹öÅ¶ °ª ÀÐ±â
    //  - ÆÛ¼¾Æ® Ç×¸ñÀº 0~1 ´©Àû°ªÀ» ´Ù½Ã 0~100 ´ÜÀ§·Î µÇµ¹·Á SetStatusByUpgrade·Î Àü´Þ
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private float GetStatValue(UnitParams param, int statID)
    {
        return statID switch
        {
            0 => (float)param.u_hp,
            1 => (float)param.u_atk,
            2 => (float)param.u_def,
            3 => param.u_immunePer * 100f, // 0~1 ¡æ 0~100
            4 => (float)param.u_armorLevel,
            5 => param.u_moveSpeed,
            6 => param.u_jumpPower,
            7 => param.u_multijumpCount,
            8 => param.u_shotAccuracy * 100f, 
            9 => param.u_criRate * 100f,
            10 => param.u_criDamage * 100f,
            11 => param.u_damage - 1f,
            12 => res_AmmoGain,
            13 => res_AmmoMax,
            14 => res_DropRate,
            15 => (float)param.u_hpRegen,
            16 => param.u_hpRegenSpeed,
            _ => 0f
        };
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // À¯Æ¿: ÀüÃ¼ ÃÊ±âÈ­/ÀçÀû¿ë (Àåºñ ±³Ã¼/¸®¼Â µî ½Ã Æí¸®)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void ResetAllUpgrades()
    {
        upParamsPlus = new UnitParams();
        upParamsMulti = new UnitParams();
        upgradesCur.Clear();

        // ¸ðµç ½ºÅÈ¿¡ ´ëÇØ 0 ¹Ý¿µ(ÇÊ¿äÇÏ¸é ½ÇÁ¦ ½ºÅÈ ¸®¼Â ·çÆ¾ È£Ãâ)
        // ¿©±â¼­´Â °³º° statID¸¦ ¾Ë ¼ö ¾øÀ¸´Ï, ÇÁ·ÎÁ§Æ®ÀÇ ±âº»Ä¡ Àç°è»ê ·çÆ¾ÀÌ ÀÖÀ¸¸é È£ÃâÇØÁà.
        weaponCon?.ApplyUnitUpgrade();
    }

    public void ReapplyAllUpgrades()
    {
        // ÇöÀç º¸À¯ÇÑ IDµéÀ» Ã³À½ºÎÅÍ ´Ù½Ã Àû¿ëÇÏ°í ½ÍÀ» ¶§ »ç¿ë
        var snapshot = new List<int>(upgradesCur);
        ResetAllUpgrades();
        for (int i = 0; i < snapshot.Count; i++)
            UpgradeStatPackageById(snapshot[i]);
    }
}
