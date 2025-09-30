using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class WeaponParamsImporter : EditorWindow
{
    private TextAsset csvFile;

    [MenuItem("Tools/Import Weapon CSV")]
    public static void ShowWindow()
    {
        GetWindow<WeaponParamsImporter>("Weapon CSV Importer");
    }

    void OnGUI()
    {
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Import & Generate SOs"))
        {
            if (csvFile != null)
            {
                CreateWeaponParams(csvFile.text);
            }
            else
            {
                Debug.LogWarning("CSV 파일을 선택하세요.");
            }
        }
    }

    void CreateWeaponParams(string csv)
    {
        string[] lines = csv.Split('\n');
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV가 비어 있거나 유효하지 않음");
            return;
        }

        string folderPath = "Assets/02_Datas/WeaponSO";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/02_Datas", "WeaponSO");
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = ParseCsvLine(line);

            if (parts.Length < 16)
            {
                Debug.LogWarning($"[{i}] 줄 생략됨 - 필드 부족 ({parts.Length})개");
                continue;
            }

            WeaponParamsSO so = ScriptableObject.CreateInstance<WeaponParamsSO>();

            try
            {
                so.w_name = parts[2];
                so.w_desc = parts[4];

                so.w_type = (WeaponParamsSO.WeaponTypes)int.Parse(parts[3].Trim());

                // 카메라 거리 증가 조건 적용
                if (so.w_type == WeaponParamsSO.WeaponTypes.LightSR || so.w_type == WeaponParamsSO.WeaponTypes.HeavySR)
                {
                    so.isCamRangeUp = true;
                }
                else
                {
                    so.isCamRangeUp = false;
                }

                so.w_atkType = (WeaponParamsSO.AtkTypes)int.Parse(parts[5]);
                so.w_usingAmmo = (WeaponParamsSO.Ammos)int.Parse(parts[6]);

                float.TryParse(parts[7], out so.w_ammoMulti);
                int.TryParse(parts[8], out so.w_atk);
                int.TryParse(parts[9], out so.w_rpm);
                int.TryParse(parts[10], out so.w_magSize);
                float.TryParse(parts[11], out so.w_reloadTime);
                float.TryParse(parts[12], out so.w_accuracy);
                float.TryParse(parts[13], out so.w_range);
                int.TryParse(parts[14], out so.e_quality);
                int.TryParse(parts[15], out so.bulletID);


            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{i}] 줄 파싱 오류: {e.Message} \n내용: {line}");
                continue;
            }

            string assetName = $"Weapon_{parts[0].Trim()}";
            AssetDatabase.CreateAsset(so, $"{folderPath}/{assetName}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("무기 파라미터 SO 생성 완료");
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
