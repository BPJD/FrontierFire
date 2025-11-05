using UnityEngine;
using UnityEngine.SceneManagement;


public class Stage_ClearPortal : MonoBehaviour, IInteractable
{


    public bool TryInteract()
    {
        Data_Scenes data = GameObject.FindGameObjectWithTag("Data").GetComponent<Data_Scenes>();
        Control_Stage stageCon = GameObject.FindGameObjectWithTag("GameController").GetComponent<Control_Stage>();

        SceneManager.LoadScene(data.stageScenes[stageCon.worldCur + 1]);

        return true;
    }
}
