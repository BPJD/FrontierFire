using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage_MainPortal : MonoBehaviour, IInteractable
{

    public bool TryInteract()
    {
        string firstStage = GameObject.FindGameObjectWithTag("Data").GetComponent<Data_Scenes>().stageScenes[1];

        SceneManager.LoadScene(firstStage);

        return true;
    }
}
