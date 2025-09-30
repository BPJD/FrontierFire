#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

[CustomEditor(typeof(Data_StatUpgrades))]
public class DataStatUpgradesEditor : Editor
{
    //폴더/접두사 StatUp 규칙으로 수정
    private const string DefaultFolder = "Assets/02_Datas/StatUpSO";
    private const string NamePrefix = "StatUp_";
    private const bool SyncSoIdWithFilename = true; // 파일명 ID와 SO.id가 다르면 동기화

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("자동으로 Stat Upgrade SO 채우기 (파일명 인식)"))
        {
            AutoFillStatUpgrades((Data_StatUpgrades)target);
        }
    }

    private void AutoFillStatUpgrades(Data_StatUpgrades data)
    {
        string[] searchFolders = AssetDatabase.IsValidFolder(DefaultFolder)
            ? new[] { DefaultFolder }
            : new[] { "Assets" };

        // 1) 타입 기반 검색
        string[] guids = AssetDatabase.FindAssets("t:StatUpgradesSO", searchFolders);

        // 2) 보조: 타입 검색이 0이면 파일명 접두사로 후보 검색 (스크립트 누락/타입 불일치 탐지)
        if (guids.Length == 0)
        {
            string[] byName = AssetDatabase.FindAssets($"{NamePrefix} t:ScriptableObject", searchFolders);
            if (byName.Length > 0)
            {
                Debug.LogWarning($"[AutoFill] 't:StatUpgradesSO'로는 0개. 파일명 '{NamePrefix}'로 {byName.Length}개 후보 발견. " +
                                 $"하지만 타입이 'StatUpgradesSO'가 아니면 로드에 실패합니다(스크립트 누락 가능).");
                guids = byName;
            }
            else
            {
                Debug.LogWarning($"[AutoFill] 검색 결과가 없습니다. 폴더를 확인하세요: {string.Join(", ", searchFolders)}");
            }
        }

        var newList = new List<StatUpSOEntry>();
        var seenIds = new HashSet<int>();
        int failLoad = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // 반드시 StatUpgradesSO 타입으로 로드 시도
            var so = AssetDatabase.LoadAssetAtPath<StatUpgradesSO>(path);
            if (so == null)
            {
                failLoad++;
                // 파일명은 보이게 로그
                string fileName = Path.GetFileNameWithoutExtension(path);
                Debug.LogWarning($"[AutoFill] 로드 실패(StatUpgradesSO 아님/스크립트 누락?): {fileName} | {path}");
                continue;
            }

            // 파일명에서 ID 추출 (StatUp_######)
            string fname = Path.GetFileNameWithoutExtension(path);
            int idFromName = TryExtractIdFromName(fname, out var parsed) ? parsed : -1;

            int chosenId = so.id > 0 ? so.id : idFromName;
            if (chosenId <= 0)
            {
                Debug.LogWarning($"[AutoFill] ID 판단 실패: {fname} | SO.id={so.id} | {path}");
                continue;
            }

            // 파일명-ID와 SO.id가 다르면 동기화/경고
            if (idFromName > 0 && so.id > 0 && so.id != idFromName)
            {
                Debug.LogWarning($"[AutoFill] SO.id({so.id}) ≠ 파일명 ID({idFromName}) : {fname}");
                if (SyncSoIdWithFilename)
                {
                    Undo.RecordObject(so, "Sync SO id with filename");
                    so.id = idFromName;
                    EditorUtility.SetDirty(so);
                    chosenId = idFromName;
                }
            }
            else if (so.id <= 0 && idFromName > 0 && SyncSoIdWithFilename)
            {
                Undo.RecordObject(so, "Set SO id from filename");
                so.id = idFromName;
                EditorUtility.SetDirty(so);
                chosenId = idFromName;
            }

            if (!seenIds.Add(chosenId))
            {
                Debug.LogWarning($"[AutoFill] 중복된 ID 감지: {chosenId} ({fname})");
                continue;
            }

            newList.Add(new StatUpSOEntry
            {
                statUpID = chosenId,
                statUp = so
            });
        }

        // ID 정렬
        newList = newList.OrderBy(e => e.statUpID).ToList();

        // 반영
        Undo.RecordObject(data, "자동 Stat Upgrade SO 채우기");
        FieldInfo fi = typeof(Data_StatUpgrades)
            .GetField("statUpEntries", BindingFlags.NonPublic | BindingFlags.Instance);

        if (fi != null)
        {
            fi.SetValue(data, newList);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AutoFill] 완료: {newList.Count}개 / 로드 실패 {failLoad}개 | 검색 폴더: {string.Join(", ", searchFolders)}");
        }
        else
        {
            Debug.LogError("statUpEntries 필드를 찾을 수 없습니다. Data_StatUpgrades 구조를 확인하세요.");
        }
    }

    private static bool TryExtractIdFromName(string name, out int id)
    {
        id = -1;
        int idx = name.LastIndexOf('_');
        if (idx < 0 || idx == name.Length - 1) return false;
        string tail = name.Substring(idx + 1);
        return int.TryParse(tail, out id);
    }
}
#endif
