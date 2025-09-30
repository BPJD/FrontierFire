using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class UnitParamsImporter : EditorWindow
{
    private TextAsset csvFile;

    [MenuItem("Tools/Import UnitParams CSV")]
    public static void ShowWindow()
    {
        GetWindow<UnitParamsImporter>("UnitParams CSV Importer");
    }

    void OnGUI()
    {
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Import & Generate SOs"))
        {
            if (csvFile != null)
            {
                CreateUnitParams(csvFile.text);
            }
            else
            {
                Debug.LogWarning("CSV 파일을 선택하세요.");
            }
        }
    }

    void CreateUnitParams(string csv)
    {
        string[] lines = csv.Split('\n');
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV가 비어 있거나 유효하지 않음");
            return;
        }

        string unitFolderPath = "Assets/02_Datas/UnitSO";
        string aiFolderPath = "Assets/02_Datas/UnitAIParamsSO";

        if (!AssetDatabase.IsValidFolder(unitFolderPath))
            AssetDatabase.CreateFolder("Assets/02_Datas", "UnitSO");

        if (!AssetDatabase.IsValidFolder(aiFolderPath))
            AssetDatabase.CreateFolder("Assets/02_Datas", "UnitAIParamsSO");

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = ParseCsvLine(line);

            if (parts.Length < 21)
            {
                Debug.LogWarning($"[{i}] 줄 생략됨 - 필드 부족 ({parts.Length})개");
                continue;
            }

            // --- UnitParamsSO 생성 ---
            UnitParamsSO unitSO = ScriptableObject.CreateInstance<UnitParamsSO>();
            try
            {
                unitSO.u_name = parts[1];
                unitSO.u_type = (UnitParamsSO.UnitTypes)int.Parse(parts[2]);
                unitSO.u_hp = int.Parse(parts[3]);
                unitSO.u_atk = int.Parse(parts[4]);
                unitSO.u_def = int.Parse(parts[5]);
                unitSO.u_immunePer = float.Parse(parts[6]);
                unitSO.u_armorLevel = int.Parse(parts[7]);
                unitSO.u_moveSpeed = float.Parse(parts[8]);
                unitSO.u_jumpPower = float.Parse(parts[9]);
                unitSO.u_multijumpCount = int.Parse(parts[10]);
                unitSO.u_shotAccuracy = float.Parse(parts[11]);
                unitSO.u_criRate = float.Parse(parts[12]);
                unitSO.u_criDamage = float.Parse(parts[13]);
                unitSO.u_damage = float.Parse(parts[14]);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{i}] UnitParamsSO 파싱 오류: {e.Message} \n내용: {line}");
                continue;
            }

            string unitAssetName = $"Unit_{parts[0].Trim()}";
            AssetDatabase.CreateAsset(unitSO, $"{unitFolderPath}/{unitAssetName}.asset");

            // --- UnitAIParamsSO 생성 ---
            UnitAIParamsSO aiSO = ScriptableObject.CreateInstance<UnitAIParamsSO>();
            try
            {
                aiSO.ai_atkCount = int.Parse(parts[15]);
                aiSO.ai_atkSpeed = float.Parse(parts[16]);
                aiSO.ai_atkDelay = float.Parse(parts[17]);
                aiSO.ai_atkRange = float.Parse(parts[18]);
                aiSO.ai_sightRange = float.Parse(parts[19]);
                aiSO.ai_dropRate = float.Parse(parts[20]);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{i}] UnitAIParamsSO 파싱 오류: {e.Message} \n내용: {line}");
                continue;
            }

            string aiAssetName = $"UnitAI_{parts[0].Trim()}";
            AssetDatabase.CreateAsset(aiSO, $"{aiFolderPath}/{aiAssetName}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("UnitParamsSO + UnitAIParamsSO 생성 완료");
    }


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
