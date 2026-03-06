using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ButtonHighlighter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject obj;
    [SerializeField] private GameObject canvasObj;

    UI_InputDeviceDetector inputDetector;
    UI_GamePadSelectController gamePadSelectController;



    private void OnEnable()
    {
        GamePadDetected();
    }

    void CheckComponent()
    {
        if(inputDetector == null)
        {
            inputDetector = GameObject.FindGameObjectWithTag("Module").GetComponent<UI_InputDeviceDetector>();
            gamePadSelectController = GameObject.FindGameObjectWithTag("Module").GetComponent<UI_GamePadSelectController>();
        }
    }

    public void GamePadDetected()
    {
        CheckComponent();

        if (inputDetector.currentInputType == UI_InputDeviceDetector.InputType.Gamepad)
        {
            //EventSystem.current.SetSelectedGameObject(obj);
            
            if(canvasObj == null)
            {
                canvasObj = gameObject;
            }
            gamePadSelectController.UIFocusChanged(canvasObj);
        }
    }
}