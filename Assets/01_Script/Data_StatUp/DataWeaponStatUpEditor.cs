#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;

[CustomEditor(typeof(Data_WeaponStatUpgrades))]
public class DataWeaponStatUpEditor : Editor
{
    private const string DefaultFolder = "Assets/02_Datas/WeaponUpgradesSO";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("자동으로 무기 강화 SO 채우기 (파일명 인식)"))
        {
            AutoFillWeaponUpgrades((Data_WeaponStatUpgrades)target);
        }
    }

    private void AutoFillWeaponUpgrades(Data_WeaponStatUpgrades data)
    {
        string[] searchFolders = AssetDatabase.IsValidFolder(DefaultFolder) ? new[] { DefaultFolder } : new[] { "Assets" };
        string[] guids = AssetDatabase.FindAssets("t:WeaponStatUpgradesSO", searchFolders);

        var newList = new List<WeaponStatUpgradeEntry>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<WeaponStatUpgradesSO>(path);
            if (so == null) continue;

            // 파일명: WeaponUp_<ID>_### 형태 권장, 없을 경우 SO.id 사용
            string fileName = Path.GetFileNameWithoutExtension(path);
            int idFromName = TryExtractIdFromName(fileName, out var parsed) ? parsed : -1;

            int chosenId = so.id > 0 ? so.id : idFromName;
            if (chosenId <= 0)
            {
                //Debug.LogWarning($"[AutoFill] ID를 판단할 수 없습니다: {fileName} | SO.id={so.id} | {path}");
                continue;
            }

            // 중복 제거 없음: 같은 ID라도 모두 누적 (패키지 효과 지원)
            newList.Add(new WeaponStatUpgradeEntry
            {
                statUpID = chosenId,
                statUp = so
            });
        }

        // 보기 좋게 정렬(파일명/ID 기준)
        newList = newList.OrderBy(e => e.statUpID).ToList();

        Undo.RecordObject(data, "자동 무기 강화 SO 채우기");
        FieldInfo fi = typeof(Data_WeaponStatUpgrades).GetField("upgradeEntries", BindingFlags.NonPublic | BindingFlags.Instance);
        if (fi != null)
        {
            fi.SetValue(data, newList);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            //Debug.Log($"[AutoFill] 무기 강화 SO 자동 채우기 완료 (총 {newList.Count}개) | 검색: {string.Join(", ", searchFolders)}");
        }
        else
        {
            //Debug.LogError("upgradeEntries 필드를 찾을 수 없습니다.");
        }
    }

    private static bool TryExtractIdFromName(string name, out int id)
    {
        id = -1;
        // 파일명이 WeaponUp_<ID>_### 또는 *_<ID> 형태일 때 마지막 '_' 뒤 숫자를 파싱
        int idx = name.LastIndexOf('_');
        if (idx < 0 || idx == name.Length - 1) return false;
        string tail = name.Substring(idx + 1);
        return int.TryParse(tail, out id);
    }
}
#endif
