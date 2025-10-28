/*






#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitStatus))]
public class UnitStatusEditor : Editor
{
    private SerializedProperty unitDataSourceProp;

    private bool foldSummary = true;
    private bool foldBase = true;
    private bool foldCurrent = true;
    private bool foldArmor = false;
    private bool foldActions = true;

    // 임시 입력값
    private int healAmount = 10;
    private int damageAmount = 10;

    private void OnEnable()
    {
        unitDataSourceProp = serializedObject.FindProperty("unitDataSource");
    }

    public override void OnInspectorGUI()
    {
        // 상단: SO 선택
        serializedObject.Update();
        EditorGUILayout.PropertyField(unitDataSourceProp);
        serializedObject.ApplyModifiedProperties();

        var us = (UnitStatus)target;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("UnitStatus ? Debug Inspector", EditorStyles.boldLabel);

        // 요약
        DrawSummary(us);

        // 기본 스탯 (SO/Default 값)
        DrawBaseStats(us);

        // 현재 스탯(실시간)
        DrawCurrentStats(us);

        // 방어 리비전 테이블
        DrawArmorTable(us);

        // 디버그 액션 버튼
        DrawActions(us);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(us);
        }
    }

    private void DrawSummary(UnitStatus us)
    {
        foldSummary = EditorGUILayout.Foldout(foldSummary, "Summary");
        if (!foldSummary) return;

        EditorGUI.indentLevel++;
        var so = us.unitDataSource;
        if (so == null)
        {
            EditorGUILayout.HelpBox("UnitParamsSO가 비어 있습니다. (unitDataSource)", MessageType.Warning);
            EditorGUI.indentLevel--;
            return;
        }

        EditorGUILayout.LabelField("Name", so.u_name);
        EditorGUILayout.LabelField("Type", so.u_type.ToString());

        if (Application.isPlaying)
        {
            // 안전하게 u_hp(현재 최대치) 가져오기
            int maxHpInt = GetSafe(us, _ => _.unitParams != null ? (int?)_.unitParams.u_hp : (int?)null, (int?)so.u_hp) ?? so.u_hp;
            EditorGUILayout.LabelField("HP (Cur / Max)", $"{us.hpCur} / {maxHpInt}");

            // HP 바
            float maxHp = Mathf.Max(1f, (float)maxHpInt);
            float t = Mathf.Clamp01(us.hpCur / maxHp);
            Rect r = GUILayoutUtility.GetRect(18, 18);
            EditorGUI.DrawRect(r, new Color(0.15f, 0.15f, 0.15f));
            r = new Rect(r.x + 1, r.y + 1, (r.width - 2) * t, r.height - 2);
            EditorGUI.DrawRect(r, Color.Lerp(new Color(0.8f, 0.2f, 0.2f), new Color(0.2f, 0.8f, 0.2f), t));
        }
        else
        {
            EditorGUILayout.LabelField("HP (Cur / Max)", $"{so.u_hp} (Play 모드에서 현재값 표시)");
            EditorGUILayout.HelpBox("현재값은 Play 모드에서 업데이트됩니다.", MessageType.Info);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void DrawBaseStats(UnitStatus us)
    {
        foldBase = EditorGUILayout.Foldout(foldBase, "Base (Default) Stats");
        if (!foldBase) return;

        EditorGUI.indentLevel++;

        var so = us.unitDataSource;
        if (so == null)
        {
            EditorGUILayout.HelpBox("UnitParamsSO가 비어 있습니다.", MessageType.Warning);
            EditorGUI.indentLevel--;
            return;
        }

        var def = us.unitParamsDefault;
        bool hasRuntimeDefault = Application.isPlaying && def != null;

        // 정수
        DrawStatLine("HP", so.u_hp, hasRuntimeDefault ? def.u_hp : (int?)null);
        DrawStatLine("ATK", so.u_atk, hasRuntimeDefault ? def.u_atk : (int?)null);
        DrawStatLine("DEF", so.u_def, hasRuntimeDefault ? def.u_def : (int?)null);
        DrawStatLine("ArmorLevel", (int)so.u_armorLevel, hasRuntimeDefault ? def.u_armorLevel : (int?)null); // ★ 명시 캐스팅으로 오버로드 고정
        DrawStatLine("MultiJump", so.u_multijumpCount, hasRuntimeDefault ? def.u_multijumpCount : (int?)null);

        // 실수
        DrawStatLine("ImmunePer", so.u_immunePer, hasRuntimeDefault ? (float?)def.u_immunePer : null);
        DrawStatLine("MoveSpeed", so.u_moveSpeed, hasRuntimeDefault ? (float?)def.u_moveSpeed : null);
        DrawStatLine("JumpPower", so.u_jumpPower, hasRuntimeDefault ? (float?)def.u_jumpPower : null);
        DrawStatLine("ShotAccuracy", so.u_shotAccuracy, hasRuntimeDefault ? (float?)def.u_shotAccuracy : null);
        DrawStatLine("CriRate", so.u_criRate, hasRuntimeDefault ? (float?)def.u_criRate : null);
        DrawStatLine("CriDamage", so.u_criDamage, hasRuntimeDefault ? (float?)def.u_criDamage : null);
        DrawStatLine("DamageMul", so.u_damage, hasRuntimeDefault ? (float?)def.u_damage : null);

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void DrawCurrentStats(UnitStatus us)
    {
        foldCurrent = EditorGUILayout.Foldout(foldCurrent, "Current (Runtime) Stats");
        if (!foldCurrent) return;

        EditorGUI.indentLevel++;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play 모드에서 실시간 값이 표시됩니다.", MessageType.Info);
        }

        if (us.unitParams != null)
        {
            EditorGUILayout.LabelField("? From UnitParams (effective) ?", EditorStyles.miniBoldLabel);
            DrawStatLine("u_hp", us.unitParams.u_hp);
            DrawStatLine("u_atk", us.unitParams.u_atk);
            DrawStatLine("u_def", us.unitParams.u_def);
            DrawStatLine("u_immunePer", us.unitParams.u_immunePer);
            DrawStatLine("u_armorLevel", us.unitParams.u_armorLevel);
            DrawStatLine("u_moveSpeed", us.unitParams.u_moveSpeed);
            DrawStatLine("u_jumpPower", us.unitParams.u_jumpPower);
            DrawStatLine("u_multijumpCount", us.unitParams.u_multijumpCount);
            DrawStatLine("u_shotAccuracy", us.unitParams.u_shotAccuracy);
            DrawStatLine("u_criRate", us.unitParams.u_criRate);
            DrawStatLine("u_criDamage", us.unitParams.u_criDamage);
            DrawStatLine("u_damage", us.unitParams.u_damage);
        }
        else
        {
            EditorGUILayout.HelpBox("unitParams 가 null 입니다. Awake() 시 생성되는지 확인하세요.", MessageType.Warning);
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("? Live Fields ?", EditorStyles.miniBoldLabel);
        DrawStatLine("hpCur", us.hpCur);
        DrawStatLine("atkCur", us.atkCur);
        DrawStatLine("moveSpeed(field)", us.moveSpeed);
        DrawStatLine("jumpPower(field)", us.jumpPower);
        DrawStatLine("criRate(field)", us.criRate);
        DrawStatLine("criDamage(field)", us.criDamage);
        DrawStatLine("immunePer(field, extra)", us.immunePer);

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void DrawArmorTable(UnitStatus us)
    {
        foldArmor = EditorGUILayout.Foldout(foldArmor, "Armor Revisions (by AtkType)");
        if (!foldArmor) return;

        EditorGUI.indentLevel++;

        if (us.unitParams == null)
        {
            EditorGUILayout.HelpBox("unitParams 가 null 입니다.", MessageType.Warning);
            EditorGUI.indentLevel--;
            return;
        }

        EditorGUILayout.HelpBox(
            "finalDamage = Clamp( RoundToInt((damage - DEF) * u_immunePer * immunePer * armorRevisionByType[AtkType]), 0, baseDamage )\n\n" +
            "아래는 SetRevision()의 프리셋 참고용입니다.",
            MessageType.None);

        EditorGUILayout.LabelField("ArmorLevel", us.unitParams.u_armorLevel.ToString());

        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("Revision Presets by ArmorLevel", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Level 0 : [1.00, 0.75, 0.50]");
            EditorGUILayout.LabelField("Level 1 : [0.70, 1.00, 1.00]");
            EditorGUILayout.LabelField("Level 2 : [0.30, 0.60, 0.90]");
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void DrawActions(UnitStatus us)
    {
        foldActions = EditorGUILayout.Foldout(foldActions, "Debug Actions");
        if (!foldActions) return;

        EditorGUI.indentLevel++;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play 모드에서만 동작합니다.", MessageType.Info);
            EditorGUI.indentLevel--;
            return;
        }

        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            // Heal
            using (new EditorGUILayout.HorizontalScope())
            {
                healAmount = EditorGUILayout.IntField(new GUIContent("Heal Amount"), Mathf.Max(1, healAmount));
                if (GUILayout.Button("Heal", GUILayout.Width(80)))
                {
                    us.UnitGetHeal(healAmount);
                }
            }

            // Damage (AtkType 별 버튼)
            using (new EditorGUILayout.HorizontalScope())
            {
                damageAmount = EditorGUILayout.IntField(new GUIContent("Damage Amount"), Mathf.Max(1, damageAmount));

                if (GUILayout.Button("DMG (Type 0)", GUILayout.Width(110)))
                {
                    us.UnitGetDamage(damageAmount, default, (WeaponParamsSO.AtkTypes)0, false);
                }
                if (GUILayout.Button("DMG (Type 1)", GUILayout.Width(110)))
                {
                    us.UnitGetDamage(damageAmount, default, (WeaponParamsSO.AtkTypes)1, false);
                }
                if (GUILayout.Button("DMG (Type 2)", GUILayout.Width(110)))
                {
                    us.UnitGetDamage(damageAmount, default, (WeaponParamsSO.AtkTypes)2, false);
                }
            }

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Reset HP"))
                {
                    us.HP_Reset();
                }

                if (GUILayout.Button("Apply SetRevision()"))
                {
                    var mi = typeof(UnitStatus).GetMethod("SetRevision",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    mi?.Invoke(us, null);
                }

                if (GUILayout.Button("Recalc ATK (SetCurrentAtk)"))
                {
                    var mi = typeof(UnitStatus).GetMethod("SetCurrentAtk",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    mi?.Invoke(us, null);
                }

                if (GUILayout.Button("Notify Move (SetMoveSpeed)"))
                {
                    var mi = typeof(UnitStatus).GetMethod("SetMoveSpeed",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    mi?.Invoke(us, null);
                }
            }
        }

        EditorGUI.indentLevel--;
    }

    // ---------- Helpers ----------

    private void DrawStatLine(string label, int value, int? runtimeDefault = null)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(140));
            EditorGUILayout.LabelField(value.ToString());
            if (runtimeDefault.HasValue)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"(default: {runtimeDefault.Value})", EditorStyles.miniLabel, GUILayout.Width(120));
            }
        }
    }

    private void DrawStatLine(string label, float value, float? runtimeDefault = null)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(140));
            EditorGUILayout.LabelField(value.ToString("0.##"));
            if (runtimeDefault.HasValue)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"(default: {runtimeDefault.Value:0.##})", EditorStyles.miniLabel, GUILayout.Width(140));
            }
        }
    }

    private TOut GetSafe<TOut>(UnitStatus us, System.Func<UnitStatus, TOut> getter, TOut fallback)
    {
        try
        {
            var v = getter(us);
            if (v == null) return fallback;
            return v;
        }
        catch
        {
            return fallback;
        }
    }
}
#endif



*/