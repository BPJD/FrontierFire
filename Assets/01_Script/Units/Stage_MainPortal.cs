using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage_MainPortal : MonoBehaviour, IInteractable
{

    public bool TryInteract()
    {
        string firstStage = GameObject.FindGameObjectWithTag("Data").GetComponent<Data_Scenes>().stageScenes[1];

        GameObject _changer = GameObject.FindGameObjectWithTag("GameController");
        if(_changer != null)
        {
            _changer.GetComponent<Direction_SceneChanger>().ChangeScene(firstStage);
        }

        return true;
    }
}
