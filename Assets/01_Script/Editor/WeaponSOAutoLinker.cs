// Assets/Editor/WeaponSOAutoLinker.cs
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// PlayerWeaponData의 pWeaponPrefab 이름 앞 숫자 ID와
/// WeaponParamsSO 파일명 숫자 ID를 매칭해서
/// weaponStatSO 필드를 자동 연결합니다.
/// 
/// 예:
/// Prefab: 60003_SciFi_Pistol_4
/// SO:     Weapon_60003
/// => 연결
/// </summary>
public static class WeaponSOAutoLinker
{
    private static readonly Regex IdRegex = new Regex(@"(\d+)", RegexOptions.Compiled);

    [MenuItem("Tools/Weapons/Auto-Link Weapon Stat SOs (by Prefab Name ID)")]
    public static void LinkFromMenu()
    {
        Dictionary<int, WeaponParamsSO> soMap = BuildIdToSOMap(out int soCount);

        Object[] targets = Selection.GetFiltered<Object>(
            SelectionMode.Editable | SelectionMode.Deep
        );

        int dataObjCount = 0;
        int total = 0;
        int linked = 0;
        int alreadyCorrect = 0;
        int missingPrefab = 0;
        int missingSO = 0;
        int missingField = 0;

        foreach (Object obj in targets)
        {
            if (TryLinkOnObject(
                    obj,
                    soMap,
                    out int t,
                    out int l,
                    out int ac,
                    out int mp,
                    out int ms,
                    out int mf))
            {
                dataObjCount++;
                total += t;
                linked += l;
                alreadyCorrect += ac;
                missingPrefab += mp;
                missingSO += ms;
                missingField += mf;
            }
        }

        if (dataObjCount == 0)
        {
            foreach (PlayerWeaponData pwd in Object.FindObjectsOfType<PlayerWeaponData>(true))
            {
                if (TryLinkOnObject(
                        pwd,
                        soMap,
                        out int t,
                        out int l,
                        out int ac,
                        out int mp,
                        out int ms,
                        out int mf))
                {
                    dataObjCount++;
                    total += t;
                    linked += l;
                    alreadyCorrect += ac;
                    missingPrefab += mp;
                    missingSO += ms;
                    missingField += mf;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Weapon SO Auto-Link",
            $"WeaponParamsSO: {soCount}개 스캔 완료\n" +
            $"PlayerWeaponData: {dataObjCount}개 처리\n" +
            $"총 항목: {total}개\n" +
            $"연결/업데이트: {linked}개\n" +
            $"이미 정상 연결: {alreadyCorrect}개\n" +
            $"프리팹 누락: {missingPrefab}개\n" +
            $"매칭 SO 없음: {missingSO}개\n" +
            $"필드 탐색 실패: {missingField}개",
            "OK"
        );
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerWeaponData))]
    public class PlayerWeaponDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Auto-Link Weapon Stat SOs (by Prefab Name ID)", GUILayout.Height(28)))
            {
                Dictionary<int, WeaponParamsSO> soMap = BuildIdToSOMap(out int soCount);

                if (TryLinkOnObject(
                        target,
                        soMap,
                        out int total,
                        out int linked,
                        out int alreadyCorrect,
                        out int missingPrefab,
                        out int missingSO,
                        out int missingField))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    EditorUtility.DisplayDialog(
                        "Auto-Link 완료",
                        $"WeaponParamsSO: {soCount}개 스캔 완료\n" +
                        $"총 항목: {total}개\n" +
                        $"연결/업데이트: {linked}개\n" +
                        $"이미 정상 연결: {alreadyCorrect}개\n" +
                        $"프리팹 누락: {missingPrefab}개\n" +
                        $"매칭 SO 없음: {missingSO}개\n" +
                        $"필드 탐색 실패: {missingField}개",
                        "OK"
                    );
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Auto-Link",
                        "처리할 PlayerWeaponData를 찾지 못했습니다.",
                        "OK"
                    );
                }
            }
        }
    }
