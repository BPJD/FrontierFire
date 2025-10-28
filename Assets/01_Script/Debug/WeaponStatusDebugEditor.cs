#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(WeaponStatus))]
public class WeaponStatusDebugEditor : Editor
{
    bool showBaseCur;
    bool showDerived;
    bool showBuckets;

    static readonly string[] STAT_NAMES =
    {
        "0 공격력(atk)",
        "1 RPM(발사속도)",
        "2 탄창(magSize)",
        "3 재장전 시간(reloadTime)",
        "4 품질(quality)",
        "5 정확도(0~100)",
        "6 탄약종류(usingAmmo)",
        "7 탄약 계수(ammoMulti)",
        "8 공격유형(atkType)",
        "9 사거리(range)"
    };

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var ws = (WeaponStatus)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Weapon Debug View", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play 모드에서 값이 갱신됩니다.", MessageType.Info);
            return;
        }

        // private WeaponParams w_params / w_paramsDefault
        var wsType = typeof(WeaponStatus);
        var fiCur = wsType.GetField("w_params", BindingFlags.NonPublic | BindingFlags.Instance);
        var fiDef = wsType.GetField("w_paramsDefault", BindingFlags.NonPublic | BindingFlags.Instance);

        if (fiCur == null || fiDef == null)
        {
            EditorGUILayout.HelpBox("w_params / w_paramsDefault 필드를 찾을 수 없습니다.", MessageType.Warning);
            return;
        }

        object cur = fiCur.GetValue(ws);
        object def = fiDef.GetValue(ws);
        if (cur == null || def == null)
        {
            EditorGUILayout.HelpBox("무기 파라미터가 초기화되지 않았습니다.", MessageType.Warning);
            return;
        }

        // WeaponParams 값 읽기
        int cur_atk = GetInt(cur, "w_atk");
        int def_atk = GetInt(def, "w_atk");
        int cur_rpm = GetInt(cur, "w_rpm");
        int def_rpm = GetInt(def, "w_rpm");
        int cur_mag = GetInt(cur, "w_magSize");
        int def_mag = GetInt(def, "w_magSize");
        float cur_reload = GetFloat(cur, "w_reloadTime");
        float def_reload = GetFloat(def, "w_reloadTime");
        int cur_quality = GetInt(cur, "e_quality");
        int def_quality = GetInt(def, "e_quality");
        float cur_accuracy = GetFloat(cur, "w_accuracy");
        float def_accuracy = GetFloat(def, "w_accuracy");
        float cur_ammoMul = GetFloat(cur, "w_ammoMulti");
        float def_ammoMul = GetFloat(def, "w_ammoMulti");
        float cur_range = GetFloat(cur, "w_range");
        float def_range = GetFloat(def, "w_range");
        int cur_usingAmmo = GetInt(cur, "w_usingAmmo");
        int cur_atkType = GetInt(cur, "w_atkType"); // enum의 int값

        // === 기본/현재 값 표 ===
        showBaseCur = EditorGUILayout.Foldout(showBaseCur, "기본값 vs 현재값 (WeaponParams)");
        if (showBaseCur)
        {
            DrawPair("공격력", def_atk, cur_atk);
            DrawPair("RPM", def_rpm, cur_rpm);
            DrawPair("탄창", def_mag, cur_mag);
            DrawPair("재장전(sec)", def_reload, cur_reload);
            DrawPair("품질", def_quality, cur_quality);
            DrawPair("정확도(0~100)", def_accuracy, cur_accuracy);
            DrawPair("탄약 계수", def_ammoMul, cur_ammoMul);
            DrawPair("사거리", def_range, cur_range);
            EditorGUILayout.LabelField("탄약종류(usingAmmo)", cur_usingAmmo.ToString());
            EditorGUILayout.LabelField("공격유형(atkType)", cur_atkType.ToString());
        }

        // === 파생값 ===
        var fireRateProp = wsType.GetProperty("reloadSpeed", BindingFlags.Public | BindingFlags.Instance);
        var bulletRangeProp = wsType.GetProperty("bulletRange", BindingFlags.Public | BindingFlags.Instance);
        var bulletAtkProp = wsType.GetProperty("bulletAtk", BindingFlags.Public | BindingFlags.Instance);

        float fireInterval = 60f / Mathf.Max(1, cur_rpm); // 1 / (rpm/60)
        float reloadSpeed = (float)(fireRateProp != null ? fireRateProp.GetValue(ws) : 0f);
        float bulletRange = (float)(bulletRangeProp != null ? bulletRangeProp.GetValue(ws) : 0f);
        int bulletAtk = (int)(bulletAtkProp != null ? bulletAtkProp.GetValue(ws) : 0);

        showDerived = EditorGUILayout.Foldout(showDerived, "파생값 (System에 반영된 값)");
        if (showDerived)
        {
            DrawLine();
            EditorGUILayout.LabelField($"발사 간격: {fireInterval:0.000} sec  (RPM {cur_rpm})");
            EditorGUILayout.LabelField($"총알 공격력(bulletAtk): {bulletAtk}");
            EditorGUILayout.LabelField($"총알 사거리(bulletRange): {bulletRange:0.00}");
            EditorGUILayout.LabelField($"재장전 속도(reloadSpeed): {reloadSpeed:0.00}");

            // ====== bulletAtk 계산식 (확정 식) 출력 ======
            var unit = ws.GetComponentInParent<UnitStatus>();
            if (unit != null)
            {
                var unitType = typeof(UnitStatus);
                var propAtkCur = unitType.GetProperty("atkCur", BindingFlags.Public | BindingFlags.Instance);
                var propDamageCur = unitType.GetProperty("damageCur", BindingFlags.Public | BindingFlags.Instance);
                var propUnitParams = unitType.GetProperty("unitParams", BindingFlags.Public | BindingFlags.Instance);
                var propCriRate = unitType.GetProperty("criRate", BindingFlags.Public | BindingFlags.Instance);
                var propCriDamage = unitType.GetProperty("criDamage", BindingFlags.Public | BindingFlags.Instance);

                int atkCur_val = propAtkCur != null ? (int)propAtkCur.GetValue(unit) : 0;   // 현재 구조: u_atk (피해량 미포함)
                float damageCur_val = propDamageCur != null ? (float)propDamageCur.GetValue(unit) : 1f;  // 현재 구조: u_damage
                object uparams = propUnitParams != null ? propUnitParams.GetValue(unit) : null;
                int u_atk = uparams != null ? GetInt(uparams, "u_atk") : 0;
                float u_dmg = uparams != null ? GetFloat(uparams, "u_damage") : 1f;

                // "Fixed"인지 판별: bulletAtk == weaponAtk 라면 사실상 Fixed로 간주(간편 기준)
                bool isFixedLike = (bulletAtk == cur_atk);

                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("— bulletAtk 계산식 —", EditorStyles.miniBoldLabel);

                if (isFixedLike)
                {
                    EditorGUILayout.LabelField("✓ Fixed 타입: bulletAtk = weaponAtk");
                    EditorGUILayout.LabelField($"  = {cur_atk}");
                }
                else
                {
                    int calc = Mathf.CeilToInt((cur_atk + atkCur_val) * damageCur_val);
                    EditorGUILayout.LabelField("✓ Scaling 타입: bulletAtk = ceil((weaponAtk + atkCur) × damageCur)");
                    EditorGUILayout.LabelField($"  = ceil(({cur_atk} + {atkCur_val}) × {damageCur_val:0.###}) = {calc}");
                    if (calc != bulletAtk)
                    {
                        EditorGUILayout.HelpBox($"표시 계산값({calc})과 실제 bulletAtk({bulletAtk})이 다릅니다. 내부 로직/반올림 차이/타 보정(예: 탄약 계수)이 개입했는지 확인하세요.", MessageType.Info);
                    }
                }

                // 참고용: 플레이어 스탯 분해표
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("— Player Stat Snapshot —", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"u_atk = {u_atk}, u_damage = {u_dmg:0.###}  →  atkCur(현재 구조) = {atkCur_val}, damageCur = {damageCur_val:0.###}");

                // (옵션) 크리티컬 한방 데미지 예시 (샷건 보정 미포함, 투사체 지점의 보정은 별도)
                if (propCriRate != null && propCriDamage != null)
                {
                    float criRate = (float)propCriRate.GetValue(unit);    // 0~1로 관리된다면 적절히 변환 필요
                    float criDmgP = (float)propCriDamage.GetValue(unit);  // % 단위라고 가정 (예: 50 => +50%)
                    int critOne = Mathf.RoundToInt(bulletAtk * (1f + criDmgP * 0.01f));

                    EditorGUILayout.Space(6);
                    EditorGUILayout.LabelField("— Crit 예시 (샷건 보정 미포함) —", EditorStyles.miniBoldLabel);
                    EditorGUILayout.LabelField($"critDamage% = {criDmgP:0.##}% → critDmg ≈ {critOne}");
                    EditorGUILayout.LabelField($"* 실제 투사체에서는 damageRevisionShotGun 등 추가 보정이 곱해질 수 있습니다.");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("부모에서 UnitStatus를 찾지 못했습니다. bulletAtk 계산식을 표시할 수 없습니다.", MessageType.Info);
            }
        }

        // === 업그레이드 누적 버킷 ===
        var up = ws.GetComponent<WeaponStatUpgrade>();
        showBuckets = EditorGUILayout.Foldout(showBuckets, "업그레이드 누적 (가산/계수)");
        if (showBuckets)
        {
            DrawLine();
            if (up == null)
            {
                EditorGUILayout.HelpBox("WeaponStatUpgrade 컴포넌트를 찾지 못했습니다.", MessageType.Info);
            }
            else
            {
                var addDict = GetDict(up, "add");        // Dictionary<int,float>
                var multDict = GetDict(up, "mult");       // Dictionary<int,float> (0.30 = +30)
                if (multDict == null)
                    multDict = GetDict(up, "multPercent"); // 대안 필드명

                EditorGUILayout.LabelField("가산 합계 (Add):");
                DrawBucket(addDict, true);

                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("계수 합계 (Multiply, 0.30 = +30%):");
                DrawBucket(multDict, false);
            }
        }

        // 실시간 갱신
        if (Application.isPlaying)
            Repaint();
    }

    static void DrawPair(string name, int baseVal, int curVal)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(name, GUILayout.Width(160));
        EditorGUILayout.LabelField($"기본 {baseVal}", GUILayout.Width(90));
        EditorGUILayout.LabelField($"현재 {curVal}");
        EditorGUILayout.EndHorizontal();
    }

    static void DrawPair(string name, float baseVal, float curVal)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(name, GUILayout.Width(160));
        EditorGUILayout.LabelField($"기본 {baseVal:0.##}", GUILayout.Width(90));
        EditorGUILayout.LabelField($"현재 {curVal:0.##}");
        EditorGUILayout.EndHorizontal();
    }

    static void DrawBucket(Dictionary<int, float> dict, bool isAdd)
    {
        if (dict == null || dict.Count == 0)
        {
            EditorGUILayout.HelpBox(isAdd ? "가산 누적 없음" : "계수 누적 없음", MessageType.None);
            return;
        }
        foreach (var kv in dict)
        {
            string statName = (kv.Key >= 0 && kv.Key < STAT_NAMES.Length) ? STAT_NAMES[kv.Key] : $"stat {kv.Key}";
            string valStr = isAdd ? kv.Value.ToString("0.###")
                                  : $"{kv.Value:+0.###;-0.###;+0} (={(1f + kv.Value) * 100f:0.##}% 계수)";
            EditorGUILayout.LabelField($"• {statName}  ->  {valStr}");
        }
    }

    static int GetInt(object obj, string field)
    {
        var fi = obj.GetType().GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return fi != null ? (int)fi.GetValue(obj) : 0;
    }
    static float GetFloat(object obj, string field)
    {
        var fi = obj.GetType().GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return fi != null ? Convert.ToSingle(fi.GetValue(obj)) : 0f;
    }
    static Dictionary<int, float> GetDict(object obj, string field)
    {
        var fi = obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
        return fi != null ? (Dictionary<int, float>)fi.GetValue(obj) : null;
    }

    static void DrawLine() => EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
}
#endif
