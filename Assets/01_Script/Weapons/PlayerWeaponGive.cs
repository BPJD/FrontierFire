using UnityEngine;

public class PlayerWeaponGive : MonoBehaviour
{
    PlayerWeaponController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).GetComponent<PlayerWeaponController>();
        controller.GetWeapon(60000, 9999, 0, 0, null, 60);
    }
}