#endif

    private static Dictionary<int, WeaponParamsSO> BuildIdToSOMap(out int count)
    {
        Dictionary<int, WeaponParamsSO> dict = new Dictionary<int, WeaponParamsSO>();

        string[] guids = AssetDatabase.FindAssets("t:WeaponParamsSO");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (!TryGetIdFromName(fileName, out int id))
            {
                Debug.LogWarning($"[WeaponSOAutoLinker] SO 파일명에서 ID를 찾지 못했습니다: {fileName}");
                continue;
            }

            WeaponParamsSO weaponSO = AssetDatabase.LoadAssetAtPath<WeaponParamsSO>(path);

            if (weaponSO == null)
            {
                Debug.LogWarning($"[WeaponSOAutoLinker] WeaponParamsSO 로드 실패: {path}");
                continue;
            }

            if (!dict.ContainsKey(id))
            {
                dict.Add(id, weaponSO);
            }
            else
            {
                Debug.LogWarning(
                    $"[WeaponSOAutoLinker] 중복 SO ID 감지: {id} / {fileName} / {path}"
                );
            }
        }

        count = dict.Count;
        return dict;
    }

    private static bool TryLinkOnObject(
        Object target,
        Dictionary<int, WeaponParamsSO> soMap,
        out int total,
        out int linked,
        out int alreadyCorrect,
        out int missingPrefab,
        out int missingSO,
        out int missingField)
    {
        total = 0;
        linked = 0;
        alreadyCorrect = 0;
        missingPrefab = 0;
        missingSO = 0;
        missingField = 0;

        if (target == null)
            return false;

        PlayerWeaponData playerWeaponData = null;

        if (target is PlayerWeaponData data)
        {
            playerWeaponData = data;
        }
        else if (target is GameObject go)
        {
            playerWeaponData = go.GetComponent<PlayerWeaponData>();
        }

        if (playerWeaponData == null)
            return false;

        SerializedObject serializedObject = new SerializedObject(playerWeaponData);
        serializedObject.Update();

        SerializedProperty listProp = serializedObject.FindProperty("pWeaponPrefabEntries");

        if (listProp == null || !listProp.isArray)
        {
            Debug.LogWarning(
                $"[WeaponSOAutoLinker] pWeaponPrefabEntries 리스트를 찾지 못했습니다: {playerWeaponData.name}"
            );
            return false;
        }

        total = listProp.arraySize;

        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);

            SerializedProperty prefabProp = element.FindPropertyRelative("pWeaponPrefab");
            SerializedProperty weaponStatSOProp = element.FindPropertyRelative("weaponStatSO");

            if (prefabProp == null || weaponStatSOProp == null)
            {
                missingField++;

                Debug.LogWarning(
                    $"[WeaponSOAutoLinker] Element {i}에서 pWeaponPrefab 또는 weaponStatSO 필드를 찾지 못했습니다."
                );

                continue;
            }

            GameObject prefab = prefabProp.objectReferenceValue as GameObject;

            if (prefab == null)
            {
                missingPrefab++;

                Debug.LogWarning(
                    $"[WeaponSOAutoLinker] Element {i}: pWeaponPrefab이 비어 있습니다."
                );

                continue;
            }

            string prefabName = prefab.name;

            if (!TryGetIdFromName(prefabName, out int prefabId))
            {
                missingSO++;

                Debug.LogWarning(
                    $"[WeaponSOAutoLinker] 프리팹 이름에서 ID를 찾지 못했습니다: {prefabName}"
                );

                continue;
            }

            if (!soMap.TryGetValue(prefabId, out WeaponParamsSO matchedSO))
            {
                missingSO++;

                Debug.LogWarning(
                    $"[WeaponSOAutoLinker] 프리팹 ID {prefabId}와 매칭되는 WeaponParamsSO가 없습니다. Prefab: {prefabName}"
                );

                continue;
            }

            if (weaponStatSOProp.objectReferenceValue == matchedSO)
            {
                alreadyCorrect++;
                continue;
            }

            weaponStatSOProp.objectReferenceValue = matchedSO;
            linked++;

            Debug.Log(
                $"[WeaponSOAutoLinker] Linked Element {i}: {prefabName} -> {matchedSO.name}"
            );
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(playerWeaponData);

        return true;
    }

    private static bool TryGetIdFromName(string name, out int id)
    {
        id = 0;

        if (string.IsNullOrEmpty(name))
            return false;

        Match match = IdRegex.Match(name);

        if (!match.Success)
            return false;

        return int.TryParse(match.Groups[1].Value, out id);
    }
}