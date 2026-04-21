using UnityEngine;
using UnityEngine.SceneManagement;


public class Stage_ClearPortal : MonoBehaviour, IInteractable
{
    Direction_SceneChanger sceneChanger;

    void Start()
    {
        sceneChanger = GameObject.FindGameObjectWithTag("GameController").GetComponent<Direction_SceneChanger>();
    }

    public bool TryInteract()
    {
        Data_Scenes data = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag).GetComponent<Data_Scenes>();
        Control_Stage stageCon = GameObject.FindGameObjectWithTag("GameController").GetComponent<Control_Stage>();

        sceneChanger.ChangeScene(data.stageScenes[stageCon.worldCur + 1], false);

        return true;
    }
}
