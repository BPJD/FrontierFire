using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;

public class WeaponStatUpParamsImporter : EditorWindow
{
    private TextAsset csvFile;

    [MenuItem("Tools/Import Weapon Upgrade CSV")]
    public static void ShowWindow()
    {
        GetWindow<WeaponStatUpParamsImporter>("Weapon Upgrade CSV Importer");
    }

    void OnGUI()
    {
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Import & Generate Upgrade SOs"))
        {
            if (csvFile != null)
            {
                CreateUpgradeAssets(csvFile.text);
            }
            else
            {
                Debug.LogWarning("CSV 파일을 선택하세요.");
            }
        }
    }

    void CreateUpgradeAssets(string csv)
    {
        string[] lines = csv.Split('\n');
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV가 비어 있거나 유효하지 않음");
            return;
        }

        // 에셋 저장 폴더
        string folderPath = "Assets/02_Datas/WeaponUpgradesSO";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/02_Datas", "WeaponUpgradesSO");
        }

        // ID별 생성 순번 카운터 (같은 ID의 다중 효과 지원)
        var idCounters = new Dictionary<int, int>();

        // 0번째 줄은 헤더로 가정
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = ParseCsvLine(line);

            // 기대 컬럼: 7개 (id, up_name, up_desc, up_type, up_stat, up_value, up_uiDesc)
            if (parts.Length < 7)
            {
                Debug.LogWarning($"[{i}] 줄 생략됨 - 필드 부족 ({parts.Length})개");
                continue;
            }

            try
            {
                int id = 0;
                int.TryParse(parts[0].Trim(), out id);

                string up_name = parts[1];
                string up_desc = parts[2];

                int up_type = 0;  // 0:Add, 1:Multiply
                int.TryParse(parts[3].Trim(), out up_type);
                up_type = Mathf.Clamp(up_type, 0, 1);

                int up_stat = 0;  // CSV상의 stat 인덱스
                int.TryParse(parts[4].Trim(), out up_stat);
                if (up_stat < 0) up_stat = 0;

                float up_value = 0f;
                float.TryParse(parts[5].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out up_value);

                string up_uiDesc = parts[6];

                int up_tier = 0; //아이템 등급
                int.TryParse(parts[7].Trim(), out up_tier);

                int up_model = 0; //아이템 모델링
                int.TryParse(parts[8].Trim(), out up_model);

                // 같은 ID라도 모든 행을 SO로 생성 (덮어쓰기 방지: 순번 부여)
                if (!idCounters.TryGetValue(id, out int counter)) counter = 0;
                counter++;
                idCounters[id] = counter;

                string assetName = $"WeaponUp_{id}_{counter:000}";
                string assetPath = $"{folderPath}/{assetName}.asset";

                var so = ScriptableObject.CreateInstance<WeaponStatUpgradesSO>();
                so.id = id;
                so.up_name = up_name;
                so.up_desc = up_desc;
                so.up_type = up_type;
                so.up_stat = up_stat;
                so.up_value = up_value;
                so.up_uiDesc = up_uiDesc;
                so.up_tier = up_tier;
                so.up_model = up_model;

                AssetDatabase.CreateAsset(so, assetPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{i}] 줄 파싱 오류: {e.Message} \n내용: {line}");
                continue;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("무기 강화 SO 생성 완료 (패키지 효과 지원)");
    }

    // 쉼표 파싱 대응 (큰따옴표 내부 쉼표 무시)
    private static string[] ParseCsvLine(string line)
    {
        MatchCollection matches = Regex.Matches(line, "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
        string[] result = new string[matches.Count];
        for (int i = 0; i < matches.Count; i++)
        {
            // 큰따옴표 제거 및 양끝 공백 제거
            result[i] = matches[i].Value.Trim().Trim('"');
            // CSV 안에서 "" -> " 로 원복
            result[i] = result[i].Replace("\"\"", "\"");
        }
        return result;
    }

    private static string SanitizeFileName(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
        return s;
    }
}
