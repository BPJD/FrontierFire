using UnityEngine;

public class Stage_MainPortal : MonoBehaviour, IInteractable
{
    [SerializeField] string firstSceneName;

    public bool TryInteract()
    {

        GameObject _changer = GameObject.FindGameObjectWithTag("GameController");
        if(_changer != null)
        {
            _changer.GetComponent<Direction_SceneChanger>().ChangeScene(firstSceneName);
        }

        return true;
    }
}
