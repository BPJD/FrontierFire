using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class StatUpgradeImporter : EditorWindow
{
    private TextAsset csvFile;

    [MenuItem("Tools/Import Stat CSV")]
    public static void ShowWindow()
    {
        GetWindow<StatUpgradeImporter>("Stat CSV Importer");
    }

    void OnGUI()
    {
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Import & Generate SOs"))
        {
            if (csvFile != null)
            {
                CreateStatParams(csvFile.text);
            }
            else
            {
                Debug.LogWarning("CSV 파일을 선택하세요.");
            }
        }
    }

    void CreateStatParams(string csv)
    {
        string[] lines = csv.Split('\n');
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV가 비어 있거나 유효하지 않음");
            return;
        }

        string folderPath = "Assets/02_Datas/StatUpSO";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/02_Datas", "StatUpSO");
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = ParseCsvLine(line);

            if (parts.Length < 7)
            {
                Debug.LogWarning($"[{i}] 줄 생략됨 - 필드 부족 ({parts.Length})개");
                continue;
            }

            StatUpgradesSO so = ScriptableObject.CreateInstance<StatUpgradesSO>();

            try
            {
                so.id = int.Parse(parts[0]);
                so.up_name = parts[1];
                so.up_desc = parts[2];
                so.up_type = int.Parse(parts[3]);
                so.up_stat = int.Parse(parts[4]);
                so.up_value = float.Parse(parts[5]);
                so.up_uiDesc = parts[6];
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{i}] 줄 파싱 오류: {e.Message} \n내용: {line}");
                continue;
            }

            string assetName = $"StatUp_{so.id}";
            AssetDatabase.CreateAsset(so, $"{folderPath}/{assetName}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("스탯 파라미터 SO 생성 완료");
    }

    // 쉼표 파싱 대응 (큰따옴표 내부 쉼표 무시)
    private static string[] ParseCsvLine(string line)
    {
        MatchCollection matches = Regex.Matches(line, "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
        string[] result = new string[matches.Count];
        for (int i = 0; i < matches.Count; i++)
        {
            result[i] = matches[i].Value.Trim().Trim('"');
        }
        return result;
    }
}
