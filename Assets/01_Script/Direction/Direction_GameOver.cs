using UnityEngine;

public class Direction_GameOver : MonoBehaviour
{
    [SerializeField] GameObject uiGameOver;
    [SerializeField] float directionTimer = 5f;

    [SerializeField] GameObject gameWinPanel;

    bool isGameSet = false;

    [SerializeField] AudioClip gameOverClip;
    [SerializeField] AudioClip gameWinClip;

    public void PlayerDead()
    {
        if(isGameSet) return;

        Time.timeScale = 0.75f;
        uiGameOver.SetActive(true);
        isGameSet = true;
        GameOverBGM(false);
    }

    public void GameWin()
    {
        if (isGameSet) return;

        gameWinPanel.SetActive(true);
        isGameSet = true;
        GameOverBGM(true);
    }

    void GameOverBGM(bool isWin)
    {
        Direction_BGMPlay _player = GameObject.FindGameObjectWithTag("Sound").GetComponent<Direction_BGMPlay>();

        if (_player != null)
        {
            if (isWin)
            {
                _player.PlayBGM(gameWinClip);
            }
            else
            {
                _player.PlayBGM(gameOverClip);
            }
        }

    }
}
