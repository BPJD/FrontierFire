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

        // 필드 값 꺼내기 (WeaponParams의 public 필드 사용)
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
        int cur_atkType = GetInt(cur, "w_atkType");

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
                // private dict 읽기: add / mult / multPercent 지원
                var addDict = GetDict(up, "add"); // Dictionary<int,float>
                var multDict = GetDict(up, "mult"); // Dictionary<int,float> (0.30 = +30)
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
