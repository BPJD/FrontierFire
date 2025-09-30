// Assets/Editor/WeaponSOAutoLinker.cs
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// PlayerWeaponData의 리스트에서 "P Weapon Id" 값과
/// 프로젝트 내 WeaponParamsSO 파일명(예: Weapon_60000) 속 숫자 ID를 매칭해
/// Weapon Stat SO 필드를 자동 연결합니다.
/// </summary>
public static class WeaponSOAutoLinker
{
    // 파일명에서 숫자 ID만 뽑아내기: Weapon_60000, Wpn-60000 등 모두 허용
    static readonly Regex IdRegex = new Regex(@"(\d+)", RegexOptions.Compiled);

    [MenuItem("Tools/Weapons/Auto-Link Weapon Stat SOs (by filename ID)")]
    public static void LinkFromMenu()
    {
        // 현재 선택에서 PlayerWeaponData 찾기 (씬/프리팹/프로젝트 자산 어디든)
        var targets = Selection.GetFiltered<Object>(SelectionMode.Editable | SelectionMode.Deep);
        int linked = 0, total = 0, dataObjs = 0;

        // WeaponParamsSO 모두 수집: 파일명 → ID 파싱 → 사전화
        var map = BuildIdToSOMap(out int soCount);

        foreach (var obj in targets)
        {
            if (TryLinkOnObject(obj, map, out int t, out int l))
            {
                dataObjs++;
                total += t;
                linked += l;
            }
        }

        if (dataObjs == 0)
        {
            // 선택이 없거나 못 찾았으면 씬 전체 탐색도 시도
            foreach (var pwd in Object.FindObjectsOfType<PlayerWeaponData>(true))
            {
                if (TryLinkOnObject(pwd, map, out int t, out int l))
                {
                    dataObjs++;
                    total += t;
                    linked += l;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Weapon SO Auto-Link",
            $"WeaponParamsSO: {soCount}개 스캔 완료\n" +
            $"PlayerWeaponData: {dataObjs}개 처리\n" +
            $"항목 {total}개 중 {linked}개 연결(업데이트) 완료",
            "OK"
        );
    }

    // --- 인스펙터 버튼용 CustomEditor ---
#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerWeaponData))]
    public class PlayerWeaponDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Auto-Link Weapon Stat SOs (by filename ID)", GUILayout.Height(28)))
            {
                var map = BuildIdToSOMap(out _);
                if (TryLinkOnObject(target, map, out int total, out int linked))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Auto-Link 완료",
                        $"항목 {total}개 중 {linked}개 연결(업데이트) 완료", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Auto-Link", "처리할 항목을 찾지 못했습니다.", "OK");
                }
            }
        }
    }
#endif

    // --- 내부 유틸리티 ---

    // 프로젝트 내 WeaponParamsSO 전부를 스캔해 "ID → SO" 맵 구성
    static Dictionary<int, ScriptableObject> BuildIdToSOMap(out int count)
    {
        var dict = new Dictionary<int, ScriptableObject>();
        string[] guids = AssetDatabase.FindAssets("t:WeaponParamsSO");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            var m = IdRegex.Match(name);
            if (!m.Success) continue;

            if (!int.TryParse(m.Groups[1].Value, out int id)) continue;

            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so == null) continue;

            // 중복 ID가 있다면 가장 먼저 찾은 것만 유지하고 경고만 남김
            if (!dict.ContainsKey(id))
                dict.Add(id, so);
            else
                Debug.LogWarning($"[WeaponSOAutoLinker] 중복 ID 감지: {id} / {name} ({path})");
        }

        count = dict.Count;
        return dict;
    }

    // target 오브젝트(컴포넌트/자산)에 대해 SerializedObject로 필드 접근해 링크 시도
    static bool TryLinkOnObject(Object target, Dictionary<int, ScriptableObject> map, out int total, out int linked)
    {
        total = 0; linked = 0;

        // PlayerWeaponData 컴포넌트/자산만 처리
        if (target == null || !(target is PlayerWeaponData) && !(target is GameObject go && go.GetComponent<PlayerWeaponData>()))
            return false;

        var pwd = target as PlayerWeaponData ?? (target as GameObject).GetComponent<PlayerWeaponData>();
        var so = new SerializedObject(pwd);

        // 리스트 프로퍼티 이름 추정: 스크린샷 기준 "P Weapon Prefab Entries"
        // -> 필드명 후보를 순회하며 첫 매칭 사용
        SerializedProperty listProp = FindAny(so, new[]
        {
            "pWeaponPrefabEntries", "weaponPrefabEntries", "PWeaponPrefabEntries", "entries"
        });

        if (listProp == null || !listProp.isArray) return false;

        so.Update();
        total = listProp.arraySize;

        for (int i = 0; i < listProp.arraySize; i++)
        {
            var elem = listProp.GetArrayElementAtIndex(i);

            var idProp = FindAny(elem, new[] { "pWeaponId", "weaponId", "id", "PWeaponId" });
            var soProp = FindAny(elem, new[] { "weaponStatSO", "WeaponStatSO", "pWeaponStatSO" });

            if (idProp == null || soProp == null) continue;

            int id = idProp.intValue;
            if (map.TryGetValue(id, out var weaponSO))
            {
                if (soProp.objectReferenceValue != weaponSO)
                {
                    soProp.objectReferenceValue = weaponSO;
                    linked++;
                }
            }
            else
            {
                // 파일명을 ID 규칙으로 만들었는데도 못 찾은 경우 안내
                // (예: 파일명이 다르거나 SO가 아직 없음)
                // 필요시 여기서 로그를 남겨 추적
                // Debug.LogWarning($"[WeaponSOAutoLinker] ID {id}에 해당하는 SO를 찾지 못함");
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(pwd);
        return true;
    }

    // 여러 후보 이름 중 존재하는 첫 프로퍼티 반환
    static SerializedProperty FindAny(SerializedObject so, string[] names)
    {
        foreach (var n in names)
        {
            var p = so.FindProperty(n);
            if (p != null) return p;
        }
        return null;
    }

    static SerializedProperty FindAny(SerializedProperty parent, string[] names)
    {
        foreach (var n in names)
        {
            var p = parent.FindPropertyRelative(n);
            if (p != null) return p;
        }
        return null;
    }
}
