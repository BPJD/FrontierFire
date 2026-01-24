using UnityEngine;

public class UI_MainMenu_Starter : MonoBehaviour
{
    GameObject player;
    PlayerInputController inputController;

    [SerializeField] GameObject mainUI;

    public bool isStarted = false;

    // Update is called once per frame
    void Update()
    {
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
            if(player != null)
            {
                inputController = player.GetComponent<PlayerInputController>();
                inputController.enabled = false;
            }
        }



    }


    void PlayerGameStart(bool isStart)
    {
        isStarted = isStart;

        mainUI.SetActive(!isStarted);
        if (inputController != null)
            inputController.enabled = isStarted;
    }


}
