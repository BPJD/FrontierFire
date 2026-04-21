using UnityEngine;

public class UI_WeaponInfoHider : MonoBehaviour
{
    PlayerInputController playerInputController;
    GameObject player;
    [SerializeField] GameObject weaponInfo;


    bool isHidden = false;

    private void Start()
    {
        ComponentLoad();
    }

    private void FixedUpdate()
    {
        if (playerInputController != null)
        {
            isHidden = playerInputController.isInfoHide;
            weaponInfo.SetActive(isHidden);
        }
        else
        {
            ComponentLoad();
        }
    }

    void ComponentLoad()
    {
        player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
        playerInputController = player.GetComponent<PlayerInputController>();
        isHidden = ES3.Load<bool>(PlayerInputController.infoHideData, false);
        weaponInfo.SetActive(!isHidden);
    }

}
