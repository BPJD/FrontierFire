using UnityEngine;

public class GameStopper : MonoBehaviour
{
    bool isStopped = false;
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F3))
        {
            isStopped = !isStopped;
            Time.timeScale = isStopped ? 0f : 1f;
            Debug.Log(isStopped ? "Game Stopped" : "Game Resumed");
        }
    }
}
