using UnityEngine;

public class Lobby_TutorialWeaponGive : MonoBehaviour
{
    [SerializeField] LobbyPlayerController lobbyController;

    [SerializeField] bool isWeaponGive = false;

    [SerializeField] GameObject panelTutorial;

    private void OnTriggerEnter(Collider other)
    {

        //Debug.Log("Tutorial Weapon Give Triggered");
        if (isWeaponGive)
        {
            lobbyController.PlayerWeaponEquip();
            panelTutorial.SetActive(true);
        }
        else
        {
            lobbyController.PlayerWeaponUnEquip();
            panelTutorial.SetActive(false);
        }

    }

}
