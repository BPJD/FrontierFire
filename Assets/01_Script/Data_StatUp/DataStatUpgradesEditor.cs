#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

[CustomEditor(typeof(Data_StatUpgrades))]
public class DataStatUpgradesEditor : Editor
{
    // 폴더/접두사 규칙 (StatUp_{id}_{seq})
    private const string DefaultFolder = "Assets/02_Datas/StatUpSO";
    private const string NamePrefix = "StatUp_";
    private const bool SyncSoIdWithFilename = true; // 파일명 ID와 SO.id가 다르면 동기화

    // 파일명 파싱: StatUp_61000_003 → id=61000, seq=3
    // 또한 StatUp_61000 (구형)도 지원
    private static readonly Regex kNameRegex =
        new Regex(@"^StatUp_(\d+)(?:_(\d{1,}))?$", RegexOptions.Compiled);

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
                //Debug.LogWarning($"[AutoFill] 't:StatUpgradesSO'로는 0개. 파일명 '{NamePrefix}'로 {byName.Length}개 후보 발견. " +
                //                 $"하지만 타입이 'StatUpgradesSO'가 아니면 로드에 실패합니다(스크립트 누락 가능).");
                guids = byName;
            }
            else
            {
                //Debug.LogWarning($"[AutoFill] 검색 결과가 없습니다. 폴더를 확인하세요: {string.Join(", ", searchFolders)}");
            }
        }

        var newList = new List<StatUpSOEntry>();
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
                //Debug.LogWarning($"[AutoFill] 로드 실패(StatUpgradesSO 아님/스크립트 누락?): {fileName} | {path}");
                continue;
            }

            // 파일명에서 (id, seq) 추출
            string fname = Path.GetFileNameWithoutExtension(path);
            int idFromName = -1;
            int seqFromName = -1;
            TryExtractIdAndSeqFromName(fname, out idFromName, out seqFromName);

            int chosenId = (so.id > 0) ? so.id : idFromName;
            if (chosenId <= 0)
            {
                //Debug.LogWarning($"[AutoFill] ID 판단 실패: {fname} | SO.id={so.id} | {path}");
                continue;
            }

            // 파일명-ID와 SO.id가 다르면 동기화/경고
            if (idFromName > 0 && so.id > 0 && so.id != idFromName)
            {
                //Debug.LogWarning($"[AutoFill] SO.id({so.id}) ≠ 파일명 ID({idFromName}) : {fname}");
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

            // 다행(같은 id 다수) 지원: 중복 검사는 제거하고 모두 추가
            newList.Add(new StatUpSOEntry
            {
                statUpID = chosenId,
                statUp = so
            });
        }

        // 정렬: id → seq → 파일명
        newList = newList
            .OrderBy(e => e.statUpID)
            .ThenBy(e => GetSeqFromAsset(e.statUp))                // 파일명 seq 기준(없으면 0)
            .ThenBy(e => e.statUp ? e.statUp.name : string.Empty)  // 안전한 tie-breaker
            .ToList();

        // 반영
        Undo.RecordObject(data, "자동 Stat Upgrade SO 채우기");
        FieldInfo fi = typeof(Data_StatUpgrades)
            .GetField("statUpEntries", BindingFlags.NonPublic | BindingFlags.Instance);

        if (fi != null)
        {
            fi.SetValue(data, newList);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            //Debug.Log($"[AutoFill] 완료: {newList.Count}개 / 로드 실패 {failLoad}개 | 검색 폴더: {string.Join(", ", searchFolders)}");
        }
        else
        {
            //Debug.LogError("statUpEntries 필드를 찾을 수 없습니다. Data_StatUpgrades 구조를 확인하세요.");
        }
    }

    private static bool TryExtractIdAndSeqFromName(string name, out int id, out int seq)
    {
        id = -1;
        seq = 0; // 없는 경우 0으로 간주(정렬 시 먼저 나오도록)
        var m = kNameRegex.Match(name);
        if (!m.Success) return false;

        // 그룹1: id, 그룹2: seq(옵션)
        int.TryParse(m.Groups[1].Value, out id);
        if (m.Groups[2].Success)
        {
            int.TryParse(m.Groups[2].Value, out seq);
        }
        return id > 0;
    }

    private static int GetSeqFromAsset(StatUpgradesSO so)
    {
        if (!so) return 0;
        string name = so.name; // 에셋 이름(확장자 제외)
        int id, seq;
        if (TryExtractIdAndSeqFromName(name, out id, out seq))
            return Mathf.Max(seq, 0);
        return 0;
    }
}
#endif
