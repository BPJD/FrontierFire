#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(Data_Enemies))]
public class DataEnemiesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("자동으로 Enemy 프리팹 채우기"))
        {
            AutoFillEnemyPrefabs((Data_Enemies)target);
        }
    }

    private void AutoFillEnemyPrefabs(Data_Enemies data)
    {
        string folderPath = "Assets/04_Prefabs/EnemyUnits";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        var newList = new List<EnemyPrefabEntry>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            string name = prefab.name;
            string[] split = name.Split('_');

            if (split.Length == 0 || !int.TryParse(split[0], out int id))
            {
                //Debug.LogWarning($"ID 추출 실패: {name}");
                continue;
            }

            newList.Add(new EnemyPrefabEntry
            {
                enemyId = id,
                enemyPrefab = prefab
            });
        }

        Undo.RecordObject(data, "자동 Enemy 프리팹 채우기");
        SerializedObject so = new SerializedObject(data);
        SerializedProperty listProp = so.FindProperty("enemyPrefabEntries");

        data.GetType()
            .GetField("enemyPrefabEntries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(data, newList);

        EditorUtility.SetDirty(data);
        //Debug.Log($"Enemy 프리팹 자동 채우기 완료 ({newList.Count}개)");
    }
}
#endif
