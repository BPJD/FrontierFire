#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class UnitSOAutoConnector
{
    [MenuItem("Tools/유닛 SO 자동 연결 (컴포넌트 대상)")]
    public static void AutoAssignSOToComponents()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            string objName = obj.name;
            string id = objName.Split('_')[0]; // "20001" 추출

            // 경로 지정
            string unitPath = $"Assets/02_Datas/UnitSO/Unit_{id}.asset";
            string aiPath = $"Assets/02_Datas/UnitAIParamsSO/UnitAI_{id}.asset";

            // SO 불러오기
            var unitSO = AssetDatabase.LoadAssetAtPath<UnitParamsSO>(unitPath);
            var aiSO = AssetDatabase.LoadAssetAtPath<UnitAIParamsSO>(aiPath);

            if (unitSO == null)
            {
                Debug.LogWarning($"Unit SO 없음: {unitPath}");
            }

            if (aiSO == null)
            {
                Debug.LogWarning($"AI SO 없음: {aiPath}");
            }

            // 컴포넌트 찾아서 할당
            var unitStatus = obj.GetComponent<UnitStatus>();
            if (unitStatus != null && unitSO != null)
            {
                Undo.RecordObject(unitStatus, "Unit SO 연결");
                unitStatus.unitDataSource = unitSO;
                EditorUtility.SetDirty(unitStatus);
            }

            var attackSys = obj.GetComponent<EnemyAttackSystem>();
            if (attackSys != null && aiSO != null)
            {
                Undo.RecordObject(attackSys, "AI SO 연결");
                attackSys.unitAIDataSource = aiSO;
                EditorUtility.SetDirty(attackSys);
            }

            Debug.Log($"{obj.name} - SO 연결 완료 (ID: {id})");
        }
    }
}
#endif
