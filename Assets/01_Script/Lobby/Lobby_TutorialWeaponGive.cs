using UnityEngine;

public class Lobby_TutorialWeaponGive : MonoBehaviour
{
    [SerializeField] LobbyPlayerController lobbyController;

    [SerializeField] bool isWeaponGive = false;

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("Tutorial Weapon Give Triggered");
        if (isWeaponGive)
        {
            lobbyController.PlayerWeaponEquip();
        }
        else
        {
            lobbyController.PlayerWeaponUnEquip();
        }

    }
}
