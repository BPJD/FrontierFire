using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;

public class StatUpgradeImporter : EditorWindow
{
    private TextAsset csvFile;

    [MenuItem("Tools/Import Stat CSV (Multi-Effect)")]
    public static void ShowWindow()
    {
        GetWindow<StatUpgradeImporter>("Stat CSV Importer");
    }

    void OnGUI()
    {
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Import & Generate StatUp SOs"))
        {
            if (csvFile != null)
            {
                CreateStatAssets(csvFile.text);
            }
            else
            {
                Debug.LogWarning("CSV 파일을 선택하세요.");
            }
        }
    }

    void CreateStatAssets(string csv)
    {
        if (string.IsNullOrEmpty(csv))
        {
            Debug.LogError("CSV가 비어 있음");
            return;
        }

        // 라인 분해
        string[] lines = csv.Split('\n');
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV가 비어 있거나 유효하지 않음");
            return;
        }

        // 폴더 보장
        EnsureFolder("Assets/02_Datas");
        string folderPath = "Assets/02_Datas/StatUpSO";
        EnsureFolder(folderPath);

        // 같은 ID의 다중 효과(행) 지원을 위한 카운터
        var idCounters = new Dictionary<int, int>();

        int created = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            // 0번째는 헤더
            for (int i = 1; i < lines.Length; i++)
            {
                string raw = lines[i];
                if (string.IsNullOrWhiteSpace(raw)) continue;

                string line = raw.Trim();
                string[] parts = ParseCsvLine(line);

                // 기대 컬럼(9개): id, up_name, up_desc, up_type, up_stat, up_value, up_uiDesc, up_class, up_category
                if (parts.Length < 9)
                {
                    Debug.LogWarning($"[{i}] 줄 생략됨 - 필드 부족 ({parts.Length}/9): {line}");
                    continue;
                }

                try
                {
                    // 안전 파싱
                    int id = SafeParseInt(parts[0]);
                    string up_name = UnescapeCsv(parts[1]);
                    string up_desc = UnescapeCsv(parts[2]);

                    int up_type = Mathf.Clamp(SafeParseInt(parts[3]), 0, 1);             // 0/1
                    int up_stat = Mathf.Clamp(SafeParseInt(parts[4]), 0, 30);            // 0~30
                    float up_value = SafeParseFloat(parts[5]);                            // InvariantCulture

                    string up_uiDesc = UnescapeCsv(parts[6]);

                    int up_tier = Mathf.Clamp(SafeParseInt(parts[7]), 0, 5);            // D=0 ~ SS=5
                    int up_category = Mathf.Clamp(SafeParseInt(parts[8]), 0, 5);         // 0~5

                    // ID별 순번 증가
                    if (!idCounters.TryGetValue(id, out int counter)) counter = 0;
                    counter++;
                    idCounters[id] = counter;

                    // 에셋 생성
                    var so = ScriptableObject.CreateInstance<StatUpgradesSO>();
                    so.id = id;
                    so.up_name = up_name;
                    so.up_desc = up_desc;
                    so.up_type = up_type;
                    so.up_stat = up_stat;
                    so.up_value = up_value;
                    so.up_uiDesc = up_uiDesc;
                    so.up_tier = up_tier;
                    so.up_category = up_category;

                    // 파일명: StatUp_{id}_{001} (필요 시 이름 일부 포함)
                    string assetName = $"StatUp_{id}_{counter:000}";
                    string assetPath = $"{folderPath}/{SanitizeFileName(assetName)}.asset";

                    AssetDatabase.CreateAsset(so, assetPath);
                    created++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[{i}] 줄 파싱/생성 오류: {e.Message}\n내용: {line}");
                    continue;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"스탯 강화 SO 생성 완료: {created}개 (동일 ID 다중 행 지원)");
    }

    // ----- 유틸 -----

    // 쉼표 파싱(큰따옴표 내부 쉼표 무시)
    private static string[] ParseCsvLine(string line)
    {
        // CR 제거 후 처리
        line = line.Replace("\r", "");
        MatchCollection matches = Regex.Matches(line, "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
        string[] result = new string[matches.Count];
        for (int i = 0; i < matches.Count; i++)
        {
            // 트림 + 큰따옴표 제거
            string val = matches[i].Value.Trim().Trim('"');
            result[i] = val;
        }
        return result;
    }

    // CSV 안의 "" → " 언이스케이프
    private static string UnescapeCsv(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("\"\"", "\"").Trim();
    }

    private static int SafeParseInt(string s)
    {
        if (int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
            return v;
        // 한국 로케일 등 예외 상황도 방지
        if (int.TryParse(s.Trim(), out v)) return v;
        return 0;
    }

    private static float SafeParseFloat(string s)
    {
        if (float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            return v;
        // 혹시 모를 로케일 파서
        if (float.TryParse(s.Trim(), out v)) return v;
        return 0f;
    }

    private static void EnsureFolder(string fullPath)
    {
        // 예: "Assets/02_Datas/StatUpSO"
        if (AssetDatabase.IsValidFolder(fullPath)) return;

        string[] parts = fullPath.Split('/');
        string cur = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{cur}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(cur, parts[i]);
            }
            cur = next;
        }
    }

    private static string SanitizeFileName(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s;
    }
}
