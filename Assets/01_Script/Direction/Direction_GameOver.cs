using UnityEngine;

public class Direction_GameOver : MonoBehaviour
{
    [SerializeField] GameObject uiGameOver;
    [SerializeField] float directionTimer = 5f;


    public void PlayerDead()
    {
        Time.timeScale = 0.75f;
        uiGameOver.SetActive(true);
    }
}
