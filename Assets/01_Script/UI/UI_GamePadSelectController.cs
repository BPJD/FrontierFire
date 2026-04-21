using UnityEngine;
using UnityEngine.EventSystems;

public class UI_GamePadSelectController : MonoBehaviour
{
    [SerializeField] GameObject focusedObjCur; // 현재 포커스된 캔버스 오브젝트

    [SerializeField] GameObject[] canvasPool; // 전체 캔버스 오브젝트 풀 (인덱스 맞춰서 버튼 풀과 연결)
    [SerializeField] GameObject[] firstButtonPool; // 각 캔버스에서 처음으로 포커스되어야 하는 버튼 오브젝트 풀 (인덱스 맞춰서 캔버스 풀과 연결)



    public void GamePadDetected()
    {
        SelectButton();
    }

    public void KeyboardDetected()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }


    public void UIFocusChanged(GameObject canvasObj)
    {
        focusedObjCur = canvasObj;
        SelectButton();
    }

    void SelectButton()
    {
        for (int i = 0; i < canvasPool.Length; i++)
        {

            if (focusedObjCur == canvasPool[i])
            {
                focusedObjCur = canvasPool[i];

                EventSystem.current.SetSelectedGameObject(firstButtonPool[i]);

                //Debug.Log($"포커스된 캔버스: {focusedObjCur.name}, 선택된 버튼: {firstButtonPool[i].name}");
                break;
            }
        }
    }


}
