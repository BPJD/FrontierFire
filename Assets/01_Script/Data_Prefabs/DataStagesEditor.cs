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
                //Debug.LogWarning($"스테이지 ID 추출 실패: {name}");
                continue;
            }

            if (!int.TryParse(parts[1], out int stageId))
            {
                //Debug.LogWarning($"스테이지 ID 파싱 실패: {name}");
                continue;
            }

            // ===== 필터 조건 =====
            // 1) 5로 시작하는 스테이지 ID 제외 (예: 50000, 50001, 50002...)
            //    -> 5자리 기준으로 50000 ~ 59999 범위라고 가정
            if (stageId >= 50000 && stageId < 60000)
            {
                //Debug.Log($"제외 (5로 시작): {stageId} / {name}");
                continue;
            }

            // 2) 000으로 끝나는 스테이지 ID 제외 (예: 40000, 41000, 42000...)
            if (stageId % 1000 == 0)
            {
                //Debug.Log($"제외 (000으로 끝남): {stageId} / {name}");
                continue;
            }
            // ====================

            newList.Add(new StagePrefabEntry
            {
                stageId = stageId,
                stagePrefab = prefab
            });
        }

        // 필요하면 ID 기준 정렬도 가능 (원하면 주석 해제)
        // newList.Sort((a, b) => a.stageId.CompareTo(b.stageId));

        Undo.RecordObject(data, "자동 Stage 프리팹 채우기");

        // private 필드 직접 세팅
        data.GetType()
            .GetField("stagePrefabEntries",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(data, newList);

        EditorUtility.SetDirty(data);
        //Debug.Log($"Stage 프리팹 자동 채우기 완료 (추가된 개수: {newList.Count}개)");
    }
}
#endif
