using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage_MainPortal : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        SceneManager.LoadScene(1);
    }
}
