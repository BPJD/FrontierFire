#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(Data_Stages))]
public class DataStagesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("자동으로 Stage 프리팹 채우기"))
        {
            AutoFillStagePrefabs((Data_Stages)target);
        }
    }

    private void AutoFillStagePrefabs(Data_Stages data)
    {
        string folderPath = "Assets/04_Prefabs/Stages";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        var newList = new List<StagePrefabEntry>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            string name = prefab.name;

            // 이름에서 숫자 추출 (뒤쪽)
            string[] parts = name.Split('_');
            if (parts.Length < 2)
            {
                Debug.LogWarning($"스테이지 ID 추출 실패: {name}");
                continue;
            }

            if (!int.TryParse(parts[1], out int stageId))
            {
                Debug.LogWarning($"스테이지 ID 파싱 실패: {name}");
                continue;
            }

            newList.Add(new StagePrefabEntry
            {
                stageId = stageId,
                stagePrefab = prefab
            });
        }

        Undo.RecordObject(data, "자동 Stage 프리팹 채우기");
        SerializedObject so = new SerializedObject(data);
        SerializedProperty listProp = so.FindProperty("stagePrefabEntries");

        data.GetType()
            .GetField("stagePrefabEntries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(data, newList);

        EditorUtility.SetDirty(data);
        Debug.Log($"Stage 프리팹 자동 채우기 완료 ({newList.Count}개)");
    }
}
#endif
